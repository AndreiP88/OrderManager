using System;
using System.IO;
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

            if (INI.KeyExists(key))
                result = INI.ReadINI(section, key);

            //MessageBox.Show("GET: [" + section + "][" + key + "]: " + result);
            return result;
        }

        private void SetParameter(String section, String key, String value)
        {
            IniFile INI = new IniFile(_settingsFile);

            if (INI.KeyExists(key))
                INI.Write(section, key, value);

            //MessageBox.Show("SET: [" + section + "][" + key + "]: " + value);
        }

        public String DataBasePath()
        {
            String result = Directory.GetCurrentDirectory() + "\\data\\data.db";
            String value = GetParameter("general", "dataBaseLocalFile");

            bool localDB = Convert.ToBoolean(value);

            if (!localDB && File.Exists(result))
            {
                result = GetParameter("general", "dataBasePath");
            }

            result = result.Replace(@"\", @"\\");

            //MessageBox.Show(File.Exists(result).ToString() + Environment.NewLine + result);

            return result;
        }

        public String GetDBHost()
        {
            String result = GetParameter("mysqlbase", "host");

            return result;
        }

        public String GetDBPort()
        {
            String result = GetParameter("mysqlbase", "port");

            return result;
        }

        public String GetDBDatabase()
        {
            String result = GetParameter("mysqlbase", "database");

            return result;
        }

        public String GetDBUsername()
        {
            String result = GetParameter("mysqlbase", "username");

            return result;
        }

        public String GetDBPassword()
        {
            String result = GetParameter("mysqlbase", "password");

            return result;
        }

        public bool GetCheckDBLocalPath()
        {
            bool result = Convert.ToBoolean(GetParameter("general", "dataBaseLocalFile"));

            return result;
        }

        public String GetSelectedMonthLengthNorm()
        {
            String result = GetParameter("variables", "selectedMonthLengthNorm");

            return result;
        }

        public String GetSelectedCategory()
        {
            String result = GetParameter("variables", "selectedCategory");

            return result;
        }

        public String GetSelectedMachine()
        {
            String result = GetParameter("variables", "selectedMachine");

            return result;
        }

        public String GetSelectedDateTime()
        {
            String result = GetParameter("variables", "selectedDateTime");

            return result;
        }

        public String GetSelectedDateTimeNorm()
        {
            String result = GetParameter("variables", "selectedDateTimeNorm");

            return result;
        }


        public void SetSelectedMonthLengthNorm(String value)
        {
            SetParameter("variables", "selectedMonthLengthNorm", value);
        }

        public void SetSelectedCategory(String value)
        {
            SetParameter("variables", "selectedCategory", value);
        }

        public void SetSelectedMachine(String value)
        {
            SetParameter("variables", "selectedMachine", value);
        }

        public void SetSelectedDateTime(String value)
        {
            SetParameter("variables", "selectedDateTime", value);
        }

        public void SetSelectedDateTimeNorm(String value)
        {
            SetParameter("variables", "selectedDateTimeNorm", value);
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
