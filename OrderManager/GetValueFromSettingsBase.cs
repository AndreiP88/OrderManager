using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetValueFromSettingsBase
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;

        public GetValueFromSettingsBase(String dBase)
        {
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        public String GetPasswordChecked(String id)
        {
            return GetValue("userID", id, "checkPassword");
        }

        public String GetParameterLine(String id, String selectForm)
        {
            return GetValue("userID", id, selectForm);
        }

        private String GetValue(String findColomnName, String findParameter, String valueColomn)
        {
            String result = "";

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM settings WHERE " + findColomnName + " = '" + findParameter + "'"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[valueColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }



    }
}
