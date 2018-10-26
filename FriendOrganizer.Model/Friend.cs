using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendOrganizer.Model
{
    public class Friend
    {
        public Friend()
        {
            FriendPhoneNumbers = new Collection<FriendPhoneNumber>();
            Meetings = new List<Meeting>();
        }
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(50)]
        [EmailAddress]
        public string Email { get; set; }

        public int? FavoriteLanuageId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
        public ProgrammingLanguage FavoriteLanguage { get; set; }

        public ICollection<FriendPhoneNumber> FriendPhoneNumbers { get; set; }
        public ICollection<Meeting> Meetings { get; set; }
    }
}
