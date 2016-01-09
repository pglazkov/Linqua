﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Authentication.OnlineId;
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

				//LiveAuthClient liveIdClient = new LiveAuthClient(AuthenticationRedirectUrl);

				//LiveLoginResult result = null;
				//try
				//{
				//	result = await liveIdClient.InitializeAsync(AuthenticationScopes);
				//}
				//catch (LiveAuthException ex)
				//{
				//	ExceptionHandlingHelper.HandleNonFatalError(ex, "Authentication error");
				//}

				//if (result != null && result.Status == LiveConnectSessionStatus.Connected)
				//{
				//	user = await MobileService.Client.LoginWithMicrosoftAccountAsync(result.Session.AuthenticationToken);

				//	vault.Add(new PasswordCredential(ProviderId, user.UserId, user.MobileServiceAuthenticationToken));
				//}
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

			MobileServiceUser user = null;

			try
			{
				user = await DoLoginAsync(CredentialPromptType.PromptIfNeeded);
			}
			catch (LiveAuthException ex)
			{
				ExceptionHandlingHelper.HandleNonFatalError(ex, "Authentication error");
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
			var mobileServicesTicket = new OnlineIdServiceTicketRequest(AuthenticationRedirectUrl, "JWT" /*"DELEGATION"*/);

			var ticketRequests = new List<OnlineIdServiceTicketRequest> { mobileServicesTicket };

			var authResult = await authenticator.AuthenticateUserAsync(ticketRequests, promptType);

			if ((authResult.Tickets.Count == 1) && (authResult.Tickets[0].ErrorCode == 0))
			{
				var accessToken = authResult.Tickets[0];

				user = await MobileService.Client.LoginWithMicrosoftAccountAsync(accessToken.Value);
			}

			return user;
		}
	}
}