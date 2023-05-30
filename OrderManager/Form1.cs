﻿using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Odbc;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using File = System.IO.File;
using ToolTip = System.Windows.Forms.ToolTip;

namespace OrderManager
{
    public partial class Form1 : Form
    {
        bool adminMode = false;
        String loadMode = "";

        public Form1(string[] args)
        {
            InitializeComponent();

            if (args.Length > 0)
            {
                string param = args[0].Replace("-", "");

                if (param == "adminMode")
                {
                    adminMode = true;
                }
                else
                {
                    loadMode = param;
                }

            }
        }

        CancellationTokenSource cancelTokenSource;

        List<Order> ordersCurrentShift;

        int selectedIndexActive = 0;
        int selectedIndexWOut1 = 0;
        int selectedIndexWOut2 = 0;

        public static string connectionFile = "connections.ini";

        public static class Info
        {
            public static bool active = false;
            //public static int indexItem = -1;
            public static string nameOfExecutor = "";
            public static int shiftIndex = -1;
        }

        public static class BaseConnectionParameters
        {
            public static string host = "25.21.38.172";
            public static int port = 3309;
            public static string database = "order_manager";
            public static string username = "oxyfox";
            public static string password = "root";
        }

        public static class OrderDetails
        {
            public static string caption1 = "";
            public static string caption2 = "";
            public static string caption3 = "";
            public static string caption4 = "";
        }

        public bool IsServerConnected()
        {
            string host = Form1.BaseConnectionParameters.host;
            int port = Form1.BaseConnectionParameters.port;
            string database = Form1.BaseConnectionParameters.database;
            string username = Form1.BaseConnectionParameters.username;
            string password = Form1.BaseConnectionParameters.password;

            using (MySqlConnection Connect = DBConnection.GetDBConnection(host, port, database, username, password))
            {
                try
                {
                    Connect.Open();
                    Connect.Close();
                    return true;
                }
                catch
                {
                    return false;
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheckCurrentShiftActivity();

            Info.active = false;
            FormAddCloseOrder form = new FormAddCloseOrder(Info.shiftIndex, Info.nameOfExecutor);
            form.ShowDialog();
            LoadOrdersFromBase();
            Info.active = true;
        }

        private void LoadBaseConnectionParameters()
        {
            /*INISettings ini = new INISettings();

            BaseConnectionParameters.host = ini.GetDBHost();
            BaseConnectionParameters.port = Convert.ToInt32(ini.GetDBPort());
            BaseConnectionParameters.database = ini.GetDBDatabase();
            BaseConnectionParameters.username = ini.GetDBUsername();
            BaseConnectionParameters.password = ini.GetDBPassword();*/

            DBConnection connection = new DBConnection();

            connection.SetDBParameter();

            toolStripStatusLabel2.Text = BaseConnectionParameters.host;
            toolStripStatusLabel5.Text = BaseConnectionParameters.database;
        }

        private void ViewBaseConnectionParameters()
        {
            toolStripStatusLabel2.Text = BaseConnectionParameters.host;
            toolStripStatusLabel5.Text = BaseConnectionParameters.database;
        }

        private void ClearBaseConnectionParameters()
        {
            toolStripStatusLabel2.Text = "";
            toolStripStatusLabel5.Text = "";
        }

        private void ShowUserForm()
        {
            Info.active = false;

            //ViewBaseConnectionParameters();

            FormLoadUserForm form = new FormLoadUserForm(loadMode);
            //this.Visible = false;
            form.ShowDialog();

            LoadParametersFromBase("mainForm");

            LoadUser();

            ViewBaseConnectionParameters();


            if (Form1.Info.shiftIndex == -1)
            {
                ShowUserSelectMachineForm();
            }

            if (Info.nameOfExecutor == "1")
            {
                testToolStripMenuItem.Visible = true;
            }
            else
            {
                testToolStripMenuItem.Visible = false;
            }

            Info.active = true;
        }

        private void ShowUserSelectMachineForm()
        {
            Info.active = false;
            FormSelectMachine form = new FormSelectMachine();
            form.ShowDialog();

            //LoadUser();
            LoadOrdersFromBase();

            Info.active = true;
        }

        private void ShowFullOrdersForm()
        {
            FormFullListOrders form = new FormFullListOrders(false, -1);
            form.ShowDialog();
        }

        private void ShowAllOrdersForm()
        {
            FormAllOrders form = new FormAllOrders();
            form.ShowDialog();
        }

        private void ShowShiftsForm()
        {
            Info.active = false;
            FormShiftsDetails form = new FormShiftsDetails(adminMode, Form1.Info.nameOfExecutor, 0, 0);
            form.ShowDialog();

            //LoadUser();
            LoadOrdersFromBase();
            Info.active = true;
        }

        private void ShowNormForm()
        {
            Info.active = false;
            FormNormOrders form = new FormNormOrders();
            form.ShowDialog();

            Info.active = true;
        }

        private void ShowSetUserForm()
        {
            FormUserProfile form = new FormUserProfile(Form1.Info.nameOfExecutor);
            form.ShowDialog();
            ViewDetailsForUser();
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

            if (IsServerConnected())
            {
                if (Form1.Info.nameOfExecutor != "")
                    ApplyParameterLine(getSettings.GetParameterLine(Form1.Info.nameOfExecutor, nameForm));
                else
                    ApplyParameterLine(getSettings.GetParameterLine("0", nameForm));
            }
        }

        private void LoadParametersForTheSelectedUserFromBase()
        {
            ValueUserBase getUser = new ValueUserBase();

            Info.shiftIndex = getUser.GetCurrentShiftStart(Info.nameOfExecutor);

            this.Text = "Менеджер заказов - " + getUser.GetNameUser(Info.nameOfExecutor);
        }

        private void StartDowloadUpdater()
        {
            //MessageBox.Show("Запуск");

            //CreateFolder();

            //string pathTemp = @"";

            string fileTemp = "Updater.exe";

            string link = "https://drive.google.com/uc?export=download&id=1-AfXKyeSzhNlCFOLj0gpY9tcZl9yHVdw";

            var task = Task.Run(() => DowloadUpdater(link,fileTemp));

        }

        private void DowloadUpdater(string link, string path)
        {
            FileDownloader downloader = new FileDownloader();

            try
            {
                downloader.DownloadFile(link, path);
            }
            catch
            {
                //MessageBox.Show("Ошибка подключения", "Ошибка", MessageBoxButtons.OK);
            }

            Invoke(new Action(() =>
            {
                //MessageBox.Show(currentDateV.ToString() + " " + lastDateV.ToString());

            }));
        }

        private void CreateFolder()
        {
            string path = @"TempDownload";
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.System;
            }
        }

        private void StartCheckUpdate()
        {
            //MessageBox.Show("Запуск");

            CreateFolder();

            string pathTemp = @"TempDownload";

            string fileTemp = "changlog.txt";

            string link = "https://drive.google.com/uc?export=download&id=1YYbr30wiiSSwETsH8GIPFulWpebS6LeM";

            //cancelTokenSource = new CancellationTokenSource();

            var task = Task.Run(() => CheckUpdate(link, pathTemp + "\\" + fileTemp));

            //task.Wait();

            //CheckUpdate(link, pathTemp + "\\" + fileTemp);

            /*CancellationToken token = cancelTokenSource.Token;

            Task task = new Task(() => LoadDetailsMount(token));

            task.Start();*/
        }

        private void CheckUpdate(string link, string path)
        {
            FileDownloader downloader = new FileDownloader();
            INISettings ini = new INISettings();

            string[] chLog = null;

            int lastDateV = 0;
            int currentDateV = 0;

            string lastDateVersion = ini.GetLastDateVersion();
            string currentDateVersion = "";

            try
            {
                var p = new Process();
                p.StartInfo.FileName = "Updater.exe";
                p.StartInfo.Arguments = "update";

                downloader.DownloadFile(link, path);
                //downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                //downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;

                chLog = File.ReadAllLines(path, Encoding.UTF8);
                currentDateVersion = chLog[0].Substring(7);

                if (currentDateVersion != "")
                    currentDateV = Convert.ToInt32(currentDateVersion);

                if (lastDateVersion != "")
                {
                    lastDateV = Convert.ToInt32(lastDateVersion);


                    if (currentDateV > lastDateV)
                    {
                        p.Start();
                    }
                }
                else
                {
                    p.Start();
                }

            }
            catch
            {
                //MessageBox.Show("Ошибка подключения", "Ошибка", MessageBoxButtons.OK);
            }

            Invoke(new Action(() =>
            {
                //MessageBox.Show(currentDateV.ToString() + " " + lastDateV.ToString());

            }));
        }

        private void AddOrdersToListViewFromList()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueSettingsBase valueSettings = new ValueSettingsBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();

            ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(Form1.Info.shiftIndex, "");

            GetWorkingOutTime workingOutTime = new GetWorkingOutTime(Info.shiftIndex, ordersCurrentShift);

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
                    //OrderStatusValue statusValue = GetWorkingOutTimeForSelectedOrder(index, true);
                    OrderStatusValue statusValue = workingOutTime.GetWorkingOutTimeForSelectedOrder(index, true);

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
                    OrderStatusValue statusValue = workingOutTime.GetWorkingOutTimeForSelectedOrder(index, false);

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

            LoadSelectedMachines();
            LoadSelectedMachinesForPlannedWorkinout();

            /*if (listView1.Items.Count > 0)
            {
                if (Info.indexItem >= 0)
                    listView1.Items[Info.indexItem].Selected = true;
                else
                    listView1.Items[listView1.Items.Count - 1].Selected = true;
            }*/
            //listView1.Items[0].Selected = false;
        }

        private void LoadSelectedMachines()
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            List<String> machines = (List<String>)infoBase.GetMachines(Form1.Info.nameOfExecutor);

            comboBox1.Items.Clear();

            for (int i = 0; i < machines.Count; i++)
            {
                comboBox1.Items.Add(infoBase.GetMachineName(machines[i]));
            }

            if (machines.Count > 0)
            {
                if (comboBox1.Items.Count > 0 && comboBox1.Items.Count >= selectedIndexActive)
                {
                    comboBox1.SelectedIndex = selectedIndexActive;
                }
                else
                {
                    comboBox1.SelectedIndex = 0;
                }
            }

            if (comboBox1.Items.Count == 1)
            {
                tableLayoutPanel5.ColumnStyles[0].Width = 0;
                comboBox1.Visible = false;
            }
            else
            {
                tableLayoutPanel5.ColumnStyles[0].Width = 140;
                comboBox1.Visible = true;
            }
                
        }

        private void LoadSelectedMachinesForPlannedWorkinout()
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            List<String> machines = (List<String>)infoBase.GetMachines(Form1.Info.nameOfExecutor);

            comboBox2.Items.Clear();
            comboBox2.Items.Add("Общая выработка");

            comboBox3.Items.Clear();

            for (int i = 0; i < machines.Count; i++)
            {
                comboBox2.Items.Add(infoBase.GetMachineName(machines[i]));
                comboBox3.Items.Add(infoBase.GetMachineName(machines[i]));
            }

            if (machines.Count > 0)
            {
                if (comboBox2.Items.Count > 0 && comboBox2.Items.Count >= selectedIndexWOut1)
                    comboBox2.SelectedIndex = selectedIndexWOut1;
                else
                    comboBox2.SelectedIndex = 1;

                if (comboBox3.Items.Count > 0 && comboBox3.Items.Count >= selectedIndexWOut2)
                    comboBox3.SelectedIndex = selectedIndexWOut2;
                else
                    comboBox3.SelectedIndex = 0;
            }

            if (comboBox2.Items.Count <= 2)
            {
                tableLayoutPanel7.ColumnStyles[0].Width = 0;
                tableLayoutPanel7.ColumnStyles[1].Width = 0;
                //comboBox2.Visible = false;
            }
            else
            {
                tableLayoutPanel7.ColumnStyles[0].Width = 140;
                //tableLayoutPanel7.ColumnStyles[1].Width = 140;
                //comboBox2.Visible = true;
            }

            /*if (comboBox2.Items.Count <= 2)
            {
                tableLayoutPanel7.ColumnStyles[0].Width = 0;
                tableLayoutPanel7.ColumnStyles[1].Width = 0;
                //comboBox2.Visible = false;
            }
            else
            {
                tableLayoutPanel7.ColumnStyles[0].Width = 140;
                tableLayoutPanel7.ColumnStyles[1].Width = 140;
                //comboBox2.Visible = true;
            }*/

            /*if (comboBox2.Items.Count == 1)
            {
                tableLayoutPanel7.ColumnStyles[0].Width = 0;
                tableLayoutPanel7.ColumnStyles[1].Width = 0;
                //comboBox2.Visible = false;
            }
            else if (comboBox2.Items.Count == 2)
            {
                tableLayoutPanel7.ColumnStyles[0].Width = 140;
                tableLayoutPanel7.ColumnStyles[1].Width = 140;
                //comboBox2.Visible = true;
            }
            else if (comboBox2.Items.Count > 2)
            {
                tableLayoutPanel7.ColumnStyles[0].Width = 140;
                tableLayoutPanel7.ColumnStyles[1].Width = 0;
            }*/



        }

        private void LoadDetailsMount(CancellationToken token)
        {
            GetShiftsFromBase getShifts = new GetShiftsFromBase(Form1.Info.nameOfExecutor);
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            DateTime date;
            if (Form1.Info.shiftIndex != -1)
                date = Convert.ToDateTime(shiftsBase.GetStartShiftFromID(Form1.Info.shiftIndex));
            else
                date = DateTime.Now;

            while (!token.IsCancellationRequested)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                ShiftsDetails currentShift = getShifts.LoadCurrentDateShiftsDetails(date, "", token);

                if (currentShift == null)
                    break;

                Invoke(new Action(() =>
                {
                    label18.Text = currentShift.countShifts.ToString("N0");
                    label19.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift.shiftsWorkingTime) + " (" +
                        dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift.allTimeShift) + ")";

                    label20.Text = currentShift.countOrdersShift.ToString() + "/" + currentShift.countMakereadyShift.ToString();
                    label21.Text = currentShift.amountAllOrdersShift.ToString("N0");

                    label22.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift.allTimeWorkingOutShift);
                    label23.Text = currentShift.percentWorkingOutShift.ToString("N1") + "%";

                }));

                break;
            }
        }

        private void LaodDetailsForCurrentMount()
        {
            cancelTokenSource?.Cancel();

            cancelTokenSource = new CancellationTokenSource();

            Task task = new Task(() => LoadDetailsMount(cancelTokenSource.Token), cancelTokenSource.Token);
            task.Start();

            //LoadDetailsMount(cancelTokenSource.Token);
            //var task = Task.Run(() => LoadDetailsMount(cancelTokenSource.Token), cancelTokenSource.Token);
        }

        private void LoadOrdersFromBase()
        {
            AddOrdersToListViewFromList();
            ViewDetailsForUser();
            LoadMachinesDetailsForUser();

            ValueSettingsBase valueSettings = new ValueSettingsBase();

            string load = valueSettings.GetSelectedPage(Form1.Info.nameOfExecutor);
            int page = 0;

            if (load != "")
                page = Convert.ToInt32(load);

            tabControl1.SelectTab(page);
            //LaodDetailsForCurrentMount();
        }

        private void ClearAll()
        {
            cancelTokenSource?.Cancel();

            DBConnection connection = new DBConnection();

            if (connection.IsServerConnected(BaseConnectionParameters.host, BaseConnectionParameters.port, BaseConnectionParameters.database,
                BaseConnectionParameters.username, BaseConnectionParameters.password))
            {
                SaveParameterToBase("mainForm");
            }

            this.Text = "Менеджер заказов";

            listView1.Items.Clear();
            listView2.Items.Clear();

            //Info.indexItem = -1;

            label6.Text = "";
            label7.Text = "";
            label8.Text = "";
            label9.Text = "";
            label10.Text = "";

            label18.Text = "";
            label19.Text = "";
            label20.Text = "";
            label21.Text = "";
            label22.Text = "";
            label23.Text = "";

            ClearCurrentOrderDetails();
        }

        private void ViewDetailsForUser()
        {
            GetDateTimeOperations dtOperations = new GetDateTimeOperations();
            ValueUserBase usersBase = new ValueUserBase();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            ValueInfoBase getUserMachines = new ValueInfoBase();
            ValueShiftsBase valueShifts = new ValueShiftsBase();

            int fullWorkingOut = CountWorkingOutOrders(ordersCurrentShift.Count, "");
            int fullDone = CountFullDoneOrders(ordersCurrentShift.Count, "");

            if (getUserMachines.GetMachinesForUserActive(Info.nameOfExecutor) == true)
                button6.Enabled = false;
            else
                button6.Enabled = true;

            if (Form1.Info.shiftIndex != -1)
                button1.Enabled = true;
            else
                button1.Enabled = false;

            label6.Text = usersBase.GetNameUser(Info.nameOfExecutor);
            label7.Text = valueShifts.GetStartShiftFromID(Info.shiftIndex);
            label8.Text = dtOperations.TotalMinutesToHoursAndMinutesStr(fullWorkingOut);
            label9.Text = getPercent.PercentString(fullWorkingOut);
            label10.Text = fullDone.ToString("N0");

            if (Info.nameOfExecutor != "")
                LaodDetailsForCurrentMount();
        }

        private void EraseInfo()
        {
            //Form1.Info.nameOfExecutor = "";
            Form1.Info.shiftIndex = -1;

            Form1.Info.active = false;
            this.Text = "Менеджер заказов";
        }

        private void LoadUser()
        {
            ValueInfoBase getMachine = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();

            List<String> machines = (List<String>)getMachine.GetMachines(Form1.Info.nameOfExecutor);

            EraseInfo();

            this.Text = "Менеджер заказов - " + userBase.GetNameUser(Info.nameOfExecutor);

            if (machines.Count > 0)
            {
                int index = machines.IndexOf(userBase.GetLastMachineForUser(Form1.Info.nameOfExecutor));


                LoadParametersForTheSelectedUserFromBase();
                //LoadOrdersFromBase();
            }

            LoadOrdersFromBase();

        }

        private void LoadMachinesDetailsForUser()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getOrder = new ValueOrdersBase();

            ValueUserBase userBase = new ValueUserBase();

            List<String> machines = (List<String>)getInfo.GetMachines(Form1.Info.nameOfExecutor);

            listView2.Items.Clear();

            if (machines.Count > 0)
            {
                foreach (String machine in machines)
                {
                    int currentOrderID = Convert.ToInt32(getInfo.GetCurrentOrderID(machine));

                    string order = "";
                    if (currentOrderID != -1)
                    {
                        order = getOrder.GetOrderNumber(currentOrderID) + ", " + getOrder.GetOrderName(currentOrderID);
                    }

                    ListViewItem item = new ListViewItem();

                    item.Name = machine;
                    item.Text = getInfo.GetMachineName(machine);
                    item.SubItems.Add(getOrder.GetOrderStatusName(currentOrderID));
                    item.SubItems.Add(order);

                    listView2.Items.Add(item);
                }
            }
        }

        private void SelectMachines()
        {
            Info.active = false;
            ShowUserSelectMachineForm();
            if (Form1.Info.nameOfExecutor != "")
                LoadParametersForTheSelectedUserFromBase();
            LoadOrdersFromBase();
            Info.active = true;
        }

        private void ShowStatisticForm()
        {
            FormDetailsStatistic form = new FormDetailsStatistic(false);
            form.ShowDialog();
        }

        private void CancelShift()
        {
            /*DialogResult result;
            result = MessageBox.Show("Вы действительно хотите завершить смену?", "Завершение смены", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Info.active = false;

                ValueUserBase userBase = new ValueUserBase();
                ValueInfoBase infoBase = new ValueInfoBase();
                ValueShiftsBase getShift = new ValueShiftsBase();

                getShift.CloseShift(Info.startOfShift, DateTime.Now.ToString());
                infoBase.CompleteTheShift(Info.nameOfExecutor);
                userBase.UpdateCurrentShiftStart(Info.nameOfExecutor, "");

                ClearAll();
                EraseInfo();
                ShowUserForm();

                Info.active = true;
            }*/

            Info.active = false;

            FormCloseShift form = new FormCloseShift();
            form.ShowDialog();
            bool result = form.ShiftVal;

            if (result)
            {
                ValueUserBase userBase = new ValueUserBase();
                ValueInfoBase infoBase = new ValueInfoBase();
                ValueShiftsBase getShift = new ValueShiftsBase();

                getShift.CloseShift(Info.shiftIndex, DateTime.Now.ToString());
                infoBase.CompleteTheShift(Info.nameOfExecutor);
                userBase.UpdateCurrentShiftStart(Info.nameOfExecutor, "-1");
                getShift.SetNoteShift(Info.shiftIndex, form.NoteVal);
                getShift.SetCheckFullShift(Info.shiftIndex, form.FullShiftVal);
                getShift.SetCheckOvertimeShift(Info.shiftIndex, form.OvertimeShiftVal);

                ClearAll();
                EraseInfo();
                ShowUserForm();
            }

            Info.active = true;
        }

        private void CheckCurrentShiftActivity()
        {
            ValueUserBase userBase = new ValueUserBase();
            ValueShiftsBase valueShifts = new ValueShiftsBase();

            if (Info.shiftIndex != -1)
            {
                int shiftID = userBase.GetCurrentShiftStart(Info.nameOfExecutor);
                bool activityShift = valueShifts.CheckShiftActivity(Info.shiftIndex);

                //if (!activityShift || startOfShift == "")
                if (!activityShift || shiftID == -1)
                {
                    Info.active = false;
                    ClearAll();
                    EraseInfo();
                    ShowUserForm();
                    Info.active = true;
                }
            }

            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Info.active = false;
            ClearAll();
            EraseInfo();
            ShowUserForm();
            Info.active = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ShowSetUserForm();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            ValueUserBase userBase = new ValueUserBase();

            int shiftID = userBase.GetCurrentShiftStart(Info.nameOfExecutor);

            if (shiftID == -1)
            {
                Info.active = false;
                ClearAll();
                ShowUserForm();
                Info.active = true;
            }
            else
            {
                CancelShift();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            StartCheckUpdate();

            LoadBaseConnectionParameters();

            if (IsServerConnected())
            {
                if (!adminMode)
                {
                    Info.active = false;
                    ClearAll();
                    ShowUserForm();
                }
                else
                {
                    this.Visible = false;
                    FormAdmin form = new FormAdmin(adminMode);
                    form.ShowDialog();
                }
            }
            else
            {
                DataBaseSelect(false);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelTime.Text = DateTime.Now.ToString("HH:mm:ss");
            if (DateTime.Now.ToString("ss") == "00" && Info.active == true)
            {
                LoadOrdersFromBase();
                CheckCurrentShiftActivity();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShowShiftsForm();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ShowShiftsForm();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            CheckCurrentShiftActivity();
            SelectMachines();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadBaseConnectionParameters();

            if (!IsServerConnected())
            {
                DataBaseSelect(false);
            }

            LoadParametersFromBase("mainForm");

            //LoadUser();
            //LoadParametersForTheSelectedUserFromBase(Form1.Info.mashine);
            //LoadOrdersFromBase();
            //ViewDetailsForUser();
            //button1.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancelTokenSource?.Cancel();

            if (IsServerConnected())
            {
                SaveParameterToBase("mainForm");
            }            
        }

        private void labelTime_Click(object sender, EventArgs e)
        {
            if (Form1.Info.nameOfExecutor == "1")
            {

                FormAdmin form = new FormAdmin(adminMode);
                form.ShowDialog();

                LoadParametersFromBase("mainForm");
                LoadUser();
            }

        }

        private void отработанныеСменыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowShiftsForm();
        }

        private void параметрыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSetUserForm();
        }

        private void просмотрВсехЗаказовToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowAllOrdersForm();
        }

        private void просмотрОперацийToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowFullOrdersForm();
        }

        private void выборОборудованияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Info.active = false;
            ShowUserSelectMachineForm();
            if (Form1.Info.nameOfExecutor != "")
                LoadParametersForTheSelectedUserFromBase();
            LoadOrdersFromBase();
            Info.active = false;
        }

        private void выйтиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Info.active = false;
            ClearAll();
            ShowUserForm();
            Info.active = true;
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            LoadOrderDetails();
        }

        private void LoadOrderDetails()
        {
            if (listView1.SelectedItems.Count != 0)
            {
                ValueInfoBase getInfo = new ValueInfoBase();

                Info.active = false;
                FormAddCloseOrder form;

                if (listView1.SelectedIndices[0] == listView1.Items.Count - 1 && Convert.ToBoolean(getInfo.GetActiveOrder(ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder)))
                {
                    form = new FormAddCloseOrder(Info.shiftIndex, Info.nameOfExecutor);
                }
                else
                {
                    form = new FormAddCloseOrder(Info.shiftIndex,
                        ordersCurrentShift[listView1.SelectedIndices[0]].orderIndex,
                        ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder,
                        ordersCurrentShift[listView1.SelectedIndices[0]].counterRepeat);
                }

                form.ShowDialog();
                LoadOrdersFromBase();
                Info.active = true;
            }
        }

        private void LoadOrderNote()
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            Info.active = false;
            FormPrivateNote form;

            form = new FormPrivateNote(Info.shiftIndex,
                ordersCurrentShift[listView1.SelectedIndices[0]].orderIndex,
                ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].counterRepeat);

            form.ShowDialog();
            LoadOrdersFromBase();
            Info.active = true;
        }

        private void LoadTypes()
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            Info.active = false;
            FormTypesInTheOrder form;

            form = new FormTypesInTheOrder(Info.shiftIndex,
                ordersCurrentShift[listView1.SelectedIndices[0]].orderIndex,
                ordersCurrentShift[listView1.SelectedIndices[0]].counterRepeat,
                ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder,
                Info.nameOfExecutor);

            form.ShowDialog();
            LoadOrdersFromBase();
            Info.active = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = listView1.SelectedItems.Count == 0;
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadOrderDetails();
        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadOrderNote();
        }

        private void machinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectMachines();
        }

        private void statisticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowStatisticForm();
        }

        private void cancelShiftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CancelShift();
        }

        private int CountFullDoneOrders(int indexOrder, string machine)
        {
            int result = 0;

            for (int i = 0; i < indexOrder; i++)
            {
                if (ordersCurrentShift[i].machineOfOrder == machine || machine == "")
                {
                    result += ordersCurrentShift[i].done;
                }
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


        private void LoadCurrentOrderDetails(int idx)
        {
            ClearCurrentOrderDetails();

            if (idx != -1)
            {
                ValueSettingsBase valueSettings = new ValueSettingsBase();
                GetWorkingOutTime workingOutTime = new GetWorkingOutTime(Info.shiftIndex, ordersCurrentShift);

                bool typeLoad;

                if (valueSettings.GetTypeLoadOrderDetails(Info.nameOfExecutor) == 0)
                {
                    typeLoad = true;
                }
                else
                {
                    typeLoad = false;
                }

                OrderStatusValue statusStrings = workingOutTime.GetWorkingOutTimeForSelectedOrder(idx, typeLoad);

                /*string statusStr = GetWorkingOutTimeForSelectedOrder(idx).Item1;
                string[] caption = GetWorkingOutTimeForSelectedOrder(idx).Item3;
                string[] strings = GetWorkingOutTimeForSelectedOrder(idx).Item4;*/

                /*string statusStr = GetWorkingOutMessage(idx).Item1;
                string[] caption = GetWorkingOutMessage(idx).Item3;
                string[] strings = GetWorkingOutMessage(idx).Item4;*/

                label24.Text = ordersCurrentShift[idx].numberOfOrder + ": " + ordersCurrentShift[idx].nameOfOrder;
                label26.Text = statusStrings.statusStr;

                label25.Text = statusStrings.caption_1;
                label27.Text = statusStrings.value_1;

                label28.Text = statusStrings.caption_2;
                label30.Text = statusStrings.value_2;

                label29.Text = statusStrings.caption_3;
                label31.Text = statusStrings.value_3;
            }
            else
            {
                ClearCurrentOrderDetails();
            }
        }

        private void ClearCurrentOrderDetails()
        {
            label24.Text = "";
            label26.Text = "";

            label25.Text = "";
            label27.Text = "";

            label28.Text = "";
            label30.Text = "";

            label29.Text = "";
            label31.Text = "";

            label34.Text = "";
            label35.Text = "";
            label38.Text = "";
            label39.Text = "";
        }

        private void listView1_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            GetWorkingOutTime workingOutTime = new GetWorkingOutTime(Info.shiftIndex, ordersCurrentShift);

            ToolTip tooltp = new ToolTip();

            tooltp.AutomaticDelay = 2000;

            int idx = e.Item.Index;

            if (idx != -1 && e.Item != null)
            {
                /*string statusStr = GetWorkingOutMessage(idx).Item1;
                string message = GetWorkingOutMessage(idx).Item2;*/

                /*string statusStr = GetWorkingOutTimeForSelectedOrder(idx).Item1;
                string message = GetWorkingOutTimeForSelectedOrder(idx).Item2;*/

                ValueSettingsBase valueSettings = new ValueSettingsBase();

                bool typeLoad;

                if (valueSettings.GetTypeLoadItemMouseHover(Info.nameOfExecutor) == 0)
                {
                    typeLoad = true;
                }
                else
                {
                    typeLoad = false;
                }

                OrderStatusValue statusStrings = workingOutTime.GetWorkingOutTimeForSelectedOrder(idx, typeLoad);

                string statusStr = statusStrings.statusStr;
                string message = statusStrings.message;

                tooltp.Active = true;
                tooltp.ToolTipTitle = ordersCurrentShift[idx].numberOfOrder + ": " + ordersCurrentShift[idx].nameOfOrder + " - " + statusStr;
                tooltp.SetToolTip(listView1, message);
            }
            else
            {
                tooltp.Active = false;
            }

        }

        private void DataBaseSelect(bool available)
        {
            FormAddEditTestMySQL form = new FormAddEditTestMySQL(available);
            form.ShowDialog();

            LoadBaseConnectionParameters();

            LoadUser();

            if (Form1.Info.nameOfExecutor != "")
                LoadParametersForTheSelectedUserFromBase();

            LoadOrdersFromBase();
        }

        private void базаДанныхToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataBaseSelect(true);
        }

        private void normToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowNormForm();
        }

        private int GetIDLastOrderFromSelectedMachine(string machineName)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            int idx = -1;

            for (int i = 0; i < ordersCurrentShift.Count; i++)
            {
                if (infoBase.GetMachineName(ordersCurrentShift[i].machineOfOrder) == machineName)
                {
                    idx = i;
                }
            }

            return idx;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            int idx = GetIDLastOrderFromSelectedMachine(comboBox1.Text);

            /*ValueInfoBase infoBase = new ValueInfoBase();

            int idx = -1;

            for (int i = 0; i < ordersCurrentShift.Count; i++)
            {
                if (infoBase.GetMachineFromName(ordersCurrentShift[i].machineOfOrder) == comboBox1.Text)
                {
                    idx = i;
                }
            }*/

            /*for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].SubItems[1].Text == comboBox1.Text)
                {
                    idx = i;
                }
            }*/

            LoadCurrentOrderDetails(idx);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)
            {
                tableLayoutPanel7.ColumnStyles[1].Width = 140;

                //comboBox3.SelectedIndex = selectedIndexWOut2;
            }
            else
            {
                tableLayoutPanel7.ColumnStyles[1].Width = 0;

                selectedIndexWOut2 = comboBox2.SelectedIndex - 1;
            }

            if (comboBox3.SelectedIndex == selectedIndexWOut2)
            {
                UpdateWorkingOut();
            }
            else
            {
                comboBox3.SelectedIndex = selectedIndexWOut2;
            }

            selectedIndexWOut1 = comboBox2.SelectedIndex;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIndexWOut2 = comboBox3.SelectedIndex;

            UpdateWorkingOut();
        }

        private void UpdateWorkingOut()
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            int percentOverAmount = 5;

            string[] captions = { "10:00", "10:30", "11:00", "12:00" };
            string[] values = { "", "", "", "" };

            string captionNotEnough = "Тиража не достаточно для выполнения минимальной нормы";
            string captionMakeReady = "Выполняется приладка";

            int wOut;
            //int norm;
            int idLastOrder = GetIDLastOrderFromSelectedMachine(comboBox3.Text);
            string machine = infoBase.GetMachineFromName(comboBox3.Text);

            if (idLastOrder >= 0)
            {
                if (comboBox2.SelectedIndex == 0)
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, "");
                }
                else
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, machine);
                }

                string status = ordersBase.GetOrderStatus(ordersCurrentShift[idLastOrder].orderIndex);

                int norm = ordersCurrentShift[idLastOrder].norm;
                int amount = ordersCurrentShift[idLastOrder].amountOfOrder;
                int done = amount - ordersCurrentShift[idLastOrder].lastCount;// + ordersCurrentShift[idLastOrder].done;
                int mkTime = ordersCurrentShift[idLastOrder].plannedTimeMakeready;

                for (int i = 0; i < captions.Length; i++)
                {
                    int targetTime = timeOperations.totallTimeHHMMToMinutes(captions[i]);

                    if (status == "1")
                    {
                        int lastTimeToWork = targetTime - wOut;

                        if (lastTimeToWork > 0)
                        {
                            if (lastTimeToWork < mkTime)
                            {
                                values[i] = "часть приладки: " + timeOperations.TotalMinutesToHoursAndMinutesStr(lastTimeToWork);
                            }
                            else
                            {
                                int targetCount = (lastTimeToWork - mkTime) * norm / 60;

                                if ((targetCount + done) <= amount * (1 + percentOverAmount / 100))
                                {
                                    values[i] = "вся приладка";

                                    if (targetCount > 0)
                                        values[i] += " + " + targetCount.ToString("N0");
                                }
                                else
                                {
                                    values[i] = "н/д";
                                }
                            }
                        }
                        else
                        {
                            values[i] = "выполнено";
                        }
                    }

                    if (status == "3" || status == "2")
                    {
                        if (targetTime > wOut)
                        {
                            int targetCount = (targetTime - wOut) * norm / 60;

                            if ((targetCount + done) <= amount * (1 + percentOverAmount / 100))
                            {
                                values[i] = targetCount.ToString("N0");
                            }
                            else
                            {
                                values[i] = "н/д";
                            }
                        }
                        else
                        {
                            values[i] = "выполнено";
                        }
                    }

                    
                }

                /*if (status == "3")
                {
                    norm = ordersCurrentShift[idLastOrder].norm;
                    int done = ordersCurrentShift[idLastOrder].done;

                    for (int i = 0; i < captions.Length; i++)
                    {
                        int targetTime = timeOperations.totallTimeHHMMToMinutes(captions[i]);

                        if (targetTime > wOut)
                        {
                            int targetCount = (targetTime - wOut) * norm / 60;

                            if ((targetCount + done) <= ordersCurrentShift[idLastOrder].amountOfOrder * 1.05)
                            {
                                values[i] = targetCount.ToString("N0");
                            }
                            else
                            {
                                values[i] = "н/д";
                            }
                        }
                        else
                        {
                            values[i] = "выполнено";
                        }
                    }
                }*/

            }

            label32.Text = captions[0] + ":";
            label33.Text = captions[1] + ":";
            label36.Text = captions[2] + ":";
            label37.Text = captions[3] + ":";

            label34.Text = values[0];
            label35.Text = values[1];
            label38.Text = values[2];
            label39.Text = values[3];
        }

        /*private int GetWOutFromMachine(string machine)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            int wOut = 0;

            for (int i = 0; i < ordersCurrentShift.Count; i++)
            {
                if (ordersCurrentShift[i].machineOfOrder == machine)
                {
                    int mkTime = ordersCurrentShift[i].plannedTimeMakeready;
                    //int Time = timeOperations.totallTimeHHMMToMinutes(ordersCurrentShift[i].plannedTimeWork);

                    int wTime = ordersCurrentShift[i].done * 60 / ordersCurrentShift[i].norm;

                    wOut += mkTime + wTime;
                }
                    
            }

            return wOut;
        }*/

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormForExperience form = new FormForExperience();
            form.ShowDialog();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValueSettingsBase valueSettings = new ValueSettingsBase();

            valueSettings.UpdateSelectedPage(Form1.Info.nameOfExecutor, tabControl1.SelectedIndex.ToString());
        }

        private void downloadUpadaterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartDowloadUpdater();
        }

        private void typesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTypes();
        }

        private void labelTime_DoubleClick(object sender, EventArgs e)
        {
            StartDowloadUpdater();
        }

        private void toolStripDropDownButton2_DropDownOpening(object sender, EventArgs e)
        {
            ValueSettingsBase settingsBase = new ValueSettingsBase();

            bool checkPass = false;

            if (settingsBase.GetPasswordChecked(Info.nameOfExecutor) != "")
                checkPass = Convert.ToBoolean(settingsBase.GetPasswordChecked(Info.nameOfExecutor));

            alwaysCheckPasswordToolStripMenuItem.Checked = !checkPass;
        }

        private void passwordChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormLoadUserPasswordForm form = new FormLoadUserPasswordForm(true, Info.nameOfExecutor);
            form.ShowDialog();
        }

        private void alwaysCheckPasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValueSettingsBase updateSettingsValue = new ValueSettingsBase();

            bool checkPass = alwaysCheckPasswordToolStripMenuItem.Checked;

            updateSettingsValue.UpdateCheckPassword(Info.nameOfExecutor, checkPass.ToString());
        }

        private void cancelAutorizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValueUserBase setValueUsers = new ValueUserBase();

            setValueUsers.UpdateLastUID(Info.nameOfExecutor, "");
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings form = new FormSettings(Info.nameOfExecutor);
            form.ShowDialog();

            LoadOrdersFromBase();
        }

        private void planToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormLoadOrders fm = new FormLoadOrders(true, Info.nameOfExecutor);
            fm.ShowDialog();
        }
    }
}
