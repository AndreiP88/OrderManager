using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OrderManager.Form1;

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

        public static MySqlConnection GetDBConnection(string host, int port, string database, string username, string password)
        {
            /*string host = "25.21.38.172";
            int port = 3309;
            string database = "order_manager";
            string username = "oxyfox";
            string password = "root";*/

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }

        public bool IsServerConnected(string host, int port, string database, string username, string password)
        {
            using (MySqlConnection Connect = GetDBConnection(host, port, database, username, password))
            {
                try
                {
                    Connect.Open();
                    Connect.Close();
                    return true;
                }
                catch
                {
                    return false;
                }

            }
        }

        public static SqlConnection GetSQLServerConnection()
        {
            /*string host = "25.21.38.172";
            int port = 3309;
            string database = "order_manager";
            string username = "oxyfox";
            string password = "root";*/

            string host = "SRV-ACS\\DSACS";
            string database = "asystem";
            string username = "ds";
            string password = "1";

            return DBMySQLUtils.GetSQLServerConnection(host, database, username, password);
        }

        public void SetDBParameter()
        {
            IniFile ini = new IniFile(Form1.connectionFile);

            String section = ini.ReadString("selected", "general");

            String host = "host";
            String port = "port";
            String database = "database";
            String username = "username";
            String password = "password";

            Form1.BaseConnectionParameters.host = ini.ReadString(host, section);
            Form1.BaseConnectionParameters.port = ini.ReadInt(port, section);
            Form1.BaseConnectionParameters.database = ini.ReadString(database, section);
            Form1.BaseConnectionParameters.username = ini.ReadString(username, section);
            Form1.BaseConnectionParameters.password = ini.ReadString(password, section);
        }
    }
}
