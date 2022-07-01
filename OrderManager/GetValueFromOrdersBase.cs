using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetValueFromOrdersBase
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dBase"></param>
        public GetValueFromOrdersBase(String dBase)
        {
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }


        public String GetOrderCount(String currentMachine, String orderNumber, String orderModification)
        {
            return GetValue(currentMachine, orderNumber, orderModification, "count");
        }

        public String GetOrderStatus(String currentMachine, String orderNumber, String orderModification)
        {
            return GetValue(currentMachine, orderNumber, orderModification, "statusOfOrder");
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

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
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

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE machine = @machine AND (numberOfOrder = @number AND modification = @orderModification)"
                };
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@number", numberOfOrder);
                Command.Parameters.AddWithValue("@orderModification", modificationOfOrder);
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[nameOfColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }
    }
}
