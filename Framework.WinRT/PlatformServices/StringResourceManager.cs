using System.Composition;
using Windows.ApplicationModel.Resources;

namespace Framework.PlatformServices
{
	[Export(typeof(IStringResourceManager))]
	public class StringResourceManager : IStringResourceManager
	{
		public string GetString(string key)
		{
			return ResourceLoader.GetForCurrentView().GetString(key);
		}
	}
}