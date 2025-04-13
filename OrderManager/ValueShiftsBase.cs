using libData;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

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

        public int GetNameUserFromStartShift(int shiftID)
        {
            int result = -1;

            List<string> value = new List<string>(GetValue("id", shiftID.ToString(), "nameUser"));

            if (Int32.TryParse(value.Last(), out int res))
            {
                result = res;
            }

            return result;
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
        /// <summary>
        /// Добавление завершенной смены при автоматическом внесении смен
        /// </summary>
        /// <param name="shift"></param>
        /// <returns>Индекс добавленной смены</returns>
        public async Task<int> AddClosedShiftAsync(LoadShift shift)
        {
            int shiftID = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                await Connect.OpenAsync();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"INSERT INTO shifts (nameUser, shiftNumber, startShift, stopShift, note, fullShift, overtimeShift) 
                                        SELECT @nameUser, @shiftNumber, @startShift, @stopShift, @note, @fullShift, @overtimeShift 
                                    WHERE 
                                        NOT EXISTS (SELECT nameUser, shiftNumber, startShift FROM shifts WHERE nameUser = @nameUser AND shiftNumber = @shiftNumber AND startShift = @startShift) LIMIT 1; 
                                    SELECT id FROM shifts 
                                    WHERE  
                                        nameUser = @nameUser AND shiftNumber = @shiftNumber AND startShift = @startShift"
                };
                Command.Parameters.AddWithValue("@nameUser", shift.UserIDBaseOM);
                Command.Parameters.AddWithValue("@shiftNumber", shift.ShiftNumber);
                Command.Parameters.AddWithValue("@startShift", shift.ShiftStart);
                Command.Parameters.AddWithValue("@stopShift", shift.ShiftEnd);
                Command.Parameters.AddWithValue("@note", "");
                Command.Parameters.AddWithValue("@fullShift", "True");
                Command.Parameters.AddWithValue("@overtimeShift", "False");

                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
                {
                    shiftID = sqlReader["id"] == DBNull.Value ? -1 : Convert.ToInt32(sqlReader["id"]);
                }

                Connect.Close();
            }

            return shiftID;
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

        public List<int> LoadUsersListFromMonth(DateTime date)
        {
            List<int> usersList = new List<int>();

            DateTime startPeriod = Convert.ToDateTime("01." + date.Month + "." + date.Year + " 00:00:00");
            DateTime endPeriod = Convert.ToDateTime(date.AddMonths(1).AddDays(-1).Day + "." + date.Month + "." + date.Year + " 23:59:59");

            string startDateTime = startPeriod.ToString("dd.MM.yyyy HH:mm:ss");
            string endDateTime = endPeriod.ToString("dd.MM.yyyy HH:mm:ss");

            try
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    Connect.Open();
                    MySqlCommand Command = new MySqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT DISTINCT nameUser FROM shifts WHERE 
                                        (STR_TO_DATE(startShift,'%d.%m.%Y %H:%i:%S') >= STR_TO_DATE(@startDate,'%d.%m.%Y %H:%i:%S') 
	                                    AND STR_TO_DATE(startShift,'%d.%m.%Y %H:%i:%S') <= STR_TO_DATE(@endDate,'%d.%m.%Y %H:%i:%S'))"
                    };
                    Command.Parameters.AddWithValue("@startDate", startDateTime);
                    Command.Parameters.AddWithValue("@endDate", endDateTime);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        int load = Convert.ToInt32(sqlReader["nameUser"]);
                        
                        if (!usersList.Contains(load))
                        {
                            usersList.Add(load);
                        }
                    }

                    Connect.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Ошибка получения списка сотрудников: " + ex.ToString());
            }

            return usersList;
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

        public List<string> GetShiftFromDate(int userID, string date)
        {
            List<string> startShift = new List<string>();

            try
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    Connect.Open();
                    MySqlCommand Command = new MySqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT startShift FROM `shifts`
                                        WHERE str_to_date(startShift, '%d.%m.%Y') = str_to_date(@startDate,  '%d.%m.%Y')
                                        AND nameUser = @userID"
                    };
                    Command.Parameters.AddWithValue("@startDate", date);
                    Command.Parameters.AddWithValue("@userID", userID);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        startShift.Add(sqlReader["startShift"].ToString());
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка получения списка смен: " + ex.ToString());
            }

            return startShift;
        }

        public List<LoadShift> GetShiftListFromDate(int userID, string date)
        {
            List<LoadShift> startShift = new List<LoadShift>();

            try
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    Connect.Open();
                    MySqlCommand Command = new MySqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT shiftNumber, startShift FROM `shifts`
                                        WHERE str_to_date(startShift, '%d.%m.%Y') = str_to_date(@startDate,  '%d.%m.%Y')
                                        AND nameUser = @userID"
                    };
                    Command.Parameters.AddWithValue("@startDate", date);
                    Command.Parameters.AddWithValue("@userID", userID);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        startShift.Add(new LoadShift(userID, (int)sqlReader["shiftNumber"], sqlReader["startShift"].ToString()));
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка получения списка смен: " + ex.ToString());
            }

            return startShift;
        }
        public LoadShift GetShiftFromDate(int userID, string date, int shiftNumber)
        {
            LoadShift startShift = null;

            try
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    Connect.Open();
                    MySqlCommand Command = new MySqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT shiftNumber, startShift FROM `shifts`
                                        WHERE str_to_date(startShift, '%d.%m.%Y') = str_to_date(@startDate,  '%d.%m.%Y')
                                        AND nameUser = @userID
                                        AND shiftNumber = @shiftNumber"
                    };
                    Command.Parameters.AddWithValue("@startDate", date);
                    Command.Parameters.AddWithValue("@userID", userID);
                    Command.Parameters.AddWithValue("@shiftNumber", shiftNumber);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        startShift = new LoadShift(userID, (int)sqlReader["shiftNumber"], sqlReader["startShift"].ToString());
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка получения списка смен: " + ex.ToString());
            }

            return startShift;
        }
    }
}
