using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class DBConnection
    {
        public static MySqlConnection GetDBConnection()
        {
            /*string host = "25.21.38.172";
            int port = 3309;
            string database = "order_manager";
            string username = "oxyfox";
            string password = "root";*/

            string host = Form1.BaseConnectionParameters.host;
            int port = Form1.BaseConnectionParameters.port;
            string database = Form1.BaseConnectionParameters.database;
            string username = Form1.BaseConnectionParameters.username;
            string password = Form1.BaseConnectionParameters.password;

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }
    }
}
