using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;

namespace Linqua.Translation.Microsoft
{
    [Export(typeof(ITranslationService))]
    public class MicrosoftTranslationService : ITranslationService
    {
        private const string DetectLanguageUriTemplate = "http://api.microsofttranslator.com/v2/Http.svc/Detect?text={0}";
        private const string TranslateUriTemplate = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text={0}&from={1}&to={2}";
        private const string GetLanguagesForTranslateUri = "http://api.microsofttranslator.com/V2/Http.svc/GetLanguagesForTranslate";
        private const string GetLanguageNamesUriTemplate = "http://api.microsofttranslator.com/v2/Http.svc/GetLanguageNames?locale={0}";

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

        public async Task<IDictionary<string, string>> GetLanguageNamesAsync(IEnumerable<string> languageCodes, string locale)
        {
            Guard.NotNull(languageCodes, nameof(languageCodes));
            Guard.NotNullOrEmpty(locale, nameof(locale));

            var languageCodesArray = languageCodes.ToArray();

            string uri = string.Format(GetLanguageNamesUriTemplate, locale);

            using (var contentStream = new MemoryStream())
            {
                DataContractSerializer dcs = new DataContractSerializer(Type.GetType("System.String[]"));
                dcs.WriteObject(contentStream, languageCodesArray);

                contentStream.Position = 0;

                HttpContent postContent = new StreamContent(contentStream);
                postContent.Headers.ContentType = new MediaTypeHeaderValue("text/xml");

                using (var httpClient = new HttpClient())
                {
                    await ConfigureRequestAuthentication(httpClient);
                    var response = await httpClient.PostAsync(uri, postContent);

                    await MicrosoftApiHelper.ThrowIfErrorResponse(response);

                    var responseStream = await response.Content.ReadAsStreamAsync();

                    var languageNames = (string[])dcs.ReadObject(responseStream);

                    var result = new Dictionary<string, string>();

                    for (var i = 0; i < languageNames.Length; i++)
                    {
                        var code = languageCodesArray[i];
                        var name = languageNames[i];

                        result.Add(code, name);
                    }

                    return result;
                }
            }
        }

        private async Task ConfigureRequestAuthentication(HttpClient httpClient)
        {
            var accessToken = await accessTokenProvider.GetAccessTokenAsync();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
        }
    }
}