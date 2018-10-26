using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FriendOrganizer.UI.Event;
using FriendOrganizer.UI.View.Service;
using Prism.Commands;
using Prism.Events;

namespace FriendOrganizer.UI.ViewModel
{
    public abstract class DetailViewModelBase: ViewModelBase,IDetailViewModel
    {
        protected readonly IEventAggregator EventAggregator;
        public IMassegeDialogService MassegeDialogService;
        private bool _hasChanges;
        private int _id;
        private string _title;   


        protected DetailViewModelBase(IEventAggregator eventAggregator, IMassegeDialogService massegeDialogService)
        {
            EventAggregator = eventAggregator;
            MassegeDialogService = massegeDialogService;
            SaveCommand = new DelegateCommand(OnSaveExecute, OnSaveCanExecute);
            DeleteCommand = new DelegateCommand(OnDeleteExecute);
            CloseDetailViewCommand = new DelegateCommand(OnCloseViewExecute);
        }

        protected async virtual  void OnCloseViewExecute()
        {
            if (HasChanges)
            {
                var result =
                   await MassegeDialogService
                        .ShowOkCancelDialogAsync($"You've made changes, Close this item?", "Question");
                if (result == MessageDialogResult.Cancel)
                {
                   return;

                }
            }
           EventAggregator.GetEvent<AfterDetailCloseEvent>().Publish(new AfterDetailCloseEventArgs
           {
               Id = this.Id,
               ViewModelName = this.GetType().Name
               
           });
        }

        public ICommand CloseDetailViewCommand { get; set; }

        public ICommand DeleteCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        protected abstract void OnDeleteExecute();

        protected abstract bool OnSaveCanExecute();

        protected abstract void OnSaveExecute();

        public abstract Task LoadAsync(int friendId);

 

        public int Id
        {
            get { return _id; }
         protected   set { _id = value; }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }



        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                if (_hasChanges != value)
                {
                    _hasChanges = value;
                    OnPropertyChanged();
                    ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }

        protected virtual void RaiseDetailDeletedEvent(int modelId)
        {
            EventAggregator.GetEvent<AfterDetailDeletedEvent>().Publish(new AfterDetailDeletedEventArgs
            {
                Id = modelId,
                ViewModelName = this.GetType().Name

            });
        }
        protected virtual void RaiseDetailSavedEvent(int modelId, string displayMember)
        {
            EventAggregator.GetEvent<AfterDetailSaveEvent>().Publish(new AfterDetailSaveEventArgs
            {
                Id = modelId,
                DisplayMember = displayMember,
                ViewModelName = this.GetType().Name

            });
        }
        protected async         Task
SaveWithOptimisticConcurrency(Func<Task> saveFunc, Action afterSaveAction)
        {
            try
            {
                await saveFunc();

            }
            catch (DbUpdateConcurrencyException e)
            {
                var databaseValue = e.Entries.Single().GetDatabaseValues();
                if (databaseValue == null)
                {
                   await MassegeDialogService.ShowInfoDialogAsync("The entity Has been deleted by another user");
                    RaiseDetailDeletedEvent(Id);
                    return;
                }
                var result =await MassegeDialogService.ShowOkCancelDialogAsync(
                    $"The entity in database is changed in the meantime" +
                    $"by someone else click save changes anyway" +
                    $", click cannel to reload entity from database.", "Question");
                if (result == MessageDialogResult.OK)
                {
                    // upate orignal values with database-value
                    var entity = e.Entries.Single();
                    entity.OriginalValues.SetValues(entity.GetDatabaseValues());
                    await saveFunc();
                }
                else
                {
                    // reload entity from database
                    await e.Entries.Single().ReloadAsync();
                    await LoadAsync(Id);
                }
            }

            afterSaveAction();

        }

    }
}
