using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FriendOrganizer.Model;
using FriendOrganizer.UI.Data;
using FriendOrganizer.UI.Data.Lookups;
using FriendOrganizer.UI.Data.Reposities;
using FriendOrganizer.UI.Event;
using FriendOrganizer.UI.View.Service;
using FriendOrganizer.UI.Wrapper;
using Prism.Commands;
using Prism.Events;

namespace FriendOrganizer.UI.ViewModel
{
    public class FriendDetailVeiwModel : DetailViewModelBase, IFriendDetailVeiwModel
    {
        private readonly IFriendRepository _friendRepository;
        
        private readonly IProgrammingLanguageLookupDataService _programmingLanguageLookupDataService;
        private FriendWrapper _friend;
        private FriendPhoneNumberWrapper _selectedPhoneNumber;


        public FriendDetailVeiwModel(IFriendRepository friendRepository, IEventAggregator eventAggregator,
            IMassegeDialogService massegeDialogService, 
            IProgrammingLanguageLookupDataService programmingLanguageLookupDataService):base(eventAggregator, massegeDialogService)
        {
            _friendRepository = friendRepository;
            _programmingLanguageLookupDataService = programmingLanguageLookupDataService;
            AddPhoneNumberCommand = new DelegateCommand(OnAddPhoneNumberExecute);
            RemovePhoneNumberCommand = new DelegateCommand(OnRemovePhoneNumberExecute, OnRemovePhoneNumberCanExecute);

            ProgrammingLanguages = new ObservableCollection<LookupItem>();
            PhoneNumbers = new ObservableCollection<FriendPhoneNumberWrapper>();

        }

        private bool OnRemovePhoneNumberCanExecute()
        {
            return SelectedPhoneNumber != null;
        }

        private void OnRemovePhoneNumberExecute()
        {
            SelectedPhoneNumber.PropertyChanged -= FriendPhoneNumberWrapper_PropertyChanged;
            _friendRepository.RemovePhoneNumber(SelectedPhoneNumber.Model);
            PhoneNumbers.Remove(SelectedPhoneNumber);
            SelectedPhoneNumber = null;
            HasChanges = _friendRepository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private void OnAddPhoneNumberExecute()
        {
           var newNumber = new FriendPhoneNumberWrapper(new FriendPhoneNumber());
            newNumber.PropertyChanged += FriendPhoneNumberWrapper_PropertyChanged;
            PhoneNumbers.Add(newNumber);
            Friend.Model.FriendPhoneNumbers.Add(newNumber.Model);
            newNumber.Number = ""; // trigger validation 
        }


        public override async Task LoadAsync(int friendId)
        {
            var friend = friendId > 0 ? await _friendRepository.GetByIdAsync(friendId) : CreateNewFriend();

            Id = friendId;
            InitializeFriend(friend);
            InitializeFriendPhoneNumbers(friend.FriendPhoneNumbers);
            await LoadProgrammingLanguageLookupAsync();

        }

        private void InitializeFriendPhoneNumbers(ICollection<FriendPhoneNumber> phoneNumbers)
        {
            foreach (var wrapper in PhoneNumbers)
            {
                wrapper.PropertyChanged -= FriendPhoneNumberWrapper_PropertyChanged;
            }
            PhoneNumbers.Clear();
            foreach (var friendPhoneNumber in phoneNumbers)
            {
                var wrapper = new FriendPhoneNumberWrapper(friendPhoneNumber);
                PhoneNumbers.Add(wrapper);
                wrapper.PropertyChanged += FriendPhoneNumberWrapper_PropertyChanged;

            }

        }

        private void FriendPhoneNumberWrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!HasChanges)
            {
                HasChanges = _friendRepository.HasChanges();
            }

            if (e.PropertyName== nameof(FriendPhoneNumberWrapper.HasErrors))
            {
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public FriendWrapper Friend
        {
            get { return _friend; }
            private set
            {
                _friend = value;
                OnPropertyChanged();
            }
        }

       

        public FriendPhoneNumberWrapper SelectedPhoneNumber
        {
            get => _selectedPhoneNumber;
            set
            {
                _selectedPhoneNumber = value;
                OnPropertyChanged();
                ((DelegateCommand)RemovePhoneNumberCommand).RaiseCanExecuteChanged();
            }
        }

       
        public ICommand AddPhoneNumberCommand { get; }
        public ICommand RemovePhoneNumberCommand { get; }
        public ObservableCollection<LookupItem> ProgrammingLanguages { get;  }
        public ObservableCollection<FriendPhoneNumberWrapper> PhoneNumbers { get;  }

        protected  override async void OnSaveExecute()
        {
            await SaveWithOptimisticConcurrency(_friendRepository.SaveAsync, () =>
            { 
                HasChanges = _friendRepository.HasChanges();
                Id = Friend.Id;
                RaiseDetailSavedEvent(Friend.Id, $"{Friend.FirstName} {Friend.LastName}");
            });
            
           
          
        }


        protected override bool OnSaveCanExecute()
        {
            return Friend !=null && !Friend.HasErrors && PhoneNumbers.All(ph=> !ph.HasErrors) && HasChanges;
        }

        private Friend CreateNewFriend()
        {
           var friend = new Friend();
            _friendRepository.Add(friend);
            return friend;
        }

        protected override async void OnDeleteExecute()
        {
            if (await _friendRepository.HasMeetingAsync(Friend.Id))
            {
              await  MassegeDialogService.ShowInfoDialogAsync($"{Friend.FirstName} {Friend.LastName} can't be deteled, as this friend is part of at least one of meeting");
                return;
            }
            var result =
              await  MassegeDialogService
                    .ShowOkCancelDialogAsync($"Do you really want to delete {Friend.FirstName} {Friend.LastName}", "Question");
            if (result == MessageDialogResult.OK)
            {
                _friendRepository.Remove(Friend.Model);
                await _friendRepository.SaveAsync();
                RaiseDetailDeletedEvent(Friend.Id);
               
            }
          
        }
        private async Task LoadProgrammingLanguageLookupAsync()
        {
            ProgrammingLanguages.Clear();
            ProgrammingLanguages.Add(new NullLookupItem{ DisplayMember = " - "});
            var lookups = await _programmingLanguageLookupDataService.GetProgrammingLanguageLookupAsync();
            foreach (var lookup in lookups)
            {
                ProgrammingLanguages.Add(lookup);
            }
        }
        private void InitializeFriend(Friend friend)
        {
            Friend = new FriendWrapper(friend);
            Friend.PropertyChanged += (s, e) =>
            {
                if (!HasChanges)
                {
                    HasChanges = _friendRepository.HasChanges();
                }
                if (e.PropertyName == nameof(Friend.HasErrors))
                {
                    ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
                }

                if (e.PropertyName == nameof(Friend.FirstName)||e.PropertyName == nameof(Friend.LastName))
                {
                    SetTitle();
                }
            };
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            if (Friend.Id == 0)
            {
                Friend.FirstName = "";
            }
            SetTitle();
        }

        private void SetTitle()
        {
            Title = $"{Friend.FirstName} {Friend.LastName}";
        }
    }
}
