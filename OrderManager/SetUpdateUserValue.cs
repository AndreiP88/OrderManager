using System;
using System.Data.SQLite;
using System.IO;

namespace OrderManager
{
    internal class SetUpdateUsersBase
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;

        public SetUpdateUsersBase(String dBase)
        {
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        public void UpdateLastMachine(String idUser, String newValue)
        {
            UpdateValueInfo("lastMachine", idUser, newValue);
        }

        public void UpdateName(String idUser, String newValue)
        {
            UpdateValue("nameUser", idUser, newValue);
        }

        public void UpdatePassword(String idUser, String newValue)
        {
            UpdateValue("passwordUser", idUser, newValue);
        }

        public void UpdateCurrentShiftStart(String idUser, String newValue)
        {
            UpdateValueInfo("currentShiftStart", idUser, newValue);
        }

        private void UpdateValue(String colomn, String id, String value)
        {
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText = "UPDATE users SET " + colomn + " = @value " +
                    "WHERE (id = @id)";

                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void UpdateValueInfo(String colomn, String id, String value)
        {
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText = "UPDATE usersInfo SET " + colomn + " = @value " +
                    "WHERE (user = @id)";

                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }
    }
}
