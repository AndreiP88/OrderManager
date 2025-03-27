using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;

namespace OrderManager
{
    internal class ValueOrdersBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dBase"></param>
        public ValueOrdersBase()
        {

        }

        /// <summary>
        /// Получить индекс заказа в базе по номеру, модификации и оборудованию
        /// </summary>
        /// <param name="currentMachine"></param>
        /// <param name="orderNumber"></param>
        /// <param name="orderModification"></param>
        /// <returns></returns>
        public int GetOrderID(string currentMachine, string orderNumber, string orderModification)
        {
            return Convert.ToInt32(GetValue(currentMachine, orderNumber, orderModification, "count"));
        }

        /// <summary>
        /// Получить статус заказа по индексу
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetOrderStatus(int index)
        {
            string result = "0";
            string value = GetValueFromIndex(index, "statusOfOrder");

            if (value != "-1")
            {
                result = value;
            }

            return result;
        }

        public int GetMachineIDForOrder(int index)
        {
            return Convert.ToInt32(GetValueFromIndex(index, "machine"));
        }

        public string GetOrderName(int index)
        {
            return GetValueFromIndex(index, "nameOfOrder");
        }

        public string GetOrderNumber(int index)
        {
            return GetValueFromIndex(index, "numberOfOrder");
        }

        public string GetOrderModification(int index)
        {
            return GetValueFromIndex(index, "modification");
        }

        public int GetCounterRepeat(int index)
        {
            return Convert.ToInt32(GetValueFromIndex(index, "counterRepeat"));
        }

        public int GetMakereadyType(int index)
        {
            return Convert.ToInt32(GetValueFromIndex(index, "makereadyType"));
        }

        public string GetAmountOfOrder(int index)
        {
            return GetValueFromIndex(index, "amountOfOrder");
        }

        public string GetTimeMakeready(int index)
        {
            return GetValueFromIndex(index, "timeMakeready");
        }

        public string GetTimeToWork(int index)
        {
            return GetValueFromIndex(index, "timeToWork");
        }

        public void SetNewStatus(int index, string newStatus)
        {
            SetValue(index, "statusOfOrder", newStatus);
        }

        public void SetNewCounterRepeat(int index, string newCounterRepaeat)
        {
            SetValue(index, "counterRepeat", newCounterRepaeat);
        }

        public void IncrementCounterRepeat(int index)
        {
            int newCounterRep = 1;

            newCounterRep += Convert.ToInt32(GetCounterRepeat(index));

            SetValue(index, "counterRepeat", newCounterRep.ToString());
        }

        public string GetOrderStatusName(int index)
        {
            string status = GetValueFromIndex(index, "statusOfOrder");

            string result = GetOrderStatusNameFromStatusIndex(Convert.ToInt32(status));

            return result;
        }

        public string GetOrderStatusNameFromStatusIndex(int status)
        {
            string result;

            switch (status)
            {
                case 0:
                    result = "Заказ не выполняется";
                    break;
                case 1:
                    result = "Выполняется приладка";
                    break;
                case 2:
                    result = "Приладка завершена";
                    break;
                case 3:
                    result = "Заказ в работе";
                    break;
                case 4:
                    result = "Заказ завершен";
                    break;
                default:
                    result = "";
                    break;
            }

            return result;
        }

        public int GetCountOrders()
        {
            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(DISTINCT numberOfOrder) as count FROM orders"

                };

                result = Convert.ToInt32(Command.ExecuteScalar());

                Connect.Close();
            }

            return result;
        }

        private string GetValue(string machine, string numberOfOrder, string modificationOfOrder, string nameOfColomn)
        {
            string result = "-1";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE machine = @machine AND (numberOfOrder = @number AND modification = @orderModification)"
                };
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@number", numberOfOrder);
                Command.Parameters.AddWithValue("@orderModification", modificationOfOrder);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[nameOfColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        private String GetValueFromIndex(int index, String nameOfColomn)
        {
            String result = "-1";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE count = @count"
                };
                Command.Parameters.AddWithValue("@count", index);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[nameOfColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        private void SetValue(int index, string key, string value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE orders SET " + key + " = @value " +
                    "WHERE count = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", index);
                Command.Parameters.AddWithValue("@value", value);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public async Task<int> AddOrderToDB(int machine, string number, string name, string modification, int amount, int timeMakeready, int timeWork, string stamp, List<string> items, int status = 0, int counterRepeat = 0, int mkType = 1)
        {
            int addedRowsCount = -1;
            int orderID = -1;

            string orderAddedDate = DateTime.Now.ToString();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                await Connect.OpenAsync();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    /*CommandText = @"SELECT count AS previewOrderID FROM orders WHERE numberOfOrder = @number AND modification = @modification AND machine = @machine;
                                    INSERT INTO orders (orderAddedDate, machine, numberOfOrder, nameOfOrder, modification, amountOfOrder, timeMakeready, timeToWork, orderStamp, statusOfOrder, counterRepeat, makereadyType) 
                                        SELECT @oddedDate, @machine, @number, @nameOfOrder, @modification, @amount, @timeM, @timeW, @stamp, @status, @counterR, @makereadyType 
                                    WHERE 
                                        NOT EXISTS (SELECT numberOfOrder, modification, machine FROM orders WHERE numberOfOrder = @number AND modification = @modification AND machine = @machine) LIMIT 1;  
                                    SELECT count AS orderID FROM orders WHERE numberOfOrder = @number AND modification = @modification AND machine = @machine; 
                                    SELECT ROW_COUNT() AS addedRows"*/
                    CommandText = @"INSERT INTO orders (orderAddedDate, machine, numberOfOrder, nameOfOrder, modification, amountOfOrder, timeMakeready, timeToWork, orderStamp, statusOfOrder, counterRepeat, makereadyType) 
                                        SELECT @oddedDate, @machine, @number, @nameOfOrder, @modification, @amount, @timeM, @timeW, @stamp, @status, @counterR, @makereadyType 
                                    WHERE 
                                        NOT EXISTS (SELECT numberOfOrder, modification, machine FROM orders WHERE numberOfOrder = @number AND modification = @modification AND machine = @machine) LIMIT 1;  
                                    SELECT 
                                    (SELECT count FROM orders WHERE numberOfOrder = @number AND modification = @modification AND machine = @machine) AS orderID, 
                                    (SELECT ROW_COUNT()) AS addedRows"
                };
                Command.Parameters.AddWithValue("@oddedDate", orderAddedDate); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@number", number);
                Command.Parameters.AddWithValue("@nameOfOrder", name);
                Command.Parameters.AddWithValue("@modification", modification);
                Command.Parameters.AddWithValue("@amount", amount);
                Command.Parameters.AddWithValue("@timeM", timeMakeready);
                Command.Parameters.AddWithValue("@timeW", timeWork);
                Command.Parameters.AddWithValue("@stamp", stamp);
                Command.Parameters.AddWithValue("@status", status);
                Command.Parameters.AddWithValue("@counterR", counterRepeat);
                Command.Parameters.AddWithValue("@makereadyType", mkType);

                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
                {
                    addedRowsCount = sqlReader["addedRows"] == DBNull.Value ? -1 : Convert.ToInt32(sqlReader["addedRows"]);
                    orderID = sqlReader["orderID"] == DBNull.Value ? -1 : Convert.ToInt32(sqlReader["orderID"]);
                }

                Connect.Close();
            }

            if (addedRowsCount > 0)
            {
                for (int i = 0; i < items.Count; i += 2)
                {
                    using (MySqlConnection Connect = DBConnection.GetDBConnection())
                    {
                        string commandText = "INSERT INTO typesList (orderID, name, count) " +
                        "VALUES (@orderID, @name, @count)";

                        MySqlCommand Command = new MySqlCommand(commandText, Connect);
                        Command.Parameters.AddWithValue("@orderID", orderID);
                        Command.Parameters.AddWithValue("@name", items[i]);
                        Command.Parameters.AddWithValue("@count", items[i + 1]);

                        Connect.Open();
                        Command.ExecuteNonQuery();
                        Connect.Close();
                    }
                }
            }

            return orderID;
        }

        public async Task<int> AddNewOrderInProgressAsync(int machine, int executor, int typeJob, int shiftID, int orderIndex, string makereadyStart,
            string makereadyStop, string workStart, string workStop, int makereadyConsider, int makereadyPartComplete, int done, int counterRepeat, string note)
        {
            int orderInProgressID = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                await Connect.OpenAsync();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"INSERT INTO ordersInProgress (machine, executor, typeJob, shiftID, orderID, timeMakereadyStart, timeMakereadyStop, timeToWorkStart, timeToWorkStop, makereadyConsider, makereadyComplete, done, counterRepeat, note) 
                                    SELECT @machine, @executor, @typeJob, @shiftID, @orderID, @makereadyStart, @makereadyStop, @workStart, @workStop, @makereadyConsider, @makereadyComplete, @done, @counterRepeat, @note 
                                    WHERE NOT EXISTS (SELECT shiftID, orderID, counterRepeat FROM ordersInProgress WHERE shiftID = @shiftID AND orderID = @orderID AND counterRepeat = @counterRepeat) LIMIT 1; 
                                    SELECT count FROM ordersInProgress  
                                    WHERE  
                                        shiftID = @shiftID AND orderID = @orderID AND counterRepeat = @counterRepeat"
                };
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@executor", executor);
                Command.Parameters.AddWithValue("@typeJob", typeJob);
                Command.Parameters.AddWithValue("@shiftID", shiftID);
                Command.Parameters.AddWithValue("@orderID", orderIndex);
                Command.Parameters.AddWithValue("@makereadyStart", makereadyStart);
                Command.Parameters.AddWithValue("@makereadyStop", makereadyStop);
                Command.Parameters.AddWithValue("@workStart", workStart);
                Command.Parameters.AddWithValue("@workStop", workStop);
                Command.Parameters.AddWithValue("@makereadyConsider", makereadyConsider);
                Command.Parameters.AddWithValue("@makereadyComplete", makereadyPartComplete);
                Command.Parameters.AddWithValue("@done", done);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@note", note);

                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
                {
                    orderInProgressID = sqlReader["count"] == DBNull.Value ? -1 : Convert.ToInt32(sqlReader["count"]);
                }

                Connect.Close();
            }

            return orderInProgressID;
        }

        public void UpdateData(string nameOfColomn, int machineCurrent, int shiftID, int orderIndex, int counterRepeat, object value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET " + nameOfColomn + " = @value " +
                    "WHERE ((machine = @machineCurrent AND shiftID = @shiftID) AND (orderID = @id AND counterRepeat = @counterRepeat))";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineCurrent", machineCurrent); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@shiftID", shiftID);
                Command.Parameters.AddWithValue("@id", orderIndex);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private List<string> GetValueFromStampNumber(String machine, String orderStamp, String nameOfColomn)
        {
            List<string> result = new List<string>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE machine = @machine AND orderStamp = @orderStamp"
                };
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@orderStamp", orderStamp);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result.Add(sqlReader[nameOfColomn].ToString());
                }

                Connect.Close();
            }

            return result;
        }
    }
}
