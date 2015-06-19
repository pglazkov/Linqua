using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Linqua.Translation.Microsoft
{
	public static class MicrosoftApiHelper
	{
		public static async Task ThrowIfErrorResponse(HttpResponseMessage response)
		{
			if (!response.IsSuccessStatusCode)
			{
				string responseString = await response.Content.ReadAsStringAsync();

				try
				{
					var responseJson = JToken.Parse(responseString);

					string errorType = responseJson.Value<string>("error");
					string errorDescription = responseJson.Value<string>("error_description");
					throw new MicrosoftApiException(string.Format("Azure market place request failed: {0} {1}", errorType, errorDescription));
				}
				catch (Exception)
				{
					// Could not parse the response
					throw new MicrosoftApiException(string.Format("Azure market place request failed. Response was:\n{0}", responseString));
				}
			}
		}
	}
}