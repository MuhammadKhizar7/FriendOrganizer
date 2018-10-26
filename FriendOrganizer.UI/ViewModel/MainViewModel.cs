using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autofac.Features.Indexed;
using FriendOrganizer.Model;
using FriendOrganizer.UI.Data;
using FriendOrganizer.UI.Event;
using FriendOrganizer.UI.View.Service;
using Prism.Commands;
using Prism.Events;

namespace FriendOrganizer.UI.ViewModel
{
   public class MainViewModel:ViewModelBase
    {

        private readonly IEventAggregator _eventAggregator;
        private readonly IMassegeDialogService _massegeDialogService;
        private IDetailViewModel _selectedDetailVeiwModel;



        public MainViewModel(INavigationViewModel navigationViewModel,
           IIndex<string, IDetailViewModel> detailViewModelCreator,
             IEventAggregator eventAggregator, IMassegeDialogService massegeDialogService)
        {
          
            _eventAggregator = eventAggregator;
            _massegeDialogService = massegeDialogService;
            DetailViewModels = new ObservableCollection<IDetailViewModel>();
            eventAggregator.GetEvent<OpenDetailViewEvent>()
                .Subscribe(OnOpenDetailVeiw);

            CreateNewDetailCommand = new DelegateCommand<Type>(OnCreateNewDetailExecute);
            OpenSingleViewCommand = new DelegateCommand<Type>(OnOpenSingleDetailExecute);
            _eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);
            _eventAggregator.GetEvent<AfterDetailCloseEvent>().Subscribe(AfterDetailClosed);

            NavigationViewModel = navigationViewModel;
            DetailViewModelCreator = detailViewModelCreator;
        }

       


        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();

        }

        public ICommand CreateNewDetailCommand { get; }
        public ICommand OpenSingleViewCommand { get; }
        public INavigationViewModel NavigationViewModel { get;  }
        public IIndex<string, IDetailViewModel> DetailViewModelCreator { get; }

        public ObservableCollection<IDetailViewModel> DetailViewModels { get;  }

        public IDetailViewModel SelectedDetailVeiwModel
        {
            get => _selectedDetailVeiwModel;
            set
            {
                _selectedDetailVeiwModel = value;
                OnPropertyChanged();
            }
        }




        private async void OnOpenDetailVeiw(OpenDetailViewEventArgs args)
        {
            // multi tab
            var detailViewModel =
                DetailViewModels.SingleOrDefault(vm => vm.Id == args.Id && vm.GetType().Name == args.ViewModelName);
            if (detailViewModel ==null)
            {
                detailViewModel = DetailViewModelCreator[args.ViewModelName];
                try
                {
                    await  detailViewModel.LoadAsync(args.Id);
                }
                catch 
                {
                  await _massegeDialogService.ShowInfoDialogAsync($"Could not load entity, maybe it wass deleted meantime" +
                                                        $"by another user." +
                                                        $"The navigation is refreshed by for you");
                    await NavigationViewModel.LoadAsync();
                    return;
                }
                DetailViewModels.Add(detailViewModel);
            }

            SelectedDetailVeiwModel = detailViewModel;
            // Single view logic
//            if (SelectedDetailVeiwModel != null && SelectedDetailVeiwModel.HasChanges)
//            {
//                var result = _massegeDialogService.ShowOkCancelDialog("You've made changes. Navigate away?", "Question");
//                if (result == MessageDialogResult.Cancel)
//                {
//                    return;
//                }
//            }

//            SelectedDetailVeiwModel = DetailViewModelCreator[args.ViewModelName];

            //    await SelectedDetailVeiwModel.LoadAsync(args.Id);
        }

        private int nextNewItemId = 0;
        private void OnCreateNewDetailExecute(Type viewModelType)
        {
            OnOpenDetailVeiw(new OpenDetailViewEventArgs{ Id =nextNewItemId--, ViewModelName = viewModelType.Name});
        }
        private void OnOpenSingleDetailExecute(Type viewModelType)
        {
            OnOpenDetailVeiw(new OpenDetailViewEventArgs { Id = -1, ViewModelName = viewModelType.Name });

        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            // single view
            //  SelectedDetailVeiwModel = null;
            RemoveDetailViewMoel(args.Id,args.ViewModelName);

        }

        private void AfterDetailClosed(AfterDetailCloseEventArgs args)
        {
            RemoveDetailViewMoel(args.Id, args.ViewModelName);

        }
        private void RemoveDetailViewMoel(int? id, string viewModelName)
        {
            var detailViewModel =
                DetailViewModels.SingleOrDefault(vm => vm.Id == id && vm.GetType().Name == viewModelName);
            if (detailViewModel != null)
            {
                DetailViewModels.Remove(detailViewModel);
            }
        }
        

    }
}
