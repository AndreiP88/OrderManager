﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace OrderManager
{
    internal class GetValueFromInfoBase
    {

        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;

        public GetValueFromInfoBase(String dBase)
        {
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        public String GetMachineName(String machine)
        {
            return GetValueMachines("id", machine, "name");
        }

        public String GetMachineFromName(String machineName)
        {
            return GetValueMachines("name", machineName, "id");
        }

        public String GetCategoryMachine(String machine)
        {
            return GetValueMachines("id", machine, "category");
        }

        public String GetMachineStartWork(String machine)
        {
            return GetValueMachines("id", machine, "dateStartWork");
        }

        public String GetMachineNote(String machine)
        {
            return GetValueMachines("id", machine, "note");
        }

        public String GetIDUser(String machine)
        {
            return GetValue("machine", machine, "nameOfExecutor");
        }

        public String GetActiveOrder(String machine)
        {
            return GetValue("machine", machine, "activeOrder");
        }

        public String GetCurrentOrderNumber(String machine)
        {
            return GetValue("machine", machine, "currentOrder");
        }

        public String GetCurrentOrderModification(String machine)
        {
            return GetValue("machine", machine, "currentModification");
        }

        public String GetCurrentCounterRepeat(String machine)
        {
            return GetValue("machine", machine, "currentCounterRepeat");
        }

        public String GetLastOrderNumber(String machine)
        {
            return GetValue("machine", machine, "lastOrder");
        }

        public String GetLastOrderModification(String machine)
        {
            return GetValue("machine", machine, "lastModification");
        }

        public String GetMachineFromOrder(String orderNumber, String orderModification)
        {
            return GetValueTwoParam("currentOrder", orderNumber, "currentModification", orderModification, "machine");
        }

        /// <summary>
        /// Получить список всего оборудования
        /// </summary>
        /// <returns>List содержащий все оборудование</returns>
        public List<String> GetMachinesList()
        {
            List<String> machinesList = new List<String>(GetAllMachines(""));

            return machinesList;
        }

        /// <summary>
        /// Получить список оборудования указанной категории
        /// </summary>
        /// <param name="category"></param>
        /// <returns>List содержащий оборудование выбранной категории</returns>
        public List<String> GetMachinesList(String category)
        {
            List<String> machinesList = new List<String>(GetAllMachines(category));

            return machinesList;
        }

        private List<String> GetAllMachines(String category)
        {
            List<String> machinesList = new List<String>();

            String commLine = "";

            if (category != "")
                commLine = " WHERE category = '" + category + "'";

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machines" + commLine
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    machinesList.Add(sqlReader["id"].ToString());
                }

                Connect.Close();
            }

            return machinesList;
        }
        private String GetValueMachines(String findColomnName, String findParameter, String valueColomn)
        {
            String result = "";

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machines WHERE " + findColomnName + " = '" + findParameter + "'"
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

        private String GetValue(String findColomnName, String findParameter, String valueColomn)
        {
            String result = "";

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machinesInfo WHERE " + findColomnName + " = '" + findParameter + "'"
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

        /// <summary>
        /// Список активного оборудования для выпранного пользователя
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public Object GetMachines(String userID)
        {
            List<String> result = new List<String>();
            result.Clear();

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machinesInfo WHERE nameOfExecutor = '" + userID + "'"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result.Add(sqlReader["machine"].ToString());
                }

                Connect.Close();
            }
            return result;
        }

        public String GetMachinesStr(String userID)
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            List<String> orderMachines = (List<String>)GetMachines(userID);
            String machines = "";

            for (int i = 0; i < orderMachines.Count; i++)
            {
                machines += getInfo.GetMachineName(orderMachines[i]);

                if (i != orderMachines.Count - 1)
                    machines += ", ";
                else
                    machines += ".";
            }

            return machines;
        }

        public bool GetMachinesForUserActive(String userID)
        {
            List<String> orderMachines = (List<String>)GetMachines(userID);
            bool machinesActive = false;

            for (int i = 0; i < orderMachines.Count; i++)
            {
                if (Convert.ToBoolean(GetActiveOrder(orderMachines[i])) == true)
                    machinesActive = true;
            }

            return machinesActive;
        }

        private String GetValueTwoParam(String findColomnName, String findParameter, String findColomnName2, String findParameter2, String valueColomn)
        {
            String result = "";

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machinesInfo WHERE " + findColomnName + " = '" + findParameter + "' AND " + findColomnName2 + " = '" + findParameter2 + "'"
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
