using System;
using System.Collections.Generic;
using System.Drawing;

namespace OrderManager
{
    internal class GetWorkingOutTime
    {
        int shiftID;
        List <Order> ordersCurrentShift;
        public GetWorkingOutTime(int startShiftID, List<Order> ordersCurrent)
        {
            this.shiftID = startShiftID;
            this.ordersCurrentShift = ordersCurrent;
        }

        private int CountWorkingOutOrders(int indexOrder, string machine)
        {
            int result = 0;

            for (int i = 0; i < indexOrder; i++)
            {
                if (ordersCurrentShift[i].machineOfOrder == machine || machine == "")
                {
                    result += ordersCurrentShift[i].workingOut;
                }
            }

            return result;
        }

        private int CountPreviusOutages()
        {
            int result = 0;

            //

            return result;
        }

        //Переделать
        public OrderStatusValue GetWorkingOutTimeForSelectedOrder(int indexOrder, bool plannedWorkingOut, int orderRegistrationType, int typeJob = 0)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetNumberShiftFromTimeStart startShift = new GetNumberShiftFromTimeStart();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            OrderStatusValue orderStatus = new OrderStatusValue("", "", "", "", "", "", "", "", "", 0, 0, 0, "", Color.Black);

            string newLine = Environment.NewLine;

            string machine = ordersCurrentShift[indexOrder].machineOfOrder;
            //string status = valueOrders.GetOrderStatus(getOrders.GetOrderID(ordersCurrentShift[indexOrder].id));
            int loadStatus = ordersCurrentShift[indexOrder].Status;
            int status = loadStatus;

            int chevkIntoWorkingOutIdletime = 1;

            if (typeJob == 0)
            {
                if (orderRegistrationType == 1)
                {
                    switch (loadStatus)
                    {
                        case 1:
                            status = 3;
                            break;
                        case 2:
                            status = 3;
                            break;
                        default:
                            status = loadStatus;
                            break;
                    }
                }
            }
            else
            {
                ValueIdletimeBase valueIdletime = new ValueIdletimeBase();

                chevkIntoWorkingOutIdletime = valueIdletime.GetIdletimeCheckIntoWorkingOut(ordersCurrentShift[indexOrder].orderIndex);

                switch (loadStatus)
                {
                    case 1:
                        status = 3;
                        break;
                    case 2:
                        status = 4;
                        break;
                    default:
                        break;
                }
            }

            string shiftStart = shiftsBase.GetStartShiftFromID(shiftID); //get from Info or user base

            if (plannedWorkingOut)
            {
                shiftStart = startShift.PlanedStartShift(shiftStart); //get from method
            }

            int doneFromPreviewShifts = ordersCurrentShift[indexOrder].amountOfOrder - ordersCurrentShift[indexOrder].lastCount;

            int workTime = timeOperations.DateDifferenceToMinutes(DateTime.Now.ToString(), shiftStart); //общее время с начала смены
            int countPreviusWorkingOut = CountWorkingOutOrders(indexOrder, machine);// считать до указанного индекса
            int countPreviusOutages = CountPreviusOutages(); // еще проработка требуется
            int countWorkingOut = countPreviusWorkingOut + countPreviusOutages;

            int lastTimeForMK = ordersCurrentShift[indexOrder].plannedTimeMakeready;
            int lastTimeForWK = ordersCurrentShift[indexOrder].plannedTimeWork;
            int fullTimeForWork = lastTimeForMK + lastTimeForWK;

            string facticalTimeMakereadyStop = getOrders.GetTimeToMakereadyStop(ordersCurrentShift[indexOrder].id);
            string facticalTimeToWorkStop = getOrders.GetTimeToWorkStop(ordersCurrentShift[indexOrder].id);

            if (facticalTimeToWorkStop == "")
            {
                if (Convert.ToInt32(infoBase.GetCurrentOrderID(machine)) == ordersCurrentShift[indexOrder].orderIndex)
                {
                    facticalTimeToWorkStop = DateTime.Now.ToString();
                }
                else
                {
                    facticalTimeToWorkStop = facticalTimeMakereadyStop;
                }             
            }

            int currentLead;
            string timeStartOrder;

            if (plannedWorkingOut)
            {
                currentLead = workTime - countWorkingOut; //время выполнения текущего заказа
                timeStartOrder = timeOperations.DateTimeAmountMunutes(shiftStart, countWorkingOut);
            }
            else
            {
                currentLead = ordersCurrentShift[indexOrder].facticalTimeMakeready + ordersCurrentShift[indexOrder].facticalTimeWork;
                //timeStartOrder = timeOperations.DateTimeDifferenceMunutes(DateTime.Now.ToString(), (currentLead + 2));
                timeStartOrder = getOrders.GetOrderStartTime(ordersCurrentShift[indexOrder].id);
            }

            int currentLastTimeForMakeready = timeOperations.MinuteDifference(lastTimeForMK, currentLead, false); //остаток времеи на приладку только положительные
            int currentLastTimeForFullWork = timeOperations.MinuteDifference(fullTimeForWork * chevkIntoWorkingOutIdletime, currentLead, false); //остаток времеи на выполнение заказа только положительные

            int timeForWork = timeOperations.MinuteDifference(currentLead, lastTimeForMK, true); //время выполнения закзаза (без приладки) > 0

            int planedCoutOrder = timeForWork * ordersCurrentShift[indexOrder].norm / 60;

            string timeToEndMK = timeOperations.DateTimeAmountMunutes(timeStartOrder, lastTimeForMK);
            string timeToEndWork = timeOperations.DateTimeAmountMunutes(timeStartOrder, fullTimeForWork);

            int mkTimeDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeToEndMK, facticalTimeMakereadyStop);
            int wkTimeDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeToEndWork, facticalTimeToWorkStop);

            //int workTimeDifferent = timeOperations.MinuteDifference(ordersCurrentShift[indexOrder].workingOut, currentLead, false); //отклонение выработки от фактического времени выполнения заказа

            int workTimeDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeOperations.DateTimeAmountMunutes(timeStartOrder, ordersCurrentShift[indexOrder].workingOut), facticalTimeToWorkStop);

            /*if (facticalTimeMakereadyStop == "" && (status == "3" || status == "4"))
            {
                orderStatus.mkTimeDifferent = 0;
            } 
            else
            {
                orderStatus.mkTimeDifferent = mkTimeDifferent;
            }*/

            /*orderStatus.wkTimeDifferent = wkTimeDifferent;*/

            bool print = false;

            if (print)
            {
                Console.WriteLine("<<<<<" + DateTime.Now.ToString() + ">>>>>");

                Console.WriteLine("Номер заказа: " + ordersCurrentShift[indexOrder].numberOfOrder + ", Type: " + typeJob);
                Console.WriteLine("ID: " + ordersCurrentShift[indexOrder].id + ", Load status: " + loadStatus + ", Status: " + status);

                Console.WriteLine("Начало выполнения заказа: " + timeStartOrder);
                Console.WriteLine("Время выполнения заказа: " + timeOperations.MinuteToTimeString(currentLead));
                Console.WriteLine("Выработка предыдущих заказов: " + timeOperations.MinuteToTimeString(countPreviusWorkingOut));

                Console.WriteLine("Остаток времеи на приладку: " + timeOperations.MinuteToTimeString(currentLastTimeForMakeready));
                Console.WriteLine("Остаток времеи на выполнение заказа: " + timeOperations.MinuteToTimeString(currentLastTimeForFullWork));
                Console.WriteLine("Отклонение: " + timeOperations.MinuteToTimeString(workTimeDifferent));

                Console.WriteLine("Время завершения приладки: " + timeToEndMK);
                Console.WriteLine("Время завершения работы: " + timeToEndWork);

                Console.WriteLine("Отклонение времени приладки от нормы: " + timeOperations.MinuteToTimeString(mkTimeDifferent));
                Console.WriteLine("Отклонение времени работы от нормы: " + timeOperations.MinuteToTimeString(wkTimeDifferent));
            }


            /*string timeToEndMK = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), currentLastTimeForMakeready - lastTimeForMK);
            string timeToEndWork = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), currentLastTimeForFullWork - fullTimeForWork);*/

            if (status == 1 || status == 2)
            {
                orderStatus.statusStr = "приладка заказа";

                if (currentLastTimeForMakeready < 0)
                {
                    orderStatus.caption_1 = "Отставание: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForMakeready * (-1));
                    orderStatus.color = Color.DarkRed;
                }
                else
                {
                    orderStatus.caption_1 = "Остаток времени на приладку: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);
                    orderStatus.color = Color.Goldenrod;
                }

                orderStatus.caption_2 = "Остаток времени для выполнение заказа: ";
                orderStatus.value_2 = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);

                orderStatus.caption_3 = "Планирумое время завершения приладки: ";
                orderStatus.value_3 = timeToEndMK;

                orderStatus.caption_4 = "Планирумое время завершения заказа: ";
                orderStatus.value_4 = timeToEndWork;

                orderStatus.message = orderStatus.caption_1 + orderStatus.value_1 + newLine +
                    orderStatus.caption_2 + orderStatus.value_2 + newLine +
                    orderStatus.caption_3 + orderStatus.value_3 + newLine +
                    orderStatus.caption_4 + orderStatus.value_4;

                orderStatus.mkTimeDifferent = mkTimeDifferent;

                orderStatus.fullTimeDifferent = currentLastTimeForFullWork;
            }

            if (status == 3)
            {
                orderStatus.statusStr = "выполняется";

                if (currentLastTimeForFullWork < 0)
                {
                    orderStatus.caption_1 = "Отставание: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForFullWork * (-1));
                    orderStatus.color = Color.DarkRed;
                }
                else
                {
                    orderStatus.caption_1 = "Остаток времени: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
                    orderStatus.color = Color.Goldenrod;
                }

                if (currentLastTimeForMakeready > 0)
                {
                    orderStatus.caption_2 = "Приладка. Осталось: ";
                    orderStatus.value_2 = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);
                }
                else
                {
                    orderStatus.caption_2 = "Плановая выработка: ";
                    orderStatus.value_2 = planedCoutOrder.ToString("N0") + " (" + (doneFromPreviewShifts + planedCoutOrder).ToString("N0") + ")";
                }

                orderStatus.caption_3 = "Планирумое время завершения: ";
                orderStatus.value_3 = timeToEndWork;

                orderStatus.caption_4 = "Планирумое время завершения: ";
                orderStatus.value_4 = timeToEndWork;

                orderStatus.message = orderStatus.caption_1 + orderStatus.value_1 + newLine +
                    orderStatus.caption_2 + orderStatus.value_2 + newLine +
                    orderStatus.caption_3 + orderStatus.value_3;

                if (facticalTimeMakereadyStop != "")
                {
                    orderStatus.mkTimeDifferent = mkTimeDifferent;
                }

                /*if (facticalTimeToWorkStop != "")
                {
                    orderStatus.wkTimeDifferent = wkTimeDifferent;
                }*/

                orderStatus.wkTimeDifferent = wkTimeDifferent;
                orderStatus.fullTimeDifferent = currentLastTimeForFullWork;

                string timeStoFromWorkingOut = timeOperations.DateTimeAmountMunutes(timeStartOrder, ordersCurrentShift[indexOrder].workingOut);
                int timeWorkingOutDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeStoFromWorkingOut, facticalTimeToWorkStop);
                //int timeWorkingOutDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeStoFromWorkingOut, DateTime.Now.ToString());

                if (shiftsBase.CheckShiftActivity(shiftID))//активна ли смена
                {
                    bool active = infoBase.GetActiveOrder(machine);

                    if (!active)
                    {
                        orderStatus.wkTimeDifferent = timeWorkingOutDifferent;
                        orderStatus.fullTimeDifferent = timeWorkingOutDifferent;

                        if (timeWorkingOutDifferent > 0)
                        {
                            orderStatus.color = Color.SeaGreen;
                        }
                        else
                        {
                            orderStatus.color = Color.DarkRed;
                        }
                    }
                }
                else
                {
                    orderStatus.wkTimeDifferent = timeWorkingOutDifferent;
                    orderStatus.fullTimeDifferent = timeWorkingOutDifferent;

                    if (timeWorkingOutDifferent > 0)
                    {
                        orderStatus.color = Color.SeaGreen;
                    }
                    else
                    {
                        orderStatus.color = Color.DarkRed;
                    }
                }

                //orderStatus.fullTimeDifferent = currentLastTimeForFullWork;
            }

            if (status == 4)
            {
                orderStatus.statusStr = "завершено";

                if (workTimeDifferent < 0)
                {
                    orderStatus.caption_1 = "Отставание: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(workTimeDifferent * (-1));
                    orderStatus.color = Color.DarkRed;
                }
                else
                {
                    orderStatus.caption_1 = "Опережение: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(workTimeDifferent);
                    orderStatus.color = Color.SeaGreen;
                }

                orderStatus.message = orderStatus.caption_1 + orderStatus.value_1;

                if (facticalTimeMakereadyStop != "")
                {
                    orderStatus.mkTimeDifferent = mkTimeDifferent;
                }

                /*if (facticalTimeToWorkStop != "")
                {
                    orderStatus.wkTimeDifferent = workTimeDifferent;
                }*/

                orderStatus.wkTimeDifferent = workTimeDifferent;

                orderStatus.fullTimeDifferent = workTimeDifferent;
            }

            return orderStatus;
        }






        public OrderStatusValue GetWorkingOutTimeForSelectedOrderOLD(int indexOrder, bool plannedWorkingOut, int orderRegistrationType, int typeJob = 0)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase valueOrders = new ValueOrdersBase();
            GetNumberShiftFromTimeStart startShift = new GetNumberShiftFromTimeStart();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            OrderStatusValue orderStatus = new OrderStatusValue("", "", "", "", "", "", "", "", "", 0, 0, 0, "", Color.Black);

            string newLine = Environment.NewLine;

            string machine = ordersCurrentShift[indexOrder].machineOfOrder;
            string status = valueOrders.GetOrderStatus(getOrders.GetOrderID(ordersCurrentShift[indexOrder].id));

            if (orderRegistrationType == 1)
            {
                if (status == "1" || status == "2" || typeJob == 1)//тут менял
                {
                    status = "3";
                }
            }

            string shiftStart = shiftsBase.GetStartShiftFromID(shiftID); //get from Info or user base

            if (plannedWorkingOut)
            {
                shiftStart = startShift.PlanedStartShift(shiftStart); //get from method
            }

            int doneFromPreviewShifts = ordersCurrentShift[indexOrder].amountOfOrder - ordersCurrentShift[indexOrder].lastCount;

            int workTime = timeOperations.DateDifferenceToMinutes(DateTime.Now.ToString(), shiftStart); //общее время с начала смены
            int countPreviusWorkingOut = CountWorkingOutOrders(indexOrder, machine);// считать до указанного индекса
            int countPreviusOutages = CountPreviusOutages(); // еще проработка требуется
            int countWorkingOut = countPreviusWorkingOut + countPreviusOutages;

            int lastTimeForMK = ordersCurrentShift[indexOrder].plannedTimeMakeready;
            int lastTimeForWK = ordersCurrentShift[indexOrder].plannedTimeWork;
            int fullTimeForWork = lastTimeForMK + lastTimeForWK;

            string facticalTimeMakereadyStop = getOrders.GetTimeToMakereadyStop(ordersCurrentShift[indexOrder].id);
            string facticalTimeToWorkStop = getOrders.GetTimeToWorkStop(ordersCurrentShift[indexOrder].id);

            if (facticalTimeToWorkStop == "")
            {
                if (Convert.ToInt32(infoBase.GetCurrentOrderID(machine)) == ordersCurrentShift[indexOrder].orderIndex)
                {
                    facticalTimeToWorkStop = DateTime.Now.ToString();
                }
                else
                {
                    facticalTimeToWorkStop = facticalTimeMakereadyStop;
                }
            }

            int currentLead;
            string timeStartOrder;

            if (plannedWorkingOut)
            {
                currentLead = workTime - countWorkingOut; //время выполнения текущего заказа
                timeStartOrder = timeOperations.DateTimeAmountMunutes(shiftStart, countWorkingOut);
            }
            else
            {
                currentLead = ordersCurrentShift[indexOrder].facticalTimeMakeready + ordersCurrentShift[indexOrder].facticalTimeWork;
                //timeStartOrder = timeOperations.DateTimeDifferenceMunutes(DateTime.Now.ToString(), (currentLead + 2));
                timeStartOrder = getOrders.GetOrderStartTime(ordersCurrentShift[indexOrder].id);
            }

            int currentLastTimeForMakeready = timeOperations.MinuteDifference(lastTimeForMK, currentLead, false); //остаток времеи на приладку только положительные
            int currentLastTimeForFullWork = timeOperations.MinuteDifference(fullTimeForWork, currentLead, false); //остаток времеи на выполнение заказа только положительные

            int timeForWork = timeOperations.MinuteDifference(currentLead, lastTimeForMK, true); //время выполнения закзаза (без приладки) > 0

            int planedCoutOrder = timeForWork * ordersCurrentShift[indexOrder].norm / 60;

            string timeToEndMK = timeOperations.DateTimeAmountMunutes(timeStartOrder, lastTimeForMK);
            string timeToEndWork = timeOperations.DateTimeAmountMunutes(timeStartOrder, fullTimeForWork);

            int mkTimeDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeToEndMK, facticalTimeMakereadyStop);
            int wkTimeDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeToEndWork, facticalTimeToWorkStop);

            //int workTimeDifferent = timeOperations.MinuteDifference(ordersCurrentShift[indexOrder].workingOut, currentLead, false); //отклонение выработки от фактического времени выполнения заказа

            int workTimeDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeOperations.DateTimeAmountMunutes(timeStartOrder, ordersCurrentShift[indexOrder].workingOut), facticalTimeToWorkStop);

            /*if (facticalTimeMakereadyStop == "" && (status == "3" || status == "4"))
            {
                orderStatus.mkTimeDifferent = 0;
            } 
            else
            {
                orderStatus.mkTimeDifferent = mkTimeDifferent;
            }*/

            /*orderStatus.wkTimeDifferent = wkTimeDifferent;*/

            bool print = true;

            if (print)
            {
                Console.WriteLine("<<<<<" + DateTime.Now.ToString() + ">>>>>");

                Console.WriteLine("Номер заказа: " + ordersCurrentShift[indexOrder].numberOfOrder);
                Console.WriteLine("ID: " + ordersCurrentShift[indexOrder].id + " Status: " + status);

                Console.WriteLine("Начало выполнения заказа: " + timeStartOrder);
                Console.WriteLine("Время выполнения заказа: " + timeOperations.MinuteToTimeString(currentLead));
                Console.WriteLine("Выработка предыдущих заказов: " + timeOperations.MinuteToTimeString(countPreviusWorkingOut));

                Console.WriteLine("Остаток времеи на приладку: " + timeOperations.MinuteToTimeString(currentLastTimeForMakeready));
                Console.WriteLine("Остаток времеи на выполнение заказа: " + timeOperations.MinuteToTimeString(currentLastTimeForFullWork));
                Console.WriteLine("Отклонение: " + timeOperations.MinuteToTimeString(workTimeDifferent));

                Console.WriteLine("Время завершения приладки: " + timeToEndMK);
                Console.WriteLine("Время завершения работы: " + timeToEndWork);

                Console.WriteLine("Отклонение времени приладки от нормы: " + timeOperations.MinuteToTimeString(mkTimeDifferent));
                Console.WriteLine("Отклонение времени работы от нормы: " + timeOperations.MinuteToTimeString(wkTimeDifferent));
            }


            /*string timeToEndMK = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), currentLastTimeForMakeready - lastTimeForMK);
            string timeToEndWork = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), currentLastTimeForFullWork - fullTimeForWork);*/

            if (status == "1" || status == "2")
            {
                orderStatus.statusStr = "приладка заказа";

                if (currentLastTimeForMakeready < 0)
                {
                    orderStatus.caption_1 = "Отставание: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForMakeready * (-1));
                    orderStatus.color = Color.DarkRed;
                }
                else
                {
                    orderStatus.caption_1 = "Остаток времени на приладку: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);
                    orderStatus.color = Color.Goldenrod;
                }

                orderStatus.caption_2 = "Остаток времени для выполнение заказа: ";
                orderStatus.value_2 = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);

                orderStatus.caption_3 = "Планирумое время завершения приладки: ";
                orderStatus.value_3 = timeToEndMK;

                orderStatus.caption_4 = "Планирумое время завершения заказа: ";
                orderStatus.value_4 = timeToEndWork;

                orderStatus.message = orderStatus.caption_1 + orderStatus.value_1 + newLine +
                    orderStatus.caption_2 + orderStatus.value_2 + newLine +
                    orderStatus.caption_3 + orderStatus.value_3 + newLine +
                    orderStatus.caption_4 + orderStatus.value_4;

                orderStatus.mkTimeDifferent = mkTimeDifferent;

                orderStatus.fullTimeDifferent = currentLastTimeForFullWork;
            }

            if (status == "3")
            {
                orderStatus.statusStr = "заказ выполняется";

                if (currentLastTimeForFullWork < 0)
                {
                    orderStatus.caption_1 = "Отставание: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForFullWork * (-1));
                    orderStatus.color = Color.DarkRed;
                }
                else
                {
                    orderStatus.caption_1 = "Остаток времени: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
                    orderStatus.color = Color.Goldenrod;
                }

                if (currentLastTimeForMakeready > 0)
                {
                    orderStatus.caption_2 = "Приладка заказа. Осталось: ";
                    orderStatus.value_2 = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);
                }
                else
                {
                    orderStatus.caption_2 = "Плановая выработка: ";
                    orderStatus.value_2 = planedCoutOrder.ToString("N0") + " (" + (doneFromPreviewShifts + planedCoutOrder).ToString("N0") + ")";
                }

                orderStatus.caption_3 = "Планирумое время завершения заказа: ";
                orderStatus.value_3 = timeToEndWork;

                orderStatus.caption_4 = "Планирумое время завершения заказа: ";
                orderStatus.value_4 = timeToEndWork;

                orderStatus.message = orderStatus.caption_1 + orderStatus.value_1 + newLine +
                    orderStatus.caption_2 + orderStatus.value_2 + newLine +
                    orderStatus.caption_3 + orderStatus.value_3;

                if (facticalTimeMakereadyStop != "")
                {
                    orderStatus.mkTimeDifferent = mkTimeDifferent;
                }

                /*if (facticalTimeToWorkStop != "")
                {
                    orderStatus.wkTimeDifferent = wkTimeDifferent;
                }*/

                orderStatus.wkTimeDifferent = wkTimeDifferent;
                orderStatus.fullTimeDifferent = currentLastTimeForFullWork;

                string timeStoFromWorkingOut = timeOperations.DateTimeAmountMunutes(timeStartOrder, ordersCurrentShift[indexOrder].workingOut);
                int timeWorkingOutDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeStoFromWorkingOut, facticalTimeToWorkStop);
                //int timeWorkingOutDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeStoFromWorkingOut, DateTime.Now.ToString());

                if (shiftsBase.CheckShiftActivity(shiftID))//активна ли смена
                {
                    bool active = infoBase.GetActiveOrder(machine);

                    if (!active)
                    {
                        orderStatus.wkTimeDifferent = timeWorkingOutDifferent;
                        orderStatus.fullTimeDifferent = timeWorkingOutDifferent;

                        if (timeWorkingOutDifferent > 0)
                        {
                            orderStatus.color = Color.SeaGreen;
                        }
                        else
                        {
                            orderStatus.color = Color.DarkRed;
                        }
                    }
                }
                else
                {
                    orderStatus.wkTimeDifferent = timeWorkingOutDifferent;
                    orderStatus.fullTimeDifferent = timeWorkingOutDifferent;

                    if (timeWorkingOutDifferent > 0)
                    {
                        orderStatus.color = Color.SeaGreen;
                    }
                    else
                    {
                        orderStatus.color = Color.DarkRed;
                    }
                }

                //orderStatus.fullTimeDifferent = currentLastTimeForFullWork;
            }

            if (status == "4")
            {
                orderStatus.statusStr = "заказ завершен";

                if (workTimeDifferent < 0)
                {
                    orderStatus.caption_1 = "Отставание: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(workTimeDifferent * (-1));
                    orderStatus.color = Color.DarkRed;
                }
                else
                {
                    orderStatus.caption_1 = "Опережение: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(workTimeDifferent);
                    orderStatus.color = Color.SeaGreen;
                }

                orderStatus.message = orderStatus.caption_1 + orderStatus.value_1;

                if (facticalTimeMakereadyStop != "")
                {
                    orderStatus.mkTimeDifferent = mkTimeDifferent;
                }

                /*if (facticalTimeToWorkStop != "")
                {
                    orderStatus.wkTimeDifferent = workTimeDifferent;
                }*/

                orderStatus.wkTimeDifferent = workTimeDifferent;

                orderStatus.fullTimeDifferent = workTimeDifferent;
            }

            return orderStatus;
        }
    }
}
