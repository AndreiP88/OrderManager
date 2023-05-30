using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    internal class ValueInfoBase
    {
        public ValueInfoBase()
        {

        }

        public String GetMachineName(String machine)
        {
            return GetValueMachines("id", machine, "name");
        }

        public string GetMachineFromName(string machineName)
        {
            return GetValueMachines("name", machineName, "id");
        }

        public String GetCategoryMachine(String machine)
        {
            return GetValueMachines("id", machine, "category");
        }

        public String GetIDEquipMachine(String machine)
        {
            return GetValueMachines("id", machine, "idEquip");
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
        private String GetValueMachines(String findColomnName, String findParameter, String valueColomn)
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
        public Object GetMachines(String userID)
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

        public String GetMachinesStr(String userID)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

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
            int counter = 0;

            for (int i = 0; i < orderMachines.Count; i++)
            {
                string activeOrderValue = GetActiveOrder(orderMachines[i]);

                if (activeOrderValue != "")
                {
                    if (Convert.ToBoolean(activeOrderValue) == true)
                    {
                        counter++;
                    }
                    //machinesActive = Convert.ToBoolean(activeOrderValue);
                }
                /*if (Convert.ToBoolean(GetActiveOrder(orderMachines[i])) == true)
                    machinesActive = true;*/
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

        public void CompleteTheShift(String nameOfExecutor)
        {

            ValueInfoBase getMachine = new ValueInfoBase();

            List<String> machines = (List<String>)getMachine.GetMachines(nameOfExecutor);

            foreach (String machine in machines)
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
