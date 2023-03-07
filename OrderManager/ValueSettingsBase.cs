using MySql.Data.MySqlClient;
using System;
using System.Data.Common;
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

        public String GetSelectedPage(String id)
        {
            return GetValue("userID", id, "selectedPage");
        }

        public String GetDeteilsSalary(String id)
        {
            return GetValue("userID", id, "detailsSalary");
        }

        public string GetTypeLoadOrderDetails(String id)
        {
            string result = "0";
            string value = GetValue("userID", id, "typeLoadOrderDetails");

            if (value != "" || value != null)
            {
                result = value;
            }

            return result;
        }

        public string GetTypeLoadItemMouseHover(String id)
        {
            string result = "0";
            string value = GetValue("userID", id, "typeLoadItemMouseHover");

            if (value != "" || value != null)
            {
                result = value;
            }

            return result;
        }

        public string GetTypeLoadDeviationToMainLV(String id)
        {
            string result = "0";
            string value = GetValue("userID", id, "typeLoadDeviationToMainLV");

            if (value != "" || value != null)
            {
                result = value;
            }

            return result;
        }

        public String GetParameterLine(String id, String selectForm)
        {
            return GetValueFromWindowStateBase("userID", id, selectForm);
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

        private String GetValueFromWindowStateBase(String findColomnName, String findParameter, String valueColomn)
        {
            String result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM usersWindowState WHERE " + findColomnName + " = '" + findParameter + "'"
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

        public void UpdateSelectedPage(String idUser, String newValue)
        {
            UpdateValue("selectedPage", idUser, newValue);
        }

        public void UpdateDeteilsSalary(String idUser, String newValue)
        {
            UpdateValue("detailsSalary", idUser, newValue);
        }

        public void UpdateTypeLoadOrderDetails(String idUser, String newValue)
        {
            UpdateValue("typeLoadOrderDetails", idUser, newValue);
        }

        public void UpdateTypeLoadItemMouseHover(String idUser, String newValue)
        {
            UpdateValue("typeLoadItemMouseHover", idUser, newValue);
        }

        public void UpdateTypeLoadDeviationToMainLV(String idUser, String newValue)
        {
            UpdateValue("typeLoadDeviationToMainLV", idUser, newValue);
        }

        public void UpdateParameterLine(String idUser, String selectForm, String newValue)
        {
            UpdateValueToWindowStateBase(selectForm, idUser, newValue);
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

        private void UpdateValueToWindowStateBase(String colomn, String id, String value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE usersWindowState SET " + colomn + " = @value " +
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
