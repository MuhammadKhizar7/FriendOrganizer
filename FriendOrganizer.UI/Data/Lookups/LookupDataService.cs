using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FriendOrganizer.DataAccess;
using FriendOrganizer.Model;

namespace FriendOrganizer.UI.Data.Lookups
{
   public class LookupDataService : IFriendLookupDataService, IProgrammingLanguageLookupDataService,
       IMeetingLookupDataService
    {
        private readonly Func<FriendOrganizerDbContext> _db;

        public LookupDataService(Func<FriendOrganizerDbContext> db)
        {
            _db = db;
        }

        public async Task<IEnumerable<LookupItem>> GetLookupFriendAsync()
        {
            using (var ctx =_db() )
            {
                return await ctx.Friends.AsNoTracking()
                    .Select(f=> new LookupItem
                        {
                            Id = f.Id,
                            DisplayMember = f.FirstName + " " + f.LastName
                        }).ToListAsync();

            }
        }

       

        public async Task<IEnumerable<LookupItem>> GetProgrammingLanguageLookupAsync()
        {
            using (var ctx =_db() )
            {
                return await ctx.ProgrammingLanguages.AsNoTracking()
                    .Select(f=> new LookupItem
                        {
                            Id = f.Id,
                            DisplayMember = f.Name
                        }).ToListAsync();

            }
        }

        public async Task<List<LookupItem>> GetMeetingLookupAsync()
        {
            using (var ctx = _db())
            {
                return await ctx.Meetings.AsNoTracking()
                    .Select(f => new LookupItem
                    {
                        Id = f.Id,
                        DisplayMember = f.Title
                    }).ToListAsync();

            }
        }
    }
}
