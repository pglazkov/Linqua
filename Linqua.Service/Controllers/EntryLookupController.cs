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
    public class EntryLookupController : ApiController
    {
        // POST api/EntryLookup
        public async Task<Entry> Post(string entryText, string excludeId)
        {
            //Services.Log.Info("Hello from custom controller!");

            using (var ctx = new LinquaContext())
            {
                var foundEntries = await ctx.Entries
                                            .Where(x => x.Text == entryText && x.Definition != null && (string.IsNullOrEmpty(excludeId) || !Equals(x.Id, excludeId)))
                                            .ToListAsync();

                if (foundEntries != null && foundEntries.Count > 0)
                {
                    return foundEntries[0];
                }
            }

            return null;
        }
    }
}