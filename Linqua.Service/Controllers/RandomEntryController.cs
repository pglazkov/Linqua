using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Linqua.Service.DataObjects;
using Linqua.Service.Models;
using Microsoft.WindowsAzure.Mobile.Service.Security;

namespace Linqua.Service.Controllers
{
	[AuthorizeLevel(AuthorizationLevel.User)]
	public class RandomEntryController : ApiController
	{
		// GET api/RandomEntry
		public async Task<ClientEntry> Get(string excludeId)
		{
			var currentUser = (ServiceUser)User;

			var indexGenerator = new Random((int)DateTime.UtcNow.Ticks);

			using (var ctx = new LinquaContext())
			{
				var foundEntries = await ctx.Entries
											.Where(x => x.UserId == currentUser.Id && !x.Deleted && !x.IsLearnt && !Equals(x.Id, excludeId))
											.ToListAsync();

				if (foundEntries != null && foundEntries.Count > 0)
				{
					var randomIndex = indexGenerator.Next(0, foundEntries.Count - 1);

					var randomEntry = foundEntries[randomIndex];

					return Mapper.Map<ClientEntry>(randomEntry);
				}
			}

			return null;
		}
	}
}