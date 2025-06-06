﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.DataBaseReconnect;

namespace OrderManager
{
    public partial class FormFullListOrders : Form
    {
        bool detailsLoad;
        int orderID;
        int DefaultMachine;

        CancellationTokenSource cancelTokenSource;
        CancellationTokenSource cancelTokenSourceYear;
        CancellationTokenSource cancelTokenSourceMachine;
        /*String orderrMachineLoad;
        String orderNumberLoad;
        String orderModificationLoad;
        public FormFullListOrders(bool details, String orderMachine, String orderNumber, String orderModification)
        {
            InitializeComponent();

            this.orderID = "";
            this.detailsLoad = details;
            this.orderrMachineLoad = orderMachine;
            this.orderNumberLoad = orderNumber;
            this.orderModificationLoad = orderModification;
        }*/

        public FormFullListOrders(bool details, int orderIDFromOrdersBase, int defaultMachine = -1)
        {
            InitializeComponent();

            this.detailsLoad = details;
            this.orderID = orderIDFromOrdersBase;
            this.DefaultMachine = defaultMachine;
        }

        class OrdersFromShift
        {
            int _shiftID;
            float _workingOut;
            List<ListViewItem> _listViewItems;

            public OrdersFromShift(int shiftID)
            {
                _shiftID = shiftID;
                _workingOut = 0;
                _listViewItems = new List<ListViewItem>();
            }

            public int ShiftID
            {
                get => _shiftID;
                //set => _shiftID = value;
            }

            public float WorkingOut
            {
                get => _workingOut;
                set => _workingOut = value;
            }

            public List<ListViewItem> ListViewItems
            {
                get => _listViewItems;
                set => _listViewItems = value;
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

        private void SetItemsComboBox()
        {
            DateTime dateTime = DateTime.Now;

            //comboBox1.SelectedIndex = comboBox1.FindString(dateTime.Year.ToString());
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(dateTime.Year.ToString());
            comboBox2.SelectedIndex = dateTime.Month - 1;
        }

        private async Task LoadMachine(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            ValueInfoBase getInfo = new ValueInfoBase();

                            int indexDeafaultMachineItem = -1;

                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT DISTINCT id FROM machines"
                                };
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync()) // считываем и вносим в комбобокс список заголовков
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    int idMachine = (int)sqlReader["id"];

                                    Invoke(new Action(async () =>
                                    {
                                        comboBox3.Items.Add(await getInfo.GetMachineName(idMachine.ToString()));

                                        if (DefaultMachine == idMachine)
                                            indexDeafaultMachineItem = comboBox3.Items.Count - 1;
                                    }));
                                }

                                Connect.Close();
                            }
                            Invoke(new Action(() =>
                            {
                                if (comboBox3.Items.Count > 0)
                                {
                                    if (indexDeafaultMachineItem != -1 && indexDeafaultMachineItem < comboBox3.Items.Count)
                                    {
                                        comboBox3.SelectedIndex = indexDeafaultMachineItem;
                                    }
                                    else
                                    {
                                        comboBox3.SelectedIndex = 0;
                                    }
                                }
                            }));

                            reconnectionRequired = false;
                        }
                        catch (Exception ex)
                        {
                            LogException.WriteLine(ex.StackTrace + "; " + ex.Message);

                            dialog = DataBaseReconnectionRequest(ex.Message);

                            if (dialog == DialogResult.Retry)
                            {
                                reconnectionRequired = true;
                            }
                            if (dialog == DialogResult.Abort || dialog == DialogResult.Cancel)
                            {
                                reconnectionRequired = false;
                                Application.Exit();
                            }
                        }
                    }
                }
                while (reconnectionRequired);
            }, token);
        }

        private async Task LoadYears(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            ValueShiftsBase shiftsBase = new ValueShiftsBase();

                            List<string> years = new List<string>();

                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT DISTINCT startShift FROM shifts"
                                };
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync()) // считываем и вносим в комбобокс список заголовков
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    string year = Convert.ToDateTime(sqlReader["startShift"]).ToString("yyyy");

                                    if (years.IndexOf(year) == -1)
                                        years.Add(year);
                                }

                                Connect.Close();
                            }

                            for (int i = years.Count - 1; i >= 0; i--)
                            {
                                if (token.IsCancellationRequested)
                                {
                                    break;
                                }

                                Invoke(new Action(() =>
                                {
                                    comboBox1.Items.Add(years[i].ToString());
                                }));
                            }

                            reconnectionRequired = false;
                        }
                        catch (Exception ex)
                        {
                            LogException.WriteLine(ex.StackTrace + "; " + ex.Message);

                            dialog = DataBaseReconnectionRequest(ex.Message);

                            if (dialog == DialogResult.Retry)
                            {
                                reconnectionRequired = true;
                            }
                            if (dialog == DialogResult.Abort || dialog == DialogResult.Cancel)
                            {
                                reconnectionRequired = false;
                                Application.Exit();
                            }
                        }
                    }
                }
                while (reconnectionRequired);
            });
        }

        private async Task LoadOrdersFromBase()
        {
            if ((comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1 && comboBox3.SelectedIndex != -1) || detailsLoad)
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                await LoadOrders(cancelTokenSource.Token);
            }
        }

        private async Task LoadOrders(CancellationToken token)
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            ValueUserBase usersBase = new ValueUserBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetNumberShiftFromTimeStart getNumberShift = new GetNumberShiftFromTimeStart();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();
            GetPercentFromWorkingOut getPercentWorkingOut = new GetPercentFromWorkingOut();

            await Task.Run(async () =>
            {
                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            Invoke(new Action(() =>
                            {
                                listView1.Items.Clear();
                            }));

                            string tmpNumberOrders = "";
                            int tmpShiftsID = -1;
                            int tmpAmountOrder = 0;
                            int index = 0;
                            int countOrders = 0;
                            int amountAllOrders = 0;

                            int indexOrdersFromShift = -1;

                            string year = "";
                            string month = "";
                            string machine = "";

                            string selectYear = "2024";
                            string selectMonth = "1";
                            string machineName = "";
                            string searchValue = "";

                            List<OrdersFromShift> ordersFromShift = new List<OrdersFromShift>();

                            Invoke(new Action(() =>
                            {
                                selectYear = comboBox1.Text;
                                selectMonth = (comboBox2.SelectedIndex + 1).ToString("D2");
                                machineName = comboBox3.Text;
                                searchValue = textBox1.Text;
                            }));

                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                string commandLine;
                                //commandLine = "strftime('%Y,%m', date(substr(startOfShift, 7, 4) || '-' || substr(startOfShift, 4, 2) || '-' || substr(startOfShift, 1, 2))) = '";
                                commandLine = "shiftID IN (SELECT id FROM shifts WHERE ";
                                commandLine += "DATE_FORMAT(STR_TO_DATE(startShift,'%d.%m.%Y %H:%i:%S'), '%Y,%m') = '";
                                commandLine += selectYear + "," + selectMonth + "')";

                                string commandText;
                                if (detailsLoad == true)
                                {
                                    commandText = "SELECT * FROM allordersinjob WHERE orderID = '" + orderID + "' ";
                                }
                                else
                                {
                                    commandText = "SELECT * FROM allordersinjob WHERE " + commandLine + " AND machine = '" + await getInfo.GetMachineFromName(machineName) + "' ";
                                }

                                commandText += "ORDER BY IF (timeMakereadyStart <> '', STR_TO_DATE (timeMakereadyStart, '%H:%i %d.%m.%Y'), STR_TO_DATE (timeToWorkStart, '%H:%i %d.%m.%Y')) ASC";

                                await Connect.OpenAsync();

                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @commandText
                                    //CommandText = @"SELECT * FROM ordersInProgress WHERE " + commandLine + " AND machine = '" + comboBox3.Text + "'"
                                };
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync())
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    string orderNumber = ordersBase.GetOrderNumber((int)sqlReader["orderID"]);

                                    if (orderNumber.Contains(searchValue))
                                    {
                                        int shiftID = (int)sqlReader["shiftID"];
                                        string shiftStart = shiftsBase.GetStartShiftFromID(shiftID);

                                        year = Convert.ToDateTime(shiftStart).ToString("yyyy");
                                        month = Convert.ToDateTime(shiftStart).ToString("MMMM");
                                        machine = sqlReader["machine"].ToString();

                                        //отображение имени исполнителя не в каждой строке, а только в начале смены
                                        //возможно сделать, как опцию
                                        string date, name;
                                        if (tmpShiftsID == shiftID)
                                        {
                                            date = "";
                                            name = "";

                                            //workingOutputs += ((float)sqlReader["workingOut"]);
                                        }
                                        else
                                        {
                                            date = Convert.ToDateTime(shiftStart).ToString("d");
                                            date += ", " + getNumberShift.NumberShift(shiftStart);
                                            name = usersBase.GetNameUser(sqlReader["executor"].ToString());

                                            ordersFromShift.Add(new OrdersFromShift(shiftID));
                                            indexOrdersFromShift = ordersFromShift.Count - 1;
                                        }

                                        ordersFromShift[indexOrdersFromShift].WorkingOut += (float)Convert.ToDouble(sqlReader["workingOut"]);

                                        //отображение общего количества тиража не в каждой строке, а только в первой
                                        string amountOrder;
                                        if (tmpAmountOrder == Convert.ToInt32(sqlReader["amountOfOrder"]) && tmpNumberOrders == sqlReader["orderID"].ToString())
                                        {
                                            amountOrder = "";
                                        }
                                        else
                                        {
                                            amountOrder = Convert.ToInt32(sqlReader["amountOfOrder"]).ToString("N0");
                                        }

                                        string modification = sqlReader["modification"].ToString();

                                        if (modification != "")
                                            modification = " (" + modification + ")";

                                        if (tmpNumberOrders != sqlReader["orderID"].ToString())
                                            countOrders++;

                                        amountAllOrders += Convert.ToInt32(sqlReader["done"]);

                                        ListViewItem item = new ListViewItem();

                                        item.Name = sqlReader["orderID"].ToString();
                                        item.Text = (index + 1).ToString();
                                        item.SubItems.Add(date);
                                        item.SubItems.Add(name);
                                        item.SubItems.Add(sqlReader["numberOfOrder"].ToString() + modification);
                                        item.SubItems.Add(sqlReader["nameOfOrder"].ToString());
                                        item.SubItems.Add(timeOperations.DateDifferent(sqlReader["timeMakereadyStop"].ToString(), sqlReader["timeMakereadyStart"].ToString()));
                                        item.SubItems.Add(timeOperations.DateDifferent(sqlReader["timeToWorkStop"].ToString(), sqlReader["timeToWorkStart"].ToString()));
                                        item.SubItems.Add(amountOrder);
                                        item.SubItems.Add(Convert.ToInt32(sqlReader["done"]).ToString("N0"));
                                        item.SubItems.Add(timeOperations.MinuteToTimeString(Convert.ToInt32(sqlReader["workingOut"])));
                                        item.SubItems.Add("");
                                        item.SubItems.Add(sqlReader["note"].ToString());

                                        if (token.IsCancellationRequested)
                                        {
                                            break;
                                        }

                                        ordersFromShift[indexOrdersFromShift].ListViewItems.Add(item);

                                        Invoke(new Action(() =>
                                        {
                                            //listView1?.Items?.Add(item);
                                        }));

                                        index++;

                                        tmpShiftsID = shiftID;
                                        tmpNumberOrders = sqlReader["orderID"].ToString();
                                        tmpAmountOrder = Convert.ToInt32(sqlReader["amountOfOrder"]);
                                    }

                                }
                                await Connect.CloseAsync();
                            }

                            for (int i = 0; i < ordersFromShift.Count; i++)
                            {
                                Color color = Color.White;

                                if (i % 2 != 0)
                                {
                                    color = Color.Silver;
                                }

                                for (int j = 0; j < ordersFromShift[i].ListViewItems.Count; j++)
                                {
                                    ListViewItem item = ordersFromShift[i].ListViewItems[j];

                                    if (j == ordersFromShift[i].ListViewItems.Count - 1 && !detailsLoad)
                                    {
                                        item.SubItems[10].Text = (timeOperations.MinuteToTimeString((int)ordersFromShift[i].WorkingOut) + " (" + getPercentWorkingOut.PercentString((int)ordersFromShift[i].WorkingOut) + ")");
                                    }

                                    item.BackColor = color;

                                    Invoke(new Action(() =>
                                    {
                                        listView1?.Items?.Add(item);
                                    }));
                                }
                            }

                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            Invoke(new Action(async () =>
                            {
                                if (detailsLoad == true)
                                {
                                    comboBox1.Text = year;
                                    comboBox2.Text = month;
                                    comboBox3.Text = await getInfo.GetMachineName(machine);
                                }

                                label7.Text = countOrders.ToString();
                                label8.Text = amountAllOrders.ToString("N0");
                            }));

                            reconnectionRequired = false;
                        }
                        catch (Exception ex)
                        {
                            LogException.WriteLine(ex.StackTrace + "; " + ex.Message);

                            dialog = DialogResult.Retry;// DataBaseReconnectionRequest(ex.Message);

                            if (dialog == DialogResult.Retry)
                            {
                                reconnectionRequired = true;
                            }
                            if (dialog == DialogResult.Abort || dialog == DialogResult.Cancel)
                            {
                                reconnectionRequired = false;
                                Application.Exit();
                            }
                        }
                    }
                }
                while (reconnectionRequired);
            }, token);
        }

        private async void FormFullListOrders_Load(object sender, EventArgs e)
        {
            LoadParametersFromBase("fullListForm");

            cancelTokenSourceYear?.Cancel();
            cancelTokenSourceYear = new CancellationTokenSource();

            cancelTokenSourceMachine?.Cancel();
            cancelTokenSourceMachine = new CancellationTokenSource();

            if (detailsLoad == true)
            {
                this.Text = "Детали заказа";

                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;

                comboBox2.SelectedIndex = -1;

                label6.Visible = false;
                textBox1.Visible = false;

                await LoadYears(cancelTokenSourceYear.Token);
                await LoadMachine(cancelTokenSourceMachine.Token);

                await LoadOrdersFromBase();
            }
            else
            {
                await LoadYears(cancelTokenSourceYear.Token);
                await LoadMachine(cancelTokenSourceMachine.Token);
                SetItemsComboBox();
            }
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!detailsLoad)
                await LoadOrdersFromBase();
        }

        private async void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!detailsLoad)
                await LoadOrdersFromBase();
        }

        private async void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!detailsLoad)
                await LoadOrdersFromBase();
        }

        private async void textBox1_TextChanged(object sender, EventArgs e)
        {
            await LoadOrdersFromBase();
        }

        private void FormFullListOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancelTokenSource?.Cancel();
            cancelTokenSourceYear?.Cancel();
            cancelTokenSourceMachine?.Cancel();

            Thread.Sleep(200);

            SaveParameterToBase("fullListForm");
        }
    }
}
