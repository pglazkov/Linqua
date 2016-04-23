using System.Collections.Generic;

namespace Framework.PlatformServices
{
    public interface ISettingsService
    {
        IDictionary<string, object> Values { get; }
    }
}