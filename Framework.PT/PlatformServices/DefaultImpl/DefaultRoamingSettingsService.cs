using System.Collections.Generic;
using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
	[DefaultExport(typeof(IRoamingSettingsService))]
	public class DefaultRoamingSettingsService : IRoamingSettingsService
	{
		private readonly Dictionary<string, object> values = new Dictionary<string, object>();

		public IDictionary<string, object> Values
		{
			get { return values; }
		}
	}
}