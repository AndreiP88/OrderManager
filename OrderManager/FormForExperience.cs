using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.Form1;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OrderManager
{
    public partial class FormForExperience : Form
    {
        public FormForExperience()
        {
            InitializeComponent();
        }

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

        List<Order> ordersCurrentShift;

        internal System.Windows.Forms.ListView myListView;

        // Initialize the ListView object with subitems of a different
        // style than the default styles for the ListView.
        private void InitializeListView()
        {

            // Set the Location, View and Width properties for the 
            // ListView object. 
            myListView = new System.Windows.Forms.ListView();
            myListView.Location = new System.Drawing.Point(20, 20);
            myListView.Width = 250;

            // The View property must be set to Details for the 
            // subitems to be visible.
            myListView.View = View.Details;
            myListView.FullRowSelect = true;

            // Each SubItem object requires a column, so add three columns.
            this.myListView.Columns.Add("Key", 50, HorizontalAlignment.Left);
            this.myListView.Columns.Add("A", 100, HorizontalAlignment.Left);
            this.myListView.Columns.Add("B", 100, HorizontalAlignment.Left);

            // Add a ListItem object to the ListView.
            ListViewItem entryListItem = myListView.Items.Add("Items");

            // Set UseItemStyleForSubItems property to false to change 
            // look of subitems.
            entryListItem.UseItemStyleForSubItems = false;

            // Add the expense subitem.
            ListViewItem.ListViewSubItem expenseItem =
                entryListItem.SubItems.Add("Expense");

            // Change the expenseItem object's color and font.
            expenseItem.BackColor = System.Drawing.Color.Red;
            expenseItem.Font = new System.Drawing.Font(
                "Arial", 10, System.Drawing.FontStyle.Italic);

            // Add a subitem called revenueItem 
            ListViewItem.ListViewSubItem revenueItem =
                entryListItem.SubItems.Add("Revenue");

            // Change the revenueItem object's color and font.
            revenueItem.ForeColor = System.Drawing.Color.Blue;
            revenueItem.Font = new System.Drawing.Font(
                "Times New Roman", 10, System.Drawing.FontStyle.Bold);

            // Add the ListView to the form.
            this.Controls.Add(this.myListView);
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

            OrderStatusValue orderStatus = new OrderStatusValue("", "", "", "", "", "", "", "", "", 0, 0, "", Color.Black);

            string newLine = Environment.NewLine;

            string machine = ordersCurrentShift[indexOrder].machineOfOrder;
            string status = valueOrders.GetOrderStatus(getOrders.GetOrderID(ordersCurrentShift[indexOrder].id));

            string shiftStart = Info.startOfShift; //get from Info or user base

            if (plannedWorkingOut)
            {
                shiftStart = startShift.PlanedStartShift(Info.startOfShift); //get from method
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
                timeStartOrder = timeOperations.DateTimeDifferenceMunutes(DateTime.Now.ToString(), (currentLead + 2));
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

            if (facticalTimeMakereadyStop == "" && status == "3")
            {
                orderStatus.mkTimeDifferent = 0;
            }
            else
            {
                orderStatus.mkTimeDifferent = mkTimeDifferent;
            }

            orderStatus.wkTimeDifferent = wkTimeDifferent;

            /*Console.WriteLine("<<<<<" + DateTime.Now.ToString() + ">>>>>");

            Console.WriteLine("Начало выполнения заказа: " + timeStartOrder);
            Console.WriteLine("Время выполнения заказа: " + timeOperations.MinuteToTimeString(currentLead));
            Console.WriteLine("Выработка предыдущих заказов: " + timeOperations.MinuteToTimeString(countPreviusWorkingOut));

            Console.WriteLine("Остаток времеи на приладку: " + timeOperations.MinuteToTimeString(currentLastTimeForMakeready));
            Console.WriteLine("Остаток времеи на выполнение заказа: " + timeOperations.MinuteToTimeString(currentLastTimeForFullWork));
            Console.WriteLine("Отклонение: " + timeOperations.MinuteToTimeString(workTimeDifferent));

            Console.WriteLine("Время завершения приладки: " + timeToEndMK);
            Console.WriteLine("Время завершения работы: " + timeToEndWork);

            Console.WriteLine("Отклонение времени приладки от нормы: " + timeOperations.MinuteToTimeString(mkTimeDifferent));
            Console.WriteLine("Отклонение времени работы от нормы: " + timeOperations.MinuteToTimeString(wkTimeDifferent));*/

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
            }

            return orderStatus;
        }

        private void AddOrdersToListViewFromList()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueSettingsBase valueSettings = new ValueSettingsBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();

            ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(Form1.Info.startOfShift, "");

            /*if (listView1.SelectedItems.Count > 0)
            {
                Info.indexItem = listView1.SelectedIndices[0];
            }*/

            listView1.Items.Clear();

            for (int index = 0; index < ordersCurrentShift.Count; index++)
            {
                string modification = "";
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
                        deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent + statusValue.wkTimeDifferent);
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
                        deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent + statusValue.wkTimeDifferent);
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
                item.SubItems.Add(ordersCurrentShift[index].norm.ToString("N0"));
                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].plannedTimeMakeready) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].plannedTimeWork));
                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].facticalTimeMakeready) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].facticalTimeWork));
                item.SubItems.Add(deviation);
                item.SubItems.Add(ordersCurrentShift[index].done.ToString("N0"));
                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].workingOut));
                item.SubItems.Add(ordersCurrentShift[index].note.ToString());
                item.SubItems.Add(ordersCurrentShift[index].notePrivate.ToString());

                item.ForeColor = color;

                listView1.Items.Add(item);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            InitializeListView();
        }
        

        private void button2_Click(object sender, EventArgs e)
        {
            AddOrdersToListViewFromList();
        }

        private void SetIDToBase(string startOfShift, int id)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET startOfShiftID = @id " +
                    "WHERE startOfShift = @startOfShift";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@id", id.ToString());
                Command.Parameters.AddWithValue("@startOfShift", startOfShift);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private int GetID(string startOfShift)
        {
            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM shifts WHERE startShift = '" + startOfShift + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = Convert.ToInt32(sqlReader["id"].ToString());
                }

                Connect.Close();
            }

            return result;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<String> listStarts = new List<String>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM ordersInProgress"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    listStarts.Add(sqlReader["startOfShift"].ToString());

                    //SetIDToBase(sqlReader["startOfShift"].ToString(), GetID(sqlReader["startOfShift"].ToString()));
                }

                Connect.Close();
            }




























        }
    }
}
