using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
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
                DataBaseSelect();
            }
        }

        private void ShowUserForm()
        {
            Info.active = false;

            FormLoadUserForm form = new FormLoadUserForm(loadMode);
            //this.Visible = false;
            form.ShowDialog();

            LoadParametersFromBase("mainForm");

            LoadUser();

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

            DateTime lastDateV = DateTime.Now;
            DateTime currentDateV = DateTime.Now;

            string lastDateVersion = ini.GetLastDateVersion();
            string currentDateVersion = "";

            try
            {
                downloader.DownloadFile(link, path);
                //downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                //downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;

                chLog = File.ReadAllLines(path, Encoding.Unicode);
                currentDateVersion = chLog[0];

                if (currentDateVersion != "")
                    currentDateV = Convert.ToDateTime(currentDateVersion);

                

                if (lastDateVersion != "")
                {
                    lastDateV = Convert.ToDateTime(lastDateVersion);

                    if (currentDateV > lastDateV)
                    {
                        Process.Start("Updater.exe");
                    }
                }
                else
                {
                    Process.Start("Updater.exe");
                }

            }
            catch
            {
                MessageBox.Show("Ошибка подключения", "Ошибка", MessageBoxButtons.OK);
            }

            Invoke(new Action(() =>
            {
                //MessageBox.Show(currentDateV.ToString() + " " + lastDateV.ToString());

            }));
        }

        private void AddOrdersToListViewFromList()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
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
                String modification = "";
                if (ordersCurrentShift[index].modificationOfOrder != "")
                    modification = " (" + ordersCurrentShift[index].modificationOfOrder + ")";

                ListViewItem item = new ListViewItem();

                item.Name = ordersCurrentShift[index].numberOfOrder.ToString();
                item.Text = (index + 1).ToString();
                item.SubItems.Add(getInfo.GetMachineName(ordersCurrentShift[index].machineOfOrder.ToString()));
                item.SubItems.Add(ordersCurrentShift[index].numberOfOrder.ToString() + modification);
                item.SubItems.Add(ordersCurrentShift[index].nameOfOrder.ToString());
                item.SubItems.Add(ordersCurrentShift[index].amountOfOrder.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].lastCount.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].plannedTimeMakeready.ToString() + ", " + ordersCurrentShift[index].plannedTimeWork.ToString());
                item.SubItems.Add(ordersCurrentShift[index].facticalTimeMakeready.ToString() + ", " + ordersCurrentShift[index].facticalTimeWork.ToString());
                item.SubItems.Add(ordersCurrentShift[index].done.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].norm.ToString("N0"));
                item.SubItems.Add(timeOperations.TotalMinutesToHoursAndMinutesStr(ordersCurrentShift[index].workingOut));
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
                    comboBox1.SelectedIndex = selectedIndexActive;
                else
                    comboBox1.SelectedIndex = 0;
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
                    label19.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift.allTimeShift);

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

                ClearAll();
                EraseInfo();
                ShowUserForm();
            }

            Info.active = true;
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
            if (Form1.Info.startOfShift == "")
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

        private string[] GetWorkingOutTime(int idx)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase valueOrders = new ValueOrdersBase();

            String status = valueOrders.GetOrderStatus(ordersCurrentShift[idx].machineOfOrder, ordersCurrentShift[idx].numberOfOrder, ordersCurrentShift[idx].modificationOfOrder);

            String fullLastTime = "00:00";
            String fullFactTime = "00:00";

            String mkLastTime = "00:00";
            String mkFactTime = "00:00";

            String workingOutTime = "00:00";

            String[] result = new String[4];

            //output
            String timeDiff = "";
            String mkTimeDiff = "";
            String woTimeDiff = "";
            String plannedCountDone = "0";

            fullLastTime = timeOperations.TimeAmount(ordersCurrentShift[idx].plannedTimeMakeready, ordersCurrentShift[idx].plannedTimeWork);
            fullFactTime = timeOperations.TimeAmount(ordersCurrentShift[idx].facticalTimeMakeready, ordersCurrentShift[idx].facticalTimeWork);

            mkLastTime = ordersCurrentShift[idx].plannedTimeMakeready;
            mkFactTime = ordersCurrentShift[idx].facticalTimeMakeready;
            workingOutTime = timeOperations.TotalMinutesToHoursAndMinutesStr(ordersCurrentShift[idx].workingOut);

            if (timeOperations.TimeDifferent(fullLastTime, fullFactTime) == "00:00")
            {
                timeDiff = "-" + timeOperations.TimeDifferent(fullFactTime, fullLastTime);
            }
            else if (timeOperations.TimeDifferent(fullFactTime, fullLastTime) == "00:00")
            {
                timeDiff = timeOperations.TimeDifferent(fullLastTime, fullFactTime);
            }
            else
            {
                timeDiff = "00:00";
            }

            if (timeOperations.TimeDifferent(mkLastTime, mkFactTime) == "00:00")
            {
                mkTimeDiff = "-" + timeOperations.TimeDifferent(mkFactTime, mkLastTime);
            }
            else if (timeOperations.TimeDifferent(mkFactTime, mkLastTime) == "00:00")
            {
                mkTimeDiff = timeOperations.TimeDifferent(mkLastTime, mkFactTime);
            }
            else
            {
                mkTimeDiff = "00:00";
            }

            if (timeOperations.TimeDifferent(workingOutTime, fullFactTime) == "00:00")
            {
                woTimeDiff = "-" + timeOperations.TimeDifferent(fullFactTime, workingOutTime);
            }
            else if (timeOperations.TimeDifferent(fullFactTime, workingOutTime) == "00:00")
            {
                woTimeDiff = timeOperations.TimeDifferent(workingOutTime, fullFactTime);
            }
            else
            {
                woTimeDiff = "00:00";
            }

            if (timeOperations.TimeDifferent(mkLastTime, fullFactTime) == "00:00")
            {
                //String diff = timeOperations.TimeDifferent(fullLastTime, mkLastTime);
                int diff = timeOperations.TimeDifferentToMinutes(fullFactTime, mkLastTime);

                plannedCountDone = (diff * ordersCurrentShift[idx].norm / 60).ToString("N0");
            }
            else
            {
                plannedCountDone = "0";
            }

            result[0] = timeDiff;
            result[1] = mkTimeDiff;
            result[2] = woTimeDiff;
            result[3] = plannedCountDone;

            return result;
        }

        private (string, string, string[], string[]) GetWorkingOutMessage(int idx)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueOrdersBase valueOrders = new ValueOrdersBase();

            String newLine = Environment.NewLine;

            String message = "";
            String statusStr = "";

            String[] caption = { "", "", "", "" };
            String[] strings = { "", "", "", "" };

            String[] values = GetWorkingOutTime(idx);

            String timeDiff = values[0];
            String mkTimeDiff = values[1];
            String woTimeDiff = values[2];
            String plannedCountDone = values[3];

            String status = valueOrders.GetOrderStatus(ordersCurrentShift[idx].machineOfOrder, ordersCurrentShift[idx].numberOfOrder, ordersCurrentShift[idx].modificationOfOrder);

            if (status == "1" || status == "2")
            {
                String plannedTimeDone = timeOperations.DateAmount(DateTime.Now.ToString(), mkTimeDiff);
                String plannedTimeDoneOrder = timeOperations.DateAmount(DateTime.Now.ToString(), timeDiff);

                statusStr = "приладка заказа";

                if (mkTimeDiff.Substring(0, 1) == "-")
                {
                    caption[0] = "Отставание: ";
                    strings[0] = mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                    //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                }
                else
                {
                    caption[0] = "Остаток времени на приладку: ";
                    strings[0] = mkTimeDiff;
                    //message = "Остаток времени на приладку: " + mkTimeDiff;
                }

                caption[1] = "Остаток времени для выполнение заказа: ";
                strings[1] = timeDiff;

                caption[2] = "Планирумое время завершения приладки: ";
                strings[2] = plannedTimeDone;

                caption[3] = "Планирумое время завершения заказа: ";
                strings[3] = plannedTimeDoneOrder;

                message = caption[0] + strings[0] + newLine;
                message += caption[1] + strings[1] + newLine;
                message += caption[2] + strings[2] + newLine;
                message += caption[3] + strings[3];
            }

            if (status == "3")
            {
                String plannedTimeDone = timeOperations.DateAmount(DateTime.Now.ToString(), timeDiff);

                statusStr = "заказ выполняется";

                if (timeDiff.Substring(0, 1) == "-")
                {
                    caption[0] = "Отставание: ";
                    strings[0] = timeDiff.Substring(1, timeDiff.Length - 1);
                    //message = "Отставание: " + mkTimeDiff.Substring(1, mkTimeDiff.Length - 1);
                }
                else
                {
                    caption[0] = "Остаток времени: ";
                    strings[0] = timeDiff;
                    //message = "Остаток времени на приладку: " + mkTimeDiff;
                }

                caption[1] = "Плановая выработка: ";
                strings[1] = plannedCountDone;

                caption[2] = "Планирумое время завершения: ";
                strings[2] = plannedTimeDone;

                message = caption[0] + strings[0] + newLine;
                message += caption[1] + strings[1] + newLine;
                message += caption[2] + strings[2];
            }

            if (status == "4")
            {
                statusStr = "заказ завершен";

                if (woTimeDiff.Substring(0, 1) == "-")
                {
                    caption[0] = "Отставание: ";
                    strings[0] = woTimeDiff.Substring(1, woTimeDiff.Length - 1);
                }
                else
                {
                    caption[0] = "Опережение: ";
                    strings[0] = woTimeDiff;
                }

                message = caption[0] + strings[0];
            }

            /*else
                {
                    statusStr = "";
                    message = "Оставшееся время: " + timeDiff;
                    message += Environment.NewLine;
                    message += "Сделать подсчёт времени завершения заказа, количества, которое должно быть сделано на текущий момент по норме";
                }*/

            /*message += Environment.NewLine;
            message += MousePosition.X + ", " + MousePosition.Y;
            message += Environment.NewLine;
            message += e.Item.Position.X + ", " + e.Item.Position.Y;*/


            return (statusStr, message, caption, strings);
        }

        private void LoadCurrentOrderDetails(int idx)
        {
            ClearCurrentOrderDetails();

            if (idx != -1)
            {
                string statusStr = GetWorkingOutMessage(idx).Item1;
                string[] caption = GetWorkingOutMessage(idx).Item3;
                string[] strings = GetWorkingOutMessage(idx).Item4;

                label24.Text = ordersCurrentShift[idx].numberOfOrder + ": " + ordersCurrentShift[idx].nameOfOrder;
                label26.Text = statusStr;

                label25.Text = caption[0];
                label27.Text = strings[0];

                label28.Text = caption[1];
                label30.Text = strings[1];

                label29.Text = caption[2];
                label31.Text = strings[2];
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

            tooltp.AutomaticDelay = 1500;

            int idx = e.Item.Index;

            if (idx != -1 && e.Item != null)
            {
                string statusStr = GetWorkingOutMessage(idx).Item1;
                string message = GetWorkingOutMessage(idx).Item2;

                tooltp.Active = true;
                tooltp.ToolTipTitle = ordersCurrentShift[idx].numberOfOrder + ": " + ordersCurrentShift[idx].nameOfOrder + " - " + statusStr;
                tooltp.SetToolTip(listView1, message);
            }
            else
            {
                tooltp.Active = false;
            }

        }

        private void DataBaseSelect()
        {
            FormAddEditTestMySQL form = new FormAddEditTestMySQL();
            form.ShowDialog();
            LoadBaseConnectionParameters();
            LoadUser();
            if (Form1.Info.nameOfExecutor != "")
                LoadParametersForTheSelectedUserFromBase();
            LoadOrdersFromBase();
        }

        private void базаДанныхToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataBaseSelect();
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
            /*label34.Text = comboBox2.Text;
            label35.Text = comboBox3.Text;*/

            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            string[] captions = { "10:30", "11:00", "11:30", "12:00" };
            string[] values = { "", "", "", "" };

            string captionNotEnough = "Тиража не достаточно для выполнения минимальной нормы";
            string captionMakeReady = "Выполняется приладка";

            int wOut;
            int norm;
            int idLastOrder = GetIDLastOrderFromSelectedMachine(comboBox3.Text);
            string machine = infoBase.GetMachineFromName(comboBox3.Text);

            if (idLastOrder >= 0)
            {
                if (ordersBase.GetOrderStatus(machine, ordersCurrentShift[idLastOrder].numberOfOrder, ordersCurrentShift[idLastOrder].modificationOfOrder) == "3")
                {
                    norm = ordersCurrentShift[idLastOrder].norm;

                    if (comboBox2.SelectedIndex == 0)
                    {
                        wOut = fullTimeWorkingOut;
                    }
                    else
                    {
                        wOut = GetWOutFromMachine(machine);
                    }

                    for (int i = 0; i < captions.Length; i++)
                    {
                        int targetTime = timeOperations.totallTimeHHMMToMinutes(captions[i]);

                        if (targetTime > wOut)
                        {
                            int targetCount = (targetTime - wOut) * norm / 60;

                            if (targetCount <= ordersCurrentShift[idLastOrder].amountOfOrder * 1.1)
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
                    int mkTime = timeOperations.totallTimeHHMMToMinutes(ordersCurrentShift[i].plannedTimeMakeready);
                    //int Time = timeOperations.totallTimeHHMMToMinutes(ordersCurrentShift[i].plannedTimeWork);

                    int wTime = ordersCurrentShift[i].done * 60 / ordersCurrentShift[i].norm;

                    wOut += mkTime + wTime;
                }
                    
            }

            return wOut;
        }
    }
}
