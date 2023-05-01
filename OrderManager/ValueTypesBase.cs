using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;

namespace OrderManager
{
    internal class ValueTypesBase
    {
        String startOfShift;
        String orderNumber;
        String orderModification;
        String orderCounterRepeat;
        String machine;
        String user;
        String _orderInProgressID;

        public ValueTypesBase(String lStartOfShift, String lOrderNumber, String lOrderModification, String lOrderCounterRepeat, String lMachine, String lUser)
        {
            this.startOfShift = lStartOfShift;
            this.orderNumber = lOrderNumber;
            this.orderModification = lOrderModification;
            this.orderCounterRepeat = lOrderCounterRepeat;
            this.machine = lMachine;
            this.user = lUser;

            GetOrdersFromBase getOrders = new GetOrdersFromBase();

            _orderInProgressID = getOrders.GetIndex(startOfShift, orderNumber, orderModification, orderCounterRepeat, machine);
        }

        public string GetNameItemFromID(int id)
        {
            string result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM typeslist WHERE id = @id"

                };
                Command.Parameters.AddWithValue("@id", id);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader["name"].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        public List<TypeInTheOrder> GetData()
        {
            List<TypeInTheOrder> result = new List<TypeInTheOrder>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM typesInTheOrder WHERE orderInProgressID = @orderInProgressID"

                };
                Command.Parameters.AddWithValue("@orderInProgressID", _orderInProgressID);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result.Add(new TypeInTheOrder(
                        (int)sqlReader["id"],
                        (int)sqlReader["typeListID"],
                        (int)sqlReader["done"]));
                }

                Connect.Close();
            }

            return result;
        }

        public void InsertData(TypeInTheOrder value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                /*string commandText = "INSERT INTO orders (orderAddedDate, machine, numberOfOrder, nameOfOrder, modification, amountOfOrder, timeMakeready, timeToWork, orderStamp, statusOfOrder, counterRepeat) " +
                    "SELECT * FROM (SELECT @orderAddedDate, @machine, @number, @name, @modification, @amount, @timeM, @timeW, @stamp, @status, @counterR) " +
                    "AS tmp WHERE NOT EXISTS(SELECT numberOfOrder FROM orders WHERE (numberOfOrder = @number AND modification = @modification) AND machine = @machine) LIMIT 1";*/

                string commandText = "INSERT INTO typesInTheOrder (orderInProgressID, typeListID, done) " +
                    "VALUES (@orderInProgressID, @typeListID, @done)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@orderInProgressID", _orderInProgressID);
                Command.Parameters.AddWithValue("@typeListID", value.indexTypeList);
                Command.Parameters.AddWithValue("@done", value.done);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void UpdateData(TypeInTheOrder value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE typesInTheOrder SET typeListID = @typeListID, done = @done " +
                    "WHERE (id = @id)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", value.id);
                Command.Parameters.AddWithValue("@typeListID", value.indexTypeList);
                Command.Parameters.AddWithValue("@done", value.done);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void DeleteTypes(string id)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM typesInTheOrder WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }
    }
}
