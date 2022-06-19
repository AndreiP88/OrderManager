using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class SetUpdateInfoBase
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;
        String machine;

        public SetUpdateInfoBase(String dBase, String activeMachine)
        {
            this.dataBase = dBase;
            this.machine = activeMachine;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        public void UpdateCurrentOrder (String currentOrder, String currentModification)
        {
            UpdateInfoParameter("currentOrder", currentOrder);
            UpdateInfoParameter("currentModification", currentModification);
        }

        public void UpdateInfo(String currentCounterRepeat, String currentOrder, String currentModification, String lastOrder, String lastModification, bool activeOrder)
        {
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText = "UPDATE Info SET currentCounterRepeat = @currentCounterRepeat, currentOrder = @currentOrder, " +
                    "currentModification = @currentModification, lastOrder = @lastOrder, lastModification = @lastModification, activeOrder = @activeOrder " +
                    "WHERE (machine = @machine)";

                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@currentOrder", currentOrder);
                Command.Parameters.AddWithValue("@currentModification", currentModification);
                Command.Parameters.AddWithValue("@lastOrder", lastOrder);
                Command.Parameters.AddWithValue("@lastModification", lastModification);
                Command.Parameters.AddWithValue("@currentCounterRepeat", currentCounterRepeat);
                Command.Parameters.AddWithValue("@activeOrder", activeOrder.ToString());

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

        }

        private void UpdateInfoParameter(String parameter, String value)
        {
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText = "UPDATE Info SET " + parameter + " = @value " +
                    "WHERE (machine = @machine)";

                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@value", value);
                Command.Parameters.AddWithValue("@machine", machine);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

        }
    }
}
