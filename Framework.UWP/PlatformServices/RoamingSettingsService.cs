using System.Collections.Generic;
using System.Composition;
using Windows.Storage;

namespace Framework.PlatformServices
{
	[Export(typeof(IRoamingSettingsService))]
	[Shared]
	public class RoamingSettingsService : IRoamingSettingsService
	{
		public IDictionary<string, object> Values
		{
			get { return ApplicationData.Current.RoamingSettings.Values; }
		}
	}
}