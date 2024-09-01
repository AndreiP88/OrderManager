using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolTip = System.Windows.Forms.ToolTip;
using static OrderManager.DataBaseReconnect;

namespace OrderManager
{
    public partial class Form1 : Form
    {
        bool adminMode = false;
        public static ManualResetEvent _pauseEvent = new ManualResetEvent(true);
        public static bool _viewDatabaseRequestForm = false;
        public static FormDataBaseReconnect formSQLException;// = new FormDataBaseReconnect();
        public static int reconectionCount = 5;

        public Form1(string[] args)
        {
            InitializeComponent();

            //new Thread(() => { formSQLException = new FormDataBaseReconnect(); }).Start();

            if (args.Length > 0)
            {
                string param = args[0].Replace("-", "");

                if (param == "adminMode")
                {
                    adminMode = true;
                }
                else
                {
                    //loadMode = param;
                }
            }
        }

        CancellationTokenSource cancelTokenSource;

        List<Order> ordersCurrentShift;

        int selectedIndexActive = 0;
        int selectedIndexWOut1 = 0;
        int selectedIndexWOut2 = 0;
        int selectedIndexPreviewWOut1 = 0;
        int selectedIndexPreviewWOut2 = 0;

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

        private async void button1_Click(object sender, EventArgs e)
        {
            await CheckCurrentShiftActivity();

            Info.active = false;
            FormAddCloseOrder form = new FormAddCloseOrder(Info.shiftIndex, Info.nameOfExecutor);
            //FormAddCloseEditOrder form = new FormAddCloseEditOrder(Info.shiftIndex);
            form.ShowDialog();
            await LoadOrdersFromBase();
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

        private async Task ShowUserForm()
        {
            Info.active = false;

            //ViewBaseConnectionParameters();

            FormLoadUserForm form = new FormLoadUserForm();
            //this.Visible = false;
            form.ShowDialog();

            LoadParametersFromBase("mainForm");

            await LoadUser();

            ViewBaseConnectionParameters();

            if (Form1.Info.shiftIndex == -1)
            {
                await ShowUserSelectMachineForm();
            }

            comboBox8.SelectedIndex = 0;

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

        private async Task ShowUserSelectMachineForm()
        {
            Info.active = false;
            FormSelectMachine form = new FormSelectMachine();
            form.ShowDialog();

            //LoadUser();
            await LoadOrdersFromBase();

            Info.active = true;
        }

        private void ShowFullOrdersForm()
        {
            ValueUserBase valueUser = new ValueUserBase();

            int lastMachine = valueUser.GetLastMachineForUser(Form1.Info.nameOfExecutor);

            FormFullListOrders form = new FormFullListOrders(false, -1, lastMachine);
            form.ShowDialog();
        }

        private void ShowAllOrdersForm()
        {
            ValueUserBase valueUser = new ValueUserBase();

            int lastMachine = valueUser.GetLastMachineForUser(Form1.Info.nameOfExecutor);

            FormAllOrders form = new FormAllOrders(lastMachine);
            form.ShowDialog();
        }

        private async Task ShowShiftsForm()
        {
            Info.active = false;
            FormShiftsDetails form = new FormShiftsDetails(adminMode, Form1.Info.nameOfExecutor, 0, 0);
            form.ShowDialog();

            //LoadUser();
            await LoadOrdersFromBase();
            Info.active = true;
        }

        private void ShowNormForm()
        {
            Info.active = false;
            FormNormOrders form = new FormNormOrders();
            form.ShowDialog();

            Info.active = true;
        }

        private async void ShowSetUserForm()
        {
            FormUserProfile form = new FormUserProfile(Form1.Info.nameOfExecutor);
            form.ShowDialog();
            await ViewDetailsForUser();
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
            DBConnection connection = new DBConnection();

            string host = Form1.BaseConnectionParameters.host;
            int port = Form1.BaseConnectionParameters.port;
            string database = Form1.BaseConnectionParameters.database;
            string username = Form1.BaseConnectionParameters.username;
            string password = Form1.BaseConnectionParameters.password;

            if (connection.IsServerConnected(host, port, database, username, password))
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

        private void LogWrite(Exception ex)
        {
            Logger.WriteLine(ex.StackTrace + ", " + ex.Message);
        }

        private void StartTaskUpdateApplication()
        {
            var task = Task.Run(() => StartUpdateApplication());
        }

        private void StartUpdateApplication()
        {
            try
            {
                var p = new Process();
                p.StartInfo.FileName = "Updater.exe";
                p.StartInfo.Arguments = "update";

                p.Start();
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
        }

        private async Task AddOrdersToListViewFromList()
        {
            await Task.Run(async () =>
            {
                ValueInfoBase getInfo = new ValueInfoBase();
                ValueSettingsBase valueSettings = new ValueSettingsBase();
                GetDateTimeOperations timeOperations = new GetDateTimeOperations();
                GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();

                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;
                do
                {
                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            ordersCurrentShift = (List<Order>)await ordersFromBase.LoadAllOrdersFromBase(Info.shiftIndex, "");

                            GetWorkingOutTime workingOutTime = new GetWorkingOutTime(Info.shiftIndex, ordersCurrentShift);

                            int orderRegistrationType = valueSettings.GetOrderRegistrationType(Form1.Info.nameOfExecutor);

                            /*if (listView1.SelectedItems.Count > 0)
                            {
                                Info.indexItem = listView1.SelectedIndices[0];
                            }*/

                            Invoke(new Action(() =>
                            {
                                listView1.Items.Clear();

                            }));

                            for (int index = 0; index < ordersCurrentShift.Count; index++)
                            {
                                string deviation = "<>";

                                Color color = Color.DarkRed;

                                int typeLoad = valueSettings.GetTypeLoadDeviationToMainLV(Info.nameOfExecutor);
                                int typeView = valueSettings.GetTypeViewDeviationToMainLV(Info.nameOfExecutor);

                                if (orderRegistrationType == 0)
                                {
                                    if (typeLoad == 0)
                                    {
                                        //OrderStatusValue statusValue = GetWorkingOutTimeForSelectedOrder(index, true);
                                        OrderStatusValue statusValue = workingOutTime.GetWorkingOutTimeForSelectedOrder(index, true, orderRegistrationType);

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
                                        OrderStatusValue statusValue = workingOutTime.GetWorkingOutTimeForSelectedOrder(index, false, orderRegistrationType);

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
                                }
                                else
                                {
                                    if (typeLoad == 0)
                                    {
                                        //OrderStatusValue statusValue = GetWorkingOutTimeForSelectedOrder(index, true);
                                        OrderStatusValue statusValue = workingOutTime.GetWorkingOutTimeForSelectedOrder(index, true, orderRegistrationType);

                                        if (statusValue.fullTimeDifferent > 0)
                                        {
                                            color = Color.SeaGreen;
                                        }
                                        else
                                        {
                                            color = Color.DarkRed;
                                        }

                                        deviation = timeOperations.MinuteToTimeString(statusValue.fullTimeDifferent);

                                        /*if (typeView == 0)
                                        {
                                            deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent) + ", " + timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                                        }
                                        else
                                        {
                                            //deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent + statusValue.wkTimeDifferent);
                                            deviation = timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                                        }*/
                                    }
                                    else if (typeLoad == 1)
                                    {
                                        OrderStatusValue statusValue = workingOutTime.GetWorkingOutTimeForSelectedOrder(index, false, orderRegistrationType);

                                        if (statusValue.fullTimeDifferent > 0)
                                        {
                                            color = Color.SeaGreen;
                                        }
                                        else
                                        {
                                            color = Color.DarkRed;
                                        }

                                        deviation = timeOperations.MinuteToTimeString(statusValue.fullTimeDifferent);

                                        /*if (typeView == 0)
                                        {
                                            deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent) + ", " + timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                                        }
                                        else
                                        {
                                            //deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent + statusValue.wkTimeDifferent);
                                            deviation = timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                                        }*/
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

                                        deviation = timeOperations.MinuteToTimeString(ordersCurrentShift[index].mkDeviation + ordersCurrentShift[index].wkDeviation);

                                        /*if (typeView == 0)
                                        {
                                            deviation = timeOperations.MinuteToTimeString(ordersCurrentShift[index].mkDeviation) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].wkDeviation);
                                        }
                                        else
                                        {
                                            deviation = timeOperations.MinuteToTimeString(ordersCurrentShift[index].mkDeviation + ordersCurrentShift[index].wkDeviation);
                                        }*/
                                    }
                                }

                                string facticalTime = "";

                                if (orderRegistrationType == 0)
                                {
                                    facticalTime = timeOperations.MinuteToTimeString(ordersCurrentShift[index].facticalTimeMakeready) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].facticalTimeWork);
                                }
                                else
                                {
                                    facticalTime = timeOperations.MinuteToTimeString(ordersCurrentShift[index].facticalTimeMakeready + ordersCurrentShift[index].facticalTimeWork);
                                }

                                ListViewItem item = new ListViewItem();

                                item.Name = ordersCurrentShift[index].numberOfOrder.ToString();
                                item.Text = (index + 1).ToString();
                                item.SubItems.Add(await getInfo.GetMachineName(ordersCurrentShift[index].machineOfOrder.ToString()));
                                item.SubItems.Add(ordersCurrentShift[index].numberOfOrder);
                                item.SubItems.Add(ordersCurrentShift[index].nameOfOrder);
                                item.SubItems.Add(ordersCurrentShift[index].modificationOfOrder);
                                item.SubItems.Add(ordersCurrentShift[index].amountOfOrder.ToString("N0"));
                                item.SubItems.Add(ordersCurrentShift[index].lastCount.ToString("N0"));
                                item.SubItems.Add(ordersCurrentShift[index].norm.ToString("N0"));
                                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].plannedTimeMakeready) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].plannedTimeWork));
                                item.SubItems.Add(facticalTime);
                                item.SubItems.Add(deviation);
                                item.SubItems.Add(ordersCurrentShift[index].done.ToString("N0"));
                                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].workingOut));
                                item.SubItems.Add(ordersCurrentShift[index].note.ToString());
                                item.SubItems.Add(ordersCurrentShift[index].notePrivate.ToString());

                                item.ForeColor = color;

                                Invoke(new Action(() =>
                                {
                                    listView1.Items.Add(item);

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

        private async Task LoadSelectedMachines()
        {
            await Task.Run(async () =>
            {
                ValueInfoBase infoBase = new ValueInfoBase();

                List<string> machines = await infoBase.GetMachines(Form1.Info.nameOfExecutor);

                Invoke(new Action(async () =>
                {
                    comboBox1.Items.Clear();

                    for (int i = 0; i < machines.Count; i++)
                    {
                        comboBox1.Items.Add(await infoBase.GetMachineName(machines[i]));
                    }

                    int comboBoxItemsCount = comboBox1.Items.Count;

                    if (machines.Count > 0)
                    {
                        if (comboBoxItemsCount > 0 && comboBoxItemsCount >= selectedIndexActive)
                        {
                            comboBox1.SelectedIndex = selectedIndexActive;
                        }
                        else
                        {
                            comboBox1.SelectedIndex = 0;
                        }
                    }

                    if (comboBoxItemsCount == 1)
                    {
                        tableLayoutPanel5.ColumnStyles[0].Width = 0;
                        comboBox1.Visible = false;
                    }
                    else
                    {
                        tableLayoutPanel5.ColumnStyles[0].Width = 140;
                        comboBox1.Visible = true;
                    }
                }));
            });    
        }

        private async Task LoadSelectedMachinesForPlannedWorkinout()
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            //List<string> machines = await infoBase.GetMachines(Form1.Info.nameOfExecutor);
            List<string> machines = await Task.Run(() => infoBase.GetMachines(Form1.Info.nameOfExecutor));

            //WOut
            comboBox2.Items.Clear();
            comboBox2.Items.Add("Общая выработка");

            comboBox3.Items.Clear();

            //Preview
            comboBox4.Items.Clear();
            comboBox4.Items.Add("Общая выработка");

            comboBox5.Items.Clear();

            for (int i = 0; i < machines.Count; i++)
            {
                //WOut
                comboBox2.Items.Add(await infoBase.GetMachineName(machines[i]));
                comboBox3.Items.Add(await infoBase.GetMachineName(machines[i]));

                //Preview
                comboBox4.Items.Add(await infoBase.GetMachineName(machines[i]));
                comboBox5.Items.Add(await infoBase.GetMachineName(machines[i]));
            }

            if (machines.Count > 0)
            {
                //WOut
                if (comboBox2.Items.Count > 0 && comboBox2.Items.Count > selectedIndexWOut1)
                    comboBox2.SelectedIndex = selectedIndexWOut1;
                else
                    comboBox2.SelectedIndex = 1;

                if (comboBox3.Items.Count > 0 && comboBox3.Items.Count > selectedIndexWOut2)
                    comboBox3.SelectedIndex = selectedIndexWOut2;
                else
                    comboBox3.SelectedIndex = 0;

                //Preview
                if (comboBox4.Items.Count > 0 && comboBox4.Items.Count > selectedIndexPreviewWOut1)
                    comboBox4.SelectedIndex = selectedIndexPreviewWOut1;
                else
                    comboBox4.SelectedIndex = 1;

                if (comboBox5.Items.Count > 0 && comboBox5.Items.Count > selectedIndexPreviewWOut2)
                    comboBox5.SelectedIndex = selectedIndexPreviewWOut2;
                else
                    comboBox5.SelectedIndex = 0;
            }

            //WOut
            if (comboBox2.Items.Count <= 2)
            {
                tableLayoutPanel7.ColumnStyles[0].Width = 0;
                tableLayoutPanel7.ColumnStyles[1].Width = 0;
            }
            else
            {
                tableLayoutPanel7.ColumnStyles[0].Width = 140;
            }

            //Preview
            if (comboBox4.Items.Count <= 2)
            {
                tableLayoutPanel8.ColumnStyles[0].Width = 0;
            }
            else
            {
                tableLayoutPanel8.ColumnStyles[0].Width = 280;
                //tableLayoutPanel9.ColumnStyles[0].Width = 280;
            }
        }

        private async void LoadDetailsMount(CancellationToken token)
        {
            GetShiftsFromBase getShifts = new GetShiftsFromBase(Form1.Info.nameOfExecutor);
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
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

                            ShiftsDetails currentShift = await getShifts.LoadCurrentDateShiftsDetails(date, "", token);

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
                                label23.Text = currentShift.percentWorkingOutShift.ToString("P1");

                            }));

                            break;
                        }

                        reconnectionRequired = false;
                    }
                    catch (Exception ex)
                    {
                        LogException.WriteLine("LoadDetailsMount: " + ex.Message);

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

        private async Task LoadOrdersFromBase()
        {
            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        await AddOrdersToListViewFromList();
                        await LoadSelectedMachines();
                        await LoadSelectedMachinesForPlannedWorkinout();
                        await ViewDetailsForUser();
                        await LoadMachinesDetailsForUser();

                        ValueSettingsBase valueSettings = new ValueSettingsBase();

                        string load = valueSettings.GetSelectedPage(Form1.Info.nameOfExecutor);
                        int page = 0;

                        if (load != "")
                            page = Convert.ToInt32(load);

                        tabControl1.SelectTab(page);

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

            numericUpDown1.Value = 0;

            label41.Text = "";
            label40.Text = "";
            label43.Text = "00:00 ч.";
            label44.Text = "0.0%";

            selectedIndexPreviewWOut1 = 0;
            selectedIndexPreviewWOut2 = 0;
            selectedIndexWOut1 = 0;
            selectedIndexWOut2 = 0;

            ClearCurrentOrderDetails();
        }

        private async Task ViewDetailsForUser()
        {
            await Task.Run(() =>
            {
                GetDateTimeOperations dtOperations = new GetDateTimeOperations();
                ValueUserBase usersBase = new ValueUserBase();
                GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
                ValueInfoBase getUserMachines = new ValueInfoBase();
                ValueShiftsBase valueShifts = new ValueShiftsBase();

                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            int fullWorkingOut = CountWorkingOutOrders(ordersCurrentShift.Count, "");
                            int fullDone = CountFullDoneOrders(ordersCurrentShift.Count, "");

                            Invoke(new Action(async () =>
                            {
                                if (await getUserMachines.GetMachinesForUserActive(Info.nameOfExecutor) == true)
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
                            }));

                            if (Info.nameOfExecutor != "")
                                LaodDetailsForCurrentMount();

                            reconnectionRequired = false;
                        }
                        catch (Exception ex)
                        {
                            LogException.WriteLine("ViewDetailsForUser: " + ex.Message);

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

        private void EraseInfo()
        {
            //Form1.Info.nameOfExecutor = "";
            Form1.Info.shiftIndex = -1;

            Form1.Info.active = false;
            this.Text = "Менеджер заказов";
        }

        private async Task LoadUser()
        {
            ValueInfoBase getMachine = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();

            List<String> machines = await getMachine.GetMachines(Form1.Info.nameOfExecutor);

            EraseInfo();

            this.Text = "Менеджер заказов - " + userBase.GetNameUser(Info.nameOfExecutor);

            if (machines.Count > 0)
            {
                int index = machines.IndexOf(userBase.GetLastMachineForUser(Form1.Info.nameOfExecutor).ToString());

                LoadParametersForTheSelectedUserFromBase();
                //LoadOrdersFromBase();
            }

            await LoadOrdersFromBase();
        }

        private async Task LoadMachinesDetailsForUser()
        {
            await Task.Run(async () =>
            {
                ValueInfoBase getInfo = new ValueInfoBase();
                ValueOrdersBase getOrder = new ValueOrdersBase();

                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            List<string> machines = await getInfo.GetMachines(Form1.Info.nameOfExecutor);

                            Invoke(new Action(() =>
                            {
                                listView2.Items.Clear();
                            }));
                            

                            if (machines.Count > 0)
                            {
                                foreach (string machine in machines)
                                {
                                    int currentOrderID = Convert.ToInt32(getInfo.GetCurrentOrderID(machine));

                                    string order = "";
                                    if (currentOrderID != -1)
                                    {
                                        order = getOrder.GetOrderNumber(currentOrderID) + ", " + getOrder.GetOrderName(currentOrderID);
                                    }

                                    ListViewItem item = new ListViewItem();

                                    item.Name = machine;
                                    item.Text = await getInfo.GetMachineName(machine);
                                    item.SubItems.Add(getOrder.GetOrderStatusName(currentOrderID));
                                    item.SubItems.Add(order);

                                    Invoke(new Action(() =>
                                    {
                                        listView2.Items.Add(item);
                                    }));
                                }
                            }

                            reconnectionRequired = false;
                        }
                        catch (Exception ex)
                        {
                            LogException.WriteLine("LoadMachinesDetailsForUser: " + ex.Message);

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

        private async Task SelectMachines()
        {
            Info.active = false;
            await ShowUserSelectMachineForm();
            if (Form1.Info.nameOfExecutor != "")
                LoadParametersForTheSelectedUserFromBase();
            await LoadOrdersFromBase();
            Info.active = true;
        }

        private void ShowStatisticForm()
        {
            FormDetailsStatistic form = new FormDetailsStatistic(false);
            form.ShowDialog();
        }

        private async Task CancelShift()
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
                await ShowUserForm();
            }

            Info.active = true;
        }

        private async Task CheckCurrentShiftActivity()
        {
            ValueUserBase userBase = new ValueUserBase();
            ValueShiftsBase valueShifts = new ValueShiftsBase();

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
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
                                await ShowUserForm();
                                Info.active = true;
                            }
                        }

                        reconnectionRequired = false;
                    }
                    catch (Exception ex)
                    {
                        LogException.WriteLine("CheckCurrentShiftActivity: " + ex.Message);

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
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            Info.active = false;
            ClearAll();
            EraseInfo();
            await ShowUserForm();
            Info.active = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ShowSetUserForm();

        }

        private async void button6_Click(object sender, EventArgs e)
        {
            ValueUserBase userBase = new ValueUserBase();

            int shiftID = userBase.GetCurrentShiftStart(Info.nameOfExecutor);

            if (shiftID == -1)
            {
                Info.active = false;
                ClearAll();
                await ShowUserForm();
                Info.active = true;
            }
            else
            {
                await CancelShift();
            }
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            StartTaskUpdateApplication();

            LoadBaseConnectionParameters();

            DBConnection connection = new DBConnection();

            string host = Form1.BaseConnectionParameters.host;
            int port = Form1.BaseConnectionParameters.port;
            string database = Form1.BaseConnectionParameters.database;
            string username = Form1.BaseConnectionParameters.username;
            string password = Form1.BaseConnectionParameters.password;

            if (connection.IsServerConnected(host, port, database, username, password))
            {
                if (!adminMode)
                {
                    Info.active = false;
                    ClearAll();
                    await ShowUserForm();
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
                await DataBaseSelect(false);
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;

            labelTime.Text = currentTime.ToString("HH:mm:ss");

            if (currentTime.Second == 0)
            {
                StartTaskUpdateApplication();

                if (Info.active == true)
                {
                    await LoadOrdersFromBase();
                    await CheckCurrentShiftActivity();
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await ShowShiftsForm();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            await ShowShiftsForm();
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            await CheckCurrentShiftActivity();
            await SelectMachines();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            LoadBaseConnectionParameters();

            DBConnection connection = new DBConnection();

            string host = Form1.BaseConnectionParameters.host;
            int port = Form1.BaseConnectionParameters.port;
            string database = Form1.BaseConnectionParameters.database;
            string username = Form1.BaseConnectionParameters.username;
            string password = Form1.BaseConnectionParameters.password;

            if (!connection.IsServerConnected(host, port, database, username, password))
            {
                await DataBaseSelect(false);
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

            DBConnection connection = new DBConnection();

            string host = Form1.BaseConnectionParameters.host;
            int port = Form1.BaseConnectionParameters.port;
            string database = Form1.BaseConnectionParameters.database;
            string username = Form1.BaseConnectionParameters.username;
            string password = Form1.BaseConnectionParameters.password;

            if (connection.IsServerConnected(host, port, database, username, password))
            {
                SaveParameterToBase("mainForm");
            }            
        }

        private async void labelTime_Click(object sender, EventArgs e)
        {
            if (Form1.Info.nameOfExecutor == "1")
            {

                FormAdmin form = new FormAdmin(adminMode);
                form.ShowDialog();

                LoadParametersFromBase("mainForm");
                await LoadUser();
            }

        }

        private async void отработанныеСменыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await ShowShiftsForm();
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

        private async void выйтиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Info.active = false;
            ClearAll();
            await ShowUserForm();
            Info.active = true;
        }

        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void listView1_DoubleClick(object sender, EventArgs e)
        {
            await LoadOrderDetails();
        }

        private async Task LoadOrderDetails()
        {
            if (listView1.SelectedItems.Count != 0)
            {
                ValueInfoBase getInfo = new ValueInfoBase();

                Info.active = false;
                FormAddCloseOrder form;

                if (listView1.SelectedIndices[0] == listView1.Items.Count - 1 && getInfo.GetActiveOrder(ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder))
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
                await LoadOrdersFromBase();
                Info.active = true;
            }
        }

        private async Task LoadOrderNote()
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            Info.active = false;
            FormPrivateNote form;

            form = new FormPrivateNote(Info.shiftIndex,
                ordersCurrentShift[listView1.SelectedIndices[0]].orderIndex,
                ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].counterRepeat);

            form.ShowDialog();
            await LoadOrdersFromBase();
            Info.active = true;
        }

        private async Task LoadTypes()
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
            await LoadOrdersFromBase();
            Info.active = true;
        }

        private async Task LoadMakereadyParts(int orderInProgressID)
        {
            Info.active = false;

            FormEnterMakereadyPart form = new FormEnterMakereadyPart(orderInProgressID);
            form.ShowDialog();

            if (form.NewValue)
            {
                int result = form.NewMKPart;

                SaveValueMakereadyPart(orderInProgressID, result);

                await LoadOrdersFromBase();
            }

            Info.active = true;
        }

        private void SaveValueMakereadyPart(int orderInProgressID, int value)
        {
            GetOrdersFromBase getOrders = new GetOrdersFromBase();

            getOrders.SetMakereadyPart(orderInProgressID, value);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = listView1.SelectedItems.Count == 0;

            if (listView1.SelectedIndices.Count > 0)
            {
                GetOrdersFromBase getOrders = new GetOrdersFromBase();

                int currentMakereadyPart = getOrders.GetMakereadyPartFromOrderID(ordersCurrentShift[listView1.SelectedIndices[0]].id);

                if (currentMakereadyPart <= 0)
                {
                    makereadyPartToolStripMenuItem.Visible = false;
                }
                else
                {
                    ValueOrdersBase orders = new ValueOrdersBase();

                    string status = orders.GetOrderStatus(ordersCurrentShift[listView1.SelectedIndices[0]].orderIndex);

                    //if (status == "1")// || status == "2")
                    {
                        makereadyPartToolStripMenuItem.Visible = true;
                    }
                    //else
                    {
                        //makereadyPartToolStripMenuItem.Visible = false;
                    }
                }
            }
        }

        private async void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await LoadOrderDetails();
        }

        private async void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await LoadOrderNote();
        }

        private async void machinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SelectMachines();
        }

        private async void cancelShiftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CancelShift();
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

        private int CountWorkingOutOrders(int indexOrder, string machine, bool loadAllOrder = true)
        {
            int result = 0;

            for (int i = 0; i < indexOrder; i++)
            {
                if (ordersCurrentShift[i].machineOfOrder == machine || machine == "")
                {
                    if (loadAllOrder)
                    {
                        result += ordersCurrentShift[i].workingOut;
                    }
                    else
                    {
                        if (i < indexOrder - 1)
                        {
                            result += ordersCurrentShift[i].workingOut;
                        }
                    }
                }
            }

            return result;
        }

        /*private int CountWorkingOutOrders(int indexOrder, string machine, bool loadAllOrder = true)
        {
            int result = 0;

            ValueOrdersBase ordersBase = new ValueOrdersBase();

            for (int i = 0; i < indexOrder; i++)
            {
                string status = ordersBase.GetOrderStatus(ordersCurrentShift[i].orderIndex);

                if (ordersCurrentShift[i].machineOfOrder == machine || machine == "")
                {
                    if (loadAllOrder)
                    {
                        result += ordersCurrentShift[i].workingOut;
                    }
                    else
                    {
                        if (status == "4")
                        {
                            result += ordersCurrentShift[i].workingOut;
                        }
                    }
                }
            }

            return result;
        }*/

        private void LoadCurrentOrderDetails(int idx)
        {
            ClearCurrentOrderDetails();

            if (idx != -1)
            {
                ValueSettingsBase valueSettings = new ValueSettingsBase();
                GetWorkingOutTime workingOutTime = new GetWorkingOutTime(Info.shiftIndex, ordersCurrentShift);

                int orderRegistrationType = valueSettings.GetOrderRegistrationType(Form1.Info.nameOfExecutor);

                bool typeLoad;

                if (valueSettings.GetTypeLoadOrderDetails(Info.nameOfExecutor) == 0)
                {
                    typeLoad = true;
                }
                else
                {
                    typeLoad = false;
                }

                OrderStatusValue statusStrings = workingOutTime.GetWorkingOutTimeForSelectedOrder(idx, typeLoad, orderRegistrationType);

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

                /*if (orderRegistrationType == 0)
                {
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
                    label24.Text = ordersCurrentShift[idx].numberOfOrder + ": " + ordersCurrentShift[idx].nameOfOrder;
                    label26.Text = "";

                    label25.Text = "";
                    label27.Text = "";

                    label28.Text = statusStrings.caption_2;
                    label30.Text = statusStrings.value_2;

                    label29.Text = statusStrings.caption_4;
                    label31.Text = statusStrings.value_4;
                }*/
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

            ValueSettingsBase valueSettings = new ValueSettingsBase();

            int orderRegistrationType = valueSettings.GetOrderRegistrationType(Form1.Info.nameOfExecutor);

            ToolTip tooltp = new ToolTip();

            tooltp.AutomaticDelay = 2000;

            int idx = e.Item.Index;

            if (idx != -1 && e.Item != null)
            {
                /*string statusStr = GetWorkingOutMessage(idx).Item1;
                string message = GetWorkingOutMessage(idx).Item2;*/

                /*string statusStr = GetWorkingOutTimeForSelectedOrder(idx).Item1;
                string message = GetWorkingOutTimeForSelectedOrder(idx).Item2;*/

                bool typeLoad;

                if (valueSettings.GetTypeLoadItemMouseHover(Info.nameOfExecutor) == 0)
                {
                    typeLoad = true;
                }
                else
                {
                    typeLoad = false;
                }

                OrderStatusValue statusStrings = workingOutTime.GetWorkingOutTimeForSelectedOrder(idx, typeLoad, orderRegistrationType);

                string statusStr = statusStrings.statusStr;
                string message = statusStrings.message;

                tooltp.Active = true;
                tooltp.ToolTipTitle = ordersCurrentShift[idx].numberOfOrder + ": " + ordersCurrentShift[idx].nameOfOrder + " - " + statusStr;
                tooltp.SetToolTip(listView1, message);

                /*if (orderRegistrationType == 0)
                {
                    string statusStr = statusStrings.statusStr;
                    string message = statusStrings.message;

                    tooltp.Active = true;
                    tooltp.ToolTipTitle = ordersCurrentShift[idx].numberOfOrder + ": " + ordersCurrentShift[idx].nameOfOrder + " - " + statusStr;
                    tooltp.SetToolTip(listView1, message);
                }
                else
                {
                    string message = statusStrings.caption_1 + statusStrings.value_1 + Environment.NewLine + 
                        statusStrings.caption_2 + statusStrings.value_2 + Environment.NewLine +
                        statusStrings.caption_4 + statusStrings.value_4;

                    tooltp.Active = true;
                    tooltp.ToolTipTitle = ordersCurrentShift[idx].numberOfOrder + ": " + ordersCurrentShift[idx].nameOfOrder;
                    tooltp.SetToolTip(listView1, message);
                }*/
                
            }
            else
            {
                tooltp.Active = false;
            }

        }

        private async Task DataBaseSelect(bool available)
        {
            FormAddEditTestMySQL form = new FormAddEditTestMySQL(available);
            form.ShowDialog();

            LoadBaseConnectionParameters();

            await LoadUser();

            if (Form1.Info.nameOfExecutor != "")
                LoadParametersForTheSelectedUserFromBase();

            await LoadOrdersFromBase();
        }

        private async void базаДанныхToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await DataBaseSelect(true);
        }

        private void normToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowNormForm();
        }

        private async Task<int> GetIDLastOrderFromSelectedMachine(string machineName)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            int idx = -1;

            for (int i = 0; i < ordersCurrentShift.Count; i++)
            {
                if (await infoBase.GetMachineName(ordersCurrentShift[i].machineOfOrder) == machineName)
                {
                    idx = i;
                }
            }

            return idx;
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            int idx = await GetIDLastOrderFromSelectedMachine(comboBox1.Text);

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
                if (comboBox3.Items.Count > selectedIndexWOut2)
                    comboBox3.SelectedIndex = selectedIndexWOut2;
                else
                    comboBox3.SelectedIndex = 0;
            }

            selectedIndexWOut1 = comboBox2.SelectedIndex;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIndexWOut2 = comboBox3.SelectedIndex;

            UpdateWorkingOut();
        }

        private async void UpdateWorkingOut()
        {
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            int percentOverAmount = 5;

            string[] captions = { "10:00", "10:30", "11:00", "12:00" };
            string[] values = { "", "", "", "" };

            int wOut, wOutAllOrders;
            //int norm;
            int idLastOrder = await GetIDLastOrderFromSelectedMachine(comboBox3.Text);
            string machine = await infoBase.GetMachineFromName(comboBox3.Text);

            if (idLastOrder >= 0)
            {
                if (comboBox2.SelectedIndex == 0)
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, "", false);
                    wOutAllOrders = CountWorkingOutOrders(ordersCurrentShift.Count, "");
                }
                else
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, machine, false);
                    wOutAllOrders = CountWorkingOutOrders(ordersCurrentShift.Count, machine);
                }

                string status = ordersBase.GetOrderStatus(ordersCurrentShift[idLastOrder].orderIndex);
                int currentOrderForSelectedMachine = infoBase.GetCurrentOrderID(machine);
                bool isCurrentOrderForSelectedMachineActive = infoBase.GetActiveOrder(machine);

                //MessageBox.Show("Order: " + ordersCurrentShift[idLastOrder].orderIndex + " Current order: " + currentOrderForSelectedMachine + " Active: " + isCurrentOrderForSelectedMachineActive);

                int norm = ordersCurrentShift[idLastOrder].norm;
                int amount = ordersCurrentShift[idLastOrder].amountOfOrder;
                int lastCount = ordersCurrentShift[idLastOrder].lastCount;
                int done = amount - lastCount;// + ordersCurrentShift[idLastOrder].done;
                int mkTime = ordersCurrentShift[idLastOrder].plannedTimeMakeready;
                int currentMakereadyPart = getOrders.GetMakereadyPart(Info.shiftIndex, ordersCurrentShift[idLastOrder].orderIndex, ordersCurrentShift[idLastOrder].counterRepeat, Convert.ToInt32(machine)) *
                    getOrders.GetMakereadyConsider(Info.shiftIndex, ordersCurrentShift[idLastOrder].orderIndex, ordersCurrentShift[idLastOrder].counterRepeat, Convert.ToInt32(machine));
                //bool isOrderActive

                //if (currentOrderForSelectedMachine == ordersCurrentShift[idLastOrder].orderIndex)
                {
                    for (int i = 0; i < captions.Length; i++)
                    {
                        int targetTime = timeOperations.totallTimeHHMMToMinutes(captions[i]);
                        int targetCount = (targetTime - wOut - mkTime) * norm / 60;
                        int targetAmount = done + targetCount;
                        int lastTimeToWork;
                        bool isNotMorePercentOverAmount = false;

                        if (targetAmount <= amount * (1 + percentOverAmount / 100))
                        {
                            isNotMorePercentOverAmount = true;
                        }

                        if (isCurrentOrderForSelectedMachineActive)
                        {
                            lastTimeToWork = targetTime - wOut;
                            //int targetCount = (lastTimeToWork - mkTime) * norm / 60;
                            //int targetAmount = done + targetCount;
                            int lackOfTime = lastTimeToWork - (60 * lastCount / norm + mkTime);

                            if (lastTimeToWork > 0)
                            {
                                if (mkTime > 0)
                                {
                                    if (lastTimeToWork < mkTime)
                                    {
                                        values[i] = "Необходима часть приладки: " + timeOperations.TotalMinutesToHoursAndMinutesStr(lastTimeToWork);
                                    }
                                    else
                                    {
                                        if (isNotMorePercentOverAmount)
                                        {
                                            values[i] = "Необходима вся приладка";

                                            if (targetCount > 0)
                                                values[i] += " и сделать: " + targetCount.ToString("N0") + " шт.";
                                        }
                                        else
                                        {
                                            values[i] = "Не хватит " + timeOperations.TotalMinutesToHoursAndMinutesStr(lackOfTime) + "";
                                        }
                                    }
                                }
                                else
                                {
                                    if (isNotMorePercentOverAmount)
                                    {
                                        values[i] = "Необходимо сделать: " + targetCount.ToString("N0") + " шт. Сумма: " + targetAmount.ToString("N0") + " шт.";
                                    }
                                    else
                                    {
                                        values[i] = "Не хватит " + timeOperations.TotalMinutesToHoursAndMinutesStr(lackOfTime) + "";
                                    }
                                }
                            }
                            else
                            {
                                values[i] = "Выполнено";
                            }
                        }
                        else
                        {
                            lastTimeToWork = targetTime - wOutAllOrders;
                            int remainingLastCount = lastCount - ordersCurrentShift[idLastOrder].done;
                            int remainingTargetCount = targetCount - ordersCurrentShift[idLastOrder].done;

                            if (lastTimeToWork > 0)
                            {
                                if (mkTime  > 0)
                                {
                                    int remainingMakereadyPart = mkTime;

                                    if (currentMakereadyPart >= 0)
                                    {
                                        remainingMakereadyPart = mkTime - currentMakereadyPart;
                                    }

                                    if (lastTimeToWork < remainingMakereadyPart)
                                    {
                                        values[i] = "Не хватило части приладки: " + timeOperations.TotalMinutesToHoursAndMinutesStr(lastTimeToWork);
                                    }
                                    else
                                    {
                                        //int targetCount = (lastTimeToWork - remainingMakereadyPart) * norm / 60;
                                        //int targetAmount = done + targetCount;
                                        int lackOfTime = lastTimeToWork - (60 * (remainingLastCount) / norm + remainingMakereadyPart);

                                        if (isNotMorePercentOverAmount)
                                        {
                                            if (remainingTargetCount > 0)
                                            {
                                                values[i] = "Не хватило " + timeOperations.TotalMinutesToHoursAndMinutesStr(targetTime - wOutAllOrders) + " - " + remainingTargetCount.ToString("N0") + " шт. Всего: " + targetCount.ToString("N0") + " шт. Сумма: " + targetAmount.ToString("N0") + " шт.";
                                            }
                                            else
                                            {
                                                values[i] = "Не хватило всей приладки";
                                            }
                                        }
                                        else
                                        {
                                            if (remainingLastCount > 0)
                                            {
                                                values[i] = "Не хватило " + remainingLastCount.ToString("N0") + " шт. и " + timeOperations.TotalMinutesToHoursAndMinutesStr(lackOfTime) + "";
                                            }
                                            else
                                            {
                                                values[i] = "Не хватило " + timeOperations.TotalMinutesToHoursAndMinutesStr(lastTimeToWork) + "";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    int lackOfTime = lastTimeToWork - (60 * (remainingLastCount) / norm);

                                    if (isNotMorePercentOverAmount)
                                    {
                                        values[i] = "Не хватило " + timeOperations.TotalMinutesToHoursAndMinutesStr(targetTime - wOutAllOrders) + " - " + remainingTargetCount.ToString("N0") + " шт. Всего: " + targetCount.ToString("N0") + " шт. Сумма: " + targetAmount.ToString("N0") + " шт.";
                                    }
                                    else
                                    {
                                        if (remainingLastCount > 0)
                                        {
                                            values[i] = "Не хватило " + remainingLastCount.ToString("N0") + " шт. и " + timeOperations.TotalMinutesToHoursAndMinutesStr(lackOfTime) + "";
                                        }
                                        else
                                        {
                                            values[i] = "Не хватило " + timeOperations.TotalMinutesToHoursAndMinutesStr(lackOfTime) + "";
                                        }
                                    }
                                }
                            }
                            else
                            {
                                values[i] = "Выполнено";
                            }
                        }
                    }
                }
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

        //OOOLLLDDD
        private async void UpdateWorkingOutOLD()
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            int percentOverAmount = 5;

            string[] captions = { "10:00", "10:30", "11:00", "12:00" };
            string[] values = { "", "", "", "" };

            int wOut, wOutAllOrders;
            //int norm;
            int idLastOrder = await GetIDLastOrderFromSelectedMachine(comboBox3.Text);
            string machine = await infoBase.GetMachineFromName(comboBox3.Text);

            if (idLastOrder >= 0)
            {
                if (comboBox2.SelectedIndex == 0)
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, "", false);
                    wOutAllOrders = CountWorkingOutOrders(ordersCurrentShift.Count, "");
                }
                else
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, machine, false);
                    wOutAllOrders = CountWorkingOutOrders(ordersCurrentShift.Count, machine);
                }

                string status = ordersBase.GetOrderStatus(ordersCurrentShift[idLastOrder].orderIndex);

                int norm = ordersCurrentShift[idLastOrder].norm;
                int amount = ordersCurrentShift[idLastOrder].amountOfOrder;
                int lastCount = ordersCurrentShift[idLastOrder].lastCount;
                int done = amount - lastCount;// + ordersCurrentShift[idLastOrder].done;
                int mkTime = ordersCurrentShift[idLastOrder].plannedTimeMakeready;

                for (int i = 0; i < captions.Length; i++)
                {
                    int targetTime = timeOperations.totallTimeHHMMToMinutes(captions[i]);
                    int lastTimeToWork = targetTime - wOut;

                    int targetCount = (lastTimeToWork - mkTime) * norm / 60;
                    int targetAmount = done + targetCount;
                    
                    if (status == "1")
                    {
                        int lackOfTime = lastTimeToWork - (60 * lastCount / norm + mkTime);

                        if (lastTimeToWork > 0)
                        {
                            if (lastTimeToWork < mkTime)
                            {
                                values[i] = "Необходима часть приладки: " + timeOperations.TotalMinutesToHoursAndMinutesStr(lastTimeToWork);
                            }
                            else
                            {
                                if (targetAmount <= amount * (1 + percentOverAmount / 100))
                                {
                                    values[i] = "Необходима вся приладка";

                                    if (targetCount > 0)
                                        values[i] += " и сделать: " + targetCount.ToString("N0") + " шт.";
                                }
                                else
                                {
                                    values[i] = "Не хватит " + timeOperations.TotalMinutesToHoursAndMinutesStr(lackOfTime) + "";
                                }
                            }
                        }
                        else
                        {
                            values[i] = "Выполнено";
                        }
                    }

                    if (status == "3" || status == "2")
                    {
                        if (targetTime > wOut)
                        {
                            int lackOfTime = targetTime - (60 * lastCount / norm + mkTime);

                            if (targetAmount <= amount * (1 + percentOverAmount / 100))
                            {
                                values[i] = "Необходимо сделать: " + targetCount.ToString("N0") + " шт. Сумма: " + targetAmount.ToString("N0") + " шт.";
                            }
                            else
                            {
                                values[i] = "Не хватит " + timeOperations.TotalMinutesToHoursAndMinutesStr(lackOfTime) + "";
                            }
                        }
                        else
                        {
                            values[i] = "Выполнено";
                        }
                    }

                    if (status == "4")
                    {
                        if (targetTime > wOutAllOrders)
                        {
                            values[i] = "Не хватает " + timeOperations.TotalMinutesToHoursAndMinutesStr(targetTime - wOutAllOrders) + "";
                        }
                        else
                        {
                            values[i] = "Выполнено";
                        }
                    }
                }
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

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex == 0)
            {
                tableLayoutPanel9.ColumnStyles[1].Width = 50;

                //comboBox3.SelectedIndex = selectedIndexWOut2;
            }
            else
            {
                tableLayoutPanel9.ColumnStyles[1].Width = 0;

                selectedIndexPreviewWOut2 = comboBox4.SelectedIndex - 1;
            }

            if (comboBox5.SelectedIndex == selectedIndexPreviewWOut2)
            {
                UpdatePreviewCalculate();
            }
            else
            {
                if (comboBox5.Items.Count > selectedIndexPreviewWOut2)
                    comboBox5.SelectedIndex = selectedIndexPreviewWOut2;
                else
                    comboBox5.SelectedIndex = 0;
            }

            selectedIndexPreviewWOut1 = comboBox4.SelectedIndex;
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIndexWOut2 = comboBox5.SelectedIndex;

            UpdatePreviewCalculate();
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox8.SelectedIndex)
            {
                case 0:
                    tableLayoutPanel10.ColumnStyles[1].Width = 0;
                    tableLayoutPanel10.ColumnStyles[3].Width = 100;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 40;
                    panel1.Visible = false;
                    numericUpDown1.Visible = true;
                    numericUpDown1.Enabled = true;
                    numericUpDown1.Value = 0;
                    button2.Visible = true;
                    UpdatePreviewWorkingOut();
                    break;
                case 1:
                    tableLayoutPanel10.ColumnStyles[1].Width = 0;
                    tableLayoutPanel10.ColumnStyles[3].Width = 100;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    panel1.Visible = false;
                    numericUpDown1.Visible = true;
                    numericUpDown1.Enabled = false;
                    numericUpDown1.Value = 0;
                    button2.Visible = false;
                    UpdatePreviewWorkingOut();
                    break;
                case 2:
                    tableLayoutPanel10.ColumnStyles[1].Width = 0;
                    tableLayoutPanel10.ColumnStyles[3].Width = 100;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 40;
                    panel1.Visible = false;
                    numericUpDown1.Visible = true;
                    numericUpDown1.Enabled = true;
                    numericUpDown1.Value = 0;
                    button2.Visible = true;
                    UpdatePreviewQuantityCalculationByTime(true);
                    break;
                case 3:
                    tableLayoutPanel10.ColumnStyles[1].Width = 0;
                    tableLayoutPanel10.ColumnStyles[3].Width = 100;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    panel1.Visible = false;
                    numericUpDown1.Visible = true;
                    numericUpDown1.Enabled = false;
                    numericUpDown1.Value = 100;
                    button2.Visible = false;
                    UpdatePreviewQuantityCalculationByTime(true);
                    break;
                case 4:
                    tableLayoutPanel10.ColumnStyles[1].Width = 0;
                    tableLayoutPanel10.ColumnStyles[3].Width = 100;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    panel1.Visible = false;
                    numericUpDown1.Visible = true;
                    numericUpDown1.Enabled = false;
                    numericUpDown1.Value = 110;
                    button2.Visible = false;
                    UpdatePreviewQuantityCalculationByTime(true);
                    break;
                case 5:
                    tableLayoutPanel10.ColumnStyles[1].Width = 0;
                    tableLayoutPanel10.ColumnStyles[3].Width = 100;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    panel1.Visible = false;
                    numericUpDown1.Visible = true;
                    numericUpDown1.Enabled = false;
                    numericUpDown1.Value = 120;
                    button2.Visible = false;
                    UpdatePreviewQuantityCalculationByTime(true);
                    break;
                case 6:
                    tableLayoutPanel10.ColumnStyles[1].Width = 100;
                    tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    panel1.Visible = true;
                    numericUpDown1.Visible = false;
                    numericUpDown5.Enabled = true;
                    numericUpDown6.Enabled = true;
                    numericUpDown5.Value = 0;
                    numericUpDown6.Value = 0;
                    button2.Visible = true;
                    UpdatePreviewQuantityCalculationByTime();
                    break;
                case 7:
                    tableLayoutPanel10.ColumnStyles[1].Width = 100;
                    tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    panel1.Visible = true;
                    numericUpDown1.Visible = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    numericUpDown5.Value = 10;
                    numericUpDown6.Value = 0;
                    button2.Visible = false;
                    UpdatePreviewQuantityCalculationByTime();
                    break;
                case 8:
                    tableLayoutPanel10.ColumnStyles[1].Width = 100;
                    tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    panel1.Visible = true;
                    numericUpDown1.Visible = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    numericUpDown5.Value = 10;
                    numericUpDown6.Value = 30;
                    button2.Visible = false;
                    UpdatePreviewQuantityCalculationByTime();
                    break;
                case 9:
                    tableLayoutPanel10.ColumnStyles[1].Width = 100;
                    tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    panel1.Visible = true;
                    numericUpDown1.Visible = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    numericUpDown5.Value = 11;
                    numericUpDown6.Value = 0;
                    button2.Visible = false;
                    UpdatePreviewQuantityCalculationByTime();
                    break;
                case 10:
                    tableLayoutPanel10.ColumnStyles[1].Width = 100;
                    tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    //tableLayoutPanel10.ColumnStyles[3].Width = 0;
                    panel1.Visible = true;
                    numericUpDown1.Visible = false;
                    numericUpDown5.Enabled = false;
                    numericUpDown6.Enabled = false;
                    numericUpDown5.Value = 12;
                    numericUpDown6.Value = 0;
                    button2.Visible = false;
                    UpdatePreviewQuantityCalculationByTime();
                    break;
                default:
                    break;
            }
        }

        private void UpdatePreviewCalculate()
        {
            switch (comboBox8.SelectedIndex)
            {
                case 0:
                    UpdatePreviewWorkingOut();
                    break;
                case 2:
                    UpdatePreviewQuantityCalculationByTime(true);
                    break;
                case 6:
                    UpdatePreviewQuantityCalculationByTime();
                    break;
                default:
                    break;
            }
        }

        private async void UpdatePreviewWorkingOut()
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();

            int wOut, wOutAllOrders;
            //int norm;
            int idLastOrder = await GetIDLastOrderFromSelectedMachine(comboBox5.Text);
            string machine = await infoBase.GetMachineFromName(comboBox5.Text);

            bool activeOrderFromMachine = infoBase.GetActiveOrder(machine);

            label40.Text = "";
            label41.Text = "";
            label43.Text = "";
            label44.Text = "";

            if (idLastOrder >= 0)
            {
                if (comboBox4.SelectedIndex == 0)
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, "", false);
                    wOutAllOrders = CountWorkingOutOrders(ordersCurrentShift.Count, "");
                }
                else
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, machine, false);
                    wOutAllOrders = CountWorkingOutOrders(ordersCurrentShift.Count, machine);
                }

                string status = ordersBase.GetOrderStatus(ordersCurrentShift[idLastOrder].orderIndex);
                
                int norm = ordersCurrentShift[idLastOrder].norm;
                int amount = ordersCurrentShift[idLastOrder].amountOfOrder;
                int lastCount = ordersCurrentShift[idLastOrder].lastCount;
                int done = amount - lastCount;// + ordersCurrentShift[idLastOrder].done;
                int mkTime = ordersCurrentShift[idLastOrder].plannedTimeMakeready;

                int previewWOut = 0;

                if (comboBox8.SelectedIndex == 1)
                {
                    numericUpDown1.Value = lastCount;
                }

                int previewCountValue = (int)numericUpDown1.Value;
                int previewWOutCurrentOrder = previewCountValue * 60 / norm;

                label41.Text = ordersCurrentShift[idLastOrder].numberOfOrder + ": " + ordersCurrentShift[idLastOrder].nameOfOrder;
                label40.Text = "Текущая выработка: " + timeOperations.MinuteToTimeString(wOut) + " ч.";

                //if (status != "4")
                {
                    if (activeOrderFromMachine)
                    {
                        previewWOut = wOut;
                    }
                    else
                    {
                        if (ordersCurrentShift[idLastOrder].done > 0)
                        {
                            previewWOut = wOut;
                        }
                        else
                        {
                            previewWOut = wOutAllOrders;
                        }
                    }

                    if (previewCountValue > 0)
                    {
                        previewWOut += mkTime + previewWOutCurrentOrder;
                    }
                }

                /*if (status == "4")
                {
                    previewWOut = wOutAllOrders + (previewCountValue * 60 / norm);
                }*/
                label42.Text = "Выработка:";
                label43.Text = timeOperations.MinuteToTimeString(previewWOut) + " ч.";
                label44.Text = getPercent.PercentString(previewWOut);
            }
        }

        private async void UpdatePreviewQuantityCalculationByTime(bool calculateFromPercent = false)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();

            int wOut, wOutAllOrders;
            //int norm;
            int idLastOrder = await GetIDLastOrderFromSelectedMachine(comboBox5.Text);
            string machine = await infoBase.GetMachineFromName(comboBox5.Text);

            bool activeOrderFromMachine = infoBase.GetActiveOrder(machine);

            label41.Text = "";
            label40.Text = "";
            label43.Text = "";
            label44.Text = "";

            if (idLastOrder >= 0)
            {
                if (comboBox4.SelectedIndex == 0)
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, "", false);
                    wOutAllOrders = CountWorkingOutOrders(ordersCurrentShift.Count, "");
                }
                else
                {
                    wOut = CountWorkingOutOrders(ordersCurrentShift.Count, machine, false);
                    wOutAllOrders = CountWorkingOutOrders(ordersCurrentShift.Count, machine);
                }

                string previewValueFirst = "";
                string previewValueSecond = "";

                int norm = ordersCurrentShift[idLastOrder].norm;
                int amount = ordersCurrentShift[idLastOrder].amountOfOrder;
                int lastCount = ordersCurrentShift[idLastOrder].lastCount;
                int done = amount - lastCount;// + ordersCurrentShift[idLastOrder].done;
                int mkTime = ordersCurrentShift[idLastOrder].plannedTimeMakeready;

                int previewTimeValue;
                int previewCountValue = 0;

                if (calculateFromPercent)
                {
                    previewTimeValue = (int)numericUpDown1.Value * 650 / 100;
                    previewValueSecond = timeOperations.MinuteToTimeString(previewTimeValue) + " ч.";
                }
                else
                {
                    previewTimeValue = (int)numericUpDown5.Value * 60 + (int)numericUpDown6.Value;
                    previewValueSecond = getPercent.PercentString(previewTimeValue);
                }

                label41.Text = ordersCurrentShift[idLastOrder].numberOfOrder + ": " + ordersCurrentShift[idLastOrder].nameOfOrder;
                label40.Text = "Текущая выработка: " + timeOperations.MinuteToTimeString(wOut) + " ч.";

                if (previewTimeValue > wOut)
                {
                    int timeForCurrentOrder = previewTimeValue - wOut;

                    if (timeForCurrentOrder > mkTime)
                    {
                        int timeForCurentOrderWitchoutMakeready = timeForCurrentOrder - mkTime;

                        previewCountValue = timeForCurentOrderWitchoutMakeready * norm / 60;
                        previewValueFirst = previewCountValue.ToString("N0") + " шт. Сумма: " + (done + previewCountValue).ToString("N0") + " шт.";
                        label42.Text = "Количество:";
                    }
                    else
                    {
                        previewValueFirst = timeOperations.MinuteToTimeString(timeForCurrentOrder) + " ч.";
                        label42.Text = "Приладка:";
                    }
                }
                else
                {
                    label42.Text = "Выполнено";
                }

                label43.Text = previewValueFirst;
                label44.Text = previewValueSecond;
            }
        }

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

        private async void typesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await LoadTypes();
        }

        private async void makereadyPartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await LoadMakereadyParts(ordersCurrentShift[listView1.SelectedIndices[0]].id);
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

        private async void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings form = new FormSettings(Info.nameOfExecutor);
            form.ShowDialog();

            await LoadOrdersFromBase();
        }

        private void planToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValueUserBase valueUser = new ValueUserBase();

            int lastMachine = valueUser.GetLastMachineForUser(Form1.Info.nameOfExecutor);

            FormLoadOrders fm = new FormLoadOrders(true, Info.nameOfExecutor, lastMachine);
            fm.ShowDialog();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            UpdatePreviewCalculate();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            UpdatePreviewQuantityCalculationByTime();
        }

        private void numericUpDown1_Click(object sender, EventArgs e)
        {
            numericUpDown1.Select(0, numericUpDown1.Text.Length);
        }

        private void statisticAllUsersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowStatisticForm();
        }

        private void statisticAllWorkingOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormStatisticAllUsers fm = new FormStatisticAllUsers();
            fm.ShowDialog();
        }

        private void statisticYearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormStatisticYearForUser fm = new FormStatisticYearForUser(Convert.ToInt32(Info.nameOfExecutor));
            fm.ShowDialog();
        }

        private void numericUpDown5_Click(object sender, EventArgs e)
        {
            numericUpDown5.Select(0, numericUpDown5.Text.Length);
        }

        private void numericUpDown6_Click(object sender, EventArgs e)
        {
            numericUpDown6.Select(0, numericUpDown6.Text.Length);
        }
        private void numericUpDown1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                UpdatePreviewCalculate();
            }
        }

        private void numericUpDown5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                UpdatePreviewCalculate();
            }
        }

        private void numericUpDown6_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                UpdatePreviewCalculate();
            }
        }

        private async void addEditorderNEWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CheckCurrentShiftActivity();

            Info.active = false;
            //FormAddCloseOrder form = new FormAddCloseOrder(Info.shiftIndex, Info.nameOfExecutor);
            FormAddCloseEditOrder form = new FormAddCloseEditOrder(Info.shiftIndex);
            form.ShowDialog();
            await LoadOrdersFromBase();
            Info.active = true;
        }
    }
}
