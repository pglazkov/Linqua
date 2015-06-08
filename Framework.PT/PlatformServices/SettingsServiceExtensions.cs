using System;

namespace Framework.PlatformServices
{
	public static class SettingsServiceExtensions
	{
		public static void SetValue(this ISettingsService service, string key, object value)
		{
			service.Values[key] = value;
		}

		public static object GetValue(this ISettingsService service, string key)
		{
			object result;

			service.Values.TryGetValue(key, out result);

			return result;
		}

		public static T GetValue<T>(this ISettingsService service, string key, T defaultValue = default(T))
		{
			object rawValue;

			service.Values.TryGetValue(key, out rawValue);

			if (ReferenceEquals(rawValue, null)) 
				return defaultValue;

			try
			{
				var result = (T)Convert.ChangeType(rawValue, typeof(T));

				return result;
			}
			catch (Exception)
			{
				if (typeof(T) == typeof(bool) && rawValue is string)
				{
					bool boolValue;

					if (bool.TryParse(rawValue as string, out boolValue))
					{
						SetValue(service, key, boolValue);

						return (T)(object)boolValue;
					}
				}

				throw;
			}

			
		}
	}
}