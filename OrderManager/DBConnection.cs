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
        //static ManualResetEvent _pauseEvent = new ManualResetEvent(false);

        /*public static MySqlConnection GetDBConnection()
        {
            *//*string host = "25.21.38.172";
            int port = 3309;
            string database = "order_manager";
            string username = "oxyfox";
            string password = "root";*//*

            _pauseEvent.WaitOne(Timeout.Infinite);

        NewConnect:

            string host = Form1.BaseConnectionParameters.host;
            int port = Form1.BaseConnectionParameters.port;
            string database = Form1.BaseConnectionParameters.database;
            string username = Form1.BaseConnectionParameters.username;
            string password = Form1.BaseConnectionParameters.password;

            MySqlConnection connect = DBMySQLUtils.GetDBConnection(host, port, database, username, password);

            //_pauseEvent.WaitOne(Timeout.Infinite);

            string exception;

            using (connect)
            {
                try
                {
                    connect.Open();
                    connect.Close();
                    exception = "";
                }
                catch (Exception ex)
                {
                    exception = ex.Message;

                    bool reconectionRequest = DataBaseReconnectionRequest(exception);

                    //_pauseEvent.WaitOne(Timeout.Infinite);

                    if (reconectionRequest)
                    {
                        goto NewConnect;
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }

            if (exception != "")
            {
                *//*bool reconectionRequest = DataBaseReconnectionRequest(exception);

                _pauseEvent.WaitOne(Timeout.Infinite);

                if (reconectionRequest)
                {
                    goto NewConnect;
                }
                else
                {
                    Application.Exit();
                }*//*
            }

            return connect;
        }*/

        public static MySqlConnection GetDBConnection()
        {
            /*string host = "25.21.38.172";
            int port = 3309;
            string database = "order_manager";
            string username = "oxyfox";
            string password = "root";*/
            
            _pauseEvent.WaitOne();

        NewConnect:

            _pauseEvent.WaitOne();

            string host = Form1.BaseConnectionParameters.host;
            int port = Form1.BaseConnectionParameters.port;
            string database = Form1.BaseConnectionParameters.database;
            string username = Form1.BaseConnectionParameters.username;
            string password = Form1.BaseConnectionParameters.password;

            MySqlConnection connect = DBMySQLUtils.GetDBConnection(host, port, database, username, password);

            //_pauseEvent.WaitOne(Timeout.Infinite);

            using (connect)
            {
                try
                {
                    connect.Open();
                    connect.Close();
                }
                catch (Exception ex)
                {
                    bool reconectionRequest = DataBaseReconnectionRequest(ex.Message);

                    //_pauseEvent.WaitOne();

                    if (reconectionRequest)
                    {
                        goto NewConnect;
                    }

                    //_pauseEvent.WaitOne();
                }
            }

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

            FormDataBaseReconnect form = new FormDataBaseReconnect(exception);
            //form.ShowDialog();

            result = formSQLException.Reconnect;

            //
            Thread.Sleep(5000);
            //form.Close();
            Console.WriteLine("Reconnect: " + DateTime.Now);
            result = true;
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
