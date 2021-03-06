﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Linqua.Service.Models;
using Microsoft.Azure.Mobile.Server.Config;

namespace Linqua.Service.Controllers
{
    [MobileAppController]
    [Authorize]
    public class RandomEntryController : ApiController
    {
        // GET api/RandomEntry
        public async Task<IEnumerable<Entry>> Get(int number)
        {
            var currentUser = await this.GetUserInfoAsync();

            var indexGenerator = new Random((int)DateTime.UtcNow.Ticks);

            using (var ctx = new LinquaContext())
            {
                var foundEntries = await ctx.Entries
                                            .Where(x => (x.UserId == currentUser.UserEntity.Id || x.ClientAppSpecificUserId == currentUser.AppSpecificMicrosoftUserId) && !x.Deleted && !x.IsLearnt)
                                            .ToListAsync();

                if (foundEntries != null && foundEntries.Count > 0)
                {
                    var randomEntries = new List<Entry>();
                    var excludeIndices = new HashSet<int>();

                    number = Math.Min(number, foundEntries.Count);

                    for (var i = 0; i < number; i++)
                    {
                        int randomIndex;
                        do
                        {
                            randomIndex = indexGenerator.Next(0, foundEntries.Count - 1);
                        } while (excludeIndices.Contains(randomIndex));

                        excludeIndices.Add(randomIndex);

                        var randomEntry = foundEntries[randomIndex];

                        randomEntries.Add(randomEntry);
                    }

                    var result = randomEntries.ToArray();

                    return result;
                }
            }

            return null;
        }
    }
}