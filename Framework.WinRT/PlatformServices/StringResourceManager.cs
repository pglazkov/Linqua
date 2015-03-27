using System.Composition;
using Windows.ApplicationModel.Resources;

namespace Framework.PlatformServices
{
	[Export(typeof(IStringResourceManager))]
	public class StringResourceManager : IStringResourceManager
	{
		public string GetString(string key, string defaultValue)
		{
			var result = ResourceLoader.GetForCurrentView().GetString(key);

			if (string.IsNullOrEmpty(result))
			{
				result = defaultValue;
			}

			return result;
		}
	}
}