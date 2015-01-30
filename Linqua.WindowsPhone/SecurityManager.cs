using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Security.Credentials;
using Linqua.DataObjects;

namespace Linqua
{
	public static class SecurityManager
	{
		private static MobileServiceUser user;

		public static async Task Authenticate()
		{
			const MobileServiceAuthenticationProvider Provider = MobileServiceAuthenticationProvider.MicrosoftAccount;

			var vault = new PasswordVault();

			PasswordCredential savedCredentials = null;

			try
			{
				savedCredentials = vault.FindAllByResource(Provider.ToString()).FirstOrDefault();
			}
			catch (Exception)
			{
				// No credentials found.
			}

			if (savedCredentials != null)
			{
				user = new MobileServiceUser(savedCredentials.UserName)
				{
					MobileServiceAuthenticationToken = vault.Retrieve(Provider.ToString(), savedCredentials.UserName).Password
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
						vault.Remove(vault.Retrieve(Provider.ToString(), user.UserId));
						user = null;
					}
				}
			}
			else
			{
				while (user == null)
				{
					string message;
					try
					{
						user = await MobileService.Client.LoginAsync(Provider);

						message = string.Format("You are now logged in.");

						vault.Add(new PasswordCredential(Provider.ToString(), user.UserId, user.MobileServiceAuthenticationToken));
					}
					catch (InvalidOperationException)
					{
						message = "You must log in. Login Required";
					}

					var messageBox = new MessageDialog(message);

					messageBox.ShowAsync();
				}
			}
		}

		private static async Task TestRetrievingData()
		{
			await MobileService.Client.GetTable<ClientEntry>().Take(1).ToListAsync();
		}
	}
}