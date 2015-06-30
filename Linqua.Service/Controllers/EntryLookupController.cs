using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Linqua.Service.DataObjects;
using Linqua.Service.Models;
using Microsoft.WindowsAzure.Mobile.Service;

namespace Linqua.Service.Controllers
{
    public class EntryLookupController : ApiController
    {
        public ApiServices Services { get; set; }

        // POST api/EntryLookup
        public async Task<ClientEntry> Post(string entryText, string excludeId)
        {
            //Services.Log.Info("Hello from custom controller!");

	        using (var ctx = new LinquaContext())
	        {
		        var foundEntries = await ctx.Entries
		                                    .Where(x => x.Text == entryText && x.Definition != null && (string.IsNullOrEmpty(excludeId) || !Equals(x.Id, excludeId)))
		                                    .ToListAsync();

		        if (foundEntries != null && foundEntries.Count > 0)
		        {
			        return Mapper.Map<ClientEntry>(foundEntries[0]);
		        }
	        }

	        return null;
        }

    }
}
