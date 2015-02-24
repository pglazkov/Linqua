using System;

namespace Linqua.Translation.Microsoft
{
	public static class TranslatorServiceConsts
	{
		public const string ClientId = "linqua";
		public const string ClientSecret = "lwze6utWbFKFYGXx3CYXJfqy2d5LoNeKn40WjvNr6ww=";
		public const string AuthenticationScope = "http://api.microsofttranslator.com";

		public static readonly Uri AuthenticationUri = new Uri("https://datamarket.accesscontrol.windows.net/v2/OAuth2-13");
	}
}