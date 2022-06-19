using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class SetUpdateSettingsValue
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;

        public SetUpdateSettingsValue(String dBase)
        {
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
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
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText = "UPDATE usersSettings SET " + colomn + " = @value " +
                    "WHERE (userID = @id)";

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
