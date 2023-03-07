using MySql.Data.MySqlClient;
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

            INISettings ini = new INISettings();

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

        
        class OrderStatusStrings
        {
            public string statusStr;
            public string currentLastTimeForMakereadyCaption;
            public string currentLastTimeForMakeready;
            public string currentLastTimeForFullWorkCaption;
            public string currentLastTimeForFullWork;
            public string timeToEndMKCaption;
            public string timeToEndMK;
            public string timeToEndWorkCaption;
            public string timeToEndWork;
            public string planedCoutOrderCaption;
            public string planedCoutOrder;
            public string workTimeDifferentCaption;
            public string workTimeDifferent;
            public int mkTimeDifferent;
            public int wkTimeDifferent;
            public string message;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="statusStrVal">Статус выполнения</param>
            /// <param name="currentLastTimeForMakereadyCaptionVal">Заголовок остатка времени на приладку</param>
            /// <param name="currentLastTimeForMakereadyVal">Значение остатка времени на приладку</param>
            /// <param name="currentLastTimeForFullWorkCaptionVal">Заголовок остатка общего времени для выполнения заказа</param>
            /// <param name="currentLastTimeForFullWorkVal">Значение остатка общего времени для выполнения заказа</param>
            /// <param name="timeToEndMKCaptionVal">Заголовок времени завершения приладки</param>
            /// <param name="timeToEndMKVal">Значение времени завершения приладки</param>
            /// <param name="timeToEndWorkCaptionVal">Заголовок времени завершения заказа</param>
            /// <param name="timeToEndWorkVal">Значенеи времени завершения заказа</param>
            /// <param name="planedCoutOrderCaptionVal">Заголовок планируемой выработки</param>
            /// <param name="planedCoutOrderVal">Значение планируемой выработки</param>
            /// <param name="workTimeDifferentCaptionVal">Заголовок отклонекния от нормы</param>
            /// <param name="workTimeDifferentVal">Значение отклонекния от нормы</param>
            /// <param name="mkTimeDifferentVal">Отклонение времени приладки от нормы</param>
            /// <param name="wkTimeDifferentVal">Отклонение времени работы от  нормы</param>
            /// <param name="messageVal">Сообщение во всплывающей подсказке</param>
            public OrderStatusStrings(string statusStrVal, string currentLastTimeForMakereadyCaptionVal, string currentLastTimeForMakereadyVal, string currentLastTimeForFullWorkCaptionVal,
                string currentLastTimeForFullWorkVal, string timeToEndMKCaptionVal, string timeToEndMKVal, string timeToEndWorkCaptionVal, string timeToEndWorkVal, string planedCoutOrderCaptionVal,
                string planedCoutOrderVal, string workTimeDifferentCaptionVal, string workTimeDifferentVal, int mkTimeDifferentVal, int wkTimeDifferentVal, string messageVal)
            {
                this.statusStr = statusStrVal;
                this.currentLastTimeForMakereadyCaption = currentLastTimeForMakereadyCaptionVal;
                this.currentLastTimeForMakeready = currentLastTimeForMakereadyVal;
                this.currentLastTimeForFullWorkCaption = currentLastTimeForFullWorkCaptionVal;
                this.currentLastTimeForFullWork = currentLastTimeForFullWorkVal;
                this.timeToEndMKCaption = timeToEndMKCaptionVal;
                this.timeToEndMK = timeToEndMKVal;
                this.timeToEndWorkCaption = timeToEndWorkCaptionVal;
                this.timeToEndWork = timeToEndWorkVal;
                this.planedCoutOrderCaption = planedCoutOrderCaptionVal;
                this.planedCoutOrder = planedCoutOrderVal;
                this.workTimeDifferentCaption = workTimeDifferentCaptionVal;
                this.workTimeDifferent = workTimeDifferentVal;
                this.mkTimeDifferent = mkTimeDifferentVal;
                this.wkTimeDifferent = wkTimeDifferentVal;
                this.message = messageVal;
            }

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

            public OrderStatusValue(string statusStrVal, string captionVal_1, string valueVal_1, string captionVal_2, string valueVal_2, 
                string captionVal_3, string valueVal_3, string captionVal_4, string valueVal_4,
                int mkTimeDifferentVal, int wkTimeDifferentVal, string messageVal)
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
            }

        }

        bool _wOutFromStartShift = false;

        List<Order> ordersCurrentShift;

        int fullTimeWorkingOut;
        int fullDone;

        int selectedIndexActive = 0;
        int selectedIndexWOut1 = 0;
        int selectedIndexWOut2 = 0;

        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        public static String connectionFile = "connections.ini";

        public static class Info
        {
            public static bool active = false;
            //public static int indexItem = -1;
            public static String nameOfExecutor = "";
            public static String startOfShift = "";
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
            FormAddCloseOrder form = new FormAddCloseOrder(Info.startOfShift, Info.nameOfExecutor);
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

            if(!connection.IsServerConnected(BaseConnectionParameters.host, BaseConnectionParameters.port, BaseConnectionParameters.database, 
                BaseConnectionParameters.username, BaseConnectionParameters.password))
            {
                DataBaseSelect(false);
            }
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

            FormLoadUserForm form = new FormLoadUserForm(loadMode);
            //this.Visible = false;
            form.ShowDialog();

            LoadParametersFromBase("mainForm");

            LoadUser();

            ViewBaseConnectionParameters();


            if (Form1.Info.startOfShift == "")
            {
                ShowUserSelectMachineForm();
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
            FormFullListOrders form = new FormFullListOrders(false, "", "", "");
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

            if (Form1.Info.nameOfExecutor != "")
                ApplyParameterLine(getSettings.GetParameterLine(Form1.Info.nameOfExecutor, nameForm));
            else
                ApplyParameterLine(getSettings.GetParameterLine("0", nameForm));
        }

        private void LoadParametersForTheSelectedUserFromBase()
        {
            ValueUserBase getUser = new ValueUserBase();

            Info.startOfShift = getUser.GetCurrentShiftStart(Info.nameOfExecutor);

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

            ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(Form1.Info.startOfShift, "");

            fullTimeWorkingOut = 0;
            fullDone = 0;

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

                int typeLoad = valueSettings.GetTypeLoadDeviationToMainLV(Info.nameOfExecutor);
                int typeView = valueSettings.GetTypeViewDeviationToMainLV(Info.nameOfExecutor);

                if (typeLoad == 0)
                {
                    OrderStatusValue statusValue = GetWorkingOutTimeForSelectedOrder(index, true);

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

                listView1.Items.Add(item);

                fullTimeWorkingOut += ordersCurrentShift[index].workingOut;
                fullDone += ordersCurrentShift[index].done;
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

            DateTime date;
            if (Form1.Info.startOfShift != "")
                date = Convert.ToDateTime(Form1.Info.startOfShift);
            else
                date = DateTime.Now;

            while (!token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();

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
            //cancelTokenSource = new CancellationTokenSource();

            var task = Task.Run(() => LoadDetailsMount(cancelTokenSource.Token), cancelTokenSource.Token);

            /*CancellationToken token = cancelTokenSource.Token;

            Task task = new Task(() => LoadDetailsMount(token));

            task.Start();*/
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
            CancellationToken token = cancelTokenSource.Token;
            token.ThrowIfCancellationRequested();

            SaveParameterToBase("mainForm");

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

            if (getUserMachines.GetMachinesForUserActive(Info.nameOfExecutor) == true)
                button6.Enabled = false;
            else
                button6.Enabled = true;

            if (Form1.Info.startOfShift != "")
                button1.Enabled = true;
            else
                button1.Enabled = false;

            label6.Text = usersBase.GetNameUser(Info.nameOfExecutor);
            label7.Text = Info.startOfShift;
            label8.Text = dtOperations.TotalMinutesToHoursAndMinutesStr(fullTimeWorkingOut);
            label9.Text = getPercent.PercentString(fullTimeWorkingOut);
            label10.Text = fullDone.ToString("N0");

            if (Info.nameOfExecutor != "")
                LaodDetailsForCurrentMount();
        }

        private void EraseInfo()
        {
            //Form1.Info.nameOfExecutor = "";
            Form1.Info.startOfShift = "";

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
                    String order = "";
                    if (getInfo.GetCurrentOrderNumber(machine) != "")
                        order = getInfo.GetCurrentOrderNumber(machine) + ", " + getOrder.GetOrderName(machine, getInfo.GetCurrentOrderNumber(machine), getInfo.GetCurrentOrderModification(machine));

                    ListViewItem item = new ListViewItem();

                    item.Name = machine;
                    item.Text = getInfo.GetMachineName(machine);
                    item.SubItems.Add(getOrder.GetOrderStatusName(machine, getInfo.GetCurrentOrderNumber(machine), getInfo.GetCurrentOrderModification(machine)));
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

                getShift.CloseShift(Info.startOfShift, DateTime.Now.ToString());
                infoBase.CompleteTheShift(Info.nameOfExecutor);
                userBase.UpdateCurrentShiftStart(Info.nameOfExecutor, "");
                getShift.SetNoteShift(Info.startOfShift, form.NoteVal);
                getShift.SetCheckFullShift(Info.startOfShift, form.FullShiftVal);
                getShift.SetCheckOvertimeShift(Info.startOfShift, form.OvertimeShiftVal);

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

            if (Info.startOfShift != "")
            {
                string startOfShift = userBase.GetCurrentShiftStart(Info.nameOfExecutor);
                bool activityShift = valueShifts.CheckShiftActivity(Info.startOfShift);

                //if (!activityShift || startOfShift == "")
                if (!activityShift || startOfShift == "")
                {
                    Info.active = false;
                    ClearAll();
                    ShowUserForm();
                    Info.active = true;
                }
            }

            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Info.active = false;
            ClearAll();
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

            string startOfShift = userBase.GetCurrentShiftStart(Info.nameOfExecutor);

            if (startOfShift == "")
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
            LoadParametersFromBase("mainForm");

            

            //LoadUser();
            //LoadParametersForTheSelectedUserFromBase(Form1.Info.mashine);
            //LoadOrdersFromBase();
            //ViewDetailsForUser();
            //button1.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancelTokenSource.Cancel();

            SaveParameterToBase("mainForm");
            
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
                    form = new FormAddCloseOrder(Info.startOfShift, Info.nameOfExecutor);
                }
                else
                {
                    form = new FormAddCloseOrder(Info.startOfShift,
                        ordersCurrentShift[listView1.SelectedIndices[0]].numberOfOrder,
                        ordersCurrentShift[listView1.SelectedIndices[0]].modificationOfOrder,
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

            form = new FormPrivateNote(Info.startOfShift,
                ordersCurrentShift[listView1.SelectedIndices[0]].numberOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].modificationOfOrder,
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

            form = new FormTypesInTheOrder(Info.startOfShift,
                ordersCurrentShift[listView1.SelectedIndices[0]].numberOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].modificationOfOrder,
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

        private int CountWorkingOutFromPreviusOrders(int indexOrder, string machine)
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

            OrderStatusValue orderStatus = new OrderStatusValue("", "", "", "", "", "", "", "", "", 0, 0, "");

            string newLine = Environment.NewLine;

            string machine = ordersCurrentShift[indexOrder].machineOfOrder;
            string status = valueOrders.GetOrderStatus(getOrders.GetOrderID(ordersCurrentShift[indexOrder].id));

            string shiftStart = Info.startOfShift; //get from Info or user base

            if (plannedWorkingOut)
            {
                shiftStart = startShift.PlanedStartShift(Info.startOfShift); //get from method
            }

            int workTime = timeOperations.DateDifferenceToMinutes(DateTime.Now.ToString(), shiftStart); //общее время с начала смены
            int countPreviusWorkingOut = CountWorkingOutFromPreviusOrders(indexOrder, machine);// считать до указанного индекса
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

            orderStatus.mkTimeDifferent = mkTimeDifferent;
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
                    //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                }
                else
                {
                    orderStatus.caption_1 = "Остаток времени на приладку: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);
                    //message = "Остаток времени на приладку: " + mkTimeDiff;
                }

                orderStatus.caption_2 = "Остаток времени для выполнение заказа: ";
                orderStatus.value_2 = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
                //MessageBox.Show(timeDiff + " + " + mkLastTime + " = " + timeOperations.TimeAmount(timeDiff, mkLastTime));
                //strings[1] = timeOperations.MinuteToTimeString(timeDiff);

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
                    //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                    orderStatus.caption_1 = "Отставание: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForFullWork * (-1));
                }
                else
                {
                    //message = "Остаток времени на приладку: " + mkTimeDiff;
                    orderStatus.caption_1 = "Остаток времени: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
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
                }
                else
                {
                    orderStatus.caption_1 = "Опережение: ";
                    orderStatus.value_1 = timeOperations.MinuteToTimeString(workTimeDifferent);
                }

                orderStatus.message = orderStatus.caption_1 + orderStatus.value_1;
            }

            return orderStatus;
        }





        /*private OrderStatusStrings GetWorkingOutTimeForSelectedOrder(int indexOrder)
        {
            bool _plannedWorkingOut = true;

            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase valueOrders = new ValueOrdersBase();
            GetNumberShiftFromTimeStart startShift = new GetNumberShiftFromTimeStart();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();

            OrderStatusStrings orderStatus = new OrderStatusStrings("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

            string newLine = Environment.NewLine;

            string machine = ordersCurrentShift[indexOrder].machineOfOrder;
            string status = valueOrders.GetOrderStatus(getOrders.GetOrderID(ordersCurrentShift[indexOrder].id));

            string shiftStart = Info.startOfShift; //get from Info or user base

            if (_plannedWorkingOut)
            {
                shiftStart = startShift.PlanedStartShift(Info.startOfShift); //get from method
            }

            int workTime = timeOperations.DateDifferenceToMinutes(DateTime.Now.ToString(), shiftStart); //общее время с начала смены
            int countPreviusWorkingOut = CountWorkingOutFromPreviusOrders(indexOrder, machine);// считать до указанного индекса
            int countPreviusOutages = CountPreviusOutages(); // еще проработка требуется
            int countWorkingOut = countPreviusWorkingOut + countPreviusOutages;

            int lastTimeForMK = ordersCurrentShift[indexOrder].plannedTimeMakeready;
            int lastTimeForWK = ordersCurrentShift[indexOrder].plannedTimeWork;
            int fullTimeForWork = lastTimeForMK + lastTimeForWK;

            string facticalTimeMakereadyStop = getOrders.GetTimeToMakereadyStop(ordersCurrentShift[indexOrder].id);
            string facticalTimeToWorkStop = getOrders.GetTimeToWorkStop(ordersCurrentShift[indexOrder].id);

            int currentLead;
            string timeStartOrder;

            if (_plannedWorkingOut)
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
            
            
            *//*Console.WriteLine("<<<<<" + DateTime.Now.ToString() + ">>>>>");

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
        string timeToEndWork = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), currentLastTimeForFullWork - fullTimeForWork);*//*

        if (status == "1" || status == "2")
        {
            orderStatus.statusStr = "приладка заказа";

            if (currentLastTimeForMakeready < 0)
            {
                orderStatus.currentLastTimeForMakereadyCaption = "Отставание: ";
                orderStatus.currentLastTimeForMakeready = timeOperations.MinuteToTimeString(currentLastTimeForMakeready * (-1));
                //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
            }
            else
            {
                orderStatus.currentLastTimeForMakereadyCaption = "Остаток времени на приладку: ";
                orderStatus.currentLastTimeForMakeready = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);
                //message = "Остаток времени на приладку: " + mkTimeDiff;
            }

            orderStatus.currentLastTimeForFullWorkCaption = "Остаток времени для выполнение заказа: ";
            orderStatus.currentLastTimeForFullWork = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
            //MessageBox.Show(timeDiff + " + " + mkLastTime + " = " + timeOperations.TimeAmount(timeDiff, mkLastTime));
            //strings[1] = timeOperations.MinuteToTimeString(timeDiff);

            orderStatus.timeToEndMKCaption = "Планирумое время завершения приладки: ";
            orderStatus.timeToEndMK = timeToEndMK;

            orderStatus.timeToEndWorkCaption = "Планирумое время завершения заказа: ";
            orderStatus.timeToEndWork = timeToEndWork;

            orderStatus.message = orderStatus.currentLastTimeForMakereadyCaption + orderStatus.currentLastTimeForMakeready + newLine +
                orderStatus.currentLastTimeForFullWorkCaption + orderStatus.currentLastTimeForFullWork + newLine +
                orderStatus.timeToEndMKCaption + orderStatus.timeToEndMK + newLine +
                orderStatus.timeToEndWorkCaption + orderStatus.timeToEndWork;
        }

        if (status == "3")
        {
            orderStatus.statusStr = "заказ выполняется";

            if (currentLastTimeForFullWork < 0)
            {
                //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                orderStatus.currentLastTimeForFullWorkCaption = "Отставание: ";
                orderStatus.currentLastTimeForFullWork = timeOperations.MinuteToTimeString(currentLastTimeForFullWork * (-1));
            }
            else
            {
                //message = "Остаток времени на приладку: " + mkTimeDiff;
                orderStatus.currentLastTimeForFullWorkCaption = "Остаток времени: ";
                orderStatus.currentLastTimeForFullWork = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
            }

            orderStatus.planedCoutOrderCaption = "Плановая выработка: ";
            orderStatus.planedCoutOrder = planedCoutOrder.ToString("N0");

            orderStatus.timeToEndWorkCaption = "Планирумое время завершения: ";
            orderStatus.timeToEndWork = timeToEndWork;

            orderStatus.message = orderStatus.currentLastTimeForFullWorkCaption + orderStatus.currentLastTimeForFullWork + newLine +
                orderStatus.planedCoutOrderCaption + orderStatus.planedCoutOrder + newLine +
                orderStatus.timeToEndWorkCaption + orderStatus.timeToEndWork;
        }

        if (status == "4")
        {
            orderStatus.statusStr = "заказ завершен";

            if (workTimeDifferent < 0)
            {
                orderStatus.workTimeDifferentCaption = "Отставание: ";
                orderStatus.workTimeDifferent = timeOperations.MinuteToTimeString(workTimeDifferent * (-1));
            }
            else
            {
                orderStatus.workTimeDifferentCaption = "Опережение: ";
                orderStatus.workTimeDifferent = timeOperations.MinuteToTimeString(workTimeDifferent);
            }

            orderStatus.message = orderStatus.workTimeDifferentCaption + orderStatus.workTimeDifferent;
        }

        return orderStatus;
    }*/






        /*private (string, string, string[], string[]) GetWorkingOutTimeForSelectedOrder(int indexOrder)
        {
            bool _plannedWorkingOut = true;

            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase valueOrders = new ValueOrdersBase();
            GetNumberShiftFromTimeStart startShift = new GetNumberShiftFromTimeStart();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();

            OrderStatusStrings orderStatus = new OrderStatusStrings("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "");

            string newLine = Environment.NewLine;

            string message = "";
            string statusStr = "";

            string[] caption = { "", "", "", "" };
            string[] strings = { "", "", "", "" };

            string machine = ordersCurrentShift[indexOrder].machineOfOrder;
            string status = valueOrders.GetOrderStatus(getOrders.GetOrderID(ordersCurrentShift[indexOrder].id));

            string shiftStart = Info.startOfShift; //get from Info or user base

            if (_plannedWorkingOut)
            {
                shiftStart = startShift.PlanedStartShift(Info.startOfShift); //get from method
            }

            int workTime = timeOperations.DateDifferenceToMinutes(DateTime.Now.ToString(), shiftStart); //общее время с начала смены
            int countPreviusWorkingOut = CountWorkingOutFromPreviusOrders(indexOrder, machine);// считать до указанного индекса
            int countPreviusOutages = CountPreviusOutages(); // еще проработка требуется
            int countWorkingOut = countPreviusWorkingOut + countPreviusOutages;

            int lastTimeForMK = ordersCurrentShift[indexOrder].plannedTimeMakeready;
            int lastTimeForWK = ordersCurrentShift[indexOrder].plannedTimeWork;
            int fullTimeForWork = lastTimeForMK + lastTimeForWK;

            string facticalTimeMakereadyStop = getOrders.GetTimeToMakereadyStop(ordersCurrentShift[indexOrder].id);
            string facticalTimeToWorkStop = getOrders.GetTimeToWorkStop(ordersCurrentShift[indexOrder].id);

            int currentLead;
            string timeStartOrder;

            if (_plannedWorkingOut)
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

            *//*string timeToEndMK = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), currentLastTimeForMakeready - lastTimeForMK);
            string timeToEndWork = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), currentLastTimeForFullWork - fullTimeForWork);*/


        /*
        statusStr
        currentLastTimeForMakereadyCaption
        currentLastTimeForMakeready
        currentLastTimeForFullWorkCaption
        currentLastTimeForFullWork
        timeToEndMKCaption
        timeToEndMK
        timeToEndWorkCaption
        timeToEndWork
        planedCoutOrderCaption
        planedCoutOrder
        workTimeDifferentCaption
        workTimeDifferent
        mkTimeDifferent
        wkTimeDifferent
        message
        *//*
        if (status == "1" || status == "2")
        {
            statusStr = "приладка заказа";
            orderStatus.statusStr = "приладка заказа";

            if (currentLastTimeForMakeready < 0)
            {
                caption[0] = "Отставание: ";
                strings[0] = timeOperations.MinuteToTimeString(currentLastTimeForMakeready * (-1));

                orderStatus.currentLastTimeForMakereadyCaption = "Отставание: ";
                orderStatus.currentLastTimeForMakeready = timeOperations.MinuteToTimeString(currentLastTimeForMakeready * (-1));
                //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
            }
            else
            {
                caption[0] = "Остаток времени на приладку: ";
                strings[0] = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);

                orderStatus.currentLastTimeForMakereadyCaption = "Остаток времени на приладку: ";
                orderStatus.currentLastTimeForMakeready = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);
                //message = "Остаток времени на приладку: " + mkTimeDiff;
            }

            caption[1] = "Остаток времени для выполнение заказа: ";
            strings[1] = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
            orderStatus.currentLastTimeForFullWorkCaption = "Остаток времени для выполнение заказа: ";
            orderStatus.currentLastTimeForFullWork = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
            //MessageBox.Show(timeDiff + " + " + mkLastTime + " = " + timeOperations.TimeAmount(timeDiff, mkLastTime));
            //strings[1] = timeOperations.MinuteToTimeString(timeDiff);

            caption[2] = "Планирумое время завершения приладки: ";
            strings[2] = timeToEndMK;
            orderStatus.timeToEndMKCaption = "Планирумое время завершения приладки: ";
            orderStatus.timeToEndMK = timeToEndMK;

            caption[3] = "Планирумое время завершения заказа: ";
            strings[3] = timeToEndWork;
            orderStatus.timeToEndWorkCaption = "Планирумое время завершения заказа: ";
            orderStatus.timeToEndWork = timeToEndWork;

            message = caption[0] + strings[0] + newLine;
            message += caption[1] + strings[1] + newLine;
            message += caption[2] + strings[2] + newLine;
            message += caption[3] + strings[3];

            orderStatus.message = orderStatus.currentLastTimeForMakereadyCaption + orderStatus.currentLastTimeForMakeready + newLine +
                orderStatus.currentLastTimeForFullWorkCaption + orderStatus.currentLastTimeForFullWork + newLine +
                orderStatus.timeToEndMKCaption + orderStatus.timeToEndMK + newLine +
                orderStatus.timeToEndWorkCaption + orderStatus.timeToEndWork;
        }

        if (status == "3")
        {
            statusStr = "заказ выполняется";
            orderStatus.statusStr = "заказ выполняется";

            if (currentLastTimeForFullWork < 0)
            {
                caption[0] = "Отставание: ";
                strings[0] = timeOperations.MinuteToTimeString(currentLastTimeForFullWork * (-1));
                //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                orderStatus.currentLastTimeForFullWorkCaption = "Отставание: ";
                orderStatus.currentLastTimeForFullWork = timeOperations.MinuteToTimeString(currentLastTimeForFullWork * (-1));
            }
            else
            {
                caption[0] = "Остаток времени: ";
                strings[0] = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
                //message = "Остаток времени на приладку: " + mkTimeDiff;
                orderStatus.currentLastTimeForFullWorkCaption = "Остаток времени: ";
                orderStatus.currentLastTimeForFullWork = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
            }

            caption[1] = "Плановая выработка: ";
            strings[1] = planedCoutOrder.ToString("N0");
            orderStatus.planedCoutOrderCaption = "Плановая выработка: ";
            orderStatus.planedCoutOrder = planedCoutOrder.ToString("N0");

            caption[2] = "Планирумое время завершения: ";
            strings[2] = timeToEndWork;
            orderStatus.timeToEndWorkCaption = "Планирумое время завершения: ";
            orderStatus.timeToEndWork = timeToEndWork;

            message = caption[0] + strings[0] + newLine;
            message += caption[1] + strings[1] + newLine;
            message += caption[2] + strings[2];

            orderStatus.message = orderStatus.currentLastTimeForFullWorkCaption + orderStatus.currentLastTimeForFullWork + newLine +
                orderStatus.planedCoutOrderCaption + orderStatus.planedCoutOrder + newLine +
                orderStatus.timeToEndWorkCaption + orderStatus.timeToEndWork;
        }

        if (status == "4")
        {
            statusStr = "заказ завершен";
            orderStatus.statusStr = "заказ завершен";

            if (workTimeDifferent < 0)
            {
                caption[0] = "Отставание: ";
                //strings[0] = woTimeDiff.Substring(1, woTimeDiff.Length - 1);
                strings[0] = timeOperations.MinuteToTimeString(workTimeDifferent * (-1));
                orderStatus.workTimeDifferentCaption = "Отставание: ";
                orderStatus.workTimeDifferent = timeOperations.MinuteToTimeString(workTimeDifferent * (-1));
            }
            else
            {
                caption[0] = "Опережение: ";
                strings[0] = timeOperations.MinuteToTimeString(workTimeDifferent);
                orderStatus.workTimeDifferentCaption = "Опережение: ";
                orderStatus.workTimeDifferent = timeOperations.MinuteToTimeString(workTimeDifferent);
            }

            message = caption[0] + strings[0];

            orderStatus.message = orderStatus.workTimeDifferentCaption + orderStatus.workTimeDifferent;
        }

        return (statusStr, message, caption, strings);
    }*/

        /*
         
         * 
         * 
         * private (string, string, string[], string[]) GetWorkingOutTimeForSelectedOrder(int indexOrder)
        {
            bool _plannedWorkingOut = true;

            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase valueOrders = new ValueOrdersBase();
            GetNumberShiftFromTimeStart startShift = new GetNumberShiftFromTimeStart();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();

            OrderStatusStrings orderStatus = new OrderStatusStrings("", "", "", "", "", "", "", "", "", "", "", "", "");

            string newLine = Environment.NewLine;

            string message = "";
            string statusStr = "";

            string[] caption = { "", "", "", "" };
            string[] strings = { "", "", "", "" };

            string machine = ordersCurrentShift[indexOrder].machineOfOrder;
            string status = valueOrders.GetOrderStatus(getOrders.GetOrderID(ordersCurrentShift[indexOrder].id));

            string shiftStart = Info.startOfShift; //get from Info or user base

            if (_plannedWorkingOut)
            {
                shiftStart = startShift.PlanedStartShift(Info.startOfShift); //get from method
            }

            int workTime = timeOperations.DateDifferenceToMinutes(DateTime.Now.ToString(), shiftStart); //общее время с начала смены
            int countPreviusWorkingOut = CountWorkingOutFromPreviusOrders(indexOrder, machine);// считать до указанного индекса
            int countPreviusOutages = CountPreviusOutages(); // еще проработка требуется
            int countWorkingOut = countPreviusWorkingOut + countPreviusOutages;

            int lastTimeForMK = ordersCurrentShift[indexOrder].plannedTimeMakeready;
            int lastTimeForWK = ordersCurrentShift[indexOrder].plannedTimeWork;
            int fullTimeForWork = lastTimeForMK + lastTimeForWK;

            int currentLead;
            string timeStartOrder;

            if (_plannedWorkingOut)
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
            int workTimeDifferent = timeOperations.MinuteDifference(ordersCurrentShift[indexOrder].workingOut, currentLead, false); //отклонение выработки от фактического времени выполнения заказа
            *//*int mkTimeDifferent = currentLead - timeForWork - lastTimeForMK;
            int wkTimeDifferent = timeForWork - lastTimeForWK;*//*

            string timeToEndMK = timeOperations.DateTimeAmountMunutes(timeStartOrder, lastTimeForMK);
            string timeToEndWork = timeOperations.DateTimeAmountMunutes(timeStartOrder, fullTimeForWork);

            *//*Console.WriteLine("<<<<<" + DateTime.Now.ToString() + ">>>>>");

            Console.WriteLine("Начало выполнения заказа: " + timeStartOrder);
            Console.WriteLine("Время выполнения заказа: " + timeOperations.MinuteToTimeString(currentLead));
            Console.WriteLine("Выработка предыдущих заказов: " + timeOperations.MinuteToTimeString(countPreviusWorkingOut));

            Console.WriteLine("Остаток времеи на приладку: " + timeOperations.MinuteToTimeString(currentLastTimeForMakeready));
            Console.WriteLine("Остаток времеи на выполнение заказа: " + timeOperations.MinuteToTimeString(currentLastTimeForFullWork));
            Console.WriteLine("Отклонение: " + timeOperations.MinuteToTimeString(workTimeDifferent));

            Console.WriteLine("Время завершения приладки: " + timeToEndMK);
            Console.WriteLine("Время завершения работы: " + timeToEndWork);*/

        /*Console.WriteLine("Отклонение времени приладки от нормы: " + mkTimeDifferent);
        Console.WriteLine("Отклонение времени работы от нормы: " + wkTimeDifferent);*/

        /*string timeToEndMK = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), currentLastTimeForMakeready - lastTimeForMK);
        string timeToEndWork = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), currentLastTimeForFullWork - fullTimeForWork);*//*

        int planedCoutOrder = timeForWork * ordersCurrentShift[indexOrder].norm / 60;
        *//*
        statusStr
        currentLastTimeForMakereadyCaption
        currentLastTimeForMakeready
        currentLastTimeForFullWorkCaption
        currentLastTimeForFullWork
        timeToEndMKCaption
        timeToEndMK
        timeToEndWorkCaption
        timeToEndWork
        planedCoutOrderCaption
        planedCoutOrder
        workTimeDifferentCaption
        workTimeDifferent
        *//*
        if (status == "1" || status == "2")
        {
            statusStr = "приладка заказа";
            orderStatus.statusStrVal = "приладка заказа";

            if (currentLastTimeForMakeready < 0)
            {
                caption[0] = "Отставание: ";
                strings[0] = timeOperations.MinuteToTimeString(currentLastTimeForMakeready * (-1));

                orderStatus.currentLastTimeForMakereadyCaptionVal = "Отставание: ";
                orderStatus.currentLastTimeForMakereadyVal = timeOperations.MinuteToTimeString(currentLastTimeForMakeready * (-1));
                //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
            }
            else
            {
                caption[0] = "Остаток времени на приладку: ";
                strings[0] = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);

                orderStatus.currentLastTimeForMakereadyCaptionVal = "Остаток времени на приладку: ";
                orderStatus.currentLastTimeForMakereadyVal = timeOperations.MinuteToTimeString(currentLastTimeForMakeready);
                //message = "Остаток времени на приладку: " + mkTimeDiff;
            }

            caption[1] = "Остаток времени для выполнение заказа: ";
            strings[1] = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
            orderStatus.currentLastTimeForFullWorkCaptionVal = "Остаток времени для выполнение заказа: ";
            orderStatus.currentLastTimeForFullWorkVal = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
            //MessageBox.Show(timeDiff + " + " + mkLastTime + " = " + timeOperations.TimeAmount(timeDiff, mkLastTime));
            //strings[1] = timeOperations.MinuteToTimeString(timeDiff);

            caption[2] = "Планирумое время завершения приладки: ";
            strings[2] = timeToEndMK;
            orderStatus.timeToEndMKCaptionVal = "Планирумое время завершения приладки: ";
            orderStatus.timeToEndMKVal = timeToEndMK;

            caption[3] = "Планирумое время завершения заказа: ";
            strings[3] = timeToEndWork;
            orderStatus.timeToEndWorkCaptionVal = "Планирумое время завершения заказа: ";
            orderStatus.timeToEndWorkVal = timeToEndWork;


            message = caption[0] + strings[0] + newLine;
            message += caption[1] + strings[1] + newLine;
            message += caption[2] + strings[2] + newLine;
            message += caption[3] + strings[3];


        }

        if (status == "3")
        {
            statusStr = "заказ выполняется";
            orderStatus.statusStrVal = "заказ выполняется";

            if (currentLastTimeForFullWork < 0)
            {
                caption[0] = "Отставание: ";
                strings[0] = timeOperations.MinuteToTimeString(currentLastTimeForFullWork * (-1));
                //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                orderStatus.currentLastTimeForFullWorkCaptionVal = "Отставание: ";
                orderStatus.currentLastTimeForFullWorkVal = timeOperations.MinuteToTimeString(currentLastTimeForFullWork * (-1));
            }
            else
            {
                caption[0] = "Остаток времени: ";
                strings[0] = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
                //message = "Остаток времени на приладку: " + mkTimeDiff;
                orderStatus.currentLastTimeForFullWorkCaptionVal = "Остаток времени: ";
                orderStatus.currentLastTimeForFullWorkVal = timeOperations.MinuteToTimeString(currentLastTimeForFullWork);
            }

            caption[1] = "Плановая выработка: ";
            strings[1] = planedCoutOrder.ToString("N0");
            orderStatus.planedCoutOrderCaptionVal = "Плановая выработка: ";
            orderStatus.planedCoutOrderVal = planedCoutOrder.ToString("N0");

            caption[2] = "Планирумое время завершения: ";
            strings[2] = timeToEndWork;
            orderStatus.timeToEndWorkCaptionVal = "Планирумое время завершения: ";
            orderStatus.timeToEndWorkVal = timeToEndWork;

            message = caption[0] + strings[0] + newLine;
            message += caption[1] + strings[1] + newLine;
            message += caption[2] + strings[2];
        }

        if (status == "4")
        {
            statusStr = "заказ завершен";
            orderStatus.statusStrVal = "заказ завершен";

            if (workTimeDifferent < 0)
            {
                caption[0] = "Отставание: ";
                //strings[0] = woTimeDiff.Substring(1, woTimeDiff.Length - 1);
                strings[0] = timeOperations.MinuteToTimeString(workTimeDifferent * (-1));
            }
            else
            {
                caption[0] = "Опережение: ";
                strings[0] = timeOperations.MinuteToTimeString(workTimeDifferent);
            }

            message = caption[0] + strings[0];
        }

        return (statusStr, message, caption, strings);
    }*/

        /*private int[] GetWorkingOutTime(int idx)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase valueOrders = new ValueOrdersBase();
            GetNumberShiftFromTimeStart startShift = new GetNumberShiftFromTimeStart();

            String status = valueOrders.GetOrderStatus(ordersCurrentShift[idx].machineOfOrder, ordersCurrentShift[idx].numberOfOrder, ordersCurrentShift[idx].modificationOfOrder);

            int fullLastTime = 0;
            int fullFactTime = 0;

            int mkLastTime = 0;
            int mkFactTime = 0;

            int workingOutTime = 0;

            int[] result = new int[4];

            //output
            int timeDiff = 0;
            int mkTimeDiff = 0;
            int woTimeDiff = 0;
            int plannedCountDone = 0;

            mkLastTime = ordersCurrentShift[idx].plannedTimeMakeready;
            mkFactTime = ordersCurrentShift[idx].facticalTimeMakeready;
            workingOutTime = ordersCurrentShift[idx].workingOut;

            fullLastTime = ordersCurrentShift[idx].plannedTimeMakeready + ordersCurrentShift[idx].plannedTimeWork;

            if (_wOutFromStartShift)
            {
                string shiftStart = startShift.PlanedStartShift(Info.startOfShift);

                int timeAfterStartShift = timeOperations.DateDifferenceToMinutes(DateTime.Now.ToString(), shiftStart);
                int wOut = timeOperations.MinuteDifference(fullTimeWorkingOut, mkLastTime, true);

                //string wOut = timeOperations.TotalMinutesToHoursAndMinutesStr(fullTimeWorkingOut);

                fullFactTime = timeOperations.MinuteDifference(timeAfterStartShift, wOut, true);

                //MessageBox.Show("Начало смены: " + shiftStart + ", Время после начала смены: " + timeAfterStartShift + ", Выработка без приладки: " + wOut + ", Полное время: " + fullFactTime);
            }
            else
            {
                fullFactTime = ordersCurrentShift[idx].facticalTimeMakeready + ordersCurrentShift[idx].facticalTimeWork;
            }

            timeDiff = timeOperations.MinuteDifference(fullFactTime, fullLastTime, false);
            mkTimeDiff = timeOperations.MinuteDifference(mkFactTime, mkLastTime, false);
            woTimeDiff = timeOperations.MinuteDifference(fullFactTime, workingOutTime, false);


            if (mkLastTime < fullFactTime)
            {
                *//*string fFactTime = timeOperations.TimeAmount(fullFactTime, mkLastTime);

                int diff = timeOperations.TimeDifferentToMinutes(fFactTime, mkLastTime);*//*

                int diff = timeOperations.MinuteDifference(fullFactTime, mkLastTime, true);

                plannedCountDone = (diff * ordersCurrentShift[idx].norm / 60);
            }
            else
            {
                plannedCountDone = 0;
            }

            result[0] = timeDiff;
            result[1] = mkTimeDiff;
            result[2] = woTimeDiff;
            result[3] = plannedCountDone;

            return result;
        }

        private (string, string, string[], string[]) GetWorkingOutMessage2(int idx)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase valueOrders = new ValueOrdersBase();

            string newLine = Environment.NewLine;

            string message = "";
            string statusStr = "";

            string[] caption = { "", "", "", "" };
            string[] strings = { "", "", "", "" };

            int[] values = GetWorkingOutTime(idx);

            int timeDiff = values[0];
            int mkTimeDiff = values[1];
            int woTimeDiff = values[2];
            int plannedCountDone = values[3];

            string status = valueOrders.GetOrderStatus(ordersCurrentShift[idx].machineOfOrder, ordersCurrentShift[idx].numberOfOrder, ordersCurrentShift[idx].modificationOfOrder);

            if (status == "1" || status == "2")
            {
                string plannedTimeDone = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), mkTimeDiff);
                string plannedTimeDoneOrder = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), timeDiff);
                int mkLastTime = ordersCurrentShift[idx].plannedTimeMakeready;

                statusStr = "приладка заказа";

                if (mkTimeDiff < 0)
                {
                    caption[0] = "Отставание: ";
                    strings[0] = timeOperations.MinuteToTimeString(mkTimeDiff * (-1));
                    //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                }
                else
                {
                    caption[0] = "Остаток времени на приладку: ";
                    strings[0] = timeOperations.MinuteToTimeString(mkTimeDiff);
                    //message = "Остаток времени на приладку: " + mkTimeDiff;
                }

                caption[1] = "Остаток времени для выполнение заказа: ";
                strings[1] = timeOperations.MinuteToTimeString(timeDiff + mkLastTime);
                //MessageBox.Show(timeDiff + " + " + mkLastTime + " = " + timeOperations.TimeAmount(timeDiff, mkLastTime));
                strings[1] = timeOperations.MinuteToTimeString(timeDiff);

                caption[2] = "Планирумое время завершения приладки: ";
                strings[2] = plannedTimeDone;

                caption[3] = "Планирумое время завершения заказа: ";
                strings[3] = timeOperations.DateTimeAmountMunutes(plannedTimeDoneOrder, mkLastTime);
                strings[3] = plannedTimeDoneOrder;

                message = caption[0] + strings[0] + newLine;
                message += caption[1] + strings[1] + newLine;
                message += caption[2] + strings[2] + newLine;
                message += caption[3] + strings[3];
            }

            if (status == "3")
            {
                string plannedTimeDone = timeOperations.DateTimeAmountMunutes(DateTime.Now.ToString(), timeDiff);

                statusStr = "заказ выполняется";

                if (timeDiff < 0)
                {
                    caption[0] = "Отставание: ";
                    strings[0] = timeOperations.MinuteToTimeString(timeDiff * (-1));
                    //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                }
                else
                {
                    caption[0] = "Остаток времени: ";
                    strings[0] = timeOperations.MinuteToTimeString(timeDiff);
                    //message = "Остаток времени на приладку: " + mkTimeDiff;
                }

                caption[1] = "Плановая выработка: ";
                strings[1] = plannedCountDone.ToString("N0");

                caption[2] = "Планирумое время завершения: ";
                strings[2] = plannedTimeDone;

                message = caption[0] + strings[0] + newLine;
                message += caption[1] + strings[1] + newLine;
                message += caption[2] + strings[2];
            }

            if (status == "4")
            {
                statusStr = "заказ завершен";

                if (woTimeDiff < 0)
                {
                    caption[0] = "Отставание: ";
                    //strings[0] = woTimeDiff.Substring(1, woTimeDiff.Length - 1);
                    strings[0] = timeOperations.MinuteToTimeString(woTimeDiff);
                }
                else
                {
                    caption[0] = "Опережение: ";
                    strings[0] = timeOperations.MinuteToTimeString(woTimeDiff);
                }

                message = caption[0] + strings[0];
            }

            *//*else
                {
                    statusStr = "";
                    message = "Оставшееся время: " + timeDiff;
                    message += Environment.NewLine;
                    message += "Сделать подсчёт времени завершения заказа, количества, которое должно быть сделано на текущий момент по норме";
                }*/

        /*message += Environment.NewLine;
        message += MousePosition.X + ", " + MousePosition.Y;
        message += Environment.NewLine;
        message += e.Item.Position.X + ", " + e.Item.Position.Y;*//*


        return (statusStr, message, caption, strings);
    }*/

        private void LoadCurrentOrderDetails(int idx)
        {
            ClearCurrentOrderDetails();

            if (idx != -1)
            {
                ValueSettingsBase valueSettings = new ValueSettingsBase();

                bool typeLoad;

                if (valueSettings.GetTypeLoadOrderDetails(Info.nameOfExecutor) == 0)
                {
                    typeLoad = true;
                }
                else
                {
                    typeLoad = false;
                }

                OrderStatusValue statusStrings = GetWorkingOutTimeForSelectedOrder(idx, typeLoad);

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

                OrderStatusValue statusStrings = GetWorkingOutTimeForSelectedOrder(idx, typeLoad);

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
                    wOut = fullTimeWorkingOut;
                }
                else
                {
                    wOut = GetWOutFromMachine(machine);
                }

                string status = ordersBase.GetOrderStatus(machine, ordersCurrentShift[idLastOrder].numberOfOrder, ordersCurrentShift[idLastOrder].modificationOfOrder);

                int norm = ordersCurrentShift[idLastOrder].norm;
                int amount = ordersCurrentShift[idLastOrder].amountOfOrder;
                int done = amount - ordersCurrentShift[idLastOrder].lastCount;// + ordersCurrentShift[idLastOrder].done;
                int mkTime = ordersCurrentShift[idLastOrder].plannedTimeMakeready;
                //MessageBox.Show(done.ToString());
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

        private int GetWOutFromMachine(string machine)
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
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {

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
    }
}
