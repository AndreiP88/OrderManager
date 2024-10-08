﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetShiftsFromBase
    {
        String executorName;

        //Учитывать в общую выработку смены с нулевой производительностью
        bool _calculateAllPercent = true;

        //Норма рабочего времени
        int _wTime = 680;

        public GetShiftsFromBase(String nameOfExecutor)
        {
            this.executorName = nameOfExecutor;
        }

        /// <summary>
        /// Список смен за указанный период
        /// </summary>
        /// <param name="selectDate"></param>
        /// <returns></returns>
        public List<int> LoadShiftsList(DateTime selectDate)
        {
            List<int> shifts = new List<int>();

            String commandLine;

            //commandLine = "(strftime('%Y,%m', date(substr(startShift, 7, 4) || '-' || substr(startShift, 4, 2) || '-' || substr(startShift, 1, 2))) = '";
            commandLine = "(DATE_FORMAT(STR_TO_DATE(startShift,'%d.%m.%Y %H:%i:%S'), '%Y,%m') = '";
            commandLine += selectDate.ToString("yyyy,MM") + "'";
            commandLine += " AND nameUser = '" + executorName + "')";
            commandLine += " AND stopShift != ''";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                ValueUserBase usersBase = new ValueUserBase();
                GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM shifts WHERE " + commandLine
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    shifts.Add((int)sqlReader["id"]);
                }

                Connect.Close();
            }

            return shifts;
        }

        /// <summary>
        /// Информация об указанной смене
        /// </summary>
        /// <param name="shiftStart"></param>
        /// <returns></returns>
        public async Task<Shifts> LoadCurrentShift(int shiftStartID)
        {
            Shifts shifts = null;

            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetNumberShiftFromTimeStart getNumberShift = new GetNumberShiftFromTimeStart();
            ValueShiftsBase getValueFromShiftsBase = new ValueShiftsBase();
            ValueOrdersBase getOrder = new ValueOrdersBase();

            List<Order> ordersCurrentShift = (List<Order>) await ordersFromBase.LoadAllOrdersFromBase(shiftStartID, "");

            int fullDone = 0;
            int fullTimeWorkingOut = 0;
            int makeReadyCount = 0;

            string machines = "";
            List<String> machinesList = new List<String>();

            for (int i = 0; i < ordersCurrentShift.Count; i++)
            {
                if (!machinesList.Contains(ordersCurrentShift[i].machineOfOrder))
                    machinesList.Add(ordersCurrentShift[i].machineOfOrder);

                GetLeadTime leadTime = new GetLeadTime(shiftStartID, Convert.ToInt32(ordersCurrentShift[i].machineOfOrder), ordersCurrentShift[i].orderIndex, getOrder.GetCounterRepeat(ordersCurrentShift[i].orderIndex));

                if (leadTime.GetCurrentDateTime("timeMakereadyStop") != "" && leadTime.GetNextDateTime("timeMakereadyStart") == "")
                {
                    makeReadyCount++;
                }

                fullDone += ordersCurrentShift[i].done;
                fullTimeWorkingOut += ordersCurrentShift[i].workingOut;
            }

            //Список оборудования 
            for (int i = 0; i < machinesList.Count; i++)
            {
                if (i != machinesList.Count - 1)
                    machines += await getInfo.GetMachineName(machinesList[i]) + ", ";
                else
                    machines += await getInfo.GetMachineName(machinesList[i]) + ".";
            }

            string startShift = getValueFromShiftsBase.GetStartShiftFromID(shiftStartID);

            string date;

            date = Convert.ToDateTime(startShift).ToString("d");
            date += ", " + getNumberShift.NumberShift(startShift);

            shifts = new Shifts(
                shiftStartID,
                date,
                machines,
                dateTimeOperations.DateDifferent(getValueFromShiftsBase.GetStopShiftFromID(shiftStartID), startShift),
                ordersCurrentShift.Count,
                makeReadyCount,
                fullDone,
                fullTimeWorkingOut
                );

            return shifts;
        }

        /// <summary>
        /// Детали смен за выбранный период
        /// </summary>
        /// <param name="selectDate"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<ShiftsDetails> LoadCurrentDateShiftsDetails(DateTime selectDate, string category, CancellationToken token)
        {
            ShiftsDetails shiftsDetails = null;

            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            ValueOrdersBase getOrder = new ValueOrdersBase();
            ValueShiftsBase getValueFromShiftsBase = new ValueShiftsBase();

            int countShifts = 0;
            int workingTime = 0;
            int countEffectiveShift = 0;
            int countOrders = 0;
            int countMakeready = 0;
            float allMakereadyTime = 0;
            int amountAllOrders = 0;
            int allTimeWorkingOut = 0;
            int allTime = 0;
            float allPercentWorkingOut = 0;
            float percent = 0;
            float fullPercentBonus = 0;

            List<int> shifts = new List<int>(LoadShiftsList(selectDate));

            for (int i = 0; i < shifts.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    //MessageBox.Show("Отмена");
                    break;
                }

                List<Order> ordersCurrentShift = (List<Order>) await ordersFromBase.LoadAllOrdersFromBase(shifts[i], category);

                int fullDone = 0;
                int fullTimeWorkingOut = 0;

                for (int j = 0; j < ordersCurrentShift.Count; j++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    GetLeadTime leadTime = new GetLeadTime(shifts[i], Convert.ToInt32(ordersCurrentShift[j].machineOfOrder), ordersCurrentShift[j].orderIndex, getOrder.GetCounterRepeat(ordersCurrentShift[j].orderIndex));

                    if (leadTime.GetCurrentDateTime("timeMakereadyStop") != "" && leadTime.GetNextDateTime("timeMakereadyStart") == "")
                    {
                        countMakeready++;
                    }

                    fullDone += ordersCurrentShift[j].done;
                    fullTimeWorkingOut += ordersCurrentShift[j].workingOut;
                    countOrders++;

                    allMakereadyTime += leadTime.CalculateMakereadyParts(false, true, false);
                }

                int fullTimeWorking = dateTimeOperations.DateDifferentToMinutes(getValueFromShiftsBase.GetStopShiftFromID(shifts[i]), getValueFromShiftsBase.GetStartShiftFromID(shifts[i]));

                if (fullTimeWorkingOut > 0)
                {
                    countEffectiveShift++;
                }

                if (getValueFromShiftsBase.GetCheckFullShift(shifts[i]))
                {
                    workingTime += _wTime;
                }
                else
                {
                    workingTime += fullTimeWorking;
                }

                amountAllOrders += fullDone;
                allTimeWorkingOut += fullTimeWorkingOut;
                allTime += fullTimeWorking;
                allPercentWorkingOut += getPercent.Percent(fullTimeWorkingOut);
                fullPercentBonus += getPercent.GetBonusWorkingOutF(fullTimeWorkingOut);

                countShifts++;

                //token.ThrowIfCancellationRequested();
            }

            int countShiftForPercent;

            if (_calculateAllPercent)
                countShiftForPercent = countShifts;
            else
                countShiftForPercent = countEffectiveShift;

            if (countShiftForPercent == 0)
            {
                percent = 0;
            }
            else
            {
                percent = allPercentWorkingOut / countShiftForPercent;
            }

            shiftsDetails = new ShiftsDetails(
                countShifts,
                workingTime,
                allTime,
                allTimeWorkingOut,
                countOrders,
                countMakeready,
                allMakereadyTime,
                amountAllOrders,
                percent,
                fullPercentBonus
                );

            return shiftsDetails;
        }

        //потом сделать не полную загрузку всех заказов, а рассчитанные значения
        public async Task<ShiftsDetails> LoadCurrentDateShiftsLight(DateTime selectDate, string category, CancellationToken token)
        {
            ShiftsDetails shiftsDetails = null;

            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            ValueOrdersBase getOrder = new ValueOrdersBase();
            ValueShiftsBase getValueFromShiftsBase = new ValueShiftsBase();

            int countShifts = 0;
            int workingTime = 0;
            int countEffectiveShift = 0;
            int countOrders = 0;
            int countMakeready = 0;
            float allMakereadyTime = 0;
            int amountAllOrders = 0;
            int allTimeWorkingOut = 0;
            int allTime = 0;
            float allPercentWorkingOut = 0;
            float percent = 0;
            float fullPercentBonus = 0;

            List<int> shifts = new List<int>(LoadShiftsList(selectDate));

            for (int i = 0; i < shifts.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    //MessageBox.Show("Отмена");
                    break;
                }




                List<Order> ordersCurrentShift = (List<Order>)await ordersFromBase.LoadAllOrdersFromBase(shifts[i], category);

                int fullDone = 0;
                int fullTimeWorkingOut = 0;

                for (int j = 0; j < ordersCurrentShift.Count; j++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    GetLeadTime leadTime = new GetLeadTime(shifts[i], Convert.ToInt32(ordersCurrentShift[j].machineOfOrder), ordersCurrentShift[j].orderIndex, getOrder.GetCounterRepeat(ordersCurrentShift[j].orderIndex));

                    if (leadTime.GetCurrentDateTime("timeMakereadyStop") != "" && leadTime.GetNextDateTime("timeMakereadyStart") == "")
                    {
                        countMakeready++;
                    }

                    fullDone += ordersCurrentShift[j].done;
                    fullTimeWorkingOut += ordersCurrentShift[j].workingOut;
                    countOrders++;

                    allMakereadyTime += leadTime.CalculateMakereadyParts(false, true, false);
                }

                int fullTimeWorking = dateTimeOperations.DateDifferentToMinutes(getValueFromShiftsBase.GetStopShiftFromID(shifts[i]), getValueFromShiftsBase.GetStartShiftFromID(shifts[i]));

                if (fullTimeWorkingOut > 0)
                {
                    countEffectiveShift++;
                }

                if (getValueFromShiftsBase.GetCheckFullShift(shifts[i]))
                {
                    workingTime += _wTime;
                }
                else
                {
                    workingTime += fullTimeWorking;
                }

                amountAllOrders += fullDone;
                allTimeWorkingOut += fullTimeWorkingOut;
                allTime += fullTimeWorking;
                allPercentWorkingOut += getPercent.Percent(fullTimeWorkingOut);
                fullPercentBonus += getPercent.GetBonusWorkingOutF(fullTimeWorkingOut);

                countShifts++;

                //token.ThrowIfCancellationRequested();
            }

            int countShiftForPercent;

            if (_calculateAllPercent)
                countShiftForPercent = countShifts;
            else
                countShiftForPercent = countEffectiveShift;

            if (countShiftForPercent == 0)
            {
                percent = 0;
            }
            else
            {
                percent = allPercentWorkingOut / countShiftForPercent;
            }

            shiftsDetails = new ShiftsDetails(
                countShifts,
                workingTime,
                allTime,
                allTimeWorkingOut,
                countOrders,
                countMakeready,
                allMakereadyTime,
                amountAllOrders,
                percent,
                fullPercentBonus
                );

            return shiftsDetails;
        }

        /// <summary>
        /// Подробная информация о сменах за указанный период
        /// </summary>
        /// <param name="currentDate"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<List<Shifts>> LoadShiftsFromBase(DateTime currentDate)
        {
            List<Shifts> shifts = new List<Shifts>();

            List<int> shiftsList = new List<int>(LoadShiftsList(currentDate));

            for (int i = 0; i < shiftsList.Count; i++)
            {
                //Shifts currentShift = LoadCurrentShift(shiftsList[i]);

                shifts.Add(await LoadCurrentShift(shiftsList[i]));
            }

            return shifts;
        }
    }
}
