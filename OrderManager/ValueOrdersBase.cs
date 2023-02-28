using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

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

        public String GetOrderCount(String currentMachine, String orderNumber, String orderModification)
        {
            return GetValue(currentMachine, orderNumber, orderModification, "count");
        }

        public String GetOrderStatus(String currentMachine, String orderNumber, String orderModification)
        {
            return GetValue(currentMachine, orderNumber, orderModification, "statusOfOrder");
        }

        /// <summary>
        /// Получить статус заказа по индексу
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public String GetOrderStatus(int index)
        {
            return GetValueFromIndex(index, "statusOfOrder");
        }

        public String GetOrderName(String currentMachine, String orderNumber, String orderModification)
        {
            return GetValue(currentMachine, orderNumber, orderModification, "nameOfOrder");
        }

        public String GetCounterRepeat(String currentMachine, String orderNumber, String orderModification)
        {
            return GetValue(currentMachine, orderNumber, orderModification, "counterRepeat");
        }

        public String GetAmountOfOrder(String currentMachine, String orderNumber, String orderModification)
        {
            return GetValue(currentMachine, orderNumber, orderModification, "amountOfOrder");
        }

        public String GetTimeMakeready(String currentMachine, String orderNumber, String orderModification)
        {
            return GetValue(currentMachine, orderNumber, orderModification, "timeMakeready");
        }

        public String GetTimeToWork(String currentMachine, String orderNumber, String orderModification)
        {
            return GetValue(currentMachine, orderNumber, orderModification, "timeToWork");
        }

        public void SetNewStatus(String currentMachine, String orderNumber, String orderModification, String newStatus)
        {
            SetValue(currentMachine, orderNumber, orderModification, "statusOfOrder", newStatus);
        }

        public void SetNewCounterRepeat(String currentMachine, String orderNumber, String orderModification, String newCounterRepaeat)
        {
            SetValue(currentMachine, orderNumber, orderModification, "counterRepeat", newCounterRepaeat);
        }

        public void IncrementCounterRepeat(String currentMachine, String orderNumber, String orderModification)
        {
            int newCounterRep = 1;

            newCounterRep += Convert.ToInt32(GetCounterRepeat(currentMachine, orderNumber, orderModification));

            SetValue(currentMachine, orderNumber, orderModification, "counterRepeat", newCounterRep.ToString());
        }

        public String GetOrderStatusName(String currentMachine, String orderNumber, String orderModification)
        {
            String result = "";
            String status = GetValue(currentMachine, orderNumber, orderModification, "statusOfOrder");

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
            String result = "0";

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

        private void SetValue(String orderMachine, String numberOfOrder, String orderModification, String key, String value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE orders SET " + key + " = @value " +
                    "WHERE machine = @orderMachine AND (numberOfOrder = @number AND modification = @orderModification)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@orderMachine", orderMachine);
                Command.Parameters.AddWithValue("@number", numberOfOrder);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
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
