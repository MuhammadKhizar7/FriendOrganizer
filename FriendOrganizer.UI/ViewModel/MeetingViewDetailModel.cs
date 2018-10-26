using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FriendOrganizer.Model;
using FriendOrganizer.UI.Data.Reposities;
using FriendOrganizer.UI.Event;
using FriendOrganizer.UI.View.Service;
using FriendOrganizer.UI.Wrapper;
using Prism.Commands;
using Prism.Events;

namespace FriendOrganizer.UI.ViewModel
{
    public class MeetingViewDetailModel : DetailViewModelBase, IMeetingViewDetailModel
    {
       
        private readonly IMeetingRepository _meetingRepository;

        public MeetingViewDetailModel(IEventAggregator eventAggregator, IMassegeDialogService massegeDialogService,
            IMeetingRepository meetingRepository) : base(eventAggregator, massegeDialogService)
        {
           
            _meetingRepository = meetingRepository;
            eventAggregator.GetEvent<AfterDetailSaveEvent>().Subscribe(AfterDetailSaved);
            eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);


            AddedFriends = new ObservableCollection<Friend>();
            AvailableFriends = new ObservableCollection<Friend>();
            AddFriendCommand = new DelegateCommand(OnAddFriendExecute, OnAddFriendCanExecute);
            RemoveFriendCommand = new DelegateCommand(OnRemoveFriendExecute, OnRemoveFriendCanExecute);
        }

       


        public ICommand RemoveFriendCommand { get; }

        public ICommand AddFriendCommand { get; }

        public ObservableCollection<Friend> AvailableFriends { get; }

        public ObservableCollection<Friend> AddedFriends { get; }

        public Friend SelectedFriend
        {
            get => _selectedFriend;
            set
            {
                _selectedFriend = value;
                OnPropertyChanged();
                ((DelegateCommand)RemoveFriendCommand).RaiseCanExecuteChanged();
            }
        }

        public Friend SelectedAvailableFriend
        {
            get => _selectedAvailableFriend1; set
            {
                _selectedAvailableFriend1 = value;
                OnPropertyChanged();
                ((DelegateCommand)AddFriendCommand).RaiseCanExecuteChanged();

            }

        }


        private MeetingWrapper _meeting;
        private Friend _selectedFriend;
        private Friend _selectedAvailableFriend1;
        private List<Friend> _allFriends;

        public MeetingWrapper Meeting
        {
            get { return _meeting; }
            set
            {
                _meeting = value;
                OnPropertyChanged();
            }
        }


        protected override async void OnDeleteExecute()
        {
            var result =
               await MassegeDialogService
                    .ShowOkCancelDialogAsync($"Do you really want to delete {Meeting.Title}", "Question");
            if (result == MessageDialogResult.OK)
            {
                _meetingRepository.Remove(Meeting.Model);
                await _meetingRepository.SaveAsync();
                RaiseDetailDeletedEvent(Meeting.Id);

            }
        }

        protected override bool OnSaveCanExecute()
        {
            return Meeting != null && !Meeting.HasErrors && HasChanges;
        }

        protected override async void OnSaveExecute()
        {
            await _meetingRepository.SaveAsync();
            HasChanges = _meetingRepository.HasChanges();
            Id = Meeting.Id;
            RaiseDetailSavedEvent(Meeting.Id, Meeting.Title);
        }

        public override async Task LoadAsync(int meetingId)
        {
            var meetng = meetingId >0  ? await _meetingRepository.GetByIdAsync(meetingId) : CreateNewMeeting();

            Id = meetingId;
            InitializeMeeting(meetng);

            _allFriends = await _meetingRepository.GetAllMeetingAsync();

            SetupPicklist();

        }

        private void SetupPicklist()
        {
            var meetingFriendIds = Meeting.Model.Friends.Select(f => f.Id).ToList();
            var addedFriends = _allFriends.Where(f=> meetingFriendIds.Contains(f.Id)).OrderBy(f=>f.FirstName);
            var availableFriends = _allFriends.Except(addedFriends).OrderBy(f => f.FirstName);

            AddedFriends.Clear();
            AvailableFriends.Clear();
            foreach (var addedFriend in addedFriends)
            {
                AddedFriends.Add(addedFriend);
                
            }

            foreach (var availableFriend in availableFriends)
            {
                AvailableFriends.Add(availableFriend);
            }
        }

        private void InitializeMeeting(Meeting meetng)
        {
            Meeting = new MeetingWrapper(meetng);
            Meeting.PropertyChanged += (s, e) =>
            {
                if (!HasChanges)
                {
                    HasChanges = _meetingRepository.HasChanges();
                }
                if (e.PropertyName == nameof(Meeting.HasErrors))
                {
                    ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
                }

                if (e.PropertyName== Meeting.Title)
                {
                    SetTitle();
                }
            };
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            if (Meeting.Id == 0)
            {
                Meeting.Title = "";
            }
            
            SetTitle();

        }

        private void SetTitle()
        {
            Title = Meeting.Title;
        }

        private Meeting CreateNewMeeting()
        {
            var meeting = new Meeting
            {
                DateFrom = DateTime.Now,
                DateTo = DateTime.Now
            };
            _meetingRepository.Add(meeting);
            return meeting;
        }
       

        private bool OnRemoveFriendCanExecute()
        {
            return SelectedFriend != null;
        }

        private void OnRemoveFriendExecute()
        {
            var friendToRemove = SelectedFriend;
            Meeting.Model.Friends.Remove(friendToRemove);
            AvailableFriends.Add(friendToRemove);
            AddedFriends.Remove(friendToRemove);
            HasChanges = _meetingRepository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private bool OnAddFriendCanExecute()
        {
            return SelectedAvailableFriend != null;
        }

        private void OnAddFriendExecute()
        {
            var friendToAdd = SelectedAvailableFriend;
            Meeting.Model.Friends.Add(friendToAdd);
            AddedFriends.Add(friendToAdd);
            AvailableFriends.Remove(friendToAdd);
            HasChanges = _meetingRepository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

        }
        private async void AfterDetailSaved(AfterDetailSaveEventArgs args)
        {
            if (args.ViewModelName == nameof(FriendDetailVeiwModel))
            {
                await _meetingRepository.ReloadFriendAsync(args.Id);
                _allFriends = await _meetingRepository.GetAllMeetingAsync();

                SetupPicklist();
            }
        }
        private async void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            if (args.ViewModelName == nameof(FriendDetailVeiwModel))
            {
            
                _allFriends = await _meetingRepository.GetAllMeetingAsync();

                SetupPicklist();
            }
        }
    }
}
