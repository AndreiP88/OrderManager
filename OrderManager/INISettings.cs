using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    internal class INISettings
    {
        String _settingsFile = "settings.ini";

        private String GetParameter(String section, String key)
        {
            IniFile INI = new IniFile(_settingsFile);
            String result = "";

            if (INI.KeyExists("dataBasePath"))
                result = INI.ReadINI(section, key);

            //MessageBox.Show("GET: [" + section + "][" + key + "]: " + result);
            return result;
        }

        private void SetParameter(String section, String key, String value)
        {
            IniFile INI = new IniFile(_settingsFile);

            if (INI.KeyExists("dataBasePath"))
                INI.Write(section, key, value);

            //MessageBox.Show("SET: [" + section + "][" + key + "]: " + value);
        }

        public String DataBasePath()
        {
            String result = Directory.GetCurrentDirectory() + "\\data\\data.db";
            String value = GetParameter("general", "dataBaseLocalFile");

            bool localDB = Convert.ToBoolean(value);

            if (!localDB)
            {
                result = GetParameter("general", "dataBasePath");
            }

            result = result.Replace(@"\", "\\\\");

            if (!File.Exists(result))
            {
                result = Directory.GetCurrentDirectory() + "\\data\\data.db";
            }

            return result;
        }

        public bool GetCheckDBLocalPath()
        {
            bool result = Convert.ToBoolean(GetParameter("general", "dataBaseLocalFile"));

            return result;
        }

        public String GetColumnHeadersMain()
        {
            String result = GetParameter("colomns", "columnHeadersMain");

            return result;
        }
        public String GetColumnHeadersStatistic()
        {
            String result = GetParameter("colomns", "columnHeadersStatistic");

            return result;
        }

        public String GetColumnHeadersShifts()
        {
            String result = GetParameter("colomns", "columnHeadersShifts");
            
            return result;
        }

        public String GetColumnHeadersStatisticMachines()
        {
            String result = GetParameter("colomns", "columnHeadersStatisticMachines");

            return result;
        }

        public String GetColumnHeadersAllOrders()
        {
            String result = GetParameter("colomns", "columnHeadersAllOrders");

            return result;
        }

        public String GetColumnHeadersNorm()
        {
            String result = GetParameter("colomns", "columnHeadersNorm");

            return result;
        }

        public String GetColumnHeadersUsers()
        {
            String result = GetParameter("colomns", "columnHeadersUsers");

            return result;
        }

        public String GetColumnHeadersMachines()
        {
            String result = GetParameter("colomns", "columnHeadersMachines");

            return result;
        }

        public void SetCheckDBLocalPath(bool value)
        {
            SetParameter("general", "dataBaseLocalFile", value.ToString());
        }

        public void SetDataBasePath(String path)
        {
            SetParameter("general", "dataBasePath", path.ToString());
        }

        public void SetColumnHeadersMain(String value)
        {
            SetParameter("colomns", "columnHeadersMain", value);
        }
        public void SetColumnHeadersStatistic(String value)
        {
            SetParameter("colomns", "columnHeadersStatistic", value);
        }

        public void SetColumnHeadersShifts(String value)
        {
            SetParameter("colomns", "columnHeadersShifts", value);
        }

        public void SetColumnHeadersStatisticMachines(String value)
        {
            SetParameter("colomns", "columnHeadersStatisticMachines", value);
        }

        public void SetColumnHeadersAllOrders(String value)
        {
            SetParameter("colomns", "columnHeadersAllOrders", value);
        }

        public void SetColumnHeadersNorm(String value)
        {
            SetParameter("colomns", "columnHeadersNorm", value);
        }

        public void SetColumnHeadersUsers(String value)
        {
            SetParameter("colomns", "columnHeadersUsers", value);
        }

        public void SetColumnHeadersMachines(String value)
        {
            SetParameter("colomns", "columnHeadersMachines", value);
        }

    }

    
}
