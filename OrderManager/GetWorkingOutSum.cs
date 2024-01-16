﻿using libData;
using libSql;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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

        public int CalculateWorkingOutForUserFromSelectedMonthDataBaseASUsersFromOM(int userId, List<int> equips, DateTime startMonth)
        {
            ValueUserBase userBase = new ValueUserBase();

            DateTime startPeriod = Convert.ToDateTime("01." + startMonth.Month + "." + startMonth.Year + " 00:00:00");
            DateTime endPeriod = Convert.ToDateTime(startMonth.AddMonths(1).AddDays(-1).Day + "." + startMonth.Month + "." + startMonth.Year + " 23:59:59");

            List<int> userIndexFromAS = userBase.GetIndexUserFromASBase(userId);

            return CalculateWorkingOutForUserDataBaseAS(userIndexFromAS, equips, startPeriod, endPeriod);
        }

        public int CalculateWorkingOutForUserFromSelectedMonthDataBaseASUsersFromAS(int userId, List<int> equips, DateTime startMonth)
        {
            ValueUserBase userBase = new ValueUserBase();

            DateTime startPeriod = Convert.ToDateTime("01." + startMonth.Month + "." + startMonth.Year + " 00:00:00");
            DateTime endPeriod = Convert.ToDateTime(startMonth.AddMonths(1).AddDays(-1).Day + "." + startMonth.Month + "." + startMonth.Year + " 23:59:59");

            List<int> userIndexAS = new List<int> { userId };

            return CalculateWorkingOutForUserDataBaseAS(userIndexAS, equips, startPeriod, endPeriod);
        }

        private int CalculateWorkingOutForUserDataBaseAS(List<int> userIDs, List<int> equips, DateTime startPeriod, DateTime endPeriod)
        {
            int result = 0;

            ValueInfoBase infoBase = new ValueInfoBase();

            //2023-11-01 00:00:00.000
            string startDateTime = startPeriod.ToString("yyyy-MM-dd") + "T" + startPeriod.ToString("HH:mm:ss") + ".000";
            string endDateTime = endPeriod.ToString("yyyy-MM-dd") + "T" + endPeriod.ToString("HH:mm:ss") + ".000";

            try
            {
                string usersStr = "man_factjob.id_common_employee = " + userIDs[0];

                for (int i = 1; i < userIDs.Count; i++)
                {
                    usersStr += " OR man_factjob.id_common_employee = " + userIDs[i];
                }

                //string usersStr = "man_factjob.id_common_employee = " + userId;

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

        public float CalculatePercentWorkingOutOM(int userID, DateTime selectMonth, CancellationToken token, int category)
        {
            float result = 0;

            GetShiftsFromBase getShifts = new GetShiftsFromBase(userID.ToString());

            ShiftsDetails shiftsDetails = getShifts.LoadCurrentDateShiftsDetails(selectMonth, category.ToString(), token);

            result = shiftsDetails.percentWorkingOutShift;

            return result;
        }

        private List<int> GetEquipsListASFromEquipsListOM(List<int> equipsListOM)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            List<int> equipsAS = new List<int>();

            for (int i = 0; i < equipsListOM.Count; i++)
            {
                equipsAS.Add(infoBase.GetIDEquipMachine(equipsListOM[i]));
            }

            return equipsAS;
        }

        public ShiftsDetails CalculateDetailWorkingOutAS(int userID, DateTime selectMonth, CancellationToken token)
        {
            ValueUserBase userBase = new ValueUserBase();

            List<int> userIndexAS = userBase.GetIndexUserFromASBase(userID);

            ShiftsDetails shiftsDetails = WorkingOutDetailsAS(userIndexAS, selectMonth, token);

            return shiftsDetails;
        }

        public float CalculatePercentWorkingOutAS(int userID, DateTime selectMonth, CancellationToken token, List<int> equipListOM)
        {
            float result = 0;

            List<int> userIndexAS = new List<int> { userID };
            List<int> equipListAS = GetEquipsListASFromEquipsListOM(equipListOM);

            ShiftsDetails shiftsDetails = WorkingOutDetailsAS(userIndexAS, selectMonth, token, equipListAS);

            result = shiftsDetails.percentWorkingOutShift;

            return result;
        }

        private ShiftsDetails WorkingOutDetailsAS(List<int> userIndexFromAS, DateTime selectMonth, CancellationToken token, List<int> equipListAS = null)
        {
            ShiftsDetails shiftsDetails = null;

            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            ValueShifts valueShifts = new ValueShifts();
            //ValueUserBase userBase = new ValueUserBase();

            //List<int> userIndexFromAS = userBase.GetIndexUserFromASBase(userID);

            List<User> usersList = new List<User>();

            for (int i = 0; i < userIndexFromAS.Count; i++)
            {
                usersList.Add(new User(userIndexFromAS[i]));
                usersList[usersList.Count - 1].Shifts = new List<UserShift>();
            }

            try
            {
                usersList = valueShifts.LoadShiftsForSelectedMonth(usersList, selectMonth, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

            float totalTimeWorkigOut = 0;
            //float totalPercentWorkingOut = 0;
            float totalBonusWorkingOut = 0;
            List<float> totalPercentWorkingOutList = new List<float>();

            for (int i = 0; i < usersList.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                for (int j = 0; j < usersList[i].Shifts.Count; j++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    UserShift shift = usersList[i].Shifts[j];

                    bool currentShift;// = CheckCurrentShift(shiftDate, shiftNumber);

                    if (shift.ShiftDateEnd == "")
                    {
                        currentShift = true;
                    }
                    else
                    {
                        currentShift = false;
                    }

                    if (!currentShift)
                    {
                        float timeWorkigOut = CalculateWorkTime(shift.Orders, equipListAS);

                        totalTimeWorkigOut += timeWorkigOut;
                        totalBonusWorkingOut += getPercent.GetBonusWorkingOutF((int)timeWorkigOut);

                        if (timeWorkigOut != -1)
                        {
                            totalPercentWorkingOutList.Add(getPercent.Percent((int)timeWorkigOut));
                        }
                    }
                }

                float percentWorkingOutAverage = 0;

                if (totalPercentWorkingOutList.Count > 0)
                {
                    percentWorkingOutAverage = totalPercentWorkingOutList.Sum() / totalPercentWorkingOutList.Count;
                }

                shiftsDetails = new ShiftsDetails(
                -1,
                -1,
                -1,
                (int)totalTimeWorkigOut,
                -1,
                -1,
                -1,
                percentWorkingOutAverage,
                totalBonusWorkingOut
                );
            }

            return shiftsDetails;
        }

        private float CalculateWorkTime(List<UserShiftOrder> order, List<int> equipListAS = null)
        {
            float workingOut = -1;
            bool activeShift = false;

            for (int i = 0; i < order.Count; i++)
            {
                if (equipListAS != null)
                {
                    if (equipListAS.Contains(order[i].IdEquip))
                    {
                        workingOut += CalculateWorkTimeForOneOrder(order[i]);
                        activeShift = true;
                    }
                }
                else
                {
                    workingOut += CalculateWorkTimeForOneOrder(order[i]);
                    activeShift = true;
                }
            }

            if (activeShift)
            {
                workingOut += 1;
            }

            return workingOut;
        }

        private float CalculateWorkTimeForOneOrder(UserShiftOrder order)
        {
            float workingOut = 0;

            if (order.Normtime > 0)
            {
                if (order.PlanOutQty > 0)
                {
                    workingOut += ((float)order.FactOutQty * (float)order.Normtime) / (float)order.PlanOutQty;
                }
                else
                {
                    if (order.FactOutQty > 0)
                    {
                        workingOut += (float)order.Normtime;
                    }
                }
            }

            return workingOut;
        }
    }
}
