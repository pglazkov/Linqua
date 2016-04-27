using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;

namespace Linqua.Service.Controllers
{
    public static class ApiControllerExtensions
    {
        public static async Task<string> GetLegacyUserIdAsync(this ApiController controller)
        {
            MicrosoftAccountCredentials creds = await controller.User.GetAppServiceIdentityAsync<MicrosoftAccountCredentials>(controller.Request);
            string mobileServicesUserId = creds.Provider + ":" + creds.UserClaims.Single(x => x.Type == ClaimTypes.NameIdentifier);

            return mobileServicesUserId;
        }
    }
}