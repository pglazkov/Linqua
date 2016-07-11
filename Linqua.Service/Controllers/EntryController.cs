using System.Data.Entity;
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
    [Authorize]
    public class EntryController : TableController<Entry>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            LinquaContext context = new LinquaContext();
            DomainManager = new EntityDomainManager<Entry>(context, Request, true);
        }

        // GET tables/Entry
        public async Task<IQueryable<Entry>> GetAllEntries()
        {
            // Get the logged-in user.
            var currentUser = await this.GetUserInfoAsync();

            return Query().Where(e => e.UserId == currentUser.UserEntity.Id || e.ClientAppSpecificUserId == currentUser.AppSpecificMicrosoftUserId);
        }

        // GET tables/Entry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Entry> GetEntry(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Entry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Entry> PatchEntry(string id, Delta<Entry> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/Entry
        public async Task<IHttpActionResult> PostEntry(Entry item)
        {
            // Get the logged-in user.
            var currentUser = await this.GetUserInfoAsync();
            
            // Set the user ID on the item.
            item.ClientAppSpecificUserId = currentUser.AppSpecificMicrosoftUserId;
            item.User = currentUser.UserEntity;
            //item.UserEmail = currentUser.ProviderUserInfo.EmailAddress;

            ((EntityDomainManager<Entry>)DomainManager).Context.Entry(currentUser.UserEntity).State = EntityState.Unchanged;

            Entry current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new {id = current.Id}, current);
        }

        // DELETE tables/Entry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteEntry(string id)
        {
            return DeleteAsync(id);
        }
    }
}