using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FriendOrganizer.Model;

namespace FriendOrganizer.UI.Wrapper
{
    public class FriendWrapper: ModelWrapper<Friend>
    {

        public FriendWrapper(Friend model) : base(model)
        {
        }

        public int Id
        {
            get => Model.Id;
        }

        public string FirstName
        {
            get=> GetValue<string>();
            set => SetValue(value);
        }

       

        public string LastName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string Email
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public int? FavoriteLanuageId
        {
            get => GetValue<int?>();

            set => SetValue(value);
        }



        protected override IEnumerable<string> ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(FirstName):
                    if (string.Equals(FirstName, "Rebot", StringComparison.OrdinalIgnoreCase))
                {
                    yield return "Rebot are not valid friend";
                }
                    break;
            }
            }
        }
}
