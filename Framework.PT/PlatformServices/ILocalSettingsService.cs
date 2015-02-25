using System.Collections.Generic;

namespace Framework.PlatformServices
{
	public interface ILocalSettingsService
	{
		IDictionary<string, object> Values { get; }
	}
}