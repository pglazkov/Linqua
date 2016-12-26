using System.Collections.Generic;
using System.Threading.Tasks;
using Linqua.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Linqua.Api.Controllers
{
    [Route("api/[controller]")]
    public class EntriesController : Controller
    {
        private readonly LinquaContext dbContext;

        public EntriesController(LinquaContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // GET tables/Entry
        [HttpGet]
        public async Task<IEnumerable<Entry>> Get()
        {
            return dbContext.Entries;
        }
    }
}