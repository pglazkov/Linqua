using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;
using Linqua.Service.Models;
using Microsoft.Azure.Mobile.Server.Config;

namespace Linqua.Service.Controllers
{
    [MobileAppController]
    [Authorize]
    public class EntryCountController : ApiController
    {
        // GET api/EntryHasAny
        public async Task<int> Get()
        {
            var currentUser = await this.GetUserInfoAsync();

            using (var ctx = new LinquaContext())
            {
                var result = await ctx.Entries.CountAsync(x => (x.UserId == currentUser.UserEntity.Id || x.ClientAppSpecificUserId == currentUser.AppSpecificMicrosoftUserId) && !x.Deleted);

                return result;
            }
        }
    }
}