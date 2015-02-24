using System;
using System.Composition;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Framework;
using Newtonsoft.Json.Linq;

namespace Linqua.Translation.Microsoft
{
	[Export(typeof(ITranslationService))]
	public class MicrosoftTranslationService : ITranslationService
	{
		private AdmAuthentication authentication;

		public async Task<string> DetectLanguageAsync(string text)
		{
			Guard.NotNullOrEmpty(text, () => text);

			var accessToken = await GetAccessTokenAsync();

			var uri = "http://api.microsofttranslator.com/v2/Http.svc/Detect?text=" + text;

			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
				var response = await httpClient.GetAsync(uri);

				await ThrowIfErrorResponse(response);

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

			var accessToken = await GetAccessTokenAsync();

			string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + Uri.EscapeUriString(text) + "&from=" + fromLanguageCode + "&to=" + toLanguageCode;

			using (var httpClient = new HttpClient())
			{
				httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
				var response = await httpClient.GetAsync(uri);

				await ThrowIfErrorResponse(response);

				var responseStream = await response.Content.ReadAsStreamAsync();

				DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String"));
				string translation = (string)dcs.ReadObject(responseStream);

				return translation;
			}
		}

		private static async Task ThrowIfErrorResponse(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				string responseString = await response.Content.ReadAsStringAsync();
				var responseJson = JToken.Parse(responseString);

				string errorType = responseJson.Value<string>("error");
				string errorDescription = responseJson.Value<string>("error_description");
				throw new HttpRequestException(string.Format("Azure market place request failed: {0} {1}", errorType, errorDescription));
			}
		}

		private async Task<string> GetAccessTokenAsync()
		{
			if (authentication == null)
			{
				authentication = await AdmAuthentication.CreateAndAuthenticateAsync();
			}

			return authentication.GetAccessToken().access_token;
		}
	}
}