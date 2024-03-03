﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.DataBaseReconnect;

namespace OrderManager
{
    internal class ValueInfoBase
    {
        public ValueInfoBase()
        {

        }

        public async Task<string> GetMachineName(string machine)
        {
            string result = "";
            object load = await GetValueMachines("id", machine, "name");

            result = (string)(load ?? string.Empty);
            //result = (string)(load == null ? string.Empty : load);

            return result;
        }

        public async Task<string> GetMachineFromName(string machineName)
        {
            string result = "";
            object load = await GetValueMachines("name", machineName, "id");

            result = (string)(load ?? string.Empty);
            //result = (string)(load == null ? string.Empty : load);

            return result;
        }

        public async Task<int> GetMachineIDFromName(string machineName)
        {
            int result = -1;
            object load = await GetValueMachines("name", machineName, "id");

            result = Convert.ToInt32(load ?? -1);
            //result = (string)(load == null ? string.Empty : load);

            return result;
        }

        public async Task<string> GetCategoryMachine(string machine)
        {
            string result = "";
            object load = await GetValueMachines("id", machine, "category");

            result = (string)(load ?? string.Empty);
            //result = (string)(load == null ? string.Empty : load);

            return result;
        }

        public async Task<string> GetIDEquipMachine(string machine)
        {
            string result = "";
            object load = await GetValueMachines("id", machine, "idEquip");

            result = (string)(load ?? string.Empty);
            //result = (string)(load == null ? string.Empty : load);

            return result;
        }

        public int GetIDEquipMachine(int machine)
        {
            return Convert.ToInt32(GetValueMachines("id", machine.ToString(), "idEquip"));
        }

        public async Task<string> GetMachineStartWork(string machine)
        {
            string result = "";
            object load = await GetValueMachines("id", machine, "dateStartWork");

            result = (string)(load ?? string.Empty);
            //result = (string)(load == null ? string.Empty : load);

            return result;
        }

        public async Task<string> GetMachineNote(string machine)
        {
            string result = "";
            object load = await GetValueMachines("id", machine, "note");

            result = (string)(load ?? string.Empty);
            //result = (string)(load == null ? string.Empty : load);

            return result;
        }

        public string GetIDUser(string machine)
        {
            return GetValue("machine", machine, "nameOfExecutor");
        }

        /*public String GetActiveOrder(String machine)
        {
            return GetValue("machine", machine, "activeOrder");
        }*/

        public bool GetActiveOrder(string machine)
        {
            bool result = false;

            string value = GetValue("machine", machine, "activeOrder");

            if (value != "")
            {
                result = Convert.ToBoolean(value);
            }

            return result; 
        }

        public string GetCurrentOrderID(string machine)
        {
            string result = "-1";
            string val = GetValue("machine", machine, "currentOrderID");

            if (val != "")
            {
                result = val;
            }

            return result;
        }

        public string GetCurrentCounterRepeat(String machine)
        {
            return GetValue("machine", machine, "currentCounterRepeat");
        }

        public string GetLastOrderID(string machine)
        {
            string result = "-1";
            string val = GetValue("machine", machine, "lastOrderID");

            if (val != "")
            {
                result = val;
            }

            return result;
        }

        public string GetMachineFromOrderID(int orderIndex)
        {
            return GetValue("currentOrderID", orderIndex.ToString(), "machine");
        }

        /// <summary>
        /// Получить список всего оборудования
        /// </summary>
        /// <returns>List содержащий все оборудование</returns>
        public List<string> GetMachinesList()
        {
            List<string> machinesList = new List<string>(GetAllMachines(""));

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

        public List<int> GetMachinesList(int category)
        {
            List<int> machinesList = GetAllMachines(category);

            return machinesList;
        }

        private List<String> GetAllMachines(String category)
        {
            List<String> machinesList = new List<String>();

            String commLine = "";

            if (category != "")
                commLine = " WHERE category = '" + category + "'";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machines" + commLine
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    machinesList.Add(sqlReader["id"].ToString());
                }

                Connect.Close();
            }

            return machinesList;
        }

        private List<int> GetAllMachines(int category)
        {
            List<int> machinesList = new List<int>();

            string commLine = "";

            if (category != -1)
                commLine = " WHERE category = '" + category + "'";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machines" + commLine
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    machinesList.Add((int)sqlReader["id"]);
                }

                Connect.Close();
            }

            return machinesList;
        }

        private String GetValueMachinesOLD(String findColomnName, String findParameter, String valueColomn)
        {
            String result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machines WHERE " + findColomnName + " = '" + findParameter + "'"
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

        private async Task<object> GetValueMachines(string findColomnName, string findParameter, string valueColomn)
        {
            object result = null;
            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        using (MySqlConnection Connect = DBConnection.GetDBConnection())
                        {
                            await Connect.OpenAsync();
                            MySqlCommand Command = new MySqlCommand
                            {
                                Connection = Connect,
                                CommandText = @"SELECT * FROM machines WHERE " + findColomnName + " = '" + findParameter + "'"
                            };
                            DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                            while (await sqlReader.ReadAsync())
                            {
                                result = sqlReader[valueColomn].ToString();
                            }

                            await Connect.CloseAsync();
                        }

                        reconnectionRequired = false;
                    }
                    catch (Exception ex)
                    {
                        LogException.WriteLine("GetValueMachinesAS: " + ex.Message);
                        dialog = DataBaseReconnectionRequest(ex.Message);

                        if (dialog == DialogResult.Retry)
                        {
                            reconnectionRequired = true;
                        }
                        if (dialog == DialogResult.Abort || dialog == DialogResult.Cancel)
                        {
                            reconnectionRequired = false;
                            Application.Exit();
                        }

                        throw new ApplicationException(ex.Message);
                    }
                }
            }
            while (reconnectionRequired);

            return result;
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
                    CommandText = @"SELECT * FROM machinesInfo WHERE " + findColomnName + " = '" + findParameter + "'"
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

        /// <summary>
        /// Список активного оборудования для выпранного пользователя
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        /// Для проверки обработки исключения 
        public Object GetMachinesS(String userID)
        {
            List<String> result = new List<String>();
            result.Clear();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machinesInfo WHERE nameOfExecutor = '" + userID + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result.Add(sqlReader["machine"].ToString());
                }

                Connect.Close();
            }
            return result;
        }

        public async Task<List<string>> GetMachines(string userID)
        {
            List<string> result = new List<string>();
            //result.Clear();
            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        using (MySqlConnection Connect = DBConnection.GetDBConnection())
                        {
                            await Connect.OpenAsync();
                            MySqlCommand Command = new MySqlCommand
                            {
                                Connection = Connect,
                                CommandText = @"SELECT * FROM machinesInfo WHERE nameOfExecutor = '" + userID + "'"
                            };
                            DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                            while (await sqlReader.ReadAsync())
                            {
                                result.Add(sqlReader["machine"].ToString());
                                //result.Add(sqlReader["machine"] == DBNull.Value ? string.Empty : (string)sqlReader["machine"]);
                            }

                            await Connect.CloseAsync();
                        }

                        reconnectionRequired = false;
                    }
                    catch (Exception ex)
                    {
                        LogException.WriteLine("GetMachines: " + ex.Message);

                        dialog = DataBaseReconnectionRequest(ex.Message);

                        if (dialog == DialogResult.Retry)
                        {
                            reconnectionRequired = true;
                        }
                        if (dialog == DialogResult.Abort || dialog == DialogResult.Cancel)
                        {
                            reconnectionRequired = false;
                            Application.Exit();
                        }
                    }
                }
            }
            while (reconnectionRequired);

            return result;
        }

        public async Task<string> GetMachinesStr(string userID)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            List<string> orderMachines = await GetMachines(userID);
            string machines = "";

            for (int i = 0; i < orderMachines.Count; i++)
            {
                machines += await GetMachineName(orderMachines[i]);

                if (i != orderMachines.Count - 1)
                    machines += ", ";
                else
                    machines += ".";
            }

            return machines;
        }

        public async Task<bool> GetMachinesForUserActive(string userID)
        {
            List<string> orderMachines = await GetMachines(userID);

            bool machinesActive = false;
            int counter = 0;

            for (int i = 0; i < orderMachines.Count; i++)
            {
                if (GetActiveOrder(orderMachines[i]))
                {
                    counter++;
                }
            }

            if (counter > 0)
            {
                machinesActive = true;
            }

            return machinesActive;
        }

        private String GetValueTwoParam(String findColomnName, String findParameter, String findColomnName2, String findParameter2, String valueColomn)
        {
            String result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM machinesInfo WHERE " + findColomnName + " = '" + findParameter + "' AND " + findColomnName2 + " = '" + findParameter2 + "'"
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

        public void UpdateCurrentOrder(String machine, int orderIndex)
        {
            UpdateInfoParameter(machine, "currentOrderID", orderIndex.ToString());
        }

        public async void CompleteTheShift(string nameOfExecutor)
        {

            ValueInfoBase getMachine = new ValueInfoBase();

            List<String> machines = await getMachine.GetMachines(nameOfExecutor);

            foreach (string machine in machines)
            {
                CompleteTheShiftFromMachines(machine);
            }
        }

        private void CompleteTheShiftFromMachines(String selectMachine)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE machinesInfo SET nameOfExecutor = '', currentCounterRepeat = '', " +
                    "currentOrderID = '', activeOrder = 'False' " + // проверить актив ордер
                    "WHERE (machine = @machine)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machine", selectMachine);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void UpdateInfo(string machine, int currentCounterRepeat, int currentOrderID, int lastOrderID, bool activeOrder)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE machinesInfo SET " +
                    "currentCounterRepeat = @currentCounterRepeat, currentOrderID = @currentOrderID, lastOrderID = @lastOrderID, activeOrder = @activeOrder " +
                    "WHERE (machine = @machine)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@currentOrderID", currentOrderID);
                Command.Parameters.AddWithValue("@lastOrderID", lastOrderID);
                Command.Parameters.AddWithValue("@currentCounterRepeat", currentCounterRepeat);
                Command.Parameters.AddWithValue("@activeOrder", activeOrder.ToString());

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

        }

        private void UpdateInfoParameter(String machine, String parameter, String value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE machinesInfo SET " + parameter + " = @value " +
                    "WHERE (machine = @machine)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@value", value);
                Command.Parameters.AddWithValue("@machine", machine);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

        }

        public void DeleteMachine(String id)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM machines WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM machinesInfo WHERE machine = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

    }
}
