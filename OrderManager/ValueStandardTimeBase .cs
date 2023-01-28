using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace OrderManager
{
    internal class ValueStandardTimeBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dBase"></param>
        public ValueStandardTimeBase()
        {

        }

        /// <summary>
        /// Получить основной оклад
        /// </summary>
        /// <param name="salaryId"></param>
        /// <returns></returns>
        public int GetStandard(string period)
        {
            int result = 0;

            result = Convert.ToInt32(GetValue(period, "standard52"));

            return result;
        }

        private string GetValue(string period, string nameOfColomn)
        {
            string result = "0";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM standardOfWorkingTime WHERE period = @period"
                };
                Command.Parameters.AddWithValue("@period", period);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[nameOfColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        private void SetValue(string period, string key, int value)
        {
            AddNewPeriod(period);

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE standardOfWorkingTime SET " + key + " = @value WHERE period = @period";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@period", period);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void AddNewPeriod(string period)
        {
            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(*) FROM standardOfWorkingTime WHERE period = @period"
                };

                Command.Parameters.AddWithValue("@period", period);

                Connect.Open();
                result = Convert.ToInt32(Command.ExecuteScalar());
                Connect.Close();
            }

            if (result == 0)
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    string commandText = "INSERT INTO standardOfWorkingTime (period) VALUES (@period)";

                    MySqlCommand Command = new MySqlCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@period", period);

                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }
        
    }
}
