using System;
using System.Collections.Generic;
using System.Composition;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Linqua.Translation.Microsoft
{
	[Export(typeof(ITranslationService))]
	public class MicrosoftTranslationService : ITranslationService
	{
		private const string DetectLanguageUriTemplate = "http://api.microsofttranslator.com/v2/Http.svc/Detect?text={0}";
		private const string TranslateUriTemplate = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text={0}&from={1}&to={2}";
	    private const string GetLanguagesForTranslateUri = "http://api.microsofttranslator.com/V2/Http.svc/GetLanguagesForTranslate";

        private readonly IMicrosoftAccessTokenProvider accessTokenProvider;

		[ImportingConstructor]
		public MicrosoftTranslationService([NotNull] IMicrosoftAccessTokenProvider accessTokenProvider)
		{
			Guard.NotNull(accessTokenProvider, nameof(accessTokenProvider));

			this.accessTokenProvider = accessTokenProvider;
		}

		public async Task<string> DetectLanguageAsync(string text)
		{
			Guard.NotNullOrEmpty(text, nameof(text));

			var uri = string.Format(DetectLanguageUriTemplate, text);

			using (var httpClient = new HttpClient())
			{
				await ConfigureRequestAuthentication(httpClient);

				var response = await httpClient.GetAsync(uri);

				await MicrosoftApiHelper.ThrowIfErrorResponse(response);

				var responseStream = await response.Content.ReadAsStreamAsync();

				DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
				string languageDetected = (string)dcs.ReadObject(responseStream);

				return languageDetected;
			}
		}

		public async Task<string> TranslateAsync(string text, string fromLanguageCode, string toLanguageCode)
		{
			Guard.NotNullOrEmpty(text, nameof(text));
			Guard.NotNullOrEmpty(fromLanguageCode, nameof(fromLanguageCode));
			Guard.NotNullOrEmpty(toLanguageCode, nameof(toLanguageCode));
		    
			string uri = string.Format(TranslateUriTemplate, Uri.EscapeUriString(text), fromLanguageCode, toLanguageCode);

			using (var httpClient = new HttpClient())
			{
				await ConfigureRequestAuthentication(httpClient);
				var response = await httpClient.GetAsync(uri);

				await MicrosoftApiHelper.ThrowIfErrorResponse(response);

				var responseStream = await response.Content.ReadAsStreamAsync();

				DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
				string translation = (string)dcs.ReadObject(responseStream);

				return translation;
			}
		}

	    public async Task<IEnumerable<string>> GetSupportedLanguageCodesAsync()
	    {
            string uri = GetLanguagesForTranslateUri;

            using (var httpClient = new HttpClient())
            {
                await ConfigureRequestAuthentication(httpClient);
                var response = await httpClient.GetAsync(uri);

                await MicrosoftApiHelper.ThrowIfErrorResponse(response);

                var responseStream = await response.Content.ReadAsStreamAsync();

                DataContractSerializer dcs = new DataContractSerializer(typeof(List<string>));

                var languagesForTranslate = (List<string>)dcs.ReadObject(responseStream);

                return languagesForTranslate;
            }
        }

	    private async Task ConfigureRequestAuthentication(HttpClient httpClient)
		{
			var accessToken = await accessTokenProvider.GetAccessTokenAsync();
			httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
		}
	}
}