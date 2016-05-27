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
        public static async Task<LinquaUserInfo> GetUserIdAsync(this ApiController controller)
        {
            string legacyUserId = null;

            MicrosoftAccountCredentials creds = await controller.User.GetAppServiceIdentityAsync<MicrosoftAccountCredentials>(controller.Request);
            var userId = creds.UserClaims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var provider = creds.Provider;

            var primaryUserId = FormatUserId(provider, userId);

            IEnumerable<string> legacyUserIdHeaderValues;
            if (controller.Request.Headers.TryGetValues(LegacyUserId.HeaderKey, out legacyUserIdHeaderValues))
            {
                var legacyUserIdHeaderValue = legacyUserIdHeaderValues.SingleOrDefault();

                legacyUserId = FormatUserId(provider, legacyUserIdHeaderValue);
            }

            return new LinquaUserInfo(primaryUserId, legacyUserId);
        }

        private static string FormatUserId(string provider, string userId)
        {
            return provider + ":" + userId;
        }
    }

    public class LinquaUserInfo
    {
        public LinquaUserInfo(string primaryUserId, string legacyUserId)
        {
            PrimaryUserId = primaryUserId;
            LegacyUserId = legacyUserId;
        }

        public string PrimaryUserId { get; private set; }
        public string LegacyUserId { get; private set; }
    }
}