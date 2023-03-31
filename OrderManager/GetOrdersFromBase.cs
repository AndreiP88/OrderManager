using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace OrderManager
{
    internal class GetOrdersFromBase
    {
        bool _unionDeviation = false;
        public class OrdersMonth
        {
            public String numberOrder { get; set; }
            public String modificationOrder { get; set; }

            public OrdersMonth(string numberOrder, string modificationOrder)
            {
                this.numberOrder = numberOrder;
                this.modificationOrder = modificationOrder;
            }

            public OrdersMonth()
            {
            }
        }

        public GetOrdersFromBase()
        {

        }

        public String GetIndex(String startShift, String numberOfOrder, String modification, String counterRepeat, String machine)
        {
            return GetValue("count", startShift, numberOfOrder, modification, counterRepeat, machine);
        }

        public String GetNote(String startShift, String numberOfOrder, String modification, String counterRepeat, String machine)
        {
            return GetValue("note", startShift, numberOfOrder, modification, counterRepeat, machine);
        }

        public String GetPrivateNote(String startShift, String numberOfOrder, String modification, String counterRepeat, String machine)
        {
            return GetValue("privateNote", startShift, numberOfOrder, modification, counterRepeat, machine);
        }
        public String GetDone(String startShift, String numberOfOrder, String modification, String counterRepeat, String machine)
        {
            return GetValue("done", startShift, numberOfOrder, modification, counterRepeat, machine);
        }

        /// <summary>
        /// Получить время начала выполнения заказа
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Фактическое время начал выполнения заказа</returns>
        public string GetOrderStartTime(int id)
        {
            string startMK = GetValueFromIndex(id, "timeMakereadyStart");
            string startWK = GetValueFromIndex(id, "timeToWorkStart");

            if (startMK != "")
            {
                return startMK;
            }
            else
            {
                return startWK;
            }
        }

        /// <summary>
        /// Получить время завершения заказа
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTimeToWorkStop(int id)
        {
            return GetValueFromIndex(id, "timeToWorkStop");
        }

        /// <summary>
        /// Получить время завершения приладки
        /// </summary>
        /// <param name="id">индекс строки в таблице</param>
        /// <returns></returns>
        public string GetTimeToMakereadyStop(int id)
        {
            return GetValueFromIndex(id, "timeMakereadyStop");
        }

        /// <summary>
        /// получить индекс заказа из базы данных "orders"
        /// </summary>
        /// <param name="id">Индекс операции в БД "orderInProgress"</param>
        /// <returns>Индекс заказа</returns>
        public int GetOrderID(int id)
        {
            return Convert.ToInt32(GetValueFromIndex(id, "orderID"));
        }

        private String GetValue(String nameOfColomn, String startShift, String numberOfOrder, String modification, String counterRepeat, String machine)
        {
            String result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE ((startOfShift = @startOfShift AND numberOfOrder = @numberOfOrder) AND (modification = @modification AND counterRepeat = @counterRepeat)) AND machine = @machine"
                };
                Command.Parameters.AddWithValue("@startOfShift", startShift);
                Command.Parameters.AddWithValue("@numberOfOrder", numberOfOrder);
                Command.Parameters.AddWithValue("@modification", modification);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@machine", machine);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[nameOfColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        private String GetValueFromIndex(int count, string nameOfColomn)
        {
            String result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE count = @count"
                };
                Command.Parameters.AddWithValue("@count", count);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader[nameOfColomn].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        public int GetCountOrdersFromMachineForTheMonth(DateTime currentDate, String machine)
        {
            int count = 0;

            List<int> co = new List<int>((List<int>)GetOrdersFromMachineForTheMonth(currentDate, machine));
            count = co[0];

            return count;
        }

        public int GetAmountOrdersFromMachineForTheMonth(DateTime currentDate, String machine)
        {
            int amount = 0;

            List<int> am = new List<int>((List<int>)GetOrdersFromMachineForTheMonth(currentDate, machine));
            amount = am[1];

            return amount;
        }

        public Object GetOrdersFromMachineForTheMonth(DateTime currentDate, String machine)
        {
            int count = 0;
            int amount = 0;

            List<int> ordersCountAmount = new List<int>();

            List<String> orderList = new List<String>();
            orderList.Add("null");

            String commandLine;

            //commandLine = "(strftime('%Y,%m', date(substr(startOfShift, 7, 4) || '-' || substr(startOfShift, 4, 2) || '-' || substr(startOfShift, 1, 2))) = '";
            commandLine = "(DATE_FORMAT(STR_TO_DATE(startOfShift,'%d.%m.%Y %H:%i:%S'), '%Y,%m') = '";
            commandLine += currentDate.ToString("yyyy,MM") + "'";
            commandLine += " AND machine = '" + machine + "')";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                ValueUserBase usersBase = new ValueUserBase();
                GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE " + commandLine
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    String line = sqlReader["numberOfOrder"].ToString() + "," + sqlReader["modification"].ToString();

                    //MessageBox.Show((orderList.Count - 1).ToString() + "(" + orderList.Contains(line).ToString() + "): " + orderList[orderList.Count - 1] + " = " + line);

                    if (!orderList.Contains(line))
                    {
                        //MessageBox.Show(orderList[orderList.Count - 1]);
                        count++;
                    }

                    orderList.Add(line);

                    amount += Convert.ToInt32(sqlReader["done"]);
                }

                Connect.Close();
            }

            ordersCountAmount.Add(count);
            ordersCountAmount.Add(amount);

            return ordersCountAmount;
        }

        public Object GetOrdersFromMachineForTheYear(DateTime currentDate, String machine)
        {
            int count = 0;
            int amount = 0;

            List<int> ordersCountAmount = new List<int>();

            List<String> orderList = new List<String>();
            orderList.Add("null");

            String commandLine;

            //commandLine = "(strftime('%Y', date(substr(startOfShift, 7, 4) || '-' || substr(startOfShift, 4, 2) || '-' || substr(startOfShift, 1, 2))) = '";
            commandLine = "(DATE_FORMAT(STR_TO_DATE(startOfShift,'%d.%m.%Y %H:%i:%S'), '%Y') = '";
            commandLine += currentDate.ToString("yyyy") + "'";
            commandLine += " AND machine = '" + machine + "')";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                ValueUserBase usersBase = new ValueUserBase();
                GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE " + commandLine
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    String line = sqlReader["numberOfOrder"].ToString() + "," + sqlReader["modification"].ToString();

                    //MessageBox.Show((orderList.Count - 1).ToString() + "(" + orderList.Contains(line).ToString() + "): " + orderList[orderList.Count - 1] + " = " + line);

                    if (!orderList.Contains(line))
                    {
                        //MessageBox.Show(orderList[orderList.Count - 1]);
                        count++;
                    }

                    orderList.Add(line);

                    amount += Convert.ToInt32(sqlReader["done"]);
                }

                Connect.Close();
            }

            ordersCountAmount.Add(count);
            ordersCountAmount.Add(amount);

            return ordersCountAmount;
        }

        public Object GetNumbersOrders(String machine, String numberOfOrder, String modification)
        {
            List<String> numbers = new List<String>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE machine = @machine AND (numberOfOrder = @numberOfOrder AND modification = @modification)"
                };
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@numberOfOrder", numberOfOrder);
                Command.Parameters.AddWithValue("@modification", modification);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    numbers.Add(sqlReader["count"].ToString());
                }

                Connect.Close();
            }

            return numbers;
        }

        /*public Object LoadAllOrdersFromBase2(String startOfShift, String category)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            List<Order> orders = new List<Order>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE startOfShift = '" + startOfShift + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (category == getInfo.GetCategoryMachine(sqlReader["machine"].ToString()) || category == "")
                    {
                        //sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()
                        GetCountOfDone orderCount = new GetCountOfDone(startOfShift, sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["counterRepeat"].ToString()); // ordersBase.GetValue("counterRepeat").ToString() - раньше этот запрос был

                        int amountThisOrder = Convert.ToInt32(ordersBase.GetAmountOfOrder(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));
                        int lastCount = amountThisOrder - orderCount.OrderCalculate(true, false);

                        if (lastCount < 0)
                            lastCount = 0;

                        int workTime = Convert.ToInt32(ordersBase.GetTimeToWork(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));
                        int orderNorm = 0;
                        int timeWorkingOut = 0;
                        string lastTimeWork = "00:00";

                        if (workTime != 0)
                        {
                            orderNorm = amountThisOrder * 60 / workTime;
                            timeWorkingOut = Convert.ToInt32(sqlReader["done"]) * 60 / orderNorm;
                            lastTimeWork = timeOperations.TotalMinutesToHoursAndMinutesStr((lastCount * 60) / orderNorm);
                        }

                        string lastTimeMakeready = LastTimeMakereadyStr(startOfShift, sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["counterRepeat"].ToString());
                        string timeMakeready = timeOperations.DateDifferent(sqlReader["timeMakereadyStop"].ToString(), sqlReader["timeMakereadyStart"].ToString()).ToString();
                        string timeWork = timeOperations.DateDifferent(sqlReader["timeToWorkStop"].ToString(), sqlReader["timeToWorkStart"].ToString()).ToString();
                        string lastTimeWorkForDeviation;

                        if (timeWorkingOut > 0)
                        {
                            lastTimeWorkForDeviation = timeOperations.TotalMinutesToHoursAndMinutesStr(timeWorkingOut);
                        }
                        else
                        {
                            lastTimeWorkForDeviation = lastTimeWork;
                        }
                        
                        string deviation;

                        if (_unionDeviation)
                        {
                            string timeFull = timeOperations.TimeAmount(timeMakeready, timeWork);
                            string lastTimeFull = timeOperations.TimeAmount(lastTimeMakeready, lastTimeWorkForDeviation);

                            deviation = timeOperations.TimeDifferentAndNegative(lastTimeFull, timeFull); 
                        }
                        else
                        {
                            string mkDeviation = timeOperations.TimeDifferentAndNegative(lastTimeMakeready, timeMakeready);
                            string wkDeviation = timeOperations.TimeDifferentAndNegative(lastTimeWorkForDeviation, timeWork);

                            deviation = mkDeviation + ", " + wkDeviation;
                        }

                        timeWorkingOut += FullWorkoutTime(startOfShift, sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["counterRepeat"].ToString(),
                            sqlReader["timeMakereadyStop"].ToString(), sqlReader["timeMakereadyStart"].ToString());

                        orders.Add(new Order(
                            sqlReader["machine"].ToString(),
                            sqlReader["numberOfOrder"].ToString(),
                            sqlReader["modification"].ToString(),
                            ordersBase.GetOrderName(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()).ToString(),
                            amountThisOrder,
                            lastCount,
                            lastTimeMakeready,
                            lastTimeWork,
                            timeMakeready,
                            timeWork,
                            Convert.ToInt32(sqlReader["done"]),
                            orderNorm,
                            timeWorkingOut,
                            deviation,
                            sqlReader["counterRepeat"].ToString(),
                            sqlReader["note"].ToString(),
                            sqlReader["privateNote"].ToString()
                            ));
                    }
                }

                Connect.Close();
            }

            return orders;
        }

        private String LastTimeMakereadyStr(String startOfShift, String machine, String numberOrder, String modificationOrder, String counterRepeat)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetLeadTime lastTime = new GetLeadTime(startOfShift, numberOrder, modificationOrder, machine, counterRepeat);

            String lastTimeMakeready = "00:00";

            String lastTimeMakereadyStop = lastTime.GetLastDateTime("timeMakereadyStop");
            String currentTimeMakereadyStart = lastTime.GetCurrentDateTime("timeMakereadyStart");
            String currentTimeMakereadyStop = lastTime.GetCurrentDateTime("timeMakereadyStop");

            String lastTimeMake = timeOperations.DateDifferent(lastTimeMakereadyStop, lastTime.GetLastDateTime("timeMakereadyStart"));
            int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(machine, numberOrder, modificationOrder));

            //разобраться с условиями не отображается остаток времени на приладку... вродебы работает)
            //считает только одно предыдущее значение, т.е. если приладка заняла больше двух смен, тло считает только текущую смнену и предыдыдущую...
            //вероятность того, что такие ситуации будут крайне мала, поэтому оставляю как есть)
            if (lastTimeMakereadyStop != "" && currentTimeMakereadyStart != "" && currentTimeMakereadyStop == "")
            {
                lastTimeMakeready = timeOperations.TimeDifferent(timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime), lastTimeMake);
            }
            else if (lastTimeMakereadyStop == "" && currentTimeMakereadyStart != "" && currentTimeMakereadyStop == "")
            {
                lastTimeMakeready = timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime);
            }
            else if (lastTimeMakereadyStop == "" && currentTimeMakereadyStart != "" && currentTimeMakereadyStop != "")
            {
                lastTimeMakeready = timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime);
            }
            else if (lastTimeMakereadyStop != "" && currentTimeMakereadyStart != "" && currentTimeMakereadyStop != "")
            {
                lastTimeMakeready = timeOperations.TimeDifferent(timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime), lastTimeMake);
            }
            else
                lastTimeMakeready = timeOperations.TotalMinutesToHoursAndMinutesStr(0);

            return lastTimeMakeready;
        }*/














        public Object LoadAllOrdersFromBase(String startOfShift, String category)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            DateTime shiftStart = timeOperations.StringToDateTime(startOfShift);

            List<Order> orders = new List<Order>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE startOfShift = '" + startOfShift + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (category == getInfo.GetCategoryMachine(sqlReader["machine"].ToString()) || category == "")
                    {
                        //sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()
                        GetCountOfDone orderCount = new GetCountOfDone(startOfShift, sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["counterRepeat"].ToString()); // ordersBase.GetValue("counterRepeat").ToString() - раньше этот запрос был

                        int amountThisOrder = Convert.ToInt32(ordersBase.GetAmountOfOrder(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));
                        int lastCount = amountThisOrder - orderCount.OrderCalculate(true, false);

                        if (lastCount < 0)
                            lastCount = 0;

                        int workTime = Convert.ToInt32(ordersBase.GetTimeToWork(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));
                        int orderNorm = 0;
                        int timeWorkingOut = 0;
                        int lastTimeWork = 0;

                        if (workTime != 0)
                        {
                            orderNorm = amountThisOrder * 60 / workTime;
                            timeWorkingOut = Convert.ToInt32(sqlReader["done"]) * 60 / orderNorm;
                            lastTimeWork = (lastCount * 60) / orderNorm;
                        }

                        int lastTimeMakeready = LastTimeMakeready(shiftStart, sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["counterRepeat"].ToString());
                        int timeMakeready = timeOperations.DateDifferenceToMinutes(sqlReader["timeMakereadyStop"].ToString(), sqlReader["timeMakereadyStart"].ToString());
                        int timeWork = timeOperations.DateDifferenceToMinutes(sqlReader["timeToWorkStop"].ToString(), sqlReader["timeToWorkStart"].ToString());
                        int lastTimeWorkForDeviation = 0;

                        if (timeWorkingOut > 0)
                        {
                            lastTimeWorkForDeviation = timeWorkingOut;
                        }
                        else
                        {
                            lastTimeWorkForDeviation = lastTimeWork;
                        }

                        int mkDeviation = timeOperations.MinuteDifference(lastTimeMakeready, timeMakeready, false);
                        int wkDeviation = timeOperations.MinuteDifference(lastTimeWorkForDeviation, timeWork, false);

                        /*int deviation;

                        if (_unionDeviation)
                        {
                            int timeFull = timeMakeready + timeWork;
                            int lastTimeFull = lastTimeMakeready + lastTimeWorkForDeviation;

                            deviation = timeOperations.MinuteDifference(lastTimeFull, timeFull, false);
                        }
                        else
                        {
                            int mkDeviation = timeOperations.MinuteDifference(lastTimeMakeready, timeMakeready, false);
                            int wkDeviation = timeOperations.MinuteDifference(lastTimeWorkForDeviation, timeWork, false);

                            deviation = mkDeviation + ", " + wkDeviation;
                        }*/

                        timeWorkingOut += FullWorkoutTime(startOfShift, sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["counterRepeat"].ToString(),
                            sqlReader["timeMakereadyStop"].ToString(), sqlReader["timeMakereadyStart"].ToString());

                        orders.Add(new Order(
                            Convert.ToInt32(sqlReader["count"]),
                            sqlReader["machine"].ToString(),
                            sqlReader["numberOfOrder"].ToString(),
                            sqlReader["modification"].ToString(),
                            ordersBase.GetOrderName(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()).ToString(),
                            amountThisOrder,
                            lastCount,
                            lastTimeMakeready,
                            lastTimeWork,
                            timeMakeready,
                            timeWork,
                            Convert.ToInt32(sqlReader["done"]),
                            orderNorm,
                            timeWorkingOut,
                            mkDeviation,
                            wkDeviation,
                            sqlReader["counterRepeat"].ToString(),
                            sqlReader["note"].ToString(),
                            sqlReader["privateNote"].ToString()
                            ));
                    }
                }

                Connect.Close();
            }

            return orders;
        }

        private int LastTimeMakeready(DateTime startOfShift, String machine, String numberOrder, String modificationOrder, String counterRepeat)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetLeadTime lastTime = new GetLeadTime(startOfShift.ToString(), numberOrder, modificationOrder, machine, counterRepeat);

            int lastTimeMakeready = 0;

            String lastTimeMakereadyStop = lastTime.GetLastDateTime("timeMakereadyStop");
            String currentTimeMakereadyStart = lastTime.GetCurrentDateTime("timeMakereadyStart");
            String currentTimeMakereadyStop = lastTime.GetCurrentDateTime("timeMakereadyStop");

            int lastTimeMake = timeOperations.DateDifferenceToMinutes(lastTimeMakereadyStop, lastTime.GetLastDateTime("timeMakereadyStart"));
            int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(machine, numberOrder, modificationOrder));

            //разобраться с условиями не отображается остаток времени на приладку... вродебы работает)
            //считает только одно предыдущее значение, т.е. если приладка заняла больше двух смен, тло считает только текущую смнену и предыдыдущую...
            //вероятность того, что такие ситуации будут крайне мала, поэтому оставляю как есть)
            if (lastTimeMakereadyStop != "" && currentTimeMakereadyStart != "" && currentTimeMakereadyStop == "")
            {
                lastTimeMakeready = timeOperations.MinuteDifference(makereadyTime, lastTimeMake, true);
            }
            else if (lastTimeMakereadyStop == "" && currentTimeMakereadyStart != "" && currentTimeMakereadyStop == "")
            {
                lastTimeMakeready = makereadyTime;
            }
            else if (lastTimeMakereadyStop == "" && currentTimeMakereadyStart != "" && currentTimeMakereadyStop != "")
            {
                lastTimeMakeready =makereadyTime;
            }
            else if (lastTimeMakereadyStop != "" && currentTimeMakereadyStart != "" && currentTimeMakereadyStop != "")
            {
                lastTimeMakeready = timeOperations.MinuteDifference(makereadyTime, lastTimeMake, true);
            }
            else
                lastTimeMakeready = 0;

            return lastTimeMakeready;
        }

        private int FullWorkoutTime(String startOfShift, String machine, String numberOrder, String modificationOrder, String counterRepeat, String timeMkrStop, String timeMkrStart)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetLeadTime lastTime = new GetLeadTime(startOfShift, numberOrder, modificationOrder, machine, counterRepeat);

            int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(machine, numberOrder, modificationOrder));
            int mkrStartStop = timeOperations.DateDifferentToMinutes(timeMkrStop, timeMkrStart);

            int mkrWorkingOut = 0;

            String lastTimeMakereadyStop = lastTime.GetLastDateTime("timeMakereadyStop");
            String currentTimeMakereadyStop = lastTime.GetCurrentDateTime("timeMakereadyStop");
            String nextTimeMakereadyStop = lastTime.GetNextDateTime("timeMakereadyStop");

            if (mkrStartStop > makereadyTime)
            {
                mkrWorkingOut = makereadyTime;
            }
            else
            {
                mkrWorkingOut = mkrStartStop;
            }

            String lastTimeMake = timeOperations.DateDifferent(lastTimeMakereadyStop, lastTime.GetLastDateTime("timeMakereadyStart")); ;

            int timeWorkingOut = 0;

            if (lastTimeMakereadyStop != "" && currentTimeMakereadyStop != "" && nextTimeMakereadyStop == "")
            {
                timeWorkingOut += (timeOperations.TimeDifferentToMinutes(timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime), lastTimeMake));
            }
            else if (lastTimeMakereadyStop == "" && currentTimeMakereadyStop != "" && nextTimeMakereadyStop != "")
            {
                timeWorkingOut += mkrWorkingOut;
            }
            else if (lastTimeMakereadyStop == "" && currentTimeMakereadyStop != "" && nextTimeMakereadyStop == "")
            {
                if (ordersBase.GetOrderStatus(machine, numberOrder, modificationOrder) != "1" && lastTime.GetNextDateTime("timeMakereadyStart") == "")
                    timeWorkingOut += makereadyTime;
                else
                    timeWorkingOut += mkrWorkingOut;
            }

            return timeWorkingOut;
        }
    }
}
