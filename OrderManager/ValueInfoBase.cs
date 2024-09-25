using libData;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

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

        public async Task<int> GetIDEquipMachine(int machine)
        {
            int result = -1;
            object load = await GetValueMachines("id", machine.ToString(), "idEquip");

            result = Convert.ToInt32(load ?? -1);
            //result = (string)(load == null ? string.Empty : load);

            return result;
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
        public int GetCurrentTypeJob(string machine)
        {
            int result = -1;
            string val = GetValue("machine", machine, "currentTypeJob");

            if (val != "")
            {
                result = Convert.ToInt32(val);
            }

            return result;
        }

        public int GetCurrentOrderID(string machine)
        {
            int result = -1;
            string val = GetValue("machine", machine, "currentOrderID");

            if (val != "")
            {
                result = Convert.ToInt32(val);
            }

            return result;
        }

        public string GetCurrentCounterRepeat(String machine)
        {
            return GetValue("machine", machine, "currentCounterRepeat");
        }

        public int GetLastOrderID(string machine)
        {
            int result = -1;
            string val = GetValue("machine", machine, "lastOrderID");

            if (val != "")
            {
                result = Convert.ToInt32(val);
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

        public List<int> GetEquipsASBaseList(int category)
        {
            List<int> machinesList = GetAllEquipsASBase(category);

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

        public List<Equip> GetEquipsList(int category = -1, int typeBaseLoad = 0)
        {
            List<Equip> equipList = new List<Equip>();

            string loadIndex;

            if (typeBaseLoad == 0)
            {
                loadIndex = "id";
            }
            else
            {
                loadIndex = "idEquip";
            }

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
                    equipList.Add(new Equip((int)sqlReader[loadIndex],
                        sqlReader["name"].ToString()));
                }

                Connect.Close();
            }

            return equipList;
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

        private List<int> GetAllEquipsASBase(int category)
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
                    machinesList.Add((int)sqlReader["idEquip"]);
                }

                Connect.Close();
            }

            return machinesList;
        }

        private async Task<object> GetValueMachines(string findColomnName, string findParameter, string valueColomn)
        {
            object result = null;

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

                return result;
            }
            catch (SqlException sqlEx)
            {
                LogException.WriteLine("GetValueMachines: " + string.Format("MySQL #{0}: {1}\n{3}", sqlEx.Number, sqlEx.Message, sqlEx.StackTrace));
                throw new ApplicationException(string.Format("MySQL #{0}: {1}", sqlEx.Number, sqlEx.Message));
            }
            catch (Exception ex)
            {
                LogException.WriteLine(string.Format("Error #{0}: {1}\n{3}", ex.Source, ex.Message, ex.StackTrace));
                throw new ApplicationException(ex.Message);
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

                return result;
            }
            catch (SqlException sqlEx)
            {
                LogException.WriteLine("GetMachines: " + string.Format("MySQL #{0}: {1}", sqlEx.Number, sqlEx.Message));
                throw new ApplicationException(string.Format("MySQL #{0}: {1}", sqlEx.Number, sqlEx.Message));
            }
            catch (Exception ex)
            {
                LogException.WriteLine("GetMachines: " + ex.Message);
                throw new ApplicationException(ex.Message);
            }
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

        public void UpdateInfo(string machine, int currentTypeJob, int currentCounterRepeat, int currentOrderID, int lastOrderID, bool activeOrder)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE machinesInfo SET " +
                    "currentTypeJob = @currentTypeJob, currentCounterRepeat = @currentCounterRepeat, currentOrderID = @currentOrderID, lastOrderID = @lastOrderID, activeOrder = @activeOrder " +
                    "WHERE (machine = @machine)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@currentOrderID", currentOrderID);
                Command.Parameters.AddWithValue("@lastOrderID", lastOrderID);
                Command.Parameters.AddWithValue("@currentCounterRepeat", currentCounterRepeat);
                Command.Parameters.AddWithValue("@currentTypeJob", currentTypeJob);
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
