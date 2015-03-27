using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Framework;

namespace Linqua
{
	// Taken from here: http://blogs.msdn.com/b/carlosfigueira/archive/2014/03/13/caching-and-handling-expired-tokens-in-azure-mobile-services-managed-sdk.aspx
	public class AuthFailureHandler : DelegatingHandler
	{
		private const string AuthHeaderName = "X-ZUMO-AUTH";

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			// Cloning the request, in case we need to send it again
			var clonedRequest = await HttpUtils.CloneRequestAsync(request);

			var response = await base.SendAsync(clonedRequest, cancellationToken);

			if (response.StatusCode != HttpStatusCode.Unauthorized) 
				return response;

			try
			{
				bool authenticationSuccessful = await SecurityManager.Authenticate(useCachedCredentials: false);

				if (authenticationSuccessful)
				{
					clonedRequest = await HttpUtils.CloneRequestAsync(request);

					clonedRequest.Headers.Remove(AuthHeaderName);

					var user = MobileService.Client.CurrentUser;

					if (user != null)
					{
						clonedRequest.Headers.Add(AuthHeaderName, user.MobileServiceAuthenticationToken);
							
						response = await base.SendAsync(clonedRequest, cancellationToken);
					}
				}
			}
			catch (Exception)
			{
				return response;
			}

			return response;
		}

		
	}
}