using System.Collections.Generic;

namespace Framework.PlatformServices
{
    public interface ISettingsService
    {
        int Count { get; }

        bool TryGetValue(string key, out object value);

        void SetValue(string key, object value);

        bool RemoveValue(string key);

        void Clear();
    }
}