using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetValueFromShiftsBase
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;

        public GetValueFromShiftsBase(String dBase)
        {
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        public String GetStopShift(String startShift)
        {
            List<String> result = new List<String>(GetValue("startShift", startShift, "stopShift"));

            return result[result.Count - 1];
        }

        public String GetNameUserFromStartShift(String startShift)
        {
            List<String> result = new List<String>(GetValue("startShift", startShift, "nameUser"));

            return result[result.Count - 1];
        }
        public List<String> GetActiveUser()
        {
            List<String> result = new List<String>(GetValue("stopShift", "", "nameUser"));

            return result;
        }

        public void CloseShift(String startShift, String stopShift)
        {
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText = "UPDATE shifts SET stopShift = @stopShift " +
                    "WHERE startShift = @startShift";

                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@startShift", startShift);
                Command.Parameters.AddWithValue("@stopShift", stopShift);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private List<String> GetValue(String findColomnName, String findParameter, String valueColomn)
        {
            List<String> result = new List<String>();

            String cLine = "";

            if (findParameter == "")
                cLine = "SELECT * FROM shifts WHERE (" + findColomnName + " is null or " + findColomnName + " = '')";
            else
                cLine = "SELECT * FROM shifts WHERE " + findColomnName + " = '" + findParameter + "'";

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @cLine
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result.Add(sqlReader[valueColomn].ToString());
                }

                Connect.Close();
            }

            return result;
        }

    }
}
