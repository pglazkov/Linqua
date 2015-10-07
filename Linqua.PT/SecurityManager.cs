using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Microsoft.Live;
using Microsoft.WindowsAzure.MobileServices;

namespace Linqua
{
	public static class SecurityManager
	{
		private const string ProviderId = "MicrosoftLive";
		private const string AuthenticationRedirectUrl = MobileService.MobileServiceUrl;

		private static readonly string[] AuthenticationScopes = { "wl.signin" };

		public static async Task<bool> TryAuthenticateSilently(bool useCachedCredentials = true)
		{
            if (MobileService.Client.ApplicationUri.Host == "localhost")
            {
                return true;
            }

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
				LiveAuthClient liveIdClient = new LiveAuthClient(AuthenticationRedirectUrl);

				var result = await liveIdClient.InitializeAsync(AuthenticationScopes);

				if (result.Status == LiveConnectSessionStatus.Connected)
				{
					user = await MobileService.Client.LoginWithMicrosoftAccountAsync(result.Session.AuthenticationToken);

					vault.Add(new PasswordCredential(ProviderId, user.UserId, user.MobileServiceAuthenticationToken));
				}
			}

			return user != null;
		}

		public static async Task<bool> Authenticate(bool useCachedCredentials = true)
		{
            if (MobileService.Client.ApplicationUri.Host == "localhost")
            {
                return true;
            }

            var authenticated = await TryAuthenticateSilently(useCachedCredentials);

			if (authenticated)
			{
				return true;
			}

			LiveAuthClient liveIdClient = new LiveAuthClient(AuthenticationRedirectUrl);

			LiveLoginResult result = null;

		    try
		    {
		        result = await liveIdClient.LoginAsync(AuthenticationScopes);
		    }
		    catch (NullReferenceException ex)
		    {
		        // We have to handle this because this exception occurs if user declines to login with Microsoft account. 
                // However this error can occur for many other reasons, including bugs in out code.
#if DEBUG
                if (Debugger.IsAttached)
		        {
		            Debugger.Break();
		        }
#endif
				ExceptionHandlingHelper.HandleNonFatalError(ex);
		    }

		    if (result != null && result.Status == LiveConnectSessionStatus.Connected)
			{
				var user = await MobileService.Client.LoginWithMicrosoftAccountAsync(result.Session.AuthenticationToken);

				var vault = new PasswordVault();
				vault.Add(new PasswordCredential(ProviderId, user.UserId, user.MobileServiceAuthenticationToken));

				return true;
			}

			return false;
		}
	}
}