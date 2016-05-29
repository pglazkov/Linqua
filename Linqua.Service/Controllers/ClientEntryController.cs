using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Linqua.Service.DataObjects;
using Linqua.Service.Models;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;

namespace Linqua.Service.Controllers
{
    [MobileAppController]
    [Authorize]
    public class ClientEntryController : TableController<ClientEntry>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            LinquaContext context = new LinquaContext();
            DomainManager = new EntryDomainManager(context, Request, true);
        }

        // GET tables/ClientEntry
        public async Task<IQueryable<ClientEntry>> GetAllEntries()
        {
            // Get the logged-in user.
            var currentUser = await this.GetUserInfoAsync();

            return Query().Where(e => e.UserId == currentUser.ProviderUserInfo.ProviderPrefixedUserId || e.UserId == currentUser.AppSpecificMicrosoftUserId);
        }

        // GET tables/ClientEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<ClientEntry> GetEntry(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/ClientEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<ClientEntry> PatchEntry(string id, Delta<ClientEntry> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/ClientEntry
        public async Task<IHttpActionResult> PostEntry(ClientEntry item)
        {
            // Get the logged-in user.
            var currentUser = await this.GetUserInfoAsync();

            // Set the user ID on the item.
            item.UserId = currentUser.ProviderUserInfo.ProviderPrefixedUserId;

            ClientEntry current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new {id = current.Id}, current);
        }

        // DELETE tables/ClientEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteEntry(string id)
        {
            return DeleteAsync(id);
        }
    }
}