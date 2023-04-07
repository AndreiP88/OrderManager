using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static OrderManager.Form1;

namespace OrderManager
{
    public partial class FormOneShiftDetails : Form
    {
        String timeShiftStart;
        bool adminMode;

        public FormOneShiftDetails(bool aMode, String shiftStart)
        {
            InitializeComponent();
            this.timeShiftStart = shiftStart;
            this.adminMode = aMode;
        }

        int fullTimeWorkingOut;
        int fullDone;

        List<Order> ordersCurrentShift;

        class OrderStatusValue
        {
            public string statusStr;
            public string caption_1;
            public string value_1;
            public string caption_2;
            public string value_2;
            public string caption_3;
            public string value_3;
            public string caption_4;
            public string value_4;
            public int mkTimeDifferent;
            public int wkTimeDifferent;
            public string message;
            public Color color;

            public OrderStatusValue(string statusStrVal, string captionVal_1, string valueVal_1, string captionVal_2, string valueVal_2,
                string captionVal_3, string valueVal_3, string captionVal_4, string valueVal_4,
                int mkTimeDifferentVal, int wkTimeDifferentVal, string messageVal, Color colorVal)
            {
                this.statusStr = statusStrVal;
                this.caption_1 = captionVal_1;
                this.value_1 = valueVal_1;
                this.caption_2 = captionVal_2;
                this.value_2 = valueVal_2;
                this.caption_3 = captionVal_3;
                this.value_3 = valueVal_3;
                this.caption_4 = captionVal_4;
                this.value_4 = valueVal_4;
                this.mkTimeDifferent = mkTimeDifferentVal;
                this.wkTimeDifferent = wkTimeDifferentVal;
                this.message = messageVal;
                this.color = colorVal;
            }
        }

        String GetParametersLine()
        {
            String pLine = "";

            if (this.WindowState == FormWindowState.Normal)
            {
                pLine += this.Location.X.ToString() + ";";
                pLine += this.Location.Y.ToString() + ";";
                pLine += this.Width.ToString() + ";";
                pLine += this.Height.ToString() + ";";
            }
            else
            {
                pLine += this.RestoreBounds.Location.X.ToString() + ";";
                pLine += this.RestoreBounds.Location.Y.ToString() + ";";
                pLine += this.RestoreBounds.Width.ToString() + ";";
                pLine += this.RestoreBounds.Height.ToString() + ";";
            }

            pLine += this.WindowState.ToString() + ";";

            for (int i = 0; i < listView1.Columns.Count; i++)
                pLine += listView1.Columns[i].Width.ToString() + ";";

            return pLine;
        }

        private void ApplyParameterLine(String pLine)
        {
            String[] parameter = pLine.Split(';');

            if (pLine != "" && parameter.Length == listView1.Columns.Count + 6)
            {
                this.Location = new Point(Convert.ToInt32(parameter[0]), Convert.ToInt32(parameter[1]));

                if (parameter[4] == "Normal")
                {
                    WindowState = FormWindowState.Normal;
                    this.Width = Convert.ToInt32(parameter[2]);
                    this.Height = Convert.ToInt32(parameter[3]);
                }

                if (parameter[4] == "Maximized")
                {
                    WindowState = FormWindowState.Maximized;
                }

                if (parameter[4] == "Minimized")
                    WindowState = FormWindowState.Minimized;

                for (int i = 0; i < listView1.Columns.Count; i++)
                    listView1.Columns[i].Width = Convert.ToInt32(parameter[5 + i]);
            }

        }

        private void SaveParameterToBase(String nameForm)
        {
            ValueSettingsBase setting = new ValueSettingsBase();

            if (Form1.Info.nameOfExecutor != "")
                setting.UpdateParameterLine(Form1.Info.nameOfExecutor, nameForm, GetParametersLine());
            else
                setting.UpdateParameterLine("0", nameForm, GetParametersLine());
        }

        private void LoadParametersFromBase(String nameForm)
        {
            ValueSettingsBase getSettings = new ValueSettingsBase();

            if (Form1.Info.nameOfExecutor != "")
                ApplyParameterLine(getSettings.GetParameterLine(Form1.Info.nameOfExecutor, nameForm));
            else
                ApplyParameterLine(getSettings.GetParameterLine("0", nameForm));
        }

        private bool PrivateData(String shiftStart, String userID)
        {
            bool result = false;

            ValueShiftsBase getUserShift = new ValueShiftsBase();

            if (getUserShift.GetNameUserFromStartShift(shiftStart) == userID)
            {
                result = true;
            }

            else
            {
                result = false;
            }

            return result;
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



            return result;
        }

        private OrderStatusValue GetWorkingOutTimeForSelectedOrder(int indexOrder, bool plannedWorkingOut)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase valueOrders = new ValueOrdersBase();
            GetNumberShiftFromTimeStart startShift = new GetNumberShiftFromTimeStart();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            ValueInfoBase infoBase = new ValueInfoBase();

            OrderStatusValue orderStatus = new OrderStatusValue("", "", "", "", "", "", "", "", "", 0, 0, "", Color.Black);

            string newLine = Environment.NewLine;

            string machine = ordersCurrentShift[indexOrder].machineOfOrder;
            string status = valueOrders.GetOrderStatus(getOrders.GetOrderID(ordersCurrentShift[indexOrder].id));

            string shiftStart = timeShiftStart; //get from Info or user base

            if (plannedWorkingOut)
            {
                shiftStart = startShift.PlanedStartShift(shiftStart); //get from method
            }

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
                facticalTimeToWorkStop = facticalTimeMakereadyStop;
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

            bool print = false;

            if (print)
            {
                Console.WriteLine("<<<<<" + DateTime.Now.ToString() + ">>>>>");

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

                orderStatus.caption_2 = "Плановая выработка: ";
                orderStatus.value_2 = planedCoutOrder.ToString("N0");

                orderStatus.caption_3 = "Планирумое время завершения: ";
                orderStatus.value_3 = timeToEndWork;

                orderStatus.message = orderStatus.caption_1 + orderStatus.value_1 + newLine +
                    orderStatus.caption_2 + orderStatus.value_2 + newLine +
                    orderStatus.caption_3 + orderStatus.value_3;

                if (facticalTimeMakereadyStop != "")
                {
                    orderStatus.mkTimeDifferent = mkTimeDifferent;
                }

                orderStatus.wkTimeDifferent = wkTimeDifferent;

                bool active = Convert.ToBoolean(infoBase.GetActiveOrder(machine));

                if (!active)
                {
                    string timeStoFromWorkingOut = timeOperations.DateTimeAmountMunutes(timeStartOrder, ordersCurrentShift[indexOrder].workingOut);
                    int timeWorkingOutDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeStoFromWorkingOut, facticalTimeToWorkStop);
                    //int timeWorkingOutDifferent = timeOperations.DateDifferenceToMinutesAndNegative(timeStoFromWorkingOut, DateTime.Now.ToString());

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

                orderStatus.wkTimeDifferent = workTimeDifferent;
            }

            return orderStatus;
        }

        private void AddOrdersToListViewFromList()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();
            ValueSettingsBase valueSettings = new ValueSettingsBase();

            ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(timeShiftStart, "");

            fullTimeWorkingOut = 0;
            fullDone = 0;

            listView1.Items.Clear();

            for (int index = 0; index < ordersCurrentShift.Count; index++)
            {
                String modification = "";
                if (ordersCurrentShift[index].modificationOfOrder != "")
                    modification = " (" + ordersCurrentShift[index].modificationOfOrder + ")";

                string deviation = "<>";

                Color color = Color.DarkRed;

                int typeLoad = valueSettings.GetTypeLoadDeviationToMainLV(Info.nameOfExecutor);
                int typeView = valueSettings.GetTypeViewDeviationToMainLV(Info.nameOfExecutor);

                if (typeLoad == 0)
                {
                    OrderStatusValue statusValue = GetWorkingOutTimeForSelectedOrder(index, true);

                    color = statusValue.color;

                    if (typeView == 0)
                    {
                        deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent) + ", " + timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                    }
                    else
                    {
                        //deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent + statusValue.wkTimeDifferent);
                        deviation = timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                    }
                }
                else if (typeLoad == 1)
                {
                    OrderStatusValue statusValue = GetWorkingOutTimeForSelectedOrder(index, false);

                    color = statusValue.color;

                    if (typeView == 0)
                    {
                        deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent) + ", " + timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                    }
                    else
                    {
                        //deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent + statusValue.wkTimeDifferent);
                        deviation = timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                    }
                }
                else if (typeLoad == 2)
                {
                    if ((ordersCurrentShift[index].mkDeviation + ordersCurrentShift[index].wkDeviation) > 0)
                    {
                        color = Color.SeaGreen;
                    }
                    else
                    {
                        color = Color.DarkRed;
                    }

                    if (typeView == 0)
                    {
                        deviation = timeOperations.MinuteToTimeString(ordersCurrentShift[index].mkDeviation) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].wkDeviation);
                    }
                    else
                    {
                        deviation = timeOperations.MinuteToTimeString(ordersCurrentShift[index].mkDeviation + ordersCurrentShift[index].wkDeviation);
                    }
                }

                ListViewItem item = new ListViewItem();

                item.Name = ordersCurrentShift[index].numberOfOrder.ToString();
                item.Text = (index + 1).ToString();
                item.SubItems.Add(getInfo.GetMachineName(ordersCurrentShift[index].machineOfOrder.ToString()));
                item.SubItems.Add(ordersCurrentShift[index].numberOfOrder.ToString() + modification);
                item.SubItems.Add(ordersCurrentShift[index].nameOfOrder.ToString());
                item.SubItems.Add(ordersCurrentShift[index].amountOfOrder.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].lastCount.ToString("N0"));
                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].plannedTimeMakeready) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].plannedTimeWork));
                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].facticalTimeMakeready) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].facticalTimeWork));
                //item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].mkDeviation) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].wkDeviation));
                item.SubItems.Add(deviation);
                item.SubItems.Add(ordersCurrentShift[index].done.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].norm.ToString("N0"));
                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].workingOut));
                item.SubItems.Add(ordersCurrentShift[index].note.ToString());

                item.ForeColor = color;

                if (PrivateData(timeShiftStart, Form1.Info.nameOfExecutor))
                    item.SubItems.Add(ordersCurrentShift[index].notePrivate);

                listView1.Items.Add(item);

                fullTimeWorkingOut += ordersCurrentShift[index].workingOut;
                fullDone += ordersCurrentShift[index].done;
            }
        }

        private void ViewDetailsForUser()
        {
            GetDateTimeOperations dtOperations = new GetDateTimeOperations();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            ValueUserBase getUser = new ValueUserBase();
            ValueShiftsBase getShift = new ValueShiftsBase();


            label4.Text = getUser.GetNameUser(getShift.GetNameUserFromStartShift(timeShiftStart));
            label5.Text = timeShiftStart;
            label6.Text = getShift.GetStopShift(timeShiftStart);

            label10.Text = dtOperations.TotalMinutesToHoursAndMinutesStr(fullTimeWorkingOut);
            label11.Text = getPercent.PercentString(fullTimeWorkingOut);
            label12.Text = fullDone.ToString("N0");


        }

        private void LoadOrdersFromBase()
        {
            ClearAll();
            AddOrdersToListViewFromList();
            ViewDetailsForUser();
        }

        private void ClearAll()
        {
            listView1.Items.Clear();

            label4.Text = "";
            label5.Text = "";
            label6.Text = "";

            label10.Text = "";
            label11.Text = "";
            label12.Text = "";
        }

        private void DetailsOrder()
        {
            if (listView1.SelectedItems.Count != 0)
            {
                ValueInfoBase getInfo = new ValueInfoBase();

                FormAddCloseOrder form;

                form = new FormAddCloseOrder(adminMode, timeShiftStart,
                ordersCurrentShift[listView1.SelectedIndices[0]].numberOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].modificationOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].counterRepeat);

                form.ShowDialog();
                LoadOrdersFromBase();
            }
        }

        private void LoadOrderNote()
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            FormPrivateNote form;

            form = new FormPrivateNote(timeShiftStart,
                ordersCurrentShift[listView1.SelectedIndices[0]].numberOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].modificationOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].counterRepeat);

            form.ShowDialog();
            LoadOrdersFromBase();
        }

        private void FormOneShiftDetails_Load(object sender, EventArgs e)
        {
            LoadParametersFromBase("oneShiftDetails");
            LoadOrdersFromBase();
        }

        private void FormOneShiftDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveParameterToBase("oneShiftDetails");
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            DetailsOrder();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = listView1.SelectedItems.Count == 0;

            if (!PrivateData(timeShiftStart, Form1.Info.nameOfExecutor))
                e.Cancel = true;
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetailsOrder();
        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadOrderNote();
        }

        private void listView1_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (e.Item != null)
            {

                //toolTip1.Show(e.Item.Index + ": " + listView1.Items[e.Item.Index].SubItems[12].Text, listView1);
                //toolTip1.Show(e.Item.SubItems[12].Text, listView1);
            }
        }
    }
}
