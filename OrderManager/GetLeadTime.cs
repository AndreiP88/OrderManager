using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace OrderManager
{
    internal class GetLeadTime
    {
        String shiftStart;
        int orderID;
        String repeatCounter;

        public GetLeadTime(string startOfShift, int orderIndex, string counterRepeat)
        {
            this.shiftStart = startOfShift;
            this.orderID = orderIndex;
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
                    CommandText = @"SELECT * FROM ordersInProgress WHERE orderID = @id AND counterRepeat = @counterRepeat"
                };
                Command.Parameters.AddWithValue("@id", orderID);
                Command.Parameters.AddWithValue("@counterRepeat", repeatCounter);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    datetimes.Add(sqlReader[nameOfColomn].ToString());

                    if (sqlReader["startOfShift"].ToString() == shiftStart)
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
