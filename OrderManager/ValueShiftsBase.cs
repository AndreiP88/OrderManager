﻿using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OrderManager
{
    internal class ValueShiftsBase
    {
        public ValueShiftsBase()
        {

        }

        public bool CheckShiftActivity(int startShiftID)
        {
            bool result = false;

            if (startShiftID != -1)
            {
                string stopShift = GetStopShiftFromID(startShiftID);

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

        public string GetStopShiftFromID(int shiftID)
        {
            List<String> list = new List<String>(GetValue("id", shiftID.ToString(), "stopShift"));

            string result = "";

            if (list.Count > 0)
            {
                result = list[list.Count - 1].ToString();
            }

            return result;
        }

        public int GetIDFromStartShift(string startShift)
        {
            List<string> val = new List<string>(GetValue("startShift", startShift, "id"));

            int result = -1;

            if (val.Count > 0)
            {
                result = Convert.ToInt32(val.Last());
            }

            return result;
        }

        public string GetNameUserFromStartShift(int shiftID)
        {
            List<String> result = new List<String>(GetValue("id", shiftID.ToString(), "nameUser"));

            return result[result.Count - 1];
        }

        public string GetNoteShift(int shiftID)
        {
            List<String> result = new List<String>(GetValue("id", shiftID.ToString(), "note"));

            return result[result.Count - 1];
        }

        public string GetStartShiftFromID(int index)
        {
            List<string> result = new List<string>(GetValue("id", index.ToString(), "startShift"));

            if (result.Count > 0)
            {
                return result[result.Count - 1];
            }
            else
            {
                return "";
            }


        }

        public bool GetCheckFullShift(int shiftID)
        {
            List<String> value = new List<String>(GetValue("id", shiftID.ToString(), "fullShift"));

            string oneVal = value[value.Count - 1];
            bool result = true;

            if (oneVal != "" && oneVal != null)
            {
                result = Convert.ToBoolean(oneVal);
            }
            
            return result;
        }

        public bool GetCheckOvertimeShift(int shiftID)
        {
            List<String> value = new List<String>(GetValue("id", shiftID.ToString(), "overtimeShift"));

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

        public void SetNoteShift(int shiftID, string note)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE shifts SET note = @note " +
                    "WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", shiftID);
                Command.Parameters.AddWithValue("@note", note);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void SetCheckFullShift(int shiftID, bool check)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE shifts SET fullShift = @fullShift " +
                    "WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", shiftID);
                Command.Parameters.AddWithValue("@fullShift", check.ToString());

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void SetCheckOvertimeShift(int shiftID, bool check)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE shifts SET overtimeShift = @overtimeShift " +
                    "WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", shiftID);
                Command.Parameters.AddWithValue("@overtimeShift", check.ToString());

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void CloseShift(int shiftID, string stopShift)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE shifts SET stopShift = @stopShift " +
                    "WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", shiftID);
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
