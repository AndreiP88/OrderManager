﻿using libData;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;

namespace OrderManager
{
    internal class ValueUserBase
    {
        public ValueUserBase()
        {

        }

        /// <summary>
        /// получить имя пользователя по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Имя пользователя из поля nameUser базы данных</returns>
        public String GetNameUser(String id)
        {
            string fullName = "";

            fullName += GetValue("id", id, "name");
            fullName += " " + GetValue("id", id, "surname");

            return fullName;
        }

        /// <summary>
        /// Полное имя по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public String GetFullNameUser(String id)
        {
            string fullName = "";

            fullName += GetValue("id", id, "surname");
            fullName += " " + GetValue("id", id, "name");
            fullName += " " + GetValue("id", id, "patronymic");

            return fullName;
        }

        /// <summary>
        /// Имя и фамилия по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public String GetSmalNameUser(String id)
        {
            string fullName = "";

            fullName += GetValue("id", id, "surname");
            fullName += " " + GetValue("id", id, "name");

            return fullName;
        }

        public List<int> GetIndexUserFromASBase(int indexUserFromASBase)
        {
            List<int> indexes = GetASIndexes(indexUserFromASBase);

            /*string load = (string)GetValue("id", userID.ToString(), "indexUserFromAS");
            //load = load.Remove(load.Length - 1);
            List<int> indexes = load?.Split(';')?.Select(Int32.Parse)?.ToList();*/

            return indexes;
        }

        public ShiftShedule GetUserShiftShedule(int userID)
        {
            object loadStartShiftDate = GetValueInfo("user", userID.ToString(), "shiftStartDate");
            string startShiftDate = loadStartShiftDate == DBNull.Value ? string.Empty : (string)loadStartShiftDate;

            object loadShiftBlanks = GetValueInfo("user", userID.ToString(), "shiftShedule");
            string blanksLine = loadShiftBlanks == DBNull.Value ? string.Empty : (string)loadShiftBlanks;

            object loadShiftColors = GetValueInfo("user", userID.ToString(), "shiftColors");
            string colorsLine = loadShiftColors == DBNull.Value ? "-1;-1;-1" : (string)loadShiftColors;

            List<ShiftBlank> shiftBlanks = new List<ShiftBlank>();

            List<string> blanks = blanksLine?.Split(';')?.ToList();

            if (blanks != null && blanks.Count > 0)
            {
                foreach (string shift in blanks)
                {
                    string[] blank = shift?.Split('|');

                    if (blank.Length > 1)
                    {
                        shiftBlanks.Add(new ShiftBlank(
                        blank[0],
                        blank[1]
                        ));
                    }
                }
            }

            Color[] shiftColors = new Color[7];

            //string[] colors = colorsLine?.Split(';');
            List<int> colors = colorsLine?.Split(';')?.Select(Int32.Parse)?.ToList();

            int colorsCount = colors.Count;

            for (int i = 0; i < 7; i++)
            {
                if (i < colorsCount)
                {
                    shiftColors[i] = Color.FromArgb(colors[i]);
                }
                else
                {
                    shiftColors[i] = Color.FromArgb(-1);
                }
            }

            /*ShiftShedule shiftShedule = new ShiftShedule
            {
                UserID = userID,
                ShiftStartDate = startShiftDate,
                ShiftBlanks = shiftBlanks,
                ShiftColors = shiftColors,
            };*/

            ShiftShedule shiftShedule = new ShiftShedule(userID, startShiftDate, shiftBlanks, shiftColors);

            return shiftShedule;
        }

        public void SetUserShiftShedule(ShiftShedule shiftShedule)
        {
            int userID = shiftShedule.UserID;
            string shiftDateStart = shiftShedule.ShiftStartDate;
            string shiftBlanksLine = "";
            string shiftColorsLine = "";

            List<ShiftBlank> shiftBlanks = shiftShedule.ShiftBlanks;

            for (int i = 0; i < shiftBlanks.Count; i++)
            {
                shiftBlanksLine += shiftBlanks[i].Shift + "|" + shiftBlanks[i].Name;

                if (i < shiftBlanks.Count - 1)
                {
                    shiftBlanksLine += ";";
                }
            }

            Color[] colors = shiftShedule.ShiftColors;

            for (int i = 0; i < colors.Length; i++)
            {
                shiftColorsLine += colors[i].ToArgb();

                if (i < colors.Length - 1)
                {
                    shiftColorsLine += ";";
                }
            }

            UpdateValueInfo("shiftStartDate", userID.ToString(), shiftDateStart);
            UpdateValueInfo("shiftShedule", userID.ToString(), shiftBlanksLine);
            UpdateValueInfo("shiftColors", userID.ToString(), shiftColorsLine);
        }

        public int GetIDUserFromName(string nameUser)
        {
            int result = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users WHERE concat(name, ' ', surname) = '" + nameUser + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = (int)sqlReader["id"];
                }

                Connect.Close();
            }

