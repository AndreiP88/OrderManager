using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace OrderManager
{
    internal class ValueSalaryBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dBase"></param>
        public ValueSalaryBase()
        {

        }

        /// <summary>
        /// Получить основной оклад
        /// </summary>
        /// <param name="salaryId"></param>
        /// <returns></returns>
        public decimal GetBasicSalary(string salaryId)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(salaryId, "basicSalary"));

            return result;
        }

        public decimal GetBonusSalary(string salaryId)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(salaryId, "bonusSalary"));

            return result;
        }

        public decimal GetTax(string salaryId)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(salaryId, "tax"));

            return result;
        }

        public decimal GetPension(string salaryId)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(salaryId, "pension"));

            return result;
        }

        public string GetIndexFromSelectedPeriod(DateTime date, string userID)
        {
            string period = date.ToString("yyyy-MM-dd");
            string result = "0";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM salary WHERE STR_TO_DATE(period,'%d.%m.%Y') IN " +
                                    "(SELECT MAX(STR_TO_DATE(period,'%d.%m.%Y')) FROM salary WHERE " +
                                    "(DATE_FORMAT(STR_TO_DATE(period,'%d.%m.%Y'), '%Y-%m-%d') <= @period) AND userID = @userID)"

                };
                Command.Parameters.AddWithValue("@period", period);
                Command.Parameters.AddWithValue("@userID", userID);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader["id"].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        private String GetValue(string id, string nameOfColomn)
        {
            String result = "0";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM salary WHERE id = @id"
                };
                Command.Parameters.AddWithValue("@id", id);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[nameOfColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        private void SetValue(string userID, string period, string key, decimal value)
        {
            string fullPeriod = "01." + period;

            AddNewPeriod(userID, fullPeriod);

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE salaryPaid SET " + key + " = @value WHERE userID = @userID AND period = @period";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@userID", userID);
                Command.Parameters.AddWithValue("@period", fullPeriod);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void AddNewPeriod(string userID, string fullPeriod)
        {
            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(*) FROM salary WHERE userID = @userID AND period = @period"
                };

                Command.Parameters.AddWithValue("@userID", userID);
                Command.Parameters.AddWithValue("@period", fullPeriod);

                Connect.Open();
                result = Convert.ToInt32(Command.ExecuteScalar());
                Connect.Close();
            }

            if (result == 0)
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    string commandText = "INSERT INTO salary (userID, period) VALUES (@userID, @period)";

                    MySqlCommand Command = new MySqlCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@userID", userID);
                    Command.Parameters.AddWithValue("@period", fullPeriod);

                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }

        private void SetValue(string id, string key, decimal value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE orders SET " + key + " = @value " +
                    "WHERE machine = @orderMachine AND (numberOfOrder = @number AND modification = @orderModification)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Command.Parameters.AddWithValue("@value", value);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        
    }
}
