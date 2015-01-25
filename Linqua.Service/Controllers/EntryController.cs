using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Linqua.Service.DataObjects;
using Linqua.Service.Models;
using Microsoft.WindowsAzure.Mobile.Service;

namespace Linqua.Service.Controllers
{
    public class EntryController : TableController<ClientEntry>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            LinquaContext context = new LinquaContext();
            DomainManager = new EntryDomainManager(context, Request, Services);
        }

        // GET tables/ClientEntry
        public IQueryable<ClientEntry> GetAllEntries()
        {
            return Query();
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
            ClientEntry current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/ClientEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteEntry(string id)
        {
            return DeleteAsync(id);
        }
    }
}