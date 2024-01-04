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

        public string GetCategoryName(string id)
        {
            return (string)GetValue("id", id, "category").ToString();
        }

        public String GetMainIDNormOperation(String id)
        {
            return GetValue("id", id, "mainIdNormOperation").ToString();
        }

        public String GetIDOptionView(String id)
        {
            return (string)GetValue("id", id, "idOptionForView").ToString();
        }

        public String GetMKIDNormOperation(String id)
        {
            return (string)GetValue("id", id, "mkIdNormOperation").ToString();
        }

        public String GetWKIDNormOperation(String id)
        {
            return (string)GetValue("id", id, "wkIdNormOperation").ToString();
        }

        public string GetCategoryFromName(string category)
        {
            return GetValue("category", category, "id").ToString();
        }

        public int GetIDCategoryFromName(string category)
        {
            int result = -1;

            object load = GetValue("category", category, "id");

            if (load != null)
            {
                result = (int)load;
            }

            return result;
        }

        public List<string> GetCategoryesList()
        {
            List<string> categoryList = new List<string>();

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

        private object GetValue(string findColomnName, string findParameter, string valueColomn)
        {
            object result = -1;

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
                    result = sqlReader[valueColomn];
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
