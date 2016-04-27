﻿using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using Linqua.Service.DataObjects;
using Linqua.Service.Models;
using Microsoft.Azure.Mobile.Server;

namespace Linqua.Service.Controllers
{
    public class EntryDomainManager : MappedEntityDomainManager<ClientEntry, Entry>
    {
        public EntryDomainManager(DbContext context, HttpRequestMessage request) : base(context, request)
        {
        }

        public EntryDomainManager(DbContext context, HttpRequestMessage request, bool enableSoftDelete) : base(context, request, enableSoftDelete)
        {
        }

        public override SingleResult<ClientEntry> Lookup(string id)
        {
            return LookupEntity(x => x.Id == id);
        }

        public override Task<ClientEntry> UpdateAsync(string id, Delta<ClientEntry> patch)
        {
            return UpdateEntityAsync(patch, id);
        }

        public override Task<bool> DeleteAsync(string id)
        {
            return this.DeleteItemAsync(id);
        }
    }
}