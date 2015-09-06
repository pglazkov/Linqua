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
					throw new MicrosoftApiException($"Azure market place request failed: {errorType} {errorDescription}");
				}
				catch (Exception)
				{
					// Could not parse the response
					throw new MicrosoftApiException($"Azure market place request failed. Response was:\n{responseString}");
				}
			}
		}
	}
}