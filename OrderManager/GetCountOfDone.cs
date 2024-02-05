using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace OrderManager
{
    internal class GetCountOfDone
    {
        int shiftIndex;
        int orderIndex;
        int counterRepeat;

        public GetCountOfDone(int shiftID, int orderID, int counterOfRepeat)
        {
            this.shiftIndex = shiftID;
            this.orderIndex = orderID;
            this.counterRepeat = counterOfRepeat;
        }

        public int OrderCalculate(bool previousShift, bool currentShift)
        {
            int result = 0;

            if (previousShift)
                result += CountOfOrder().Item1;
            if (currentShift)
                result += CountOfOrder().Item2;

            return result;
        }

        public int OrderFullCalculate()
        {
            return CountOfOrder().Item3;
        }

        private (int, int, int) CountOfOrder()
        {
            int previous = 0, current = 0, full = 0;

            List<int> countOfShifts = new List<int>();
            int indexCurrentShift = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE orderID = @id"
                };
                //Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@id", orderIndex);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    countOfShifts.Add(Convert.ToInt32(sqlReader["done"]));

                    //sqlReader["shiftID"] == DBNull.Value ? null : (int?)sqlReader["shiftID"]

                    if (((int)sqlReader["shiftID"] == shiftIndex) && ((int)sqlReader["counterRepeat"] == counterRepeat))
                        indexCurrentShift = countOfShifts.Count - 1;
                }

                Connect.Close();
            }

            if (indexCurrentShift == -1)
            {
                indexCurrentShift = countOfShifts.Count;
            }

            for (int i = 0; i < countOfShifts.Count; i++)
            {
                if (i < indexCurrentShift)
                    previous += countOfShifts[i];
                if (i == indexCurrentShift)
                    current += countOfShifts[i];

                full += countOfShifts[i];
            }

            return (previous, current, full);
        }

        /*
        public int OrderCalculateOld(bool previousShift, bool currentShift)
        {
            int previous = 0, current = 0;
            int value = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE numberOfOrder = @number AND modification = @orderModification"
                };
                Command.Parameters.AddWithValue("@number", orderNumber);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if ((sqlReader["startOfShift"].ToString() == startShift) && (sqlReader["counterRepeat"].ToString() == counterRepeat))
                    {
                        if (sqlReader["done"].ToString() != "")
                            current += Convert.ToInt32(sqlReader["done"].ToString());
                    }
                    else
                    {
                        if (sqlReader["done"].ToString() != "")
                            previous += Convert.ToInt32(sqlReader["done"].ToString());
                    }
                }

                Connect.Close();
            }

            if (previousShift)
                value += previous;
            if (currentShift)
                value += current;
            return value;
        }
        */
    }


}
