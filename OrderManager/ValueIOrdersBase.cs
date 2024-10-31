using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;

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
            String result = "";
            String status = GetValueFromIndex(index, "statusOfOrder");

            if (status == "0")
                result = "Заказ не выполняется";
            if (status == "1")
                result = "Выполняется приладка";
            if (status == "2")
                result = "Приладка завершена";
            if (status == "3")
                result = "Заказ в работе";
            if (status == "4")
                result = "Заказ завершен";

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

        private String GetValue(String machine, String numberOfOrder, String modificationOfOrder, String nameOfColomn)
        {
            String result = "0";

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
