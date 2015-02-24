using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using MetroLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Linqua.Translation.Microsoft
{
	public class AdmAuthentication : IDisposable
	{
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(AdmAuthentication).Name);

		private AdmAccessToken token;
		private readonly Timer accessTokenRenewer;

		//Access token expires every 10 minutes. Renew it every 9 minutes only.
		private const int RefreshTokenDuration = 9;

		private AdmAuthentication()
		{
			accessTokenRenewer = new Timer(OnTokenExpiredCallback, this, TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
		}

		public static async Task<AdmAuthentication> CreateAndAuthenticateAsync()
		{
			var result = new AdmAuthentication();

			await result.RenewAccessTokenAsync();

			return result;
		}

		public AdmAccessToken GetAccessToken()
		{
			return token;
		}

		private async Task RenewAccessTokenAsync()
		{
			AdmAccessToken newAccessToken = await GetTokenAsync();

			//swap the new token with old one
			//Note: the swap is thread unsafe
			token = newAccessToken;
		}

		private async void OnTokenExpiredCallback(object stateInfo)
		{
			try
			{
				await RenewAccessTokenAsync();
			}
			catch (Exception ex)
			{
				Log.Error("Failed to renew microsoft translator access token. Will try again later.", ex);
			}
			finally
			{
				accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
			}
		}

		//private AdmAccessToken HttpPost(string datamarketAccessUri, string requestDetails)
		//{
		//	HttpClient client = new HttpClient();


		//	using (var httpClient = new HttpClient())
		//	{
		//		var request = new HttpRequestMessage(HttpMethod.Post, datamarketAccessUri);
		//		request.Content = new StringContent(Serialize(obj), Encoding.UTF8, "text/xml");
		//		var response = await httpClient.SendAsync(request);
		//		return await response.Content.ReadAsStringAsync();
		//	}


		//	//Prepare OAuth request 
		//	WebRequest webRequest = WebRequest.Create(datamarketAccessUri);
		//	webRequest.ContentType = "application/x-www-form-urlencoded";
		//	webRequest.Method = "POST";
		//	byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
		//	webRequest.ContentLength = bytes.Length;
		//	using (Stream outputStream = webRequest.GetRequestStream())
		//	{
		//		outputStream.Write(bytes, 0, bytes.Length);
		//	}
		//	using (WebResponse webResponse = webRequest.GetResponse())
		//	{
		//		DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
		//		//Get deserialized object from JSON stream
		//		AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
		//		return token;
		//	}
		//}

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

			// If client authentication failed then we get a JSON response from Azure Market Place
			string responseString;

			if (!dataMarketResponse.IsSuccessStatusCode)
			{
				responseString = await dataMarketResponse.Content.ReadAsStringAsync();
				var responseJson = JToken.Parse(responseString);

				string errorType = responseJson.Value<string>("error");
				string errorDescription = responseJson.Value<string>("error_description");
				throw new HttpRequestException(string.Format("Azure market place request failed: {0} {1}", errorType, errorDescription));
			}

			responseString = await dataMarketResponse.Content.ReadAsStringAsync();

			var result = JsonConvert.DeserializeObject<AdmAccessToken>(responseString);

			//var responseStream = await dataMarketResponse.Content.ReadAsStreamAsync();
			//var serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
			//var result = (AdmAccessToken)serializer.ReadObject(responseString);

			return result;
		}

		#region Dispose Pattern

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				accessTokenRenewer.Dispose();
			}
		}

		~AdmAuthentication()
		{
			Dispose(false);
		}

		#endregion

	}
}