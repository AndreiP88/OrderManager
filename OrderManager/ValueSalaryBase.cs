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

        public List<SalaryForUser> GetData(string userID)
        {
            List<SalaryForUser> result = new List<SalaryForUser>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM salary WHERE userID = @userID"

                };
                Command.Parameters.AddWithValue("@userID", userID);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result.Add(new SalaryForUser(
                        sqlReader["id"].ToString(),
                        sqlReader["period"].ToString(),
                        Convert.ToDecimal(sqlReader["basicSalary"]),
                        Convert.ToDecimal(sqlReader["bonusSalary"]),
                        Convert.ToDecimal(sqlReader["tax"]),
                        Convert.ToDecimal(sqlReader["pension"])));
                }

                Connect.Close();
            }

            return result;
        }

        public void InsertData(string user, SalaryForUser value)
        {
            AddNewPeriod(user, value.period);

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                /*string commandText = "INSERT INTO salary (userID, period, basicSalary, bonusSalary, tax, pension) " +
                    "VALUES (@userID, @period, @basicSalary, @bonusSalary, @tax, @pension)";*/

                string commandText = "UPDATE salary SET basicSalary = @basicSalary, bonusSalary = @bonusSalary, tax = @tax, pension = @pension " +
                    "WHERE userID = @userID AND period = @period";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@userID", user);
                Command.Parameters.AddWithValue("@period", value.period);
                Command.Parameters.AddWithValue("@basicSalary", value.basicSalary);
                Command.Parameters.AddWithValue("@bonusSalary", value.bonusSalary);
                Command.Parameters.AddWithValue("@tax", value.tax);
                Command.Parameters.AddWithValue("@pension", value.pension);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public void UpdateData(SalaryForUser value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE salary SET period = @period, basicSalary = @basicSalary, bonusSalary = @bonusSalary, tax = @tax, pension = @pension " +
                    "WHERE (id = @id)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", value.id);
                Command.Parameters.AddWithValue("@period", value.period);
                Command.Parameters.AddWithValue("@basicSalary", value.basicSalary);
                Command.Parameters.AddWithValue("@bonusSalary", value.bonusSalary);
                Command.Parameters.AddWithValue("@tax", value.tax);
                Command.Parameters.AddWithValue("@pension", value.pension);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
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

        public void DeleteSalary(string id)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "DELETE FROM salary WHERE id = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }
    }
}
