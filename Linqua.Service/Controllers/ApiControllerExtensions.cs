using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;

namespace Linqua.Service.Controllers
{
    public static class ApiControllerExtensions
    {
        public static async Task<LinquaUserInfo> GetUserInfoAsync(this ApiController controller)
        {
            var providerUser = await GetProviderUserInfoAsync(controller);

            var appSpecificUserId = GetAppSpecificUserId(controller);

            return new LinquaUserInfo(appSpecificUserId, providerUser);
        }

        private static string GetAppSpecificUserId(ApiController controller)
        {
            string legacyUserId = null;
            IEnumerable<string> legacyUserIdHeaderValues;
            if (controller.Request.Headers.TryGetValues(LegacyUserId.HeaderKey, out legacyUserIdHeaderValues))
            {
                var legacyUserIdHeaderValue = legacyUserIdHeaderValues.SingleOrDefault();

                legacyUserId = $"MicrosoftAccount:{legacyUserIdHeaderValue}";
            }
            return legacyUserId;
        }

        private static async Task<ProviderUserInfo> GetProviderUserInfoAsync(ApiController controller)
        {
            ClaimsPrincipal principal = (ClaimsPrincipal)controller.User;
            string provider = principal.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider").Value;

            ProviderCredentials creds;

            if (string.Equals(provider, "microsoftaccount", StringComparison.OrdinalIgnoreCase))
            {
                creds = await controller.User.GetAppServiceIdentityAsync<MicrosoftAccountCredentials>(controller.Request);
            }
            else if (string.Equals(provider, "facebook", StringComparison.OrdinalIgnoreCase))
            {
                creds = await controller.User.GetAppServiceIdentityAsync<FacebookCredentials>(controller.Request);
            }
            else if (string.Equals(provider, "google", StringComparison.OrdinalIgnoreCase))
            {
                creds = await controller.User.GetAppServiceIdentityAsync<GoogleCredentials>(controller.Request);
            }
            else if (string.Equals(provider, "twitter", StringComparison.OrdinalIgnoreCase))
            {
                creds = await controller.User.GetAppServiceIdentityAsync<TwitterCredentials>(controller.Request);
            }
            else
            {
                throw new NotImplementedException();
            }

            return new ProviderUserInfo(creds.Provider,
                                        creds.UserClaims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value,
                                        creds.UserClaims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value);
        }
    }

    public enum IdentityProvider
    {
        MicrosoftAccount,
        Google,
        Facebook,
        Twitter
    }

    public class ProviderUserInfo
    {
        public ProviderUserInfo(string provider, string userId, string emailAddress)
        {
            Provider = (IdentityProvider)Enum.Parse(typeof(IdentityProvider), provider);
            UserId = userId;
            EmailAddress = emailAddress;
        }

        public IdentityProvider Provider { get; }
        public string UserId { get; }
        public string EmailAddress { get; }

        public string ProviderPrefixedUserId
        {
            get
            {
                string userId = $"{Provider}:{UserId}";

                return userId;
            }
        }

        public override string ToString()
        {
            return ProviderPrefixedUserId;
        }
    }

    public class LinquaUserInfo
    {
        public LinquaUserInfo(string appSpecificMicrosoftUserId, ProviderUserInfo providerUserInfo)
        {
            ProviderUserInfo = providerUserInfo;

            switch (providerUserInfo.Provider)
            {
                case IdentityProvider.MicrosoftAccount:
                    MicrosoftAccount = providerUserInfo;
                    break;
                case IdentityProvider.Google:
                    Google = providerUserInfo;
                    break;
                case IdentityProvider.Facebook:
                    Facebook = providerUserInfo;
                    break;
                case IdentityProvider.Twitter:
                    Twitter = providerUserInfo;
                    break;
                default:
                    throw new NotImplementedException();
            }

            AppSpecificMicrosoftUserId = appSpecificMicrosoftUserId;
        }

        public ProviderUserInfo ProviderUserInfo { get; private set; }

        public ProviderUserInfo MicrosoftAccount { get; private set; }
        public ProviderUserInfo Facebook { get; private set; }
        public ProviderUserInfo Google { get; private set; }
        public ProviderUserInfo Twitter { get; private set; }

        public string AppSpecificMicrosoftUserId { get; private set; }
    }
}