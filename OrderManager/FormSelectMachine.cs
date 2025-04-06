using libData;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.DataBaseReconnect;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace OrderManager
{
    public partial class FormSelectMachine : Form
    {
        public FormSelectMachine()
        {
            InitializeComponent();
        }

        List<CheckBox> checkBoxesMachines = new List<CheckBox>();

        CancellationTokenSource cancelTokenSource;

        bool _isNewShift = false;
        private void ChangeCaptionButton()
        {
            int checkedCount = 0;

            for (int i = 0; i < checkBoxesMachines.Count; i++)
            {
                if (checkBoxesMachines[i].Checked)
                    checkedCount++;
            }

            if (checkedCount > 0)
            {
                if (Form1.Info.shiftIndex == -1)
                {
                    button1.Text = "Выбрать и начать смену";
                    button1.Enabled = true;
                    _isNewShift = true;

                    dateTimePicker1.Visible = true;
                    comboBox1.Visible = true;
                }
                else
                {
                    button1.Text = "Применить";
                    button1.Enabled = true;
                    _isNewShift = false;

                    dateTimePicker1.Visible = false;
                    comboBox1.Visible = false;
                }
            }
            else
            {
                button1.Text = "Выберите оборудование";
                button1.Enabled = false;
            }
        }

        private bool CheckUserToSelectedMachine(String machine, String user)
        {
            ValueInfoBase getUserID = new ValueInfoBase();
            if (getUserID.GetIDUser(machine) == user)
                return true;
            else
                return false;
        }

        private bool CheckIsActyveSelectedMachine(string machine)
        {
            ValueInfoBase getUserID = new ValueInfoBase();
            if (getUserID.GetActiveOrder(machine))
                return true;
            else
                return false;
        }

        private bool CheckFreeMachine(String machine)
        {
            ValueInfoBase getUserID = new ValueInfoBase();

            bool result;

            if (getUserID.GetIDUser(machine) == "")
                result = true;
            else
                result = false;

            return (result);
        }

        private async Task<bool> CheckCategoryForUser(String user, String machine)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueUserBase getUser = new ValueUserBase();

            string categoryCurrentMachine = await getInfo.GetCategoryMachine(machine);
            string categoryesCurrentUser = getUser.GetCategoryesMachine(user);

            string[] categoryes = categoryesCurrentUser.Split(';');

            return categoryes.Contains(categoryCurrentMachine);
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
                            checkBoxesMachines.Clear();

                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                ValueOrdersBase order = new ValueOrdersBase();
                                ValueInfoBase getInfo = new ValueInfoBase();
                                ValueUserBase user = new ValueUserBase();

                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT DISTINCT id FROM machines"
                                };
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync())
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    string machine = sqlReader["id"].ToString();

                                    int currentOrderID = Convert.ToInt32(getInfo.GetCurrentOrderID(machine));
                                    string orderName = order.GetOrderNumber(currentOrderID);

                                    if (orderName != "-1")
                                        orderName += ", " + order.GetOrderName(currentOrderID);
                                    else
                                        orderName = "";

                                    if (await CheckCategoryForUser(Form1.Info.nameOfExecutor, machine) == true)
                                    {
                                        if (token.IsCancellationRequested)
                                        {
                                            break;
                                        }

                                        if (CheckUserToSelectedMachine(machine, Form1.Info.nameOfExecutor) == true)
                                        {
                                            if (token.IsCancellationRequested)
                                            {
                                                break;
                                            }

                                            await AddControl(machine, user.GetNameUser(getInfo.GetIDUser(machine)), orderName, true, true);
                                        }
                                        else if (CheckFreeMachine(machine) == true || CheckUserToSelectedMachine(machine, Form1.Info.nameOfExecutor) == true)
                                        {
                                            if (token.IsCancellationRequested)
                                            {
                                                break;
                                            }

                                            await AddControl(machine, user.GetNameUser(getInfo.GetIDUser(machine)), orderName, true, false);
                                        }
                                        else
                                        {
                                            if (token.IsCancellationRequested)
                                            {
                                                break;
                                            }

                                            await AddControl(machine, user.GetNameUser(getInfo.GetIDUser(machine)), orderName, false, false);
                                        }
                                    }
                                }

                                if (token.IsCancellationRequested)
                                {
                                    break;
                                }

                                await Connect.CloseAsync();
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
            }, token);
        }

        private async void FolrmSelectMachine_Load(object sender, EventArgs e)
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();

            dateTimePicker1.Visible = false;
            comboBox1.Visible = false;

            await LoadMachine(cancelTokenSource.Token);
            timer1.Enabled = true;
            ChangeCaptionButton();

            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker1.Checked = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cancelTokenSource?.Cancel();

            if (Form1.Info.shiftIndex != -1)
            {
                ActivateDeactivateMachines(Form1.Info.nameOfExecutor);
            }
            else
            {
                ShiftStart(Form1.Info.nameOfExecutor, comboBox1.SelectedIndex + 1, dateTimePicker1.Value.ToString("dd.MM.yyyy HH:mm:ss"));
                ActivateDeactivateMachines(Form1.Info.nameOfExecutor);
            }

            /*
            if (Form1.Info.startOfShift != -1)
                curretnShiftID = Form1.Info.startOfShift;
            else
                curretnShiftID = DateTime.Now.ToString();

            if (Form1.Info.startOfShift == -1)
            {
                ShiftStart(Form1.Info.nameOfExecutor, startCurrentShift);
                ActivateDeactivateMachines(Form1.Info.nameOfExecutor);
            }
            else
            {
                ActivateDeactivateMachines(Form1.Info.nameOfExecutor);
            }*/
        }

        private void ShiftStart(string currentUser, int shiftNumber, string startOfShift)
        {
            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        ValueUserBase usersBase = new ValueUserBase();
                        ValueShiftsBase shiftsBase = new ValueShiftsBase();

                        string overtimeShift = IsOvertimeShift(currentUser, startOfShift).ToString();

                        using (MySqlConnection Connect = DBConnection.GetDBConnection())
                        {
                            /*string commandText = "INSERT INTO shifts (nameUser, shiftNumber, startShift, overtimeShift) " +
                                "SELECT * FROM (SELECT @nameUser, @shiftNumber, @startShift, @overtimeShift) " +
                                "AS tmp WHERE NOT EXISTS(SELECT startShift, nameUser FROM shifts WHERE startShift = @startShift AND nameUser = @nameUser) LIMIT 1";*/

                            string commandText = "INSERT INTO shifts (nameUser, shiftNumber, startShift, overtimeShift) " +
                                "SELECT @nameUser, @shiftNumber, @startShift, @overtimeShift " +
                                "WHERE NOT EXISTS (SELECT nameUser, shiftNumber, startShift FROM shifts WHERE nameUser = @nameUser AND shiftNumber = @shiftNumber AND startShift = @startShift) LIMIT 1";

                            MySqlCommand Command = new MySqlCommand(commandText, Connect);
                            Command.Parameters.AddWithValue("@nameUser", currentUser);
                            Command.Parameters.AddWithValue("@shiftNumber", shiftNumber);
                            Command.Parameters.AddWithValue("@startShift", startOfShift);
                            Command.Parameters.AddWithValue("@overtimeShift", overtimeShift);

                            Connect.Open();
                            Command.ExecuteNonQuery();
                            Connect.Close();
                        }

                        int shiftID = shiftsBase.GetIDFromStartShift(startOfShift);

                        usersBase.UpdateCurrentShiftStart(currentUser, shiftID.ToString());

                        Form1.Info.shiftIndex = shiftID;

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

        private bool IsOvertimeShift(string user, string startOfShift)
        {
            bool result = false;

            ValueUserBase userBase = new ValueUserBase();
            GetNumberShiftFromTimeStart getNumberShift = new GetNumberShiftFromTimeStart();

            int userID = Convert.ToInt32(user);

            ShiftShedule shiftShedule = userBase.GetUserShiftShedule(userID);

            string currentShiftFromShedule = shiftShedule.GetCurrentShiftFromShedule(startOfShift);
            string currentShiftFromTimeStart = getNumberShift.NumberShift(startOfShift);

            if (currentShiftFromTimeStart != "" && currentShiftFromTimeStart != currentShiftFromShedule)
            {
                DialogResult dialog = MessageBox.Show("Смена, которую Вы открываете, не соответствует Вашему графику.\n" +
                    "Отметить смену, как сверхурочную?", "Внимание", MessageBoxButtons.YesNo);

                if (dialog == DialogResult.Yes)
                {
                    result = true;
                }
            }

            return result;
        }

        private void ActivateDeactivateMachines(String currentUser)
        {
            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        for (int i = 0; i < checkBoxesMachines.Count; i++)
                        {
                            if (CheckIsActyveSelectedMachine(checkBoxesMachines[i].Name) == true &&
                                CheckUserToSelectedMachine(checkBoxesMachines[i].Name, currentUser) == true &&
                                checkBoxesMachines[i].Checked == false)
                            {
                                MessageBox.Show("Прежде чем убрать какое либо оборудование из активного, завершите текущий заказ.", "Изменение активного оборудования", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            else
                            {
                                if (CheckUserToSelectedMachine(checkBoxesMachines[i].Name, currentUser) == true || CheckFreeMachine(checkBoxesMachines[i].Name) == true)
                                {
                                    String user;

                                    if (checkBoxesMachines[i].Checked == true)
                                        user = currentUser;
                                    else
                                        user = "";

                                    using (MySqlConnection Connect = DBConnection.GetDBConnection())
                                    {
                                        string commandText = "UPDATE machinesInfo SET nameOfExecutor = @currentUser " +
                                            "WHERE (machine = @machine)";

                                        MySqlCommand Command = new MySqlCommand(commandText, Connect);
                                        Command.Parameters.AddWithValue("@machine", checkBoxesMachines[i].Name);
                                        Command.Parameters.AddWithValue("@currentUser", user);

                                        Connect.Open();
                                        Command.ExecuteNonQuery();
                                        Connect.Close();
                                    }
                                }

                                Close();
                            }
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
        }

        public void DrawLinePoint(Point p1, Point p2)
        {
            Graphics g = this.CreateGraphics();
            {
                Pen pen = new Pen(Color.Black, 1);
                g.DrawLine(pen, p1, p2);
            }
        }


        private async Task AddControl(String name, String user, String order, bool enabled, bool check)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            int x = 12;
            int y = 35;
            int h = 20;

            if (checkBoxesMachines.Count == 0)
            {
                Label labelCaption = new Label();
                labelCaption.Name = "labelCaption";
                labelCaption.AutoSize = true;
                labelCaption.Font = new Font("Consolas", 9);
                labelCaption.Text += String.Format("{0,-25}{1,-25:f4}{2,-50:f4}", "Оборудование:", "Оператор:", "Активный заказ:");
                labelCaption.Location = new Point(28, 13);
                labelCaption.TextAlign = ContentAlignment.MiddleLeft;
                //labelCaption.Font = new Font(labelCaption.Font, FontStyle.Underline);
                labelCaption.Visible = true;

                Invoke(new Action(() =>
                {
                    Controls.Add(labelCaption);
                }));
            }

            checkBoxesMachines.Add(new CheckBox());

            checkBoxesMachines[checkBoxesMachines.Count - 1].Name = name;
            checkBoxesMachines[checkBoxesMachines.Count - 1].Enabled = enabled;
            checkBoxesMachines[checkBoxesMachines.Count - 1].Checked = check;
            checkBoxesMachines[checkBoxesMachines.Count - 1].Location = new Point(x, y + h * (checkBoxesMachines.Count - 1));

            checkBoxesMachines[checkBoxesMachines.Count - 1].Text += String.Format("{0,-25}{1,-25:f4}{2,-50:f4}", await getInfo.GetMachineName(name), user, order);
            checkBoxesMachines[checkBoxesMachines.Count - 1].AutoSize = true;
            checkBoxesMachines[checkBoxesMachines.Count - 1].Font = new Font("Consolas", 9);
            checkBoxesMachines[checkBoxesMachines.Count - 1].Font = new Font(checkBoxesMachines[checkBoxesMachines.Count - 1].Font, FontStyle.Underline);

            Invoke(new Action(() =>
            {
                Controls.Add(checkBoxesMachines[checkBoxesMachines.Count - 1]);
            }));
            
            checkBoxesMachines[checkBoxesMachines.Count - 1].CheckedChanged += new EventHandler(checkBoxMachine_CheckedChanged);

            /*
            Label labelUser = new Label();
            labelUser.Name = "labelUser";
            labelUser.Text = user;
            labelUser.Location = new Point(x + wMachine, y + h * (checkBoxesMachines.Count - 1));
            labelUser.Width = wUser;
            labelUser.TextAlign = ContentAlignment.MiddleLeft;
            //labelUser.Font = new Font(labelUser.Font, FontStyle.Underline);
            labelUser.Visible = true;
            Controls.Add(labelUser);

            Label labelOrder = new Label();
            labelOrder.Name = "labelOrder";
            labelOrder.Text = order;
            labelOrder.Location = new Point(x + wMachine + wUser, y + h * (checkBoxesMachines.Count - 1));
            labelOrder.Width = wOrder;
            labelOrder.TextAlign = ContentAlignment.MiddleLeft;
            //labelOrder.Font = new Font(labelOrder.Font, FontStyle.Underline);
            labelOrder.Visible = true;
            Controls.Add(labelOrder);
            */
        }

        private async Task EditControl(string machine, string user, string order, bool enabled, bool check)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            int index = checkBoxesMachines.FindLastIndex((v) => v.Name == machine);

            checkBoxesMachines[index].Enabled = enabled;
            checkBoxesMachines[index].Checked = check;

            checkBoxesMachines[index].Text = String.Format("{0,-25}{1,-25:f4}{2,-50:f4}", await getInfo.GetMachineName(machine), user, order);
        }

        private async void UpdateMachinesStatus()
        {
            ValueOrdersBase order = new ValueOrdersBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueUserBase user = new ValueUserBase();

            for (int i = 0; i < checkBoxesMachines.Count; i++)
            {
                string machine = checkBoxesMachines[i].Name;

                int currentOrderIndex = Convert.ToInt32(getInfo.GetCurrentOrderID(machine));
                string orderName = order.GetOrderNumber(currentOrderIndex);

                if (orderName != "-1")
                    orderName += ", " + order.GetOrderName(currentOrderIndex);
                else
                    orderName = "";

                if (CheckFreeMachine(machine) == false)
                {
                    if (CheckUserToSelectedMachine(machine, Form1.Info.nameOfExecutor) == false)
                    {
                        await EditControl(machine, user.GetNameUser(getInfo.GetIDUser(machine)), orderName, false, false);
                    }
                }
                else
                {
                    bool check = checkBoxesMachines[i].Checked;

                    await EditControl(machine, user.GetNameUser(getInfo.GetIDUser(machine)), orderName, true, check);
                }

                if (await CheckCategoryForUser(Form1.Info.nameOfExecutor, machine) == true)
                {


                    /*if (CheckUserToSelectedMachine(machine, Form1.Info.nameOfExecutor) == true)
                    {
                        EditControl(i, user.GetNameUser(getInfo.GetIDUser(machine)), orderName, true, true);
                    }
                    else if (CheckFreeMachine(machine) == true || CheckUserToSelectedMachine(machine, Form1.Info.nameOfExecutor) == true)
                    {
                        EditControl(i, user.GetNameUser(getInfo.GetIDUser(machine)), orderName, true, false);
                    }
                    else
                    {
                        EditControl(i, user.GetNameUser(getInfo.GetIDUser(machine)), orderName, false, false);
                    }*/
                }
            }
        }

        private void SelectShiftNumber(string selectedDateTime)
        {
            GetNumberShiftFromTimeStart numberShiftFromTimeStart = new GetNumberShiftFromTimeStart();

            int shiftNumber = numberShiftFromTimeStart.NumberShiftNum(selectedDateTime);

            if (shiftNumber > 0)
            {
                comboBox1.SelectedIndex = shiftNumber - 1;
            }
        }

        void checkBoxMachine_CheckedChanged(object sender, EventArgs e)
        {
            //CheckBox c1 = sender as CheckBox;

            ChangeCaptionButton();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cancelTokenSource?.Cancel();

            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateMachinesStatus();

            if (!dateTimePicker1.Checked)
            {
                dateTimePicker1.Value = DateTime.Now;

                dateTimePicker1.Checked = false;
            }
        }

        private void FormSelectMachine_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
            cancelTokenSource?.Cancel();
            Thread.Sleep(100);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            SelectShiftNumber(dateTimePicker1.Value.ToString());
        }
    }
}
