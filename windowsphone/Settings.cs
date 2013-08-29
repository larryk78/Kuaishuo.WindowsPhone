using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    public class Settings
    {
        IsolatedStorageSettings settings;

        public Settings()
        {
        }

        public bool AddOrUpdateValue(string key, Object value)
        {
            try
            {
                if (settings == null)
                    settings = IsolatedStorageSettings.ApplicationSettings;

                if (!settings.Contains(key))
                {
                    settings.Add(key, value);
                    return true;
                }

                if (settings[key] != value)
                {
                    settings[key] = value;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Settings.AddOrUpdateValue] failed: {0}", ex.Message);
                return false;
            }
        }

        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            try
            {
                if (settings == null)
                    settings = IsolatedStorageSettings.ApplicationSettings;

                return settings.Contains(Key) ? (T)settings[Key] : defaultValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Settings.GetValueOrDefault<T>] failed: {0}", ex.Message);
                return defaultValue;
            }
        }

        public void Save()
        {
            try
            {
                settings.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Settings.Save] failed: {0}", ex.Message);
            }
        }

        const string AudioQualitySettingKeyName_OLD = "AudioQualitySetting";
        const string AudioQualitySettingKeyName_NEW = "AudioQualitySetting2";
        const int AudioQualitySettingDefault_OLD = 0; // 0=false, 1=true
        public bool AudioQualitySetting
        {
            get
            {
                int old = GetValueOrDefault<int>(AudioQualitySettingKeyName_OLD, AudioQualitySettingDefault_OLD);
                return GetValueOrDefault<bool>(AudioQualitySettingKeyName_NEW, (bool)(old == 1));
            }
            set
            {
                if (AddOrUpdateValue(AudioQualitySettingKeyName_NEW, value))
                    Save();
            }
        }

        // not shown in settings page...
        const string NotepadItemsSettingKeyName = "NotepadItemsSetting";
        List<int> NotepadItemsSettingDefault = new List<int>();
        public List<int> NotepadItemsSetting
        {
            get
            {
                return GetValueOrDefault<List<int>>(NotepadItemsSettingKeyName, NotepadItemsSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(NotepadItemsSettingKeyName, value))
                    Save();
            }
        }

        // not shown in settings page...
        const string NotepadCreatedSettingKeyName = "NotepadCreatedSetting";
        const bool NotepadCreatedSettingDefault = false;

        /// <summary>
        /// Indicates whether the default ("notepad") list has been created for the user.
        /// </summary>
        public bool NotepadCreatedSetting
        {
            get
            {
                return GetValueOrDefault<bool>(NotepadCreatedSettingKeyName, NotepadCreatedSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(NotepadCreatedSettingKeyName, value))
                    Save();
            }
        }

        const string PinyinColorSettingKeyName = "PinyinColorSetting";
        const int PinyinColorSettingDefault = (int)PinyinColorScheme.Dummitt;
        public int PinyinColorSetting
        {
            get
            {
                return GetValueOrDefault<int>(PinyinColorSettingKeyName, PinyinColorSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(PinyinColorSettingKeyName, value))
                    Save();
            }
        }

        const string TraditionalChineseSettingKeyName = "TraditionalChineseSetting";
        const bool TraditionalChineseSettingDefault = false;
        public bool TraditionalChineseSetting
        {
            get
            {
                return GetValueOrDefault<bool>(TraditionalChineseSettingKeyName, TraditionalChineseSettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(TraditionalChineseSettingKeyName, value))
                    Save();
            }
        }
    }
}
