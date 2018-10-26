using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace FriendOrganizer.UI.Event
{
    public class AfterDetailDeletedEvent:PubSubEvent<AfterDetailDeletedEventArgs>
    {
    }

   public class AfterDetailDeletedEventArgs
    {
        public int? Id { get; set; }
        public string ViewModelName { get; set; }
    }
}
