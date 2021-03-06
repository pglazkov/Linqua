﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using MetroLog;
using Newtonsoft.Json;

namespace Linqua.Translation.Microsoft
{
    public class MicrosoftAccessTokenProvider : IMicrosoftAccessTokenProvider
    {
        private const string AccessTokenKey = "MicrosoftAccessToken";

        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(MicrosoftAccessTokenProvider).Name);

        private readonly ILocalSettingsService localSettingsService;

        public MicrosoftAccessTokenProvider([NotNull] ILocalSettingsService localSettingsService)
        {
            Guard.NotNull(localSettingsService, nameof(localSettingsService));

            this.localSettingsService = localSettingsService;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            string token;

            var tokenExpirationTime = GetCurrentTokenExpirationTime();

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

        private DateTime GetCurrentTokenExpirationTime()
        {
            var tokenExpirationTimeValue = localSettingsService.GetValue(LocalSettingsKeys.AccessTokenExpirationTimeKey) as string;

            DateTime tokenExpirationTime;
            if (!DateTime.TryParse(tokenExpirationTimeValue, CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal, out tokenExpirationTime))
            {
                tokenExpirationTime = DateTime.UtcNow.AddMinutes(-1);
            }
            else
            {
                tokenExpirationTime = tokenExpirationTime.ToUniversalTime();
            }

            return tokenExpirationTime;
        }

        private async Task<string> RenewAccessTokenAsync()
        {
            AdmAccessToken newAccessToken = await GetTokenAsync();

            localSettingsService.SetValue(LocalSettingsKeys.AccessTokenExpirationTimeKey, DateTime.UtcNow.AddSeconds(int.Parse(newAccessToken.expires_in)).ToString("O"));

            localSettingsService.SetValue(AccessTokenKey, newAccessToken.access_token);

            return newAccessToken.access_token;
        }

        private async Task<AdmAccessToken> GetTokenAsync()
        {
            var requestDetails = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", TranslatorServiceConsts.ClientId},
                {"client_secret", TranslatorServiceConsts.ClientSecret},
                {"scope", TranslatorServiceConsts.AuthenticationScope}
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