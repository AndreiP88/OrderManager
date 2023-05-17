using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace OrderManager
{
    internal class ValueCategory
    {

        public ValueCategory()
        {

        }

        public String GetCategoryName(String id)
        {
            return GetValue("id", id, "category");
        }

        public String GetMainIDNormOperation(String id)
        {
            return GetValue("id", id, "mainIdNormOperation");
        }

        public String GetIDOptionView(String id)
        {
            return GetValue("id", id, "idOptionForView");
        }

        public String GetMKIDNormOperation(String id)
        {
            return GetValue("id", id, "mkIdNormOperation");
        }

        public String GetWKIDNormOperation(String id)
        {
            return GetValue("id", id, "wkIdNormOperation");
        }

        public String GetCategoryFromName(String category)
        {
            return GetValue("category", category, "id");
        }

        public List<String> GetCategoryesList()
        {
            List<String> categoryList = new List<String>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machinesCategoryes"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

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

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machinesCategoryes WHERE " + findColomnName + " = '" + findParameter + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[valueColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        public void DeleteCategory(String id)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            List<String> machinesList = infoBase.GetMachinesList(id);

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM machinesCategoryes WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            for (int i = 0; i < machinesList.Count; i++)
            {
                infoBase.DeleteMachine(machinesList[i]);
            }
        }

    }
}
