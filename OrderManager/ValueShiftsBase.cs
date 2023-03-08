using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    internal class ValueShiftsBase
    {
        public ValueShiftsBase()
        {

        }

        public bool CheckShiftActivity(string startShift)
        {
            bool result = false;

            if (startShift != "")
            {
                string stopShift = GetStopShift(startShift);

                if (stopShift == "")
                {
                    result = true;
                }

                if (stopShift != "")
                {
                    result = false;
                }
            }

            return result;
        }

        public String GetStopShift(String startShift)
        {
            List<String> list = new List<String>(GetValue("startShift", startShift, "stopShift"));

            string result = "";

            if (list.Count > 0)
            {
                result = list[list.Count - 1].ToString();
            }

            return result;
        }

        public String GetNameUserFromStartShift(String startShift)
        {
            List<String> result = new List<String>(GetValue("startShift", startShift, "nameUser"));

            return result[result.Count - 1];
        }

        public String GetNoteShift(String startShift)
        {
            List<String> result = new List<String>(GetValue("startShift", startShift, "note"));

            return result[result.Count - 1];
        }

        public string GetStartShiftFromID(int index)
        {
            List<String> result = new List<String>(GetValue("id", index.ToString(), "startShift"));

            return result[result.Count - 1];
        }

        public bool GetCheckFullShift(String startShift)
        {
            List<String> value = new List<String>(GetValue("startShift", startShift, "fullShift"));

            string oneVal = value[value.Count - 1];
            bool result = true;

            if (oneVal != "" && oneVal != null)
            {
                result = Convert.ToBoolean(oneVal);
            }
            
            return result;
        }

        public bool GetCheckOvertimeShift(String startShift)
        {
            List<String> value = new List<String>(GetValue("startShift", startShift, "overtimeShift"));

            string oneVal = null;
            bool result = false;

            if (value.Count > 0)
            {
                oneVal = value[value.Count - 1];
            }

            if (oneVal != "" && oneVal != null)
            {
                result = Convert.ToBoolean(oneVal);
            }

            return result;
        }
        public List<String> GetActiveUser()
        {
            List<String> result = new List<String>(GetValue("stopShift", "", "nameUser"));

            return result;
        }

        public void SetNoteShift(String startShift, String note)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE shifts SET note = @note " +
                    "WHERE startShift = @startShift";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@startShift", startShift);
                Command.Parameters.AddWithValue("@note", note);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void SetCheckFullShift(String startShift, bool check)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE shifts SET fullShift = @fullShift " +
                    "WHERE startShift = @startShift";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@startShift", startShift);
                Command.Parameters.AddWithValue("@fullShift", check.ToString());

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void SetCheckOvertimeShift(String startShift, bool check)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE shifts SET overtimeShift = @overtimeShift " +
                    "WHERE startShift = @startShift";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@startShift", startShift);
                Command.Parameters.AddWithValue("@overtimeShift", check.ToString());

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void CloseShift(String startShift, String stopShift)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE shifts SET stopShift = @stopShift " +
                    "WHERE startShift = @startShift";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@startShift", startShift);
                Command.Parameters.AddWithValue("@stopShift", stopShift);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public List<String> LoadYears()
        {
            List<String> years = new List<String>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT startShift FROM shifts"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    if (years.IndexOf(Convert.ToDateTime(sqlReader["startShift"]).ToString("yyyy")) == -1)
                        years.Add(Convert.ToDateTime(sqlReader["startShift"]).ToString("yyyy"));
                }

                Connect.Close();
            }

            return years;
        }

        private List<String> GetValue(String findColomnName, String findParameter, String valueColomn)
        {
            List<String> result = new List<String>();

            String cLine = "";

            if (findParameter == "")
                cLine = "SELECT * FROM shifts WHERE (" + findColomnName + " is null or " + findColomnName + " = '')";
            else
                cLine = "SELECT * FROM shifts WHERE " + findColomnName + " = '" + findParameter + "'";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @cLine
                };
                DbDataReader sqlReader = Command.ExecuteReader();

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
