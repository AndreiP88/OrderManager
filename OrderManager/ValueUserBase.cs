using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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
        public String GetIDUserFromName(String nameUser)
        {
            String result = "";

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
                    result = sqlReader["id"].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        public String GetCategoryesMachine(String id)
        {
            return GetValue("id", id, "categoryesMachine");
        }

        public String[] GetMachinesArr(String id)
        {
            return GetCategoryesMachine(id).Split(';');
        }
        public bool CategoryForUser(String id, String category)
        {
            return GetMachinesArr(id).Contains(category);
        }

        public String GetLastMachineForUser(String id)
        {
            return GetValueInfo("user", id, "lastMachine");
        }

        public String GetActiveUser(String id)
        {
            return GetValue("id", id, "activeUser");
        }

        public String GetPasswordUser(String id)
        {
            return GetValue("id", id, "passwordUser");
        }

        public int GetCurrentShiftStart(string id)
        {
            int result = -1;

            if (id != "")
            {
                result = Convert.ToInt32(GetValueInfo("user", id, "currentShiftStartID"));
            }

            return result;
        }

        public String GetLastUID(String id)
        {
            return GetValueInfo("user", id, "lastUID");
        }

        /// <summary>
        /// Получить id пользователя для активной смены по времени начала смены
        /// </summary>
        /// <param name="shiftStart"></param>
        /// <returns></returns>
        public String GetCurrentUserIDFromShiftStart(int shiftStart)
        {
            return GetValueInfo("currentShiftStartID", shiftStart.ToString(), "user");
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

        public bool GetUserWorking(String id)
        {
            bool result = false;
            string val = GetValueInfo("user", id, "currentShiftStartID");

            if (val != "-1")
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
                    userInfos.Add(new UserInfo(
                        Convert.ToInt32(sqlReader["id"]),
                        sqlReader["surname"].ToString(),
                        sqlReader["name"].ToString(),
                        sqlReader["patronymic"].ToString(),
                        sqlReader["categoryesMachine"].ToString(),
                        sqlReader["dateOfEmployment"].ToString(),
                        sqlReader["dateOfBirth"].ToString(),
                        sqlReader["activeUser"].ToString(),
                        sqlReader["dateOfDismissal"].ToString(),
                        sqlReader["note"].ToString()
                        ));
                }

                Connect.Close();
            }

            return userInfos;
        }

        public Object GetUserInfoFromID(String userID)
        {
            UserInfo userInfos = new UserInfo(-1, "", "", "", "", "", "", "", "", "");

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
                    userInfos = new UserInfo(
                        Convert.ToInt32(sqlReader["id"]),
                        sqlReader["surname"].ToString(),
                        sqlReader["name"].ToString(),
                        sqlReader["patronymic"].ToString(),
                        sqlReader["categoryesMachine"].ToString(),
                        sqlReader["dateOfEmployment"].ToString(),
                        sqlReader["dateOfBirth"].ToString(),
                        sqlReader["activeUser"].ToString(),
                        sqlReader["dateOfDismissal"].ToString(),
                        sqlReader["note"].ToString()
                        );
                }

                Connect.Close();
            }

            return userInfos;
        }

        public void UpdateLastMachine(String idUser, String newValue)
        {
            UpdateValueInfo("lastMachine", idUser, newValue);
        }

        public void UpdatePassword(String idUser, String newValue)
        {
            UpdateValue("passwordUser", idUser, newValue);
        }

        public void UpdateCurrentShiftStart(string idUser, string newValue)
        {
            UpdateValueInfo("currentShiftStartID", idUser, newValue);
        }

        public void UpdateLastUID(String idUser, String newValue)
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

        private void UpdateValue(String colomn, String id, String value)
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

        private String GetValue(String findColomnName, String findParameter, String valueColomn)
        {
            String result = "";

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
                    result = sqlReader[valueColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        private String GetValueInfo(String findColomnName, String findParameter, String valueColomn)
        {
            String result = "";

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
                    result = sqlReader[valueColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

    }
}
