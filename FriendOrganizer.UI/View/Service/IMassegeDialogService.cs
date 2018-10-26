using System.Threading.Tasks;

namespace FriendOrganizer.UI.View.Service
{
    public interface IMassegeDialogService
    {
        Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title);
        Task ShowInfoDialogAsync(string text);
    }
}