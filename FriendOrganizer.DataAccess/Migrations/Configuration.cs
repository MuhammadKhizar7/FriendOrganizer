using System.Collections.Generic;
using FriendOrganizer.Model;

namespace FriendOrganizer.DataAccess.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<FriendOrganizer.DataAccess.FriendOrganizerDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(FriendOrganizer.DataAccess.FriendOrganizerDbContext context)
        {
            context.Friends.AddOrUpdate(f=> f.FirstName,
             new Friend { FirstName = "Ali", LastName = "Amjad", Email = "ali@gmail.com" },
             new Friend { FirstName = "Talal", LastName = "Amjad", Email = "talal@gmail.com" },
             new Friend { FirstName = "Humza", LastName = "Amjad", Email = "humza@gmail.com" },
             new Friend { FirstName = "Hassan", LastName = "Fayaz", Email = "hassan@gmail.com" });

            context.ProgrammingLanguages.AddOrUpdate(pl => pl.Name,
                new ProgrammingLanguage { Name = " C# "},
                new ProgrammingLanguage { Name = "TypeScript" },
                new ProgrammingLanguage { Name = " F# " },
                new ProgrammingLanguage { Name = "Java"},
                new ProgrammingLanguage { Name = "Pyhton"});
            context.SaveChanges();

            context.FriendPhoneNumbers.AddOrUpdate(p=>p.Number,
                new FriendPhoneNumber{ Number = "923007004321", FriendId = context.Friends.First().Id });

            context.Meetings.AddOrUpdate(p => p.Title,
                new Meeting
                {
                    Title = "Watching Soccer",
                    DateFrom = new DateTime(2018, 5, 26),
                    DateTo = new DateTime(2018, 5, 26),
                    Friends = new List<Friend>
                    {
                        context.Friends.Single(f=>f.FirstName=="Ali" && f.LastName=="Amjad"),
                        context.Friends.Single(f=>f.FirstName=="Humza" && f.LastName=="Amjad")
                    }
                });
        }
    }
}
