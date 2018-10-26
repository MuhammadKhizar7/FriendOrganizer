using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FriendOrganizer.DataAccess;
using FriendOrganizer.Model;

namespace FriendOrganizer.UI.Data.Reposities
{
    public  class FriendRepository :GenericRepository<Friend, FriendOrganizerDbContext>, IFriendRepository
    {
       

        public FriendRepository(FriendOrganizerDbContext context):base(context)
        {
         
        }
        public override async Task<Friend> GetByIdAsync( int friendId)
        {
            
            
                return await Context.Friends.Include(f=>f.FriendPhoneNumbers).SingleAsync(f=>f.Id==friendId);
            
        }

        public void RemovePhoneNumber(FriendPhoneNumber model)
        {
            Context.FriendPhoneNumbers.Remove(model);
        }

        public async Task<bool> HasMeetingAsync(int friendId)
        {
            return await Context.Meetings.AsNoTracking().Include(m => m.Friends).AnyAsync(m=> m.Friends.Any(f=> f.Id == friendId));
        }
    }
}
