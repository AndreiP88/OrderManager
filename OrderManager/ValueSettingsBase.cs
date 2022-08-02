using MySql.Data.MySqlClient;
using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace OrderManager
{
    internal class ValueSettingsBase
    {
        public ValueSettingsBase()
        {

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

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM usersSettings WHERE " + findColomnName + " = '" + findParameter + "'"
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

        public void UpdateCheckPassword(String idUser, String newValue)
        {
            UpdateValue("checkPassword", idUser, newValue);
        }

        public void UpdateParameterLine(String idUser, String selectForm, String newValue)
        {
            UpdateValue(selectForm, idUser, newValue);
        }

        private void UpdateValue(String colomn, String id, String value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE usersSettings SET " + colomn + " = @value " +
                    "WHERE (userID = @id)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

    }
}
