using System.Threading.Tasks;
using FriendOrganizer.Model;

namespace FriendOrganizer.UI.Data.Reposities
{
    public interface IFriendRepository:IGenericRepository<Friend>
    {
       
        void RemovePhoneNumber(FriendPhoneNumber model);
        Task<bool> HasMeetingAsync(int friendId);
    }
}