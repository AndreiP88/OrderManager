using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class DBMySQLUtils
    {
        public static MySqlConnection
        GetDBConnection(string host, int port, string database, string username, string password)
        {
            // Connection String.
            String connString = "Server=" + host + ";Database=" + database
                + ";port=" + port + ";User Id=" + username + ";password=" + password;

            MySqlConnection conn = new MySqlConnection(connString);

            return conn;
        }

        public static SqlConnection
        GetSQLServerConnection(string host, string database, string username, string password)
        {
            // Connection String.
            String connString = @"Data Source = " + host + "; Initial Catalog = " + database + "; Persist Security Info = True; User ID = " + username + "; Password = " + password + "";

            string connectionString = @"Data Source = SRV-ACS\DSACS; Initial Catalog = asystem; Persist Security Info = True; User ID = ds; Password = 1";

            SqlConnection conn = new SqlConnection(connString);

            return conn;
        }
    }
}
