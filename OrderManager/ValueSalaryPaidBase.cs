using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace OrderManager
{
    internal class ValueSalaryPaidBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dBase"></param>
        public ValueSalaryPaidBase()
        {

        }

        /// <summary>
        /// Получить основной оклад
        /// </summary>
        /// <param name="salaryId"></param>
        /// <returns></returns>
        public decimal GetBasicSalary(string user, string period)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(user, period, "basicSalary"));

            return result;
        }

        public decimal GetBonusSalary(string user, string period)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(user, period, "bonusSalary"));

            return result;
        }

        public decimal GetNight(string user, string period)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(user, period, "night"));

            return result;
        }

        public decimal GetOvertime(string user, string period)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(user, period, "overtime"));

            return result;
        }

        public decimal GetHoliday(string user, string period)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(user, period, "holiday"));

            return result;
        }

        public decimal GetBonus(string user, string period)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(user, period, "bonus"));

            return result;
        }

        public decimal GetOther(string user, string period)
        {
            decimal result = 0;

            result = Convert.ToDecimal(GetValue(user, period, "other"));

            return result;
        }

        public void SetBasicSalary(string user, string period, decimal value)
        {
            SetValue(user, period, "basicSalary", value);
        }

        public void SetBonusSalary(string user, string period, decimal value)
        {
            SetValue(user, period, "bonusSalary", value);
        }

        public void SetNight(string user, string period, decimal value)
        {
            SetValue(user, period, "night", value);
        }

        public void SetOvertime(string user, string period, decimal value)
        {
            SetValue(user, period, "overtime", value);
        }

        public void SetHoliday(string user, string period, decimal value)
        {
            SetValue(user, period, "holiday", value);
        }

        public void SetBonus(string user, string period, decimal value)
        {
            SetValue(user, period, "bonus", value);
        }

        public void SetOther(string user, string period, decimal value)
        {
            SetValue(user, period, "other", value);
        }

        private string GetValue(string userID,  string period, string nameOfColomn)
        {
            string result = "0";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM salaryPaid WHERE userID = @userID AND period = @period"
                };
                Command.Parameters.AddWithValue("@userID", userID);
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

        private void SetValue(string userID,  string period, string key, decimal value)
        {
            AddNewPeriod(userID, period);

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE salaryPaid SET " + key + " = @value WHERE userID = @userID AND period = @period";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@userID", userID);
                Command.Parameters.AddWithValue("@period", period);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void AddNewPeriod(string userID, string period)
        {
            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(*) FROM salaryPaid WHERE userID = @userID AND period = @period"
                };

                Command.Parameters.AddWithValue("@userID", userID);
                Command.Parameters.AddWithValue("@period", period);

                Connect.Open();
                result = Convert.ToInt32(Command.ExecuteScalar());
                Connect.Close();
            }

            if (result == 0)
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    string commandText = "INSERT INTO salaryPaid (userID, period) VALUES (@userID, @period)";

                    MySqlCommand Command = new MySqlCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@userID", userID);
                    Command.Parameters.AddWithValue("@period", period);

                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }
    }
}
