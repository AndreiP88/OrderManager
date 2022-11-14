using System;
using System.IO;
using System.Windows.Forms;

namespace Updater
{
    internal class INISettings
    {
        String _settingsFile = "settings.ini";

        private String GetParameter(String section, String key)
        {
            IniFile INI = new IniFile(_settingsFile);

            String result = "";

            if (INI.KeyExists(key))
                result = INI.ReadString(key, section);

            //MessageBox.Show("GET: [" + section + "][" + key + "]: " + result);
            return result;
        }

        private void SetParameter(String section, String key, String value)
        {
            IniFile INI = new IniFile(_settingsFile);

            //if (INI.KeyExists(key))
                INI.Write(key, value, section);

            //MessageBox.Show("SET: [" + section + "][" + key + "]: " + value);
        }

        public bool GetAutoUpdate()
        {
            bool result = false;
            result = Convert.ToBoolean(GetParameter("update", "autoUpdate"));

            return result;
        }

        public String GetLastDateVersion()
        {
            String result = GetParameter("update", "lastVersion");

            return result;
        }

        public String GetLastUpdateTime()
        {
            String result = GetParameter("update", "lastUpdateTime");

            return result;
        }

        public String GetPeriodUpdate()
        {
            String result = GetParameter("update", "periodUpdate");

            return result;
        }


        public void SetAutoUpdate(bool value)
        {
            SetParameter("update", "autoUpdate", value.ToString());
        }

        public void SetLastDateVersion(String value)
        {
            SetParameter("update", "lastversion", value);
        }

        public void SetLastUpdateTime(String value)
        {
            SetParameter("update", "lastUpdateTime", value);
        }

        public void SetPeriodUpdate(String value)
        {
            SetParameter("update", "periodUpdate", value);
        }

    }

}
