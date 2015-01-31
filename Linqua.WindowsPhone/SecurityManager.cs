﻿using System;
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
		private static LiveConnectSession session;

		public static async Task Authenticate()
		{
			var vault = new PasswordVault();

			PasswordCredential savedCredentials = null;

			const string ProviderId = "MicrosoftLive";

			try
			{
				savedCredentials = vault.FindAllByResource(ProviderId).FirstOrDefault();
			}
			catch (Exception)
			{
				// No credentials found.
			}

			MobileServiceUser user = null;

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
				LiveAuthClient liveIdClient = new LiveAuthClient("https://linqua.azure-mobile.net/");

				while (session == null)
				{
					// Force a logout to make it easier to test with multiple Microsoft Accounts
					if (liveIdClient.CanLogout)
						liveIdClient.Logout();

					LiveLoginResult result = await liveIdClient.LoginAsync(new[] { "wl.basic", "wl.signin" });
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