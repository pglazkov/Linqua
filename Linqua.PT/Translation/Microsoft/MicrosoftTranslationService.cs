using System;
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

		private readonly IMicrosoftAccessTokenProvider accessTokenProvider;

		[ImportingConstructor]
		public MicrosoftTranslationService([NotNull] IMicrosoftAccessTokenProvider accessTokenProvider)
		{
			Guard.NotNull(accessTokenProvider, () => accessTokenProvider);

			this.accessTokenProvider = accessTokenProvider;
		}

		public async Task<string> DetectLanguageAsync(string text)
		{
			Guard.NotNullOrEmpty(text, () => text);

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
			Guard.NotNullOrEmpty(text, () => text);
			Guard.NotNullOrEmpty(fromLanguageCode, () => fromLanguageCode);
			Guard.NotNullOrEmpty(toLanguageCode, () => toLanguageCode);

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

		private async Task ConfigureRequestAuthentication(HttpClient httpClient)
		{
			var accessToken = await accessTokenProvider.GetAccessTokenAsync();
			httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
		}
	}
}