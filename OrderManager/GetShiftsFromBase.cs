using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetShiftsFromBase
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;
        String executorName;

        public GetShiftsFromBase(String dBase, String nameOfExecutor)
        {
            this.dataBase = dBase;
            this.executorName = nameOfExecutor;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        public (Object, Object) LoadShiftsFromBase(DateTime currentDate, String category)
        {
            ValueOrdersBase getOrder = new ValueOrdersBase(dataBase);
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetNumberShiftFromTimeStart getNumberShift = new GetNumberShiftFromTimeStart();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();

            List<Shifts> shifts = new List<Shifts>();
            List<ShiftsDetails> shiftsDetails = new List<ShiftsDetails>();

            int countShifts = 0;
            int countOrders = 0;
            int countMakeready = 0;
            int amountAllOrders = 0;
            int allTimeWorkingOut = 0;
            int allTime = 0;
            float allPercentWorkingOut = 0;

            String commandLine;

            commandLine = "(strftime('%Y,%m', date(substr(startShift, 7, 4) || '-' || substr(startShift, 4, 2) || '-' || substr(startShift, 1, 2))) = '";
            commandLine += currentDate.ToString("yyyy,MM") + "'";
            commandLine += " AND nameUser = '" + executorName + "')";
            commandLine += " AND stopShift != ''";


            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                GetValueFromUserBase usersBase = new GetValueFromUserBase(dataBase);
                GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM shifts WHERE " + commandLine
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    GetOrdersFromBase ordersFromBase = new GetOrdersFromBase(dataBase);

                    List<Order> ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(sqlReader["startShift"].ToString(), category);

                    int fullDone = 0;
                    int fullTimeWorkingOut = 0;

                    String machines = "";
                    List<String> machinesList = new List<String>();

                    for (int i = 0; i < ordersCurrentShift.Count; i++)
                    {
                        GetLeadTime leadTime = new GetLeadTime(dataBase, sqlReader["startShift"].ToString(),
                            ordersCurrentShift[i].numberOfOrder, ordersCurrentShift[i].modificationOfOrder, ordersCurrentShift[i].machineOfOrder, getOrder.GetCounterRepeat(ordersCurrentShift[i].machineOfOrder, ordersCurrentShift[i].numberOfOrder, ordersCurrentShift[i].modificationOfOrder));

                        if (leadTime.GetCurrentDateTime("timeMakereadyStop") != "" && leadTime.GetNextDateTime("timeMakereadyStart") == "")
                        {
                            countMakeready++;
                        }

                        if (!machinesList.Contains(ordersCurrentShift[i].machineOfOrder))
                            machinesList.Add(ordersCurrentShift[i].machineOfOrder);

                        fullDone += ordersCurrentShift[i].done;
                        fullTimeWorkingOut += ordersCurrentShift[i].workingOut;
                        countOrders++;
                    }

                    for (int i = 0; i < machinesList.Count; i++)
                    {
                        if (i != machinesList.Count - 1)
                            machines += getInfo.GetMachineName(machinesList[i]) + ", ";
                        else
                            machines += getInfo.GetMachineName(machinesList[i]) + ".";
                    }

                    amountAllOrders += fullDone;
                    allTimeWorkingOut += fullTimeWorkingOut;
                    allTime += dateTimeOperations.DateDifferentToMinutes(sqlReader["stopShift"].ToString(), sqlReader["startShift"].ToString());
                    allPercentWorkingOut += getPercent.Percent(fullTimeWorkingOut);

                    String date;

                    date = Convert.ToDateTime(sqlReader["startShift"]).ToString("d");
                    date += ", " + getNumberShift.NumberShift(sqlReader["startShift"].ToString());

                    shifts.Add(new Shifts(
                        sqlReader["startShift"].ToString(),
                        date,
                        machines,
                        dateTimeOperations.DateDifferent(sqlReader["stopShift"].ToString(), sqlReader["startShift"].ToString()),
                        ordersCurrentShift.Count,
                        fullDone,
                        fullTimeWorkingOut
                        ));

                    countShifts++;
                }

                Connect.Close();
            }

            float percent = 0;

            if (countShifts == 0)
            {
                percent = 0;
            }
            else
            {
                percent = allPercentWorkingOut / countShifts;
            }


            shiftsDetails.Add(new ShiftsDetails(
                countShifts,
                allTime,
                allTimeWorkingOut,
                countOrders,
                countMakeready,
                amountAllOrders,
                percent
                ));

            return (shifts, shiftsDetails);
        }

    }
}
