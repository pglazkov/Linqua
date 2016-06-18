using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Authentication.OnlineId;
using Windows.Security.Credentials;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;

namespace Linqua
{
    public static class SecurityManager
    {
        private const string ProviderId = "MicrosoftAccount";

        public static async Task<bool> TryAuthenticateSilently(bool useCachedCredentials = true)
        {
            MobileServiceUser user = null;

            var vault = new PasswordVault();

            PasswordCredential savedCredentials = null;

            if (useCachedCredentials)
            {
                try
                {
                    savedCredentials = vault.FindAllByResource(ProviderId).FirstOrDefault();
                }
                catch (Exception)
                {
                    // No credentials found.
                }
            }

            if (savedCredentials != null)
            {
                user = new MobileServiceUser(savedCredentials.UserName)
                {
                    MobileServiceAuthenticationToken = vault.Retrieve(ProviderId, savedCredentials.UserName).Password
                };

                MobileService.Client.CurrentUser = user;
            }

            if (user == null)
            {
                try
                {
                    user = await DoLoginAsync(CredentialPromptType.DoNotPrompt);
                }
                catch (Exception)
                {
                    // Do nothing
                }

                if (user != null)
                {
                    vault.Add(new PasswordCredential(ProviderId, user.UserId, user.MobileServiceAuthenticationToken));
                }
            }

            return user != null;
        }

        public static async Task<bool> Authenticate(bool useCachedCredentials = true)
        {
            var authenticated = await TryAuthenticateSilently(useCachedCredentials);

            if (authenticated)
            {
                return true;
            }

            MobileServiceUser user = null;

            try
            {
                user = await DoLoginAsync(CredentialPromptType.PromptIfNeeded);
            }
            catch (Exception ex)
            {
                // We have to handle this because an exception can occur if user declines to login with Microsoft account. 
                // However this error can occur for many other reasons, including bugs in out code.
#if DEBUG
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
#endif

                ExceptionHandlingHelper.HandleNonFatalError(ex, "Authentication error");
            }

            if (user != null)
            {
                var vault = new PasswordVault();
                vault.Add(new PasswordCredential(ProviderId, user.UserId, user.MobileServiceAuthenticationToken));

                return true;
            }

            return false;
        }

        private static async Task<MobileServiceUser> DoLoginAsync(CredentialPromptType promptType)
        {
            MobileServiceUser user = null;
            var authenticator = new OnlineIdAuthenticator();
            var mobileServicesTicket = new OnlineIdServiceTicketRequest("wl.signin", "DELEGATION");

            var ticketRequests = new List<OnlineIdServiceTicketRequest> {mobileServicesTicket};

            var authResult = await authenticator.AuthenticateUserAsync(ticketRequests, promptType);

            if ((authResult.Tickets.Count == 1) && (authResult.Tickets[0].ErrorCode == 0))
            {
                var accessToken = authResult.Tickets[0];

                var payload = new JObject
                {
                    ["access_token"] = accessToken.Value
                };

                user = await MobileService.Client.LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount, payload);

                LegacyUserId.Value = authResult.SafeCustomerId;
            }

            return user;
        }
    }
}