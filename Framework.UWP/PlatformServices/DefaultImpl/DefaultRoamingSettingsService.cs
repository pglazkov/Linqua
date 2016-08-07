using System.Collections.Generic;
using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
    [DefaultExport(typeof(IRoamingSettingsService))]
    public class DefaultRoamingSettingsService : IRoamingSettingsService
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        #region Implementation of ISettingsService

        public int Count
        {
            get { return values.Count; }
        }

        public bool TryGetValue(string key, out object value)
        {
            return values.TryGetValue(key, out value);
        }

        public void SetValue(string key, object value)
        {
            values[key] = value;
        }

        public bool RemoveValue(string key)
        {
            return values.Remove(key);
        }

        public void Clear()
        {
            values.Clear();
        }

        #endregion
    }
}