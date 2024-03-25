using MySql.Data.MySqlClient;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            
            //_pauseEvent.WaitOne();

            string host = Form1.BaseConnectionParameters.host;
            int port = Form1.BaseConnectionParameters.port;
            string database = Form1.BaseConnectionParameters.database;
            string username = Form1.BaseConnectionParameters.username;
            string password = Form1.BaseConnectionParameters.password;

            MySqlConnection connect = null;

            bool reconnectionRequired = false;
            int reconnectCount = 0;
            int reconnectLimit = 5;

            do
            {
                try
                {
                    connect = DBMySQLUtils.GetDBConnection(host, port, database, username, password);

                    reconnectionRequired = false;
                }
                catch (Exception ex)
                {
                    reconectionCount++;

                    LogException.WriteLine("GetDBConnection " + reconnectCount + " of " + reconnectLimit + "\n" + ex.Message + "; " + ex.StackTrace);

                    if (reconectionCount <= reconnectLimit)
                    {
                        reconnectionRequired = true;
                        Thread.Sleep(500);
                    }
                    else
                    {
                        //Application.Exit();
                    } 
                }
            }
            while (reconnectionRequired);

            return connect;
        }

        public bool IsServerConnected(string host, int port, string database, string username, string password)
        {
            using (MySqlConnection Connect = DBMySQLUtils.GetDBConnection(host, port, database, username, password))
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

        public static async Task<bool> IsServerConnectedAsync(string host, int port, string database, string username, string password)
        {
            using (MySqlConnection Connect = DBMySQLUtils.GetDBConnection(host, port, database, username, password))

                try
                {
                    await Connect.OpenAsync();
                    return true;
                }
                catch
                {
                    return false;
                }
        }

        private static bool DataBaseReconnectionRequest(string exception)
        {
            bool result = false;

            _pauseEvent.Reset();

            //if (!_viewDatabaseRequestForm)
            {
                FormDataBaseReconnect form = new FormDataBaseReconnect(exception);
                form.ShowDialog();

                result = form.Reconnect;
            }
            

            

            //
            //Thread.Sleep(5000);

            if (reconectionCount <= 0)
            {
                //Application.Exit();
            }

            reconectionCount--;

            //form.Close();
            Console.WriteLine("Reconnect: " + reconectionCount + "; Time: " + DateTime.Now);
            //result = true;
            //

            /*Form1.formSQLException.ExceptionStr = exception;
            Form1.formSQLException.ShowDialog();*/

            _pauseEvent.Set();

            return result;
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
