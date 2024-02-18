using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareX_windows.Helpers
{
    public static class SettingsHelper
    {
        public static void save_Setting(ApplicationSettingsBase settings, string setting_Name, string setting_Value)

        {
            string property_name = setting_Name;

            SettingsProperty prop = null;

            if (settings.Properties[property_name] != null)
            {
                prop = settings.Properties[property_name];
            }

            else
            {
                prop = new System.Configuration.SettingsProperty(property_name);
                prop.PropertyType = typeof(string);
                settings.Properties.Add(prop);
                settings.Save();
            }
            settings.Properties[property_name].DefaultValue = setting_Value;

            settings.Save();

        }



        public static string read_Setting(ApplicationSettingsBase settings, string setting_Name)
        {
            string sResult = "";

            if (settings.Properties[setting_Name] != null)
            {
                sResult = settings.Properties[setting_Name].DefaultValue.ToString();
            }

            if (sResult == "NaN") sResult = "0";

            return sResult;
        }
    }
}
