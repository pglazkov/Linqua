namespace Framework.PlatformServices
{
	public static class LocalSettingsServiceExtensions
	{
		public static void SetValue(this ILocalSettingsService service, string key, object value)
		{
			service.Values[key] = value;
		}

		public static object GetValue(this ILocalSettingsService service, string key)
		{
			object result;

			service.Values.TryGetValue(key, out result);

			return result;
		}
	}
}