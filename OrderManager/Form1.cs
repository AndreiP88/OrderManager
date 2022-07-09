using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class Form1 : Form
    {
        bool adminMode = false;

        String dataBase;

        public Form1(string[] args)
        {
            InitializeComponent();

            INISettings ini = new INISettings();

            if (args.Length > 0)
            {
                if (args[0] == "adminMode")
                    adminMode = true;
            }

            dataBase = ini.DataBasePath();
            toolStripStatusLabel1.Text = "База данных: " + dataBase.Replace(@"\\", @"\");
        }

        List<Order> ordersCurrentShift;

        int fullTimeWorkingOut;
        int fullDone;

        public static class Info
        {
            public static bool active = false;
            public static int indexItem = -1;
            public static String nameOfExecutor = "";
            public static String startOfShift = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Info.active = false;
            FormAddCloseOrder form = new FormAddCloseOrder(dataBase, Info.startOfShift, Info.nameOfExecutor);
            form.ShowDialog();
            LoadOrdersFromBase();
            Info.active = true;
        }

        private void ShowUserForm()
        {
            Info.active = false;

            FormLoadUserForm form = new FormLoadUserForm(dataBase);
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
            FormSelectMachine form = new FormSelectMachine(dataBase);
            form.ShowDialog();

            //LoadUser();
            LoadOrdersFromBase();

            Info.active = true;
        }

        private void ShowFullOrdersForm()
        {
            FormFullListOrders form = new FormFullListOrders(dataBase, false, "", "", "");
            form.ShowDialog();
        }

        private void ShowAllOrdersForm()
        {
            FormAllOrders form = new FormAllOrders(dataBase);
            form.ShowDialog();
        }

        private void ShowShiftsForm()
        {
            Info.active = false;
            FormShiftsDetails form = new FormShiftsDetails(adminMode, dataBase, Form1.Info.nameOfExecutor, 0, 0);
            form.ShowDialog();

            //LoadUser();
            LoadOrdersFromBase();
            Info.active = true;
        }

        private void ShowSetUserForm()
        {
            FormUserProfile form = new FormUserProfile(dataBase, Form1.Info.nameOfExecutor);
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
            SetUpdateSettingsValue setting = new SetUpdateSettingsValue(dataBase);

            if (Form1.Info.nameOfExecutor != "")
                setting.UpdateParameterLine(Form1.Info.nameOfExecutor, nameForm, GetParametersLine());
            else
                setting.UpdateParameterLine("0", nameForm, GetParametersLine());
        }

        private void LoadParametersFromBase(String nameForm)
        {
            GetValueFromSettingsBase getSettings = new GetValueFromSettingsBase(dataBase);

            if (Form1.Info.nameOfExecutor != "")
                ApplyParameterLine(getSettings.GetParameterLine(Form1.Info.nameOfExecutor, nameForm));
            else
                ApplyParameterLine(getSettings.GetParameterLine("0", nameForm));
        }

        private void LoadParametersForTheSelectedUserFromBase()
        {
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            Info.startOfShift = getUser.GetCurrentShiftStart(Info.nameOfExecutor);

            this.Text = "Менеджер заказов - " + getUser.GetNameUser(Info.nameOfExecutor);
        }

        private void AddOrdersToListViewFromList()
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase(dataBase);

            ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(Form1.Info.startOfShift, "");

            fullTimeWorkingOut = 0;
            fullDone = 0;

            if (listView1.SelectedItems.Count > 0)
            {
                Info.indexItem = listView1.SelectedIndices[0];
            }

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

            if (listView1.Items.Count > 0)
            {
                if (Info.indexItem >= 0)
                    listView1.Items[Info.indexItem].Selected = true;
                else
                    listView1.Items[listView1.Items.Count - 1].Selected = true;
            }
            //listView1.Items[0].Selected = false;
        }

        private void LoadDetailsMount(CancellationToken token)
        {
            GetShiftsFromBase getShifts = new GetShiftsFromBase(dataBase, Form1.Info.nameOfExecutor);
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

            DateTime date;
            if (Form1.Info.startOfShift != "")
                date = Convert.ToDateTime(Form1.Info.startOfShift);
            else
                date = DateTime.Now;

            while (!token.IsCancellationRequested)
            {
                List<ShiftsDetails> currentShift = (List<ShiftsDetails>)getShifts.LoadShiftsFromBase(date, "").Item2;

                Invoke(new Action(() =>
                {
                    label18.Text = currentShift[currentShift.Count - 1].countShifts.ToString("N0");
                    label19.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift[currentShift.Count - 1].allTimeShift);

                    label20.Text = currentShift[currentShift.Count - 1].countOrdersShift.ToString() + "/" + currentShift[currentShift.Count - 1].countMakereadyShift.ToString();
                    label21.Text = currentShift[currentShift.Count - 1].amountAllOrdersShift.ToString("N0");

                    label22.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift[currentShift.Count - 1].allTimeWorkingOutShift);
                    label23.Text = (currentShift[currentShift.Count - 1].percentWorkingOutShift).ToString("N1") + "%";

                }));

                break;
            }
        }

        private void LaodDetailsForCurrentMount()
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            Task task = new Task(() => LoadDetailsMount(token));

            task.Start();
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
            SaveParameterToBase("mainForm");

            listView1.Items.Clear();
            listView2.Items.Clear();

            Info.indexItem = -1;

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
        }

        private void ViewDetailsForUser()
        {
            GetDateTimeOperations dtOperations = new GetDateTimeOperations();
            GetValueFromUserBase usersBase = new GetValueFromUserBase(dataBase);
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            GetValueFromInfoBase getUserMachines = new GetValueFromInfoBase(dataBase);

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
            GetValueFromInfoBase getMachine = new GetValueFromInfoBase(dataBase);
            GetValueFromUserBase userBase = new GetValueFromUserBase(dataBase);

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
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            ValueOrdersBase getOrder = new ValueOrdersBase(dataBase);

            GetValueFromUserBase userBase = new GetValueFromUserBase(dataBase);

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
            FormDetailsStatistic form = new FormDetailsStatistic(false, dataBase);
            form.ShowDialog();
        }

        private void CancelShift()
        {
            DialogResult result;
            result = MessageBox.Show("Вы действительно хотите завершить смену?", "Завершение смены", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Info.active = false;

                SetUpdateUsersBase userBase = new SetUpdateUsersBase(dataBase);
                SetUpdateInfoBase infoBase = new SetUpdateInfoBase(dataBase);
                GetValueFromShiftsBase getShift = new GetValueFromShiftsBase(dataBase);

                getShift.CloseShift(Info.startOfShift, DateTime.Now.ToString());
                infoBase.CompleteTheShift(Info.nameOfExecutor);
                userBase.UpdateCurrentShiftStart(Info.nameOfExecutor, "");

                ClearAll();
                EraseInfo();
                ShowUserForm();

                Info.active = true;
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
            CancelShift();
        }

        private void Form1_Shown(object sender, EventArgs e)
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
                FormAdmin form = new FormAdmin(adminMode, dataBase);
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
            LoadParametersFromBase("mainForm");

            //LoadUser();
            //LoadParametersForTheSelectedUserFromBase(Form1.Info.mashine);
            //LoadOrdersFromBase();
            //ViewDetailsForUser();
            //button1.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveParameterToBase("mainForm");
        }

        private void labelTime_Click(object sender, EventArgs e)
        {
            if (Form1.Info.nameOfExecutor == "1")
            {
                FormAdmin form = new FormAdmin(adminMode, dataBase);
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
                GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

                Info.active = false;
                FormAddCloseOrder form;

                if (listView1.SelectedIndices[0] == listView1.Items.Count - 1 && Convert.ToBoolean(getInfo.GetActiveOrder(ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder)))
                {
                    form = new FormAddCloseOrder(dataBase, Info.startOfShift, Info.nameOfExecutor);
                }
                else
                {
                    form = new FormAddCloseOrder(dataBase, Info.startOfShift,
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
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            Info.active = false;
            FormPrivateNote form;

            form = new FormPrivateNote(dataBase, Info.startOfShift,
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

    }
}
