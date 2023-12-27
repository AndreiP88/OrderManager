using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.IO;

namespace OrderManager
{
    internal class GetLeadTime
    {
        int shiftIndex;
        int machine;
        int orderIndex;
        int repeatCounter;

        public GetLeadTime(int shiftID, int machineLoad, int orderID, int counterRepeat)
        {
            this.shiftIndex = shiftID;
            this.machine = machineLoad;
            this.orderIndex = orderID;
            this.repeatCounter = counterRepeat;
        }

        public String GetLastDateTime(String nameOfColomn)
        {
            return GetDateTime(nameOfColomn).Item1;
        }

        public String GetCurrentDateTime(String nameOfColomn)
        {
            return GetDateTime(nameOfColomn).Item2;
        }

        public String GetNextDateTime(String nameOfColomn)
        {
            return GetDateTime(nameOfColomn).Item3;
        }

        public String GetFirstValue(String nameOfColomn)
        {
            return GetDateTime(nameOfColomn).Item4;
        }

        public String GetLastValue(String nameOfColomn)
        {
            return GetDateTime(nameOfColomn).Item5;
        }

        public int CalculateMakereadyParts(bool calculatePreviousParts, bool calculateCurrentParts, bool calculateSubsequentParts)
        {
            int summMakereadyParts = 0;

            List<int> parts = new List<int>();
            int indexPartsCurrentShift = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE orderID = @id AND (counterRepeat = @counterRepeat AND machine = @machine)"
                };
                Command.Parameters.AddWithValue("@id", orderIndex);
                Command.Parameters.AddWithValue("@counterRepeat", repeatCounter);
                Command.Parameters.AddWithValue("@machine", machine);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if ((int)sqlReader["makereadyComplete"] >= 0)
                    {
                        parts.Add((int)sqlReader["makereadyComplete"]);
                    }
                    else
                    {
                        parts.Add(0);
                    }

                    if ((int)sqlReader["shiftID"] == shiftIndex)
                    {
                        indexPartsCurrentShift = parts.Count - 1;
                    }
                }

                Connect.Close();
            }

            for (int i = 0; i < parts.Count; i++)
            {
                if (i < indexPartsCurrentShift && calculatePreviousParts)
                {
                    summMakereadyParts += parts[i];
                }
                    
                if (i == indexPartsCurrentShift && calculateCurrentParts)
                {
                    summMakereadyParts += parts[i];
                }

                if (i > indexPartsCurrentShift && calculateSubsequentParts)
                {
                    summMakereadyParts += parts[i];
                }
            }

            return summMakereadyParts;
        }

        private (String, String, String, String, String) GetDateTime(String nameOfColomn)
        {
            String lastTime = "";
            String currentTime = "";
            String nextTime = "";
            String firstValue = "";
            String lastValue = "";

            int indexCurrent = 0;
            List<String> datetimes = new List<String>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE orderID = @id AND (counterRepeat = @counterRepeat AND machine = @machine)"
                };
                Command.Parameters.AddWithValue("@id", orderIndex);
                Command.Parameters.AddWithValue("@counterRepeat", repeatCounter);
                Command.Parameters.AddWithValue("@machine", machine);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    datetimes.Add(sqlReader[nameOfColomn].ToString());

                    if ((int)sqlReader["shiftID"] == shiftIndex)
                    {
                        indexCurrent = datetimes.Count - 1;
                        currentTime = sqlReader[nameOfColomn].ToString();
                    }
                }

                Connect.Close();
            }

            if (indexCurrent == 0)
            {
                lastTime = "";
            }
            if (datetimes.Count > indexCurrent)
            {
                for (int i = indexCurrent - 1; i >= 0; i--)
                {
                    if (datetimes[i] != "")
                    {
                        lastTime = datetimes[i].ToString();
                        break;
                    }
                }

                if (indexCurrent < datetimes.Count - 1)
                {
                    for (int i = indexCurrent + 1; i < datetimes.Count; i++)
                    {
                        if (datetimes[i] != "")
                        {
                            nextTime = datetimes[i].ToString();
                            break;
                        }
                    }
                }
            }

            if (datetimes.Count > 0)
            {
                firstValue = datetimes[0].ToString();
                lastValue = datetimes[datetimes.Count - 1].ToString();
            }

            return (lastTime, currentTime, nextTime, firstValue, lastValue);
        }

    }
}
