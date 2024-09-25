using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

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

        public int GetOrderInProgressID(int shiftID, int orderIndex, int counterRepeat, int machine)
        {
            int result = 1;

            string value = GetValue("count", shiftID, orderIndex, counterRepeat, machine);

            if (Int32.TryParse(value, out int res))
            {
                result = res;
            }

            return result;
        }

        /// <summary>
        /// Получить тип выполняемой операции
        /// </summary>
        /// <param name="orderInProgressID"></param>
        /// <returns>-1 - индекс не найден, 0 - выполнение заказа, 1 - простой</returns>
        public int GetJobType(int orderInProgressID)
        {
            int result = -1;

            string value = (string)GetValueFromIndex(orderInProgressID, "typeJob");

            if (Int32.TryParse(value, out int res))
            {
                result = res;
            }

            return result;
        }

        public int GetMakereadyConsider(int shiftID, int orderIndex, int counterRepeat, int machine)
        {
            int result = 1;

            string value = GetValue("makereadyConsider", shiftID, orderIndex, counterRepeat, machine);

            if (Int32.TryParse(value, out int res))
            {
                result = res;
            }

            return result;
        }

        public int GetMakereadyPart(int shiftID, int orderIndex, int counterRepeat, int machine)
        {
            int result = -1;

            string value = GetValue("makereadyComplete", shiftID, orderIndex, counterRepeat, machine);

            if (Int32.TryParse(value, out int res))
            {
                result = res;
            }

            return result;
        }

        public int GetMakereadyPart(int orderInProgressID)
        {
            int result = -1;

            string value = (string)GetValueFromIndex(orderInProgressID, "makereadyComplete");

            if (Int32.TryParse(value, out int res))
            {
                result = res;
            }

            return result;
        }

        public String GetNote(int shiftID, int orderIndex, int counterRepeat, int machine)
        {
            return GetValue("note", shiftID, orderIndex, counterRepeat, machine);
        }

        public String GetPrivateNote(int shiftID, int orderIndex, int counterRepeat, int machine)
        {
            return GetValue("privateNote", shiftID, orderIndex, counterRepeat, machine);
        }
        public String GetDone(int shiftID, int orderIndex, int counterRepeat, int machine)
        {
            return GetValue("done", shiftID, orderIndex, counterRepeat, machine);
        }

        /// <summary>
        /// Получить время начала выполнения заказа
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Фактическое время начал выполнения заказа</returns>
        public string GetOrderStartTime(int id)
        {
            string startMK = (string)GetValueFromIndex(id, "timeMakereadyStart");
            string startWK = (string)GetValueFromIndex(id, "timeToWorkStart");

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
            return (string)GetValueFromIndex(id, "timeToWorkStop");
        }

        /// <summary>
        /// Получить время завершения приладки
        /// </summary>
        /// <param name="id">индекс строки в таблице</param>
        /// <returns></returns>
        public string GetTimeToMakereadyStop(int id)
        {
            return (string)GetValueFromIndex(id, "timeMakereadyStop");
        }

        /// <summary>
        /// получить индекс заказа из базы данных "orders" по индексу выполняемого заказа
        /// </summary>
        /// <param name="id">Индекс операции в БД "orderInProgress"</param>
        /// <returns>Индекс заказа</returns>
        public int GetOrderID(int id)
        {
            return (int)GetValueFromIndex(id, "orderID");
        }

        /// <summary>
        /// Получить индекс смены по индексу заказа в работе
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Индекс смены</returns>
        public int GetShiftIDFromOrderInProgressID(int id)
        {
            return (int)GetValueFromIndex(id, "shiftID");
        }

        /// <summary>
        /// Получить индекс оборудования по индексу заказа в работе
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Индекс оборудования</returns>
        public int GetMachineFromOrderInProgressID(int id)
        {
            return (int)GetValueFromIndex(id, "machine");
        }

        /// <summary>
        /// Получить количество прерываний заказа по индексу заказа в работе
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Количество прерываний заказа</returns>
        public int GetCounterRepeatFromOrderInProgressID(int id)
        {
            return (int)GetValueFromIndex(id, "counterRepeat");
        }

        public int LastTimeForMakeready(int shiftID, int machine, int orderIndex, int counterRepeat)
        {
            return LastTimeMakereadyFromTime(shiftID, machine, orderIndex, counterRepeat);
        }

        public int LastTimeForMakeready(int shiftID, int orderInProgressID, int machine, int orderIndex, int counterRepeat)
        {
            return LastTimeMakeready(shiftID, orderInProgressID, machine, orderIndex, counterRepeat);
        }

        /// <summary>
        /// Есть ли заказ в базе
        /// </summary>
        /// <param name="shiftID"></param>
        /// <param name="orderIndex"></param>
        /// <param name="counterRepeat"></param>
        /// <param name="machine"></param>
        /// <returns></returns>
        public bool IsThereAnOrder(int machine, int orderIndex, int counterRepeat, int shiftID = -1)
        {
            bool result = false;

            string cLine = "";
            int count = 0;

            if (shiftID != -1)
            {
                cLine = " AND shiftID = @shiftID";
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(*) FROM ordersInProgress WHERE machine = @machine AND orderID = @id AND counterRepeat = @counterRepeat" + cLine
                };

                Command.Parameters.AddWithValue("@shiftID", shiftID);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@id", orderIndex);
                Command.Parameters.AddWithValue("@machine", machine);

                Connect.Open();
                count = Convert.ToInt32(Command.ExecuteScalar());
                Connect.Close();
            }

            if (count > 0)
            {
                result = true;
            }

            return result;
        }

        private String GetValue(String nameOfColomn, int shiftID, int orderIndex, int counterRepeat, int machine)
        {
            String result = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE ((shiftID = @shiftID AND machine = @machine) AND (orderID = @id AND counterRepeat = @counterRepeat))"
                };
                Command.Parameters.AddWithValue("@shiftID", shiftID);
                Command.Parameters.AddWithValue("@id", orderIndex);
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

        private object GetValueFromIndex(int count, string nameOfColomn)
        {
            object result = null;

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
                    result = sqlReader[nameOfColomn];
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

        public Object GetOrdersFromMachineForTheMonth(DateTime currentDate, string machine)
        {
            int count = 0;
            int amount = 0;

            List<int> ordersCountAmount = new List<int>();

            List<String> orderList = new List<String>();
            orderList.Add("null");

            string commandLine;

            //commandLine = "(strftime('%Y,%m', date(substr(startOfShift, 7, 4) || '-' || substr(startOfShift, 4, 2) || '-' || substr(startOfShift, 1, 2))) = '";
            
            commandLine = "shiftID IN (SELECT id FROM shifts WHERE ";
            commandLine += "DATE_FORMAT(STR_TO_DATE(startShift,'%d.%m.%Y %H:%i:%S'), '%Y,%m') = '";
            commandLine += currentDate.ToString("yyyy,MM") + "')";
            commandLine += " AND machine = '" + machine + "'";

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
                    string line = sqlReader["orderID"].ToString();

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
            commandLine = "shiftID IN (SELECT id FROM shifts WHERE ";
            commandLine += "DATE_FORMAT(STR_TO_DATE(startShift,'%d.%m.%Y %H:%i:%S'), '%Y') = '";
            commandLine += currentDate.ToString("yyyy") + "')";
            commandLine += " AND machine = '" + machine + "'";

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
                    String line = sqlReader["orderID"].ToString();

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

        public Object GetNumbersOrders(String machine, int orderIndex)
        {
            List<String> numbers = new List<String>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE machine = @machine AND orderID = @id"
                };
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@id", orderIndex);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    numbers.Add(sqlReader["count"].ToString());
                }

                Connect.Close();
            }

            return numbers;
        }

        public int GetMakereadyPartFromOrderID(int orderIndex)
        {
            int mkPart = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE count = @id"
                };
                Command.Parameters.AddWithValue("@id", orderIndex);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    mkPart = Convert.ToInt32(sqlReader["makereadyComplete"]);
                }

                Connect.Close();
            }

            return mkPart;
        }
        public int GetMakereadyConsiderFromOrderID(int orderIndex)
        {
            int mkConsider = 1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress WHERE count = @id"
                };
                Command.Parameters.AddWithValue("@id", orderIndex);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    mkConsider = Convert.ToInt32(sqlReader["makereadyConsider"]);
                }

                Connect.Close();
            }

            return mkConsider;
        }

        public void SetMakereadyPart(int orderInProgressID, int value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET makereadyComplete = @value " +
                    "WHERE count = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", orderInProgressID);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        public async Task<object> LoadAllOrdersFromBase(int shiftID, string category)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();

            //DateTime shiftStart = timeOperations.StringToDateTime(startOfShift);

            List<Order> orders = new List<Order>();

            try
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    await Connect.OpenAsync();
                    MySqlCommand Command = new MySqlCommand
                    {
                        Connection = Connect,
                        //CommandText = @"SELECT * FROM ordersInProgress WHERE shiftID = '" + shiftID + "'"
                        CommandText = @"SELECT * FROM allOrdersInJob WHERE shiftID = '" + shiftID + "'"
                    };
                    DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                    while (await sqlReader.ReadAsync())
                    {
                        if (category == await getInfo.GetCategoryMachine(sqlReader["machine"].ToString()) || category == "" || category == "-1")
                        {
                            //sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()
                            GetCountOfDone orderCount = new GetCountOfDone(shiftID, (int)sqlReader["orderID"], (int)sqlReader["counterRepeat"]); // ordersBase.GetValue("counterRepeat").ToString() - раньше этот запрос был

                            //int amountThisOrder = Convert.ToInt32(ordersBase.GetAmountOfOrder((int)sqlReader["orderID"]));
                            int amountThisOrder = (int)sqlReader["amountOfOrder"];
                            int lastCount = amountThisOrder - orderCount.OrderCalculate(true, false);

                            if (lastCount < 0)
                                lastCount = 0;

                            //int workTime = Convert.ToInt32(ordersBase.GetTimeToWork((int)sqlReader["orderID"]));
                            int workTime = (int)sqlReader["timeToWork"];
                            int orderNorm = 0;
                            int timeWorkingOut = 0;
                            int lastTimeWork = 0;

                            if (workTime != 0)
                            {
                                orderNorm = amountThisOrder * 60 / workTime;
                                timeWorkingOut = (int)sqlReader["done"] * 60 / orderNorm;
                                lastTimeWork = (lastCount * 60) / orderNorm;
                            }

                            int lastTimeMakeready = LastTimeMakeready(shiftID, (int)sqlReader["count"], (int)sqlReader["machine"], (int)sqlReader["orderID"], (int)sqlReader["counterRepeat"]);
                            int timeMakeready = timeOperations.DateDifferenceToMinutes(sqlReader["timeMakereadyStop"].ToString(), sqlReader["timeMakereadyStart"].ToString());
                            int timeWork = timeOperations.DateDifferenceToMinutes(sqlReader["timeToWorkStop"].ToString(), sqlReader["timeToWorkStart"].ToString());
                            int lastTimeWorkForDeviation = 0;

                            int makereadyConsider = getOrders.GetMakereadyConsider(shiftID, (int)sqlReader["orderID"], (int)sqlReader["counterRepeat"], (int)sqlReader["machine"]);

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

                            timeWorkingOut += FullWorkoutTime(shiftID, (int)sqlReader["count"], (int)sqlReader["machine"], (int)sqlReader["orderID"], (int)sqlReader["counterRepeat"],
                                sqlReader["timeMakereadyStop"].ToString(), sqlReader["timeMakereadyStart"].ToString());

                            orders.Add(new Order(
                                Convert.ToInt32(sqlReader["count"]),
                                (int)sqlReader["orderID"],
                                sqlReader["machine"].ToString(),
                                /*ordersBase.GetOrderNumber((int)sqlReader["orderID"]),
                                ordersBase.GetOrderModification((int)sqlReader["orderID"]),
                                ordersBase.GetOrderName((int)sqlReader["orderID"]).ToString(),*/
                                sqlReader["numberOfOrder"].ToString(),
                                sqlReader["modification"].ToString(),
                                sqlReader["nameOfOrder"].ToString(),
                                amountThisOrder,
                                lastCount,
                                lastTimeMakeready * makereadyConsider,
                                lastTimeWork,
                                timeMakeready,
                                timeWork,
                                Convert.ToInt32(sqlReader["done"]),
                                orderNorm,
                                timeWorkingOut,
                                mkDeviation,
                                wkDeviation,
                                (int)sqlReader["counterRepeat"],
                                sqlReader["note"].ToString(),
                                sqlReader["privateNote"].ToString()
                                ));
                        }
                    }

                    await Connect.CloseAsync();
                }

                return orders;
            }
            catch (SqlException sqlEx)
            {
                LogException.WriteLine("LoadAllOrdersFromBase: " + string.Format("MySQL #{0}: {1}", sqlEx.Number, sqlEx.Message));
                throw new ApplicationException(string.Format("MySQL #{0}: {1}", sqlEx.Number, sqlEx.Message));
            }
            catch (Exception ex)
            {
                LogException.WriteLine("LoadAllOrdersFromBase: " + ex.Message);
                throw new ApplicationException(ex.Message);
            }
        }

        /// <summary>
        /// Оставшееся время на приладку для указанной смены и заказа
        /// </summary>
        /// <param name="shiftID"></param>
        /// <param name="orderIndex"></param>
        /// <param name="counterRepeat"></param>
        /// <returns></returns>
        private int LastTimeMakeready(int shiftID, int orderInProgressID, int machine, int orderIndex, int counterRepeat)
        {
            int lastTimeMakeready = 0;

            int makereadyPart = GetMakereadyPartFromOrderID(orderInProgressID);
            
            if (makereadyPart == -2)
            {
                lastTimeMakeready = LastTimeMakereadyFromTime(shiftID, machine, orderIndex, counterRepeat);
            }//возможно еще условие понадобится
            else
            {
                ValueOrdersBase ordersBase = new ValueOrdersBase();
                GetLeadTime leadTime = new GetLeadTime(shiftID, machine, orderIndex, counterRepeat);

                int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(orderIndex));

                int makereadySummPreviousParts = leadTime.CalculateMakereadyParts(true, false, false);

                lastTimeMakeready = makereadyTime - makereadySummPreviousParts;
            }
            
            return lastTimeMakeready;
        }

        private int LastTimeMakereadyFromTime(int shiftID, int machine, int orderIndex, int counterRepeat)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetLeadTime lastTime = new GetLeadTime(shiftID, machine, orderIndex, counterRepeat);

            int lastTimeMakeready = 0;

            String lastTimeMakereadyStop = lastTime.GetLastDateTime("timeMakereadyStop");
            String currentTimeMakereadyStart = lastTime.GetCurrentDateTime("timeMakereadyStart");
            String currentTimeMakereadyStop = lastTime.GetCurrentDateTime("timeMakereadyStop");

            int lastTimeMake = timeOperations.DateDifferenceToMinutes(lastTimeMakereadyStop, lastTime.GetLastDateTime("timeMakereadyStart"));
            int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(orderIndex));

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
                lastTimeMakeready = makereadyTime;
            }
            else if (lastTimeMakereadyStop != "" && currentTimeMakereadyStart != "" && currentTimeMakereadyStop != "")
            {
                lastTimeMakeready = timeOperations.MinuteDifference(makereadyTime, lastTimeMake, true);
            }
            else
                lastTimeMakeready = 0;

            /*Console.WriteLine("Итог: " + lastTimeMakeready + "; Last: " + lastTimeMakereadyStop + "; Current Start: " +
                currentTimeMakereadyStart + "; Current Stop: " + currentTimeMakereadyStop + "; Time: " + startOfShift.ToString());*/

            return lastTimeMakeready;
        }

        private int FullWorkoutTime(int shiftID, int orderInProgressID, int machine, int orderIndex, int counterRepeat, string timeMkrStop, string timeMkrStart)
        {
            int timeWorkingOut = 0;

            int makereadyPart = GetMakereadyPartFromOrderID(orderInProgressID);
            int makereadyConsider = GetMakereadyConsiderFromOrderID(orderInProgressID);

            switch (makereadyPart)
            {
                case -2:
                    timeWorkingOut = FullWorkoutTimeFromTime(shiftID, machine, orderIndex, counterRepeat, timeMkrStop, timeMkrStart);
                    break;
                case -1:
                    timeWorkingOut = 0;
                    break;
                default:
                    timeWorkingOut = makereadyPart * makereadyConsider;
                    break;
            }

            /*if (makereadyPart == -2)
            {
                timeWorkingOut = FullWorkoutTimeFromTime(shiftID, machine, orderIndex, counterRepeat, timeMkrStop, timeMkrStart);
            }
            else if (makereadyPart == -1)
            {
                timeWorkingOut = 0;
            }
            else
            {
                ValueOrdersBase ordersBase = new ValueOrdersBase();
                GetDateTimeOperations timeOperations = new GetDateTimeOperations();
                GetLeadTime leadTime = new GetLeadTime(shiftID, machine, orderIndex, counterRepeat);

                int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(orderIndex));

                //timeWorkingOut = makereadyTime - makereadyPart;
                timeWorkingOut = makereadyPart;
            }*/

            return timeWorkingOut;
        }

        private int FullWorkoutTimeFromTime(int shiftID, int machine, int orderIndex, int counterRepeat, string timeMkrStop, string timeMkrStart)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetLeadTime lastTime = new GetLeadTime(shiftID, machine, orderIndex, counterRepeat);

            int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(orderIndex));
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

            String lastTimeMake = timeOperations.DateDifferent(lastTimeMakereadyStop, lastTime.GetLastDateTime("timeMakereadyStart"));

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
                if (ordersBase.GetOrderStatus(orderIndex) != "1" && lastTime.GetNextDateTime("timeMakereadyStart") == "")
                    timeWorkingOut += makereadyTime;
                else
                    timeWorkingOut += mkrWorkingOut;
            }

            return timeWorkingOut;
        }
    }
}
