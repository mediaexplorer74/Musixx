// ApplicationSettingsHelper

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Musixx.Shared
{
    public static class ApplicationSettingsHelper
    {
        public static object ReadResetSettingsValue(string key)
        {
            object value = ReadSettingsValue(key);

            if (value != null)
                ApplicationData.Current.LocalSettings.Values.Remove(key);

            return value;
        }

        public static object ReadSettingsValue(string key)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                return null;

            return ApplicationData.Current.LocalSettings.Values[key];
        }

        public static void SaveSettingsValue(string key, object value)
        {
            // RnD
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                try
                {
                    ApplicationData.Current.LocalSettings.Values.Add(key, value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("!! Save settings exception !! : " + ex.Message);
                }
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }
    }
}
