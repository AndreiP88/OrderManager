using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetCountOfDone
    {
        String dataBase;
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String startShift;
        String orderNumber;
        String orderModification;
        String counterRepeat;

        public GetCountOfDone (String dBase, String startOfShift, String orderOfNumber, String orderOfModification, String counterOfRepeat)
        {
            this.dataBase = dBase;
            this.startShift = startOfShift;
            this.orderNumber = orderOfNumber;
            this.orderModification = orderOfModification;
            this.counterRepeat = counterOfRepeat;

            if (dataBase == "")
                dataBase = dataBaseDefault;
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
            int indexCurrentShift = 0;

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE numberOfOrder = @number AND modification = @orderModification"
                };
                Command.Parameters.AddWithValue("@number", orderNumber);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    countOfShifts.Add(Convert.ToInt32(sqlReader["done"]));

                    if ((sqlReader["startOfShift"].ToString() == startShift) && (sqlReader["counterRepeat"].ToString() == counterRepeat))
                        indexCurrentShift = countOfShifts.Count - 1;
                }

                Connect.Close();
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

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE numberOfOrder = @number AND modification = @orderModification"
                };
                Command.Parameters.AddWithValue("@number", orderNumber);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
                SQLiteDataReader sqlReader = Command.ExecuteReader();

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
