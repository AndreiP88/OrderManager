using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace OrderManager
{
    internal class GetOrdersFromBase
    {
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

        public String GetNote(String startShift, String numberOfOrder, String modification, String counterRepeat)
        {
            return GetValue("note", startShift, numberOfOrder, modification, counterRepeat);
        }

        public String GetPrivateNote(String startShift, String numberOfOrder, String modification, String counterRepeat)
        {
            return GetValue("privateNote", startShift, numberOfOrder, modification, counterRepeat);
        }
        public String GetDone(String startShift, String numberOfOrder, String modification, String counterRepeat)
        {
            return GetValue("done", startShift, numberOfOrder, modification, counterRepeat);
        }

        private String GetValue(String nameOfColomn, String startShift, String numberOfOrder, String modification, String counterRepeat)
        {
            String result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE (startOfShift = @startOfShift AND numberOfOrder = @numberOfOrder) AND (modification = @modification AND counterRepeat = @counterRepeat)"
                };
                Command.Parameters.AddWithValue("@startOfShift", startShift);
                Command.Parameters.AddWithValue("@numberOfOrder", numberOfOrder);
                Command.Parameters.AddWithValue("@modification", modification);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
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

        public Object LoadAllOrdersFromBase(String startOfShift, String category)
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

                        int workTime = Convert.ToInt32(ordersBase.GetTimeToWork(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));
                        int orderNorm = 0;
                        int timeWorkingOut = 0;
                        String lastTimeWork = "00:00";

                        if (workTime != 0)
                        {
                            orderNorm = amountThisOrder * 60 / Convert.ToInt32(ordersBase.GetTimeToWork(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));
                            timeWorkingOut = Convert.ToInt32(sqlReader["done"]) * 60 / orderNorm;
                            lastTimeWork = timeOperations.TotalMinutesToHoursAndMinutesStr((lastCount * 60) / orderNorm);
                        }

                        String lastTimeMakeready = LastTimeMakereadyStr(startOfShift, sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["counterRepeat"].ToString());

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
                            timeOperations.DateDifferent(sqlReader["timeMakereadyStop"].ToString(), sqlReader["timeMakereadyStart"].ToString()).ToString(),
                            timeOperations.DateDifferent(sqlReader["timeToWorkStop"].ToString(), sqlReader["timeToWorkStart"].ToString()).ToString(),
                            Convert.ToInt32(sqlReader["done"]),
                            orderNorm,
                            timeWorkingOut,
                            "<>",
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

            String lastTimeMake = timeOperations.DateDifferent(lastTime.GetLastDateTime("timeMakereadyStop"), lastTime.GetLastDateTime("timeMakereadyStart"));
            int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(machine, numberOrder, modificationOrder));

            //разобраться с условиями не отображается остаток времени на приладку... вродебы работает)
            //считает только одно предыдущее значение, т.е. если приладка заняла больше двух смен, тло считает только текущую смнену и предыдыдущую...
            //вероятность того, что такие ситуации будут крайне мала, поэтому оставляю как есть)
            if (lastTime.GetLastDateTime("timeMakereadyStop") != "" && lastTime.GetCurrentDateTime("timeMakereadyStart") != "" && lastTime.GetCurrentDateTime("timeMakereadyStop") == "")
            {
                lastTimeMakeready = timeOperations.TimeDifferent(timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime), lastTimeMake);
            }
            else if (lastTime.GetLastDateTime("timeMakereadyStop") == "" && lastTime.GetCurrentDateTime("timeMakereadyStart") != "" && lastTime.GetCurrentDateTime("timeMakereadyStop") == "")
            {
                lastTimeMakeready = timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime);
            }
            else if (lastTime.GetLastDateTime("timeMakereadyStop") == "" && lastTime.GetCurrentDateTime("timeMakereadyStart") != "" && lastTime.GetCurrentDateTime("timeMakereadyStop") != "")
            {
                lastTimeMakeready = timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime);
            }
            else if (lastTime.GetLastDateTime("timeMakereadyStop") != "" && lastTime.GetCurrentDateTime("timeMakereadyStart") != "" && lastTime.GetCurrentDateTime("timeMakereadyStop") != "")
            {
                lastTimeMakeready = timeOperations.TimeDifferent(timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime), lastTimeMake);
            }
            else
                lastTimeMakeready = timeOperations.TotalMinutesToHoursAndMinutesStr(0);

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

            if (mkrStartStop > makereadyTime)
            {
                mkrWorkingOut = makereadyTime;
            }
            else
            {
                mkrWorkingOut = mkrStartStop;
            }

            String lastTimeMake = timeOperations.DateDifferent(lastTime.GetLastDateTime("timeMakereadyStop"), lastTime.GetLastDateTime("timeMakereadyStart")); ;

            int timeWorkingOut = 0;

            if (lastTime.GetLastDateTime("timeMakereadyStop") != "" && lastTime.GetCurrentDateTime("timeMakereadyStop") != "" && lastTime.GetNextDateTime("timeMakereadyStop") == "")
            {
                timeWorkingOut += (timeOperations.TimeDifferentToMinutes(timeOperations.TotalMinutesToHoursAndMinutesStr(makereadyTime), lastTimeMake));
            }
            else if (lastTime.GetLastDateTime("timeMakereadyStop") == "" && lastTime.GetCurrentDateTime("timeMakereadyStop") != "" && lastTime.GetNextDateTime("timeMakereadyStop") != "")
            {
                timeWorkingOut += mkrWorkingOut;
            }
            else if (lastTime.GetLastDateTime("timeMakereadyStop") == "" && lastTime.GetCurrentDateTime("timeMakereadyStop") != "" && lastTime.GetNextDateTime("timeMakereadyStop") == "")
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
