using System;
using System.Collections.Generic;
using System.Composition;
using System.Net.Http;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using MetroLog;
using Newtonsoft.Json;

namespace Linqua.Translation.Microsoft
{
	[Export(typeof(IMicrosoftAccessTokenProvider))]
	public class MicrosoftAccessTokenProvider : IMicrosoftAccessTokenProvider
	{
		private const string AccessTokenKey = "MicrosoftAccessToken";
		private const string AccessTokenExpirationTimeKey = "MicrosoftAccessTokenExpirationTime";

		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(MicrosoftAccessTokenProvider).Name);

		private readonly ILocalSettingsService localSettingsService;

		[ImportingConstructor]
		public MicrosoftAccessTokenProvider([NotNull] ILocalSettingsService localSettingsService)
		{
			Guard.NotNull(localSettingsService, () => localSettingsService);

			this.localSettingsService = localSettingsService;
		}

		public async Task<string> GetAccessTokenAsync()
		{
			var tokenExpirationTimeValue = localSettingsService.GetValue(AccessTokenExpirationTimeKey) as string;

			var tokenExpirationTime = DateTime.Parse(tokenExpirationTimeValue ?? DateTime.UtcNow.AddMinutes(-1).ToString());

			string token;

			if (tokenExpirationTime < DateTime.UtcNow.AddMinutes(1))
			{
				token = await RenewAccessTokenAsync();
			}
			else
			{
				token = localSettingsService.GetValue(AccessTokenKey) as string;
			}

			Guard.Assert(token != null, "token != null");
			
			return token;
		}

		private async Task<string> RenewAccessTokenAsync()
		{
			AdmAccessToken newAccessToken = await GetTokenAsync();

			localSettingsService.SetValue(AccessTokenExpirationTimeKey, DateTime.UtcNow.AddSeconds(int.Parse(newAccessToken.expires_in)).ToString());

			localSettingsService.SetValue(AccessTokenKey, newAccessToken.access_token);

			return newAccessToken.access_token;
		}

		private async Task<AdmAccessToken> GetTokenAsync()
		{
			var requestDetails = new Dictionary<string, string>
			{
				{ "grant_type", "client_credentials" },
				{ "client_id",   TranslatorServiceConsts.ClientId},
				{ "client_secret",  TranslatorServiceConsts.ClientSecret },
				{ "scope", TranslatorServiceConsts.AuthenticationScope }
			};

			var requestContent = new FormUrlEncodedContent(requestDetails);

			var client = new HttpClient();

			var dataMarketResponse = await client.PostAsync(TranslatorServiceConsts.AuthenticationUri, requestContent);

			await MicrosoftApiHelper.ThrowIfErrorResponse(dataMarketResponse);

			var responseString = await dataMarketResponse.Content.ReadAsStringAsync();

			var result = JsonConvert.DeserializeObject<AdmAccessToken>(responseString);

			return result;
		}
	}
}