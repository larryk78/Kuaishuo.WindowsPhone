using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

using CC_CEDICT.WindowsPhone;

namespace kuaishuo2
{
    public class Settings
    {
        IsolatedStorageSettings settings;

        public Settings()
        {
        }

        public bool AddOrUpdateValue(string Key, Object value)
        {
            if (settings == null)
                settings = IsolatedStorageSettings.ApplicationSettings;

            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            if (settings == null)
                settings = IsolatedStorageSettings.ApplicationSettings;

            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        public void Save()
        {
            settings.Save();
        }

        const string AudioQualitySettingKeyName = "AudioQualitySetting";
        const int AudioQualitySettingDefault = 0;
        public int AudioQualitySetting
        {
            get
            {
                return GetValueOrDefault<int>(AudioQualitySettingKeyName, AudioQualitySettingDefault);
            }
            set
            {
                if (AddOrUpdateValue(AudioQualitySettingKeyName, value))
                    Save();
            }
        }

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