            return result;
        }

        public List<int> GetEquipsListForSelectedUser(int userId)
        {
            List<int> result = new List<int>();

            ValueInfoBase infoBase = new ValueInfoBase();

            List<int> categoryes = GetCategoryesList(userId);

            for (int i = 0; i < categoryes.Count; i++)
            {
                result.AddRange(infoBase.GetMachinesList(categoryes[i]));
            }

            return result;
        }

        public List<int> GetEquipsListASBaseForSelectedUser(int userId)
        {
            List<int> result = new List<int>();

            ValueInfoBase infoBase = new ValueInfoBase();

            List<int> categoryes = GetCategoryesList(userId);

            for (int i = 0; i < categoryes.Count; i++)
            {
                result.AddRange(infoBase.GetEquipsASBaseList(categoryes[i]));
            }

            return result;
        }

        public string GetCategoryesMachine(string id)
        {
            return (string)GetValue("id", id, "categoryesMachine");
        }

        public int GetCategoryesMachine(int userID)
        {
            return (int)GetValue("id", userID.ToString(), "categoryesMachine");
        }

        public string[] GetMachinesArr(string id)
        {
            return GetCategoryesMachine(id).Split(';');
        }

        public List<int> GetCategoryesList(int userID)
        {
            return GetCategoryesMachine(userID.ToString())?.Split(';')?.Select(Int32.Parse)?.ToList();
        }

        public bool CategoryForUser(String id, String category)
        {
            return GetMachinesArr(id).Contains(category);
        }

        public bool CategoryForUser(string id, string[] category)
        {
            return GetMachinesArr(id).Any(x => category.Contains(x));
        }

        public int GetLastMachineForUser(string id)
        {
            //return Convert.ToInt32(GetValueInfo("user", id.ToString(), "lastMachine"));
            object load = GetValueInfo("user", id, "lastMachine");

            return load == DBNull.Value ? -1 : (int)load;
        }

        public String GetActiveUser(String id)
        {
            return (string)GetValue("id", id, "activeUser");
        }

        public String GetPasswordUser(string id)
        {
            object load = GetValue("id", id, "passwordUser");

            return load == DBNull.Value ? string.Empty : (string)load;
        }

        public int GetCurrentShiftStart(string id)
        {
            int result = -1;

            if (id != "")
            {
                result = (int)GetValueInfo("user", id, "currentShiftStartID");
            }

            return result;
        }

        public string GetLastUID(String id)
        {
            object load = GetValueInfo("user", id, "lastUID");

            return load == DBNull.Value ? string.Empty : (string)load;
            //return (string)GetValueInfo("user", id, "lastUID");
        }

        /// <summary>
        /// Получить id пользователя для активной смены по времени начала смены
        /// </summary>
        /// <param name="shiftStart"></param>
        /// <returns></returns>
        public int GetCurrentUserIDFromShiftStart(int shiftStart)
        {
            return (int)GetValueInfo("currentShiftStartID", shiftStart.ToString(), "user");
        }

        public List<String> GetUserList(bool activeUserOnly)
        {
            List<String> userList = new List<String>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (activeUserOnly)
                    {
                        if (Convert.ToBoolean(GetActiveUser(sqlReader["id"].ToString())))
                            userList.Add(sqlReader["id"].ToString());
                    }
                    else
                    {
                        userList.Add(sqlReader["id"].ToString());
                    }
                }

                Connect.Close();
                Connect.Dispose();
            }

            return userList;
        }

        public List<String> GetUserListForCategory(bool activeUserOnly, String loadMode)
        {
            List<String> userList = new List<String>(GetUserList(activeUserOnly));
            List<String> result = new List<String>();

            for (int i = 0; i < userList.Count; i++)
            {
                if (CategoryForUser(userList[i], loadMode) || loadMode == "")
                {
                    result.Add(userList[i].ToString());
                }
            }

            return result;
        }

        public List<string> GetUserListForCategory(bool activeUserOnly, string[] categoryList)
        {
            List<string> userList = new List<string>(GetUserList(activeUserOnly));
            List<string> result = new List<string>();
            
            for (int i = 0; i < userList.Count; i++)
            {
                if (CategoryForUser(userList[i], categoryList) || categoryList[0] == "")
                {
                    result.Add(userList[i].ToString());
                }
            }

            return result;
        }

        public int GetCountUsers()
        {
            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(DISTINCT id) as count FROM users WHERE activeUser = 'True'"
                };

                result = Convert.ToInt32(Command.ExecuteScalar());

                Connect.Close();
            }

            return result;
        }

        public bool GetUserWorking(string id)
        {
            bool result = false;

            object load = GetValueInfo("user", id, "currentShiftStartID");

            int value = load == DBNull.Value ? -1 : (int)load;

            if (value != -1)
                result = true;

            return result;
        }

        public Object GetUserInfo()
        {
            List<UserInfo> userInfos = new List<UserInfo>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                ValueUserBase usersBase = new ValueUserBase();
                GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    int userID = Convert.ToInt32(sqlReader["id"]);

                    List<int> indexASUsers = GetASIndexes(userID);

                    userInfos.Add(new UserInfo(
                        userID,
                        sqlReader["surname"].ToString(),
                        sqlReader["name"].ToString(),
                        sqlReader["patronymic"].ToString(),
                        sqlReader["categoryesMachine"].ToString(),
                        sqlReader["dateOfEmployment"].ToString(),
                        sqlReader["dateOfBirth"].ToString(),
                        sqlReader["activeUser"].ToString(),
                        indexASUsers,
                        sqlReader["dateOfDismissal"].ToString(),
                        sqlReader["note"].ToString()
                        ));
                }

                Connect.Close();
            }

            return userInfos;
        }

        public Object GetUserInfoFromID(string userID)
        {
            UserInfo userInfos = new UserInfo(-1, "", "", "", "", "", "", "", new List<int>(), "", "");

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                ValueUserBase usersBase = new ValueUserBase();
                GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users WHERE id = '" + userID + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    int loadUserID = Convert.ToInt32(sqlReader["id"]);

                    List<int> indexASUsers = GetASIndexes(loadUserID);

                    userInfos = new UserInfo(
                        loadUserID,
                        sqlReader["surname"].ToString(),
                        sqlReader["name"].ToString(),
                        sqlReader["patronymic"].ToString(),
                        sqlReader["categoryesMachine"].ToString(),
                        sqlReader["dateOfEmployment"].ToString(),
                        sqlReader["dateOfBirth"].ToString(),
                        sqlReader["activeUser"].ToString(),
                        indexASUsers,
                        sqlReader["dateOfDismissal"].ToString(),
                        sqlReader["note"].ToString()
                        );
                }

                Connect.Close();
            }

            return userInfos;
        }

        public void UpdateLastMachine(string idUser, string newValue)
        {
            UpdateValueInfo("lastMachine", idUser, newValue);
        }

        public void UpdatePassword(string idUser, string newValue)
        {
            UpdateValue("passwordUser", idUser, newValue);
        }

        public void UpdateCurrentShiftStart(string idUser, string newValue)
        {
            UpdateValueInfo("currentShiftStartID", idUser, newValue);
        }

        public void UpdateLastUID(string idUser, string newValue)
        {
            UpdateValueInfo("lastUID", idUser, newValue);
        }

        public void DeleteUser(String id)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM users WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM usersInfo WHERE user = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM usersSettings WHERE userID = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM usersWindowWtate WHERE userID = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void UpdateValue(string colomn, string id, string value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE users SET " + colomn + " = @value " +
                    "WHERE (id = @id)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void UpdateValueInfo(string colomn, string id, string value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE usersInfo SET " + colomn + " = @value " +
                    "WHERE (user = @id)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public int GetUserIdFromASystemID(int indexUserFromASBase)
        {
            int result = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM usersindexesasbase WHERE indexUserFromASBase = @indexUserFromASBase"
                };
                Command.Parameters.AddWithValue("@indexUserFromASBase", indexUserFromASBase);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = (int)sqlReader["userID"];
                }

                Connect.Close();
            }

            /*using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    string load = sqlReader["indexUserFromAS"].ToString();

                    List<int> indexesAS = load?.Split(';')?.Select(Int32.Parse)?.ToList();

                    if (indexesAS.Contains(userASystemID))
                    {
                        result = Convert.ToInt32(sqlReader["id"]);
                    }
                }

                Connect.Close();
            }*/

            return result;
        }

        private List<string> LoadUsersASystemIDs()
        {
            List<string> result = new List<string>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result.Add(sqlReader["indexUserFromAS"].ToString());
                }

                Connect.Close();
            }

            return result;
        }

        private object GetValue(String findColomnName, String findParameter, String valueColomn)
        {
            object result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users WHERE " + findColomnName + " = '" + findParameter + "'"
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

        private object GetValueInfo(string findColomnName, string findParameter, string valueColomn)
        {
            object result = null;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM usersInfo WHERE " + findColomnName + " = '" + findParameter + "'"
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

        private List<int> GetASIndexes(int userID)
        {
            List<int> result = new List<int>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();

                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM usersindexesasbase WHERE userID = @userID"
                };
                Command.Parameters.AddWithValue("@userID", userID);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result.Add((int)sqlReader["indexUserFromASBase"]);
                }

                Connect.Close();
            }

            return result;
        }

        public bool AddNewASUsersIndexes(int userID, List<int> indexesUserFromASBase)
        {
            bool result = false;

            List<int> currentIndexes = GetASIndexes(userID);
            //сделать удаление лишних
            foreach (int index in indexesUserFromASBase)
            {
                if (!currentIndexes.Contains(index))
                {
                    using (MySqlConnection Connect = DBConnection.GetDBConnection())
                    {
                        string commandText = "INSERT INTO usersindexesasbase (userID, indexUserFromASBase) VALUES (@userID, @userIDAS)";

                        MySqlCommand Command = new MySqlCommand(commandText, Connect);
                        Command.Parameters.AddWithValue("@userID", userID);
                        Command.Parameters.AddWithValue("@userIDAS", index);

                        Connect.Open();
                        Command.ExecuteNonQuery();
                        Connect.Close();
                    }
                }
            }

            return result;
        }
    }
}
