using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace OrderManager
{
    internal class ValueCategory
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;

        public ValueCategory(String dBase)
        {
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        public String GetCategoryName(String id)
        {
            return GetValue("id", id, "category");
        }

        public String GetCategoryFromName(String category)
        {
            return GetValue("category", category, "id");
        }

        public List<String> GetCategoryesList()
        {
            List<String> categoryList = new List<String>();

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machinesCategoryes"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    categoryList.Add(sqlReader["category"].ToString());
                }

                Connect.Close();
            }

            return categoryList;
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
                    CommandText = @"SELECT * FROM machinesCategoryes WHERE " + findColomnName + " = '" + findParameter + "'"
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
