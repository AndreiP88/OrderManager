using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;
using System.Windows.Forms;

namespace OrderManager
{
    internal class ValueTypesBase
    {
        int shiftID;
        int orderID;
        int orderCounterRepeat;
        string machine;
        string user;
        string _orderInProgressID;

        public ValueTypesBase(int lShiftID, int lOrderID, int lOrderCounterRepeat, String lMachine, String lUser)
        {
            this.shiftID = lShiftID;
            this.orderID = lOrderID;
            this.orderCounterRepeat = lOrderCounterRepeat;
            this.machine = lMachine;
            this.user = lUser;

            GetOrdersFromBase getOrders = new GetOrdersFromBase();

            _orderInProgressID = getOrders.GetIndex(shiftID, orderID, orderCounterRepeat, machine);
        }

        public ValueTypesBase()
        {
            this.shiftID = -1;
            this.orderID = -1;
            this.orderCounterRepeat = 0;
            this.machine = "";
            this.user = "";

            _orderInProgressID = "";
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

        public string GetTypeListIdFromIndexFromTypeListInTheOrder(string id)
        {
            string result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM typesInTheOrder WHERE id = @id"

                };
                Command.Parameters.AddWithValue("@id", id);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader["typeListID"].ToString();
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

        public void InsertItem(TypeInTheOrder value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "INSERT INTO typesList (orderId, name, count) " +
                    "VALUES (@orderId, @name, @count)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@orderId", value.indexTypeList);
                Command.Parameters.AddWithValue("@name", value.name);
                Command.Parameters.AddWithValue("@count", value.count);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void UpdateItem(TypeInTheOrder value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE typesList SET name = @name, count = @count " +
                    "WHERE (id = @id)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", value.indexTypeList);
                Command.Parameters.AddWithValue("@name", value.name);
                Command.Parameters.AddWithValue("@count", value.count);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void DeleteItem(string id)
        {
            //string indexTypeInTheOrder = GetTypeListIdFromIndexFromTypeListInTheOrder(id);
            //MessageBox.Show(indexTypeInTheOrder);

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM typesInTheOrder WHERE typeListID = @typeListID";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@typeListID", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM typesList WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }
    }
}
