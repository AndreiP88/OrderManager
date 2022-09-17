﻿using MySql.Data.MySqlClient;
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
            return GetValue("id", id, "nameUser");
        }
        public String GetIDUserFromName(String nameUser)
        {
            return GetValue("nameUser", nameUser, "id");
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

        public String GetCurrentShiftStart(String id)
        {
            return GetValueInfo("user", id, "currentShiftStart");
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
        public String GetCurrentUserIDFromShiftStart(String shiftStart)
        {
            return GetValueInfo("currentShiftStart", shiftStart, "user");
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

            if (GetValueInfo("user", id, "currentShiftStart") != "")
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
                        sqlReader["nameUser"].ToString(),
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
            UserInfo userInfos = new UserInfo(-1, "", "", "", "", "", "", "", "", "", "");

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
                        sqlReader["nameUser"].ToString(),
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

        private void UpdateValueInfo(String colomn, String id, String value)
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