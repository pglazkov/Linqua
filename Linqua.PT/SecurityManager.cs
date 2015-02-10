using System;
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
		private const string AuthenticationRedirectUrl = "https://linqua.azure-mobile.net/";

		private static readonly string[] AuthenticationScopes = new[] { "wl.basic", "wl.signin", "wl.offline_access" };

		public static async Task<bool> TryAuthenticateSilently()
		{
			MobileServiceUser user = null;

			var vault = new PasswordVault();

			PasswordCredential savedCredentials = null;

			try
			{
				savedCredentials = vault.FindAllByResource(ProviderId).FirstOrDefault();
			}
			catch (Exception)
			{
				// No credentials found.
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

		public static async Task<bool> Authenticate()
		{
			var authenticated = await TryAuthenticateSilently();

			if (authenticated)
			{
				return true;
			}

			LiveAuthClient liveIdClient = new LiveAuthClient(AuthenticationRedirectUrl);

			var result = await liveIdClient.LoginAsync(AuthenticationScopes);

			if (result.Status == LiveConnectSessionStatus.Connected)
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