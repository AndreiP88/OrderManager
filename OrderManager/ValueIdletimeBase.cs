using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace OrderManager
{
    internal class ValueIdletimeBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dBase"></param>
        public ValueIdletimeBase()
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
        /// <summary>
        /// Название простоя по индексу
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetIdletimeName(int index)
        {
            return GetValueFromIndex(index, "name");
        }

        

        public void SetNewCounterRepeat(int index, string newCounterRepaeat)
        {
            SetValue(index, "counterRepeat", newCounterRepaeat);
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
            string result = "-1";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM viewidletime WHERE id = @id"
                };
                Command.Parameters.AddWithValue("@id", index);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[nameOfColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        private String GetValueFromIndexIdletimeList(int index, String nameOfColomn)
        {
            string result = "-1";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM viewidletime WHERE idletimelistID = @id"
                };
                Command.Parameters.AddWithValue("@id", index);
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
