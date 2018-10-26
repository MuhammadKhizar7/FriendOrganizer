using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FriendOrganizer.Model;
using FriendOrganizer.UI.Data;
using FriendOrganizer.UI.Data.Lookups;
using FriendOrganizer.UI.Event;
using Prism.Events;

namespace FriendOrganizer.UI.ViewModel
{
    public class NavigationViewModel :ViewModelBase, INavigationViewModel
    {
        
        private readonly IFriendLookupDataService _friendLookupDataService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMeetingLookupDataService _meetingLookupDataService;

        public NavigationViewModel(IFriendLookupDataService friendLookupDataService,
            IEventAggregator eventAggregator, IMeetingLookupDataService meetingLookupDataService
          )
        {
            
            _friendLookupDataService = friendLookupDataService;
            _eventAggregator = eventAggregator;
            _meetingLookupDataService = meetingLookupDataService;
            Meetings = new ObservableCollection<NavigationItemViewModel>();
            Friends = new ObservableCollection<NavigationItemViewModel>();
            _eventAggregator.GetEvent<AfterDetailSaveEvent>().Subscribe(AfterDetailSaved);
            _eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);
        }

      
        public async Task LoadAsync()
        {
            var lookup = await _friendLookupDataService.GetLookupFriendAsync();
            Friends.Clear();
            foreach (var item in lookup)
            {
             Friends.Add(new NavigationItemViewModel(item.Id, item.DisplayMember,
                 _eventAggregator, nameof(FriendDetailVeiwModel)));   
            }

             lookup = await _meetingLookupDataService.GetMeetingLookupAsync();
            Meetings.Clear();
            foreach (var item in lookup)
            {
                Meetings.Add(new NavigationItemViewModel(item.Id, item.DisplayMember,
                    _eventAggregator, nameof(MeetingViewDetailModel)));
            }
        }
       


        public ObservableCollection<NavigationItemViewModel> Friends { get; set; }
        public ObservableCollection<NavigationItemViewModel> Meetings { get; set; }


        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            switch (args.ViewModelName)
            {
                case nameof(FriendDetailVeiwModel):
                    AfterDetailDeletedfter(Friends, args);
                    break;
                case nameof(MeetingViewDetailModel):
                    AfterDetailDeletedfter(Meetings, args);
                    break;

            }
            
        }

        private void AfterDetailDeletedfter(ObservableCollection<NavigationItemViewModel> items, AfterDetailDeletedEventArgs args)
        {
            var item = items.SingleOrDefault(f => f.Id == args.Id);
            if (item != null)
            {
                items.Remove(item);
            }
        }

        private void AfterDetailSaved(AfterDetailSaveEventArgs args)
        {
            switch (args.ViewModelName)
            {
                case nameof(FriendDetailVeiwModel):
                    AfterDetailSaved(Friends ,args);
                    break;
                case nameof(MeetingViewDetailModel):
                    AfterDetailSaved(Meetings, args);
                    break;
            }
           

        }

        private void AfterDetailSaved(ObservableCollection<NavigationItemViewModel> items, AfterDetailSaveEventArgs obj)
        {
            var lookupItem = items.SingleOrDefault(l => l.Id == obj.Id);
            if (lookupItem == null)
            {
                items.Add(new NavigationItemViewModel(obj.Id, obj.DisplayMember,
                    _eventAggregator, obj.ViewModelName));
            }
            else
            {
                lookupItem.DisplayMember = obj.DisplayMember;

            }
        }
    }
}
