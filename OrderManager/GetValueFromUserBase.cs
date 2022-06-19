using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetValueFromUserBase
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;

        public GetValueFromUserBase(String dBase)
        {
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

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
            return GetValue("id", id, "lastMachine");
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
            return GetValue("id", id, "currentShiftStart");
        }
        public List<String> GetUserList(bool activeUserOnly)
        {
            List<String> userList = new List<String>();

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

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
            }

            return userList;
        }

        public bool GetUserWorking(String id)
        {
            bool result = false;

            if (GetValue("id", id, "currentShiftStart") != "")
                result = true;

            return result;
        }

        public Object GetUserInfo()
        {
            List<UserInfo> userInfos = new List<UserInfo>();

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                GetValueFromUserBase usersBase = new GetValueFromUserBase(dataBase);
                GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

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

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                GetValueFromUserBase usersBase = new GetValueFromUserBase(dataBase);
                GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users WHERE id = '" + userID + "'"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

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

        private String GetValue(String findColomnName, String findParameter, String valueColomn)
        {
            String result = "";

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users WHERE " + findColomnName + " = '" + findParameter + "'"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

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
