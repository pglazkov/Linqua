using System.Collections.Generic;
using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
    [DefaultExport(typeof(ILocalSettingsService))]
    public class DefaultLocalSettingsService : ILocalSettingsService
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public IDictionary<string, object> Values
        {
            get { return values; }
        }
    }
}