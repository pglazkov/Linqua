using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Security.Credentials;
using Linqua.DataObjects;
using Microsoft.Live;

namespace Linqua
{
	public static class SecurityManager
	{
		private const string ProviderId = "MicrosoftLive";
		private const string AuthenticationRedirectUrl = "https://linqua.azure-mobile.net/";

		private static LiveConnectSession session;

		private static readonly string[] AuthenticationScopes = new[] { "wl.basic", "wl.signin", "wl.offline_access" };

		public static async Task Authenticate()
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

				try
				{
					await TestRetrievingData();
				}
				catch (MobileServiceInvalidOperationException ex)
				{
					if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
					{
						// Remove the credential with the expired token.
						vault.Remove(vault.Retrieve(ProviderId, user.UserId));
						user = null;
					}
				}
			}

			if (user == null)
			{
				LiveAuthClient liveIdClient = new LiveAuthClient(AuthenticationRedirectUrl);

				while (session == null)
				{
					var result = await liveIdClient.LoginAsync(AuthenticationScopes);
					if (result.Status == LiveConnectSessionStatus.Connected)
					{
						session = result.Session;
						user = await MobileService.Client.LoginWithMicrosoftAccountAsync(result.Session.AuthenticationToken);

						vault.Add(new PasswordCredential(ProviderId, user.UserId, user.MobileServiceAuthenticationToken));
					}
					else
					{
						session = null;
						var dialog = new MessageDialog("You must log in.", "Login Required");
						dialog.Commands.Add(new UICommand("OK"));
						await dialog.ShowAsync();
					}
				}
			}
		}

		private static async Task TestRetrievingData()
		{
			await MobileService.Client.GetTable<ClientEntry>().Take(1).ToListAsync();
		}
	}
}