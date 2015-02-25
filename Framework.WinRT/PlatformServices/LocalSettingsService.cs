using System.Collections.Generic;
using System.Composition;

namespace Framework.PlatformServices
{
	[Export(typeof(ILocalSettingsService))]
	public class LocalSettingsService : ILocalSettingsService
	{
		public IDictionary<string, object> Values
		{
			get { return Windows.Storage.ApplicationData.Current.LocalSettings.Values; }
		}
	}
}