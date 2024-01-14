using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;

namespace OrderManager
{
    internal class GetWorkingOutSum
    {
        public GetWorkingOutSum()
        {
            
        }

        public int CalculateWorkingOutForUserFromSelectedMonthDataBaseOM(int userId, List<int> equips, DateTime startMonth)
        {
            DateTime startPeriod = Convert.ToDateTime("01." + startMonth.Month + "." + startMonth.Year + " 00:00:00");
            DateTime endPeriod = Convert.ToDateTime(startMonth.AddMonths(1).AddDays(-1).Day + "." + startMonth.Month + "." + startMonth.Year + " 23:59:59");

            return CalculateWorkingOutForUserDataBaseOM(userId, equips, startPeriod, endPeriod);
        }

        private int CalculateWorkingOutForUserDataBaseOM(int userId, List<int> equips, DateTime startPeriod, DateTime endPeriod)
        {
            int result = 0;

            string startDateTime = startPeriod.ToString("dd.MM.yyyy HH:mm:ss");
            string endDateTime = endPeriod.ToString("dd.MM.yyyy HH:mm:ss");

            string equipsStr = "machine = " + equips[0];

            for (int i = 1; i < equips.Count; i++)
            {
                equipsStr += " OR machine = " + equips[i];
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT
	                                    SUM(done) summ
                                    FROM
	                                    shifts
	                                    INNER JOIN
	                                    ordersinprogress
	                                    ON 
		                                    shifts.id = ordersinprogress.shiftID
                                    WHERE
	                                    nameUser = @userId
	                                    AND	(" + equipsStr + @")
	                                    AND	STR_TO_DATE(startShift,'%d.%m.%Y %H:%i:%S') >= STR_TO_DATE(@startDate,'%d.%m.%Y %H:%i:%S') 
	                                    AND	STR_TO_DATE(startShift,'%d.%m.%Y %H:%i:%S') <= STR_TO_DATE(@endDate,'%d.%m.%Y %H:%i:%S')"
                };
                Command.Parameters.AddWithValue("@userId", userId);
                Command.Parameters.AddWithValue("@equipsStr", equipsStr);
                Command.Parameters.AddWithValue("@startDate", startDateTime);
                Command.Parameters.AddWithValue("@endDate", endDateTime);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (!DBNull.Value.Equals(sqlReader["summ"]))
                    {
                        result = Convert.ToInt32(sqlReader["summ"]);
                    }
                    else
                    {
                        result = 0;
                    }
                }

                Connect.Close();
            }

            return result;
        }

        public int CalculateWorkingOutForUserFromSelectedMonthDataBaseAS(int userId, List<int> equips, DateTime startMonth)
        {
            DateTime startPeriod = Convert.ToDateTime("01." + startMonth.Month + "." + startMonth.Year + " 00:00:00");
            DateTime endPeriod = Convert.ToDateTime(startMonth.AddMonths(1).AddDays(-1).Day + "." + startMonth.Month + "." + startMonth.Year + " 23:59:59");

            return CalculateWorkingOutForUserDataBaseAS(userId, equips, startPeriod, endPeriod);
        }

        private int CalculateWorkingOutForUserDataBaseAS(int userId, List<int> equips, DateTime startPeriod, DateTime endPeriod)
        {
            int result = 0;

            ValueInfoBase infoBase = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();

            //2023-11-01 00:00:00.000
            string startDateTime = startPeriod.ToString("yyyy-MM-dd") + "T" + startPeriod.ToString("HH:mm:ss") + ".000";
            string endDateTime = endPeriod.ToString("yyyy-MM-dd") + "T" + endPeriod.ToString("HH:mm:ss") + ".000";

            try
            {
                /*List<int> userIndexFromAS = userBase.GetIndexUserFromASBase(userId);

                string usersStr = "man_factjob.id_common_employee = " + userIndexFromAS[0];

                for (int i = 1; i < userIndexFromAS.Count; i++)
                {
                    usersStr += " OR man_factjob.id_common_employee = " + userIndexFromAS[i];
                }*/

                string usersStr = "man_factjob.id_common_employee = " + userId;

                string equipsStr = "man_factjob.id_equip = " + infoBase.GetIDEquipMachine(equips[0]);

                for (int i = 1; i < equips.Count; i++)
                {
                    equipsStr += " OR man_factjob.id_equip = " + infoBase.GetIDEquipMachine(equips[i]);
                }

                using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                {
                    Connect.Open();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT
	                                    SUM(man_factjob.fact_out_qty) summ
                                    FROM
	                                    dbo.fbc_brigade
	                                    FULL OUTER JOIN
	                                    dbo.man_factjob
	                                    ON 
		                                    (
			                                    man_factjob.date_begin >= fbc_brigade.date_begin AND
			                                    man_factjob.date_begin <= ISNULL( fbc_brigade.date_end, GETDATE( ) ) AND
			                                    man_factjob.id_common_employee = fbc_brigade.id_common_employee
		                                    )
                                    WHERE
	                                    fbc_brigade.date_begin IS NOT NULL AND
	                                    fbc_brigade.date_begin >= CONVERT ( VARCHAR ( 24 ), @startDate, 21 ) AND
	                                    fbc_brigade.date_begin <= CONVERT ( VARCHAR ( 24 ), @endDate, 21 )
	                                    AND eff_output_coeff <> 0
	                                    --AND man_factjob.flags <> 576
	                                    AND (" + usersStr + @")
	                                    AND (" + equipsStr + @")"
                    };
                    Command.Parameters.AddWithValue("@userId", usersStr);
                    Command.Parameters.AddWithValue("@equipsStr", equipsStr);
                    Command.Parameters.AddWithValue("@startDate", startDateTime);
                    Command.Parameters.AddWithValue("@endDate", endDateTime);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        if (!DBNull.Value.Equals(sqlReader["summ"]))
                        {
                            result = Convert.ToInt32(sqlReader["summ"]);
                        }
                        else
                        {
                            result = 0;
                        }
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }

            return result;
        }
    }
}
