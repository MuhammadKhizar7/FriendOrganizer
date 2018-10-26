using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace FriendOrganizer.UI.View.Service
{
    public class MassegeDialogService : IMassegeDialogService
    {
        private MetroWindow MetroWindow { get { return (MetroWindow)App.Current.MainWindow; } }
        public async Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title)
        {
            
            var result = await MetroWindow.ShowMessageAsync(title, text, MessageDialogStyle.AffirmativeAndNegative);
            return result == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative ? MessageDialogResult.OK : MessageDialogResult.Cancel;
        }

        public async Task ShowInfoDialogAsync(string text)
        {
           await MetroWindow.ShowMessageAsync("Info", text);
        }
    }

   public enum MessageDialogResult
    {
        OK,
        Cancel

    }
}
