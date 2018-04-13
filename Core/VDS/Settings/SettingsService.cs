using System.Collections.Generic;
using System.Linq;
using VDS.Helpers.Exception;
using VDS.Helpers.Extensions;

namespace VDS.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly IEnumerable<ISettingsProvider> _settingsProviders;
        
        public SettingsService(IEnumerable<ISettingsProvider> settingsProviders)
        {
            Throw.IfArgumentNullOrEmpty(settingsProviders, nameof(settingsProviders));
            _settingsProviders = settingsProviders;
        }

        public virtual bool SettingExists(SettingEntryKey key)
        {
            return _settingsProviders.Any(p => p.SettingExists(key));
        }

        public virtual bool TryGetSetting<T>(SettingEntryKey key, out T value)
        {
            Throw.IfArgumentNull(key, nameof(key));
            var provider = _settingsProviders.FirstOrDefault(p => p.SettingExists(key));
            value = default(T);
            return provider != null && provider.TryGetSetting(key, out value);
        }

        public IEnumerable<SettingEntryInfo> GetAll()
        {
            var results = new List<SettingEntryInfo>();
            _settingsProviders.ForEach(a => results.AddRange(a.GetAll()));
            return results;
        }

        public virtual bool TrySetSetting<T>(SettingEntryKey key, T value)
        {
            Throw.IfArgumentNull(key, nameof(key));
            var provider = (IWritableSettingsProvider)_settingsProviders.FirstOrDefault(p => p.SettingExists(key) && p is IWritableSettingsProvider);
            return provider != null && provider.TrySetSetting(key, value);
        }

        public T GetSetting<T>(SettingEntryKey key)
        {
            Throw.IfArgumentNull(key, nameof(key));
            Throw.IfNot(TryGetSetting(key, out T value))
                .AnArgumentException("No provider has a setting entry named '{0}' in category '{1}'.".FormatWith(key.Name, key.Category), nameof(key));
            return value;
        }

        public void SetSetting<T>(SettingEntryKey key, T value)
        {
            Throw.IfArgumentNull(key, nameof(key));
            Throw.IfNot(TrySetSetting(key, value))
                .AnArgumentException("No writable provider has a setting entry named '{0}' in category '{1}'.".FormatWith(key.Name, key.Category), nameof(key));
        }
    }
}