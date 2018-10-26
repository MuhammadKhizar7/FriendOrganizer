using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using FriendOrganizer.Model;
using Prism.Commands;

namespace FriendOrganizer.UI.Wrapper
{
    public class ProgrammingLanguageWrapper:ModelWrapper<ProgrammingLanguage>
    {
       

       

        public ProgrammingLanguageWrapper(ProgrammingLanguage model) : base(model)
        {

        }

        public int Id => Model.Id;

        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
      
    }
}