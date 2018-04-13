using System;
using System.Collections.Generic;
using System.Linq;
using VDS.Data.Repositories;
using VDS.Data.Uow;
using VDS.Helpers;
using VDS.Helpers.Exception;
using VDS.Helpers.Extensions;
using VDS.Reflection.Extensions;

namespace VDS.Settings.Providers
{
    public class RepositorySettingsProvider : IWritableSettingsProvider
    {
        private readonly IRepository<Setting, Guid> _settingRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public RepositorySettingsProvider(IRepository<Setting, Guid> settingRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            Throw.IfArgumentNull(settingRepository, nameof(settingRepository));
            _settingRepository = settingRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public bool TryGetSetting<T>(SettingEntryKey key, out T value)
        {
            value = default(T);
            key.Category = key.Category.ToLower();
            key.Name = key.Name.ToLower();
            var settingEntry = _settingRepository.FirstOrDefault(s => s.Category.ToLower() == key.Category && s.Name.ToLower() == key.Name);
            if (settingEntry == null) return false;
            var deserializedValue = BinarySerializationHelper.Deserialize(Convert.FromBase64String(settingEntry.Value));
            if (deserializedValue == null)
            {
                Throw.IfNot(typeof(T).AllowsNullValue())
                     .A<InvalidOperationException>("The setting entry is null but output type is not allowed null.");
            }
            else
            {
                Throw.IfNot(deserializedValue is T)
                     .A<InvalidOperationException>("The setting entry is of type {0} but output type is {1}.".FormatWith(deserializedValue.GetType(), typeof(T)));
            }
            value = (T)deserializedValue;
            return true;
        }

        public IEnumerable<SettingEntryInfo> GetAll()
        {
            return _settingRepository.GetAll().Select(a => new SettingEntryInfo
            {
                Key = new SettingEntryKey { Name = a.Name, Category = a.Category},
                Value = a.Value,
                ProviderName = ProviderName
            });
        }

        public bool TrySetSetting<T>(SettingEntryKey key, T value)
        {
            key.Category = key.Category.ToLower();
            key.Name = key.Name.ToLower();
            var settingEntry = _settingRepository.FirstOrDefault(s => s.Category.ToLower() == key.Category && s.Name.ToLower() == key.Name);
            if (settingEntry == null) return false;
            settingEntry.Value = Convert.ToBase64String(BinarySerializationHelper.Serialize(value));

            using (var uow = _unitOfWorkManager.Begin())
            {
                _settingRepository.Update(settingEntry);
                uow.Complete();
            }
            return true;
        }

        public void DefineSetting(SettingEntryKey key, object initialValue)
        {
            var setting = new Setting
            {
                Category = key.Category,
                Name = key.Name,
                Value = Convert.ToBase64String(BinarySerializationHelper.Serialize(initialValue))
            };
            using (var uow = _unitOfWorkManager.Begin())
            {
                _settingRepository.Insert(setting);
                uow.Complete();
            }
        }

//        public void DeleteSettings(string moduleName)
//        {
//            moduleName = moduleName.ToLower();
//            var settingEntriesToDelete = _settingRepository.Fetch(s => s.ModuleName.ToLower() == moduleName);
//            settingEntriesToDelete.ForEach(entry => _settingRepository.Settings.Remove(entry));
//            _settingRepository.SaveChanges();
//        }

        public bool SettingExists(SettingEntryKey key)
        {
            key.Category = key.Category.ToLower();
            key.Name = key.Name.ToLower();
            return _settingRepository.Count(s => s.Category.ToLower() == key.Category && s.Name.ToLower() == key.Name) > 0;
        }

        public string ProviderName => "RepositorySettingsProvider";
    }
}