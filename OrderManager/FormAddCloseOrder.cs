using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAddCloseOrder : Form
    {
        bool aMode;
        bool adminCloseOrder;
        String dataBase;
        String startOfShift;
        String nameOfExecutor;
        String loadOrderNumber;
        String loadOrderModification;
        String loadMachine;
        String loadCounterRepeat;

        public class TransparentPanel : Panel
        {
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                    return cp;
                }
            }
            protected override void OnPaintBackground(PaintEventArgs e)
            {
                //base.OnPaintBackground(e);
            }
        }

        /*public FormAddCloseOrder(String dBase, String lStartOfShift)
        {
            InitializeComponent();

            GetValueFromUserBase getUser = new GetValueFromUserBase(dBase);

            this.aMode = false;
            this.dataBase = dBase;
            this.startOfShift = lStartOfShift;
            this.nameOfExecutor = getUser.GetCurrentUserIDFromShiftStart(lStartOfShift);
            this.loadOrderNumber = "";
            this.loadOrderModification = "";
            this.loadMachine = "";
            this.loadCounterRepeat = "";

        }*/

        public FormAddCloseOrder(String dBase, String lStartOfShift, String lNameOfExecutor)
        {
            InitializeComponent();

            this.aMode = false;
            this.adminCloseOrder = false;

            this.dataBase = dBase;
            this.startOfShift = lStartOfShift;
            this.nameOfExecutor = lNameOfExecutor;
            this.loadOrderNumber = "";
            this.loadOrderModification = "";
            this.loadMachine = "";
            this.loadCounterRepeat = "";

        }

        public FormAddCloseOrder(String dBase, String lStartOfShift, String lNameOfExecutor, String lMachine)
        {
            InitializeComponent();

            this.aMode = false;
            this.adminCloseOrder = true;

            this.dataBase = dBase;
            this.startOfShift = lStartOfShift;
            this.nameOfExecutor = lNameOfExecutor;
            this.loadOrderNumber = "";
            this.loadOrderModification = "";
            this.loadMachine = lMachine;
            this.loadCounterRepeat = "";

        }

        public FormAddCloseOrder(bool adminMode, String dBase, String lStartOfShift, String lOrderNumber, String lOrderModification, String lMachine, String lCounterRepeat)
        {
            InitializeComponent();

            this.aMode = adminMode;
            this.adminCloseOrder = false;

            this.dataBase = dBase;
            this.startOfShift = lStartOfShift;
            this.loadOrderNumber = lOrderNumber;
            this.loadOrderModification = lOrderModification;
            this.loadMachine = lMachine;
            this.loadCounterRepeat = lCounterRepeat;

            if (adminMode)
            {
                CreateTransparentPannels();
            }
        }

        public FormAddCloseOrder(String dBase, String lStartOfShift, String lOrderNumber, String lOrderModification, String lMachine, String lCounterRepeat)
        {
            InitializeComponent();

            this.aMode = false;
            this.adminCloseOrder = false;

            this.dataBase = dBase;
            this.startOfShift = lStartOfShift;
            this.loadOrderNumber = lOrderNumber;
            this.loadOrderModification = lOrderModification;
            this.loadMachine = lMachine;
            this.loadCounterRepeat = lCounterRepeat;

            if (lStartOfShift == Form1.Info.startOfShift && loadOrderNumber != "")
            {
                CreateTransparentPannels();
            }
        }

        class Order
        {
            public String numberOfOrder;
            public String modificationOfOrder;
            public Order(String number, string modification)
            {
                numberOfOrder = number;
                modificationOfOrder = modification;
            }

        }

        List<Order> ordersNumbers = new List<Order>();
        bool loadAllOrdersToCurrentMachine = true;

        private void CreateTransparentPannels()
        {
            TransparentPanel panel1 = new TransparentPanel()
            {
                Location = dateTimePicker1.Location,
                Size = dateTimePicker1.Size,
            };
            panel1.Name = "panel1";
            groupBox2.Controls.Add(panel1);
            panel1.DoubleClick += panel1_DoubleClick;
            panel1.BringToFront();

            TransparentPanel panel2 = new TransparentPanel()
            {
                Location = dateTimePicker2.Location,
                Size = dateTimePicker2.Size,
            };
            panel2.Name = "panel2";
            groupBox2.Controls.Add(panel2);
            panel2.DoubleClick += panel2_DoubleClick;
            panel2.BringToFront();

            TransparentPanel panel3 = new TransparentPanel()
            {
                Location = dateTimePicker3.Location,
                Size = dateTimePicker3.Size,
            };
            panel3.Name = "panel3";
            groupBox3.Controls.Add(panel3);
            panel3.DoubleClick += panel3_DoubleClick;
            panel3.BringToFront();

            TransparentPanel panel4 = new TransparentPanel()
            {
                Location = dateTimePicker4.Location,
                Size = dateTimePicker4.Size,
            };
            panel4.Name = "panel4";
            groupBox3.Controls.Add(panel4);
            panel4.DoubleClick += panel4_DoubleClick;
            panel4.BringToFront();

            TransparentPanel panel5 = new TransparentPanel()
            {
                Location = numericUpDown4.Location,
                Size = numericUpDown4.Size,
            };
            panel5.Name = "panel5";
            groupBox4.Controls.Add(panel5);
            panel5.DoubleClick += panel5_DoubleClick;
            panel5.BringToFront();
        }

        private void SetVisibleElements(String status, String currentOrder)
        {
            if (status == "0") //новая запись
            {
                button1.Visible = true;
                button2.Visible = false;
                button3.Visible = false;

                button1.Text = "Начать приладку";

                dateTimePicker1.Visible = true;
                dateTimePicker2.Visible = false;
                textBox3.Visible = false;

                groupBox2.Visible = true;
                groupBox3.Visible = false;
                groupBox4.Visible = false;

                textBox6.Enabled = false;
            }
            if (status == "1") // начата приладка
            {
                if (currentOrder == "")
                {
                    button1.Visible = true;
                    button2.Visible = false;
                    button3.Visible = false;

                    button1.Text = "Продолжить приладку";

                    dateTimePicker1.Visible = true;
                    dateTimePicker2.Visible = false;
                    textBox3.Visible = false;

                    groupBox3.Visible = false;
                    groupBox4.Visible = false;

                    textBox6.Enabled = false;
                }
                else
                {
                    button1.Visible = true;
                    button2.Visible = true;
                    button3.Visible = true;

                    button1.Text = "Подтвердить приладку";
                    button2.Text = "Завершить приладку";
                    button3.Text = "Прервать приладку";

                    dateTimePicker1.Visible = true;
                    dateTimePicker2.Visible = true;
                    textBox3.Visible = true;

                    groupBox3.Visible = false;
                    groupBox4.Visible = false;

                    textBox6.Enabled = true;
                }

            }
            if (status == "2") // приладка завершена
            {
                button1.Visible = true;
                button2.Visible = false;
                button3.Visible = false;

                button1.Text = "Начать работу";

                dateTimePicker1.Visible = true;
                dateTimePicker2.Visible = true;

                dateTimePicker3.Visible = true;
                dateTimePicker4.Visible = false;

                textBox3.Visible = true;
                textBox4.Visible = false;

                groupBox3.Visible = true;
                groupBox4.Visible = false;

                textBox6.Enabled = false;
            }
            if (status == "3") // начата склейка
            {
                if (currentOrder == "")
                {
                    button1.Visible = true;
                    button2.Visible = false;
                    button3.Visible = false;

                    button1.Text = "Продолжить работу";

                    dateTimePicker1.Visible = true;
                    dateTimePicker2.Visible = true;
                    dateTimePicker3.Visible = true;
                    dateTimePicker4.Visible = false;

                    textBox3.Visible = true;
                    textBox4.Visible = true;

                    groupBox3.Visible = true;
                    groupBox4.Visible = true;

                    numericUpDown4.Visible = false;
                    label14.Visible = false;

                    textBox6.Enabled = false;
                }
                else
                {
                    button1.Visible = true;
                    button2.Visible = true;
                    button3.Visible = true;

                    button1.Text = "Подтвердить";
                    button2.Text = "Завершить работу";
                    button3.Text = "Прервать работу";

                    dateTimePicker1.Visible = true;
                    dateTimePicker2.Visible = true;
                    dateTimePicker3.Visible = true;
                    dateTimePicker4.Visible = true;

                    numericUpDown4.Visible = true;
                    label14.Visible = true;

                    textBox3.Visible = true;
                    textBox4.Visible = true;

                    groupBox3.Visible = true;
                    groupBox4.Visible = true;

                    textBox6.Enabled = true;
                }
            }
        }

        private bool CheckNotEmptyFields()
        {
            bool result = false;
            int count = 0;

            if (textBox1.Text == "")
                count++;
            if (comboBox2.Text == "")
                count++;
            if (numericUpDown1.Value == 0)
                count++;
            if (numericUpDown7.Value == 0 && numericUpDown8.Value == 0)
                count++;

            if (count == 0)
                result = true;

            return result;
        }

        private bool CheckMakereadyNullTime()
        {
            bool result = false;

            if (numericUpDown5.Value == 0 && numericUpDown5.Value == 0)
                result = true;

            return result;
        }

        private bool CheckOrderAvailable(String orderMachine, String orderNumber, String orderModification)
        {
            bool result = false;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE machine = @machine AND (numberOfOrder = @number AND modification = @orderModification)"
                };
                Command.Parameters.AddWithValue("@machine", orderMachine);
                Command.Parameters.AddWithValue("@number", orderNumber);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
                DbDataReader sqlReader = Command.ExecuteReader();
                //result = sqlReader.Read();
                while (sqlReader.Read())
                {
                    if (sqlReader["count"].ToString() != "")
                        result = true;
                }
                Connect.Close();
            }

            return result;
        }

        private void SetEnabledElements(int indexCheckBoxOrders)
        {

            if (indexCheckBoxOrders == 0)
            {
                textBox1.Enabled = true;
                comboBox2.Enabled = true;
                numericUpDown1.Enabled = true;
                numericUpDown5.Enabled = true;
                numericUpDown6.Enabled = true;
                numericUpDown7.Enabled = true;
                numericUpDown8.Enabled = true;
                textBox2.Enabled = true;
                textBox5.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
                comboBox2.Enabled = false;
                numericUpDown1.Enabled = false;
                numericUpDown5.Enabled = false;
                numericUpDown6.Enabled = false;
                numericUpDown7.Enabled = false;
                numericUpDown8.Enabled = false;
                textBox2.Enabled = false;
                textBox5.Enabled = false;
            }

            if (adminCloseOrder)
            {
                comboBox3.Enabled = false;
            }

        }

        private void ClearAllValue()
        {
            textBox1.Text = "";
            comboBox2.Text = "";
            numericUpDown1.Value = 0;
            numericUpDown5.Value = 0;
            numericUpDown6.Value = 0;
            numericUpDown7.Value = 0;
            numericUpDown8.Value = 0;
            textBox2.Text = "";
            textBox5.Text = "";
        }

        private void LoadMachine()
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);

            comboBox3.Items.Clear();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT id FROM machines"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (CheckUserToSelectedMachine(sqlReader["id"].ToString(), nameOfExecutor) == true)
                        comboBox3.Items.Add(getInfo.GetMachineName(sqlReader["id"].ToString()));
                    //else
                    //comboBox3.Items.Add(sqlReader["machine"].ToString());
                }

                Connect.Close();
            }

            if (comboBox3.Items.Count > 0)
            {
                SelectLastMschineToComboBox(nameOfExecutor);
            }
        }

        private bool CheckUserToSelectedMachine(String machine, String user)
        {
            ValueInfoBase getUserID = new ValueInfoBase(dataBase);
            if (getUserID.GetIDUser(machine) == user)
                return true;
            else
                return false;
        }

        private void SelectLastMschineToComboBox(String idUser)
        {
            ValueUserBase getMachine = new ValueUserBase(dataBase);
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);

            String machine = "";

            if (!adminCloseOrder)
            {
                machine = getMachine.GetLastMachineForUser(idUser);
            }
            else
            {
                machine = loadMachine;
                //comboBox3.Enabled = false;
            }

            if (machine != "" && comboBox3.Items.IndexOf(getInfo.GetMachineName(machine)) != -1)
                comboBox3.SelectedIndex = comboBox3.Items.IndexOf(getInfo.GetMachineName(machine));
            else
                comboBox3.SelectedIndex = 0;
        }

        private void AddOrderToDB()
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);
            GetDateTimeOperations totalMinutes = new GetDateTimeOperations();

            String orderAddedDate = DateTime.Now.ToString();
            //String machine = mashine;
            String machine = getInfo.GetMachineFromName(comboBox3.Text);
            String number = textBox1.Text;
            String name = comboBox2.Text;
            String modification = textBox5.Text;
            String amount = numericUpDown1.Value.ToString();
            String timeM = totalMinutes.TotalHoursToMinutesTS(TimeSpan.FromHours((int)numericUpDown5.Value).Add(TimeSpan.FromMinutes((int)numericUpDown6.Value))).ToString();
            String timeW = totalMinutes.TotalHoursToMinutesTS(TimeSpan.FromHours((int)numericUpDown7.Value).Add(TimeSpan.FromMinutes((int)numericUpDown8.Value))).ToString();
            String stamp = textBox2.Text;
            String status = "0";
            String counterR = "0";

            //SELECT COUNT(*) FROM YourTable WHERE YourKeyCol = YourKeyValue

            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(*) FROM orders WHERE ((numberOfOrder = @number AND modification = @modification) AND machine = @machine)"
                };

                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@number", number);
                Command.Parameters.AddWithValue("@modification", modification);

                Connect.Open();
                result = Convert.ToInt32(Command.ExecuteScalar());
                Connect.Close();
            }

            if (result == 0)
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    /*string commandText = "INSERT INTO orders (orderAddedDate, machine, numberOfOrder, nameOfOrder, modification, amountOfOrder, timeMakeready, timeToWork, orderStamp, statusOfOrder, counterRepeat) " +
                        "SELECT * FROM (SELECT @orderAddedDate, @machine, @number, @name, @modification, @amount, @timeM, @timeW, @stamp, @status, @counterR) " +
                        "AS tmp WHERE NOT EXISTS(SELECT numberOfOrder FROM orders WHERE (numberOfOrder = @number AND modification = @modification) AND machine = @machine) LIMIT 1";*/

                    string commandText = "INSERT INTO orders (orderAddedDate, machine, numberOfOrder, nameOfOrder, modification, amountOfOrder, timeMakeready, timeToWork, orderStamp, statusOfOrder, counterRepeat) " +
                        "VALUES (@orderAddedDate, @machine, @number, @name, @modification, @amount, @timeM, @timeW, @stamp, @status, @counterR)";

                    MySqlCommand Command = new MySqlCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@orderAddedDate", orderAddedDate); // присваиваем переменной значение
                    Command.Parameters.AddWithValue("@machine", machine);
                    Command.Parameters.AddWithValue("@number", number);
                    Command.Parameters.AddWithValue("@name", name);
                    Command.Parameters.AddWithValue("@modification", modification);
                    Command.Parameters.AddWithValue("@amount", amount);
                    Command.Parameters.AddWithValue("@timeM", timeM);
                    Command.Parameters.AddWithValue("@timeW", timeW);
                    Command.Parameters.AddWithValue("@stamp", stamp);
                    Command.Parameters.AddWithValue("@status", status);
                    Command.Parameters.AddWithValue("@counterR", counterR);


                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }

        private void AddNewOrderInProgress(String machine, String executor, String shiftStart, String number, String modification, String makereadyStart,
            String makereadyStop, String workStart, String workStop, String done, String counterRepeat, String note)
        {
            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(*) FROM ordersInProgress WHERE ((startOfShift = @shiftStart AND counterRepeat = @counterRepeat) AND (numberOfOrder = @number AND modification = @modification))"
                };

                Command.Parameters.AddWithValue("@shiftStart", shiftStart);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@number", number);
                Command.Parameters.AddWithValue("@modification", modification);

                Connect.Open();
                result = Convert.ToInt32(Command.ExecuteScalar());
                Connect.Close();
            }

            if (result == 0)
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    string commandText = "INSERT INTO ordersInProgress (machine, executor, startOfShift, numberOfOrder, modification, timeMakereadyStart, timeMakereadyStop, timeToWorkStart, timeToWorkStop, done, counterRepeat, note) " +
                        "VALUES(@machine, @executor, @shiftStart, @number, @modification, @makereadyStart, @makereadyStop, @workStart, @workStop, @done, @counterRepeat, @note)";

                    MySqlCommand Command = new MySqlCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@machine", machine); // присваиваем переменной значение
                    Command.Parameters.AddWithValue("@executor", executor);
                    Command.Parameters.AddWithValue("@shiftStart", shiftStart);
                    Command.Parameters.AddWithValue("@number", number);
                    Command.Parameters.AddWithValue("modification", modification);
                    Command.Parameters.AddWithValue("@makereadyStart", makereadyStart);
                    Command.Parameters.AddWithValue("@makereadyStop", makereadyStop);
                    Command.Parameters.AddWithValue("@workStart", workStart);
                    Command.Parameters.AddWithValue("@workStop", workStop);
                    Command.Parameters.AddWithValue("@done", done);
                    Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                    Command.Parameters.AddWithValue("@note", note);

                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }

        private void AcceptOrderInProgressToDB()
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);
            ValueOrdersBase getValue = new ValueOrdersBase(dataBase);
            ValueInfoBase infoBase = new ValueInfoBase(dataBase);
            ValueUserBase userBase = new ValueUserBase(dataBase);
            ValueOrdersBase orders = new ValueOrdersBase(dataBase);

            //имя и время начала смены сделать через параметры
            String executor = nameOfExecutor;
            String shiftStart = startOfShift;
            String number = textBox1.Text;
            String modification = textBox5.Text;
            String status = getValue.GetOrderStatus(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);
            String newStatus = "0";

            String machineCurrent = getInfo.GetMachineFromName(comboBox3.Text);
            String counterRepeat = getValue.GetCounterRepeat(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);
            String currentOrderNumber = getInfo.GetCurrentOrderNumber(getInfo.GetMachineFromName(comboBox3.Text));// сделать загрузку из базы в соответствии с выбранным оборудованием
            String lastOrderNumber = getInfo.GetLastOrderNumber(getInfo.GetMachineFromName(comboBox3.Text));

            String makereadyStart = dateTimePicker1.Text;
            String makereadyStop = dateTimePicker2.Text;
            String workStart = dateTimePicker3.Text;
            String workStop = dateTimePicker4.Text;
            String note = textBox6.Text;
            int done = (int)numericUpDown4.Value;

            userBase.UpdateLastMachine(executor, getInfo.GetMachineFromName(comboBox3.Text));

            if (status == "0") //новая запись
            {
                AddNewOrderInProgress(machineCurrent, executor, shiftStart, number, modification, makereadyStart, "", "", "", done.ToString(), counterRepeat, note);
                infoBase.UpdateInfo(machineCurrent, counterRepeat, number, modification, number, modification, true);
                newStatus = "1";
            }
            if (status == "1") // начата приладка
            {
                if (currentOrderNumber == "")
                {
                    AddNewOrderInProgress(machineCurrent, executor, shiftStart, number, modification, makereadyStart, "", "", "", done.ToString(), counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, number, modification, number, modification, true);
                    newStatus = "1";
                }
                else
                {
                    UpdateData("timeMakereadyStop", machineCurrent, shiftStart, number, modification, counterRepeat, makereadyStop);
                    UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, number, modification, number, modification, false);
                    //убираем заказ из активных для возможности завершить смену
                    newStatus = status;
                }

            }
            if (status == "2") // приладка завершена
            {
                if (currentOrderNumber == "")
                {
                    AddNewOrderInProgress(machineCurrent, executor, shiftStart, number, modification, "", "", workStart, "", done.ToString(), counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, number, modification, number, modification, true);
                    newStatus = "3";
                }
                else
                {
                    UpdateData("timeToWorkStart", machineCurrent, shiftStart, number, modification, counterRepeat, workStart);
                    UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, number, modification, number, modification, true);
                    newStatus = "3";
                }
            }
            if (status == "3") // начата склейка
            {
                if (currentOrderNumber == "")
                {
                    AddNewOrderInProgress(machineCurrent, executor, shiftStart, number, modification, "", "", workStart, "", done.ToString(), counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, number, modification, number, modification, true);
                    newStatus = status;
                }
                else
                {
                    GetCountOfDone orderCalc = new GetCountOfDone(dataBase, shiftStart, machineCurrent, number, modification, counterRepeat);
                    done += orderCalc.OrderCalculate(false, true);
                    UpdateData("timeToWorkStop", machineCurrent, shiftStart, number, modification, counterRepeat, workStop);
                    UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);
                    UpdateData("done", machineCurrent, shiftStart, number, modification, counterRepeat, done.ToString());
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, number, modification, number, modification, false);
                    //убираем заказ из активных для возможности завершить смену
                    newStatus = status;
                }
            }

            orders.SetNewStatus(machineCurrent, number, modification, newStatus);
        }

        private void CloseOrderInProgressToDB()
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);
            ValueOrdersBase getValue = new ValueOrdersBase(dataBase);
            ValueUserBase userBase = new ValueUserBase(dataBase);
            ValueOrdersBase orders = new ValueOrdersBase(dataBase);

            String executor = nameOfExecutor;
            String shiftStart = startOfShift;
            String number = textBox1.Text;
            String modification = textBox5.Text;
            String status = getValue.GetOrderStatus(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);
            String newStatus = "0";

            String machineCurrent = getInfo.GetMachineFromName(comboBox3.Text);
            String counterRepeat = getValue.GetCounterRepeat(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);
            String currentOrderNumber = getInfo.GetCurrentOrderNumber(getInfo.GetMachineFromName(comboBox3.Text));
            String lastOrderNumber = getInfo.GetLastOrderNumber(getInfo.GetMachineFromName(comboBox3.Text));

            String makereadyStart = dateTimePicker1.Text;
            String makereadyStop = dateTimePicker2.Text;
            String workStart = dateTimePicker3.Text;
            String workStop = dateTimePicker4.Text;
            String note = textBox6.Text;
            int done = (int)numericUpDown4.Value;

            userBase.UpdateLastMachine(executor, getInfo.GetMachineFromName(comboBox3.Text));

            if (status == "1") // начата приладка
            {
                if (currentOrderNumber != "")
                {
                    DialogResult dialogResult = DialogResult.No;

                    if (!adminCloseOrder)
                        dialogResult = MessageBox.Show("Приладка завершена. Начать выполнение заказа?", "Завершение приладки", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    UpdateData("timeMakereadyStop", machineCurrent, shiftStart, number, modification, counterRepeat, makereadyStop);
                    UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);

                    if (dialogResult == DialogResult.Yes)
                    {
                        UpdateData("timeToWorkStart", machineCurrent, shiftStart, number, modification, counterRepeat, makereadyStop);
                        UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);
                        getInfo.UpdateInfo(getInfo.GetMachineFromName(comboBox3.Text), counterRepeat, number, modification, number, modification, true);
                        //убираем заказ из активных для возможности завершить смену
                        newStatus = "3";
                    }

                    if (dialogResult == DialogResult.No || adminCloseOrder)
                    {
                        getInfo.UpdateInfo(getInfo.GetMachineFromName(comboBox3.Text), counterRepeat, number, modification, number, modification, false);
                        //убираем заказ из активных для возможности завершить смену
                        newStatus = "2";
                    }
                }
            }
            if (status == "3") // начата склейка
            {
                if (currentOrderNumber != "")
                {
                    GetCountOfDone orderCalc = new GetCountOfDone(dataBase, shiftStart, machineCurrent, number, modification, counterRepeat);
                    done += orderCalc.OrderCalculate(false, true);
                    UpdateData("timeToWorkStop", machineCurrent, shiftStart, number, modification, counterRepeat, workStop);
                    UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);
                    UpdateData("done", machineCurrent, shiftStart, number, modification, counterRepeat, done.ToString());
                    newStatus = "4";
                    getInfo.UpdateInfo(machineCurrent, "", "", "", "", "", false);
                }
            }

            orders.SetNewStatus(machineCurrent, number, modification, newStatus);
        }

        private void AbortOrderInProgressToDB()
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);
            ValueOrdersBase getValue = new ValueOrdersBase(dataBase);
            ValueUserBase userBase = new ValueUserBase(dataBase);
            ValueOrdersBase orders = new ValueOrdersBase(dataBase);

            String executor = nameOfExecutor;
            String shiftStart = startOfShift;
            String number = textBox1.Text;
            String modification = textBox5.Text;
            String status = getValue.GetOrderStatus(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);
            String newStatus = "0";

            String machineCurrent = getInfo.GetMachineFromName(comboBox3.Text);
            String counterRepeat = getValue.GetCounterRepeat(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);
            String currentOrderNumber = getInfo.GetCurrentOrderNumber(getInfo.GetMachineFromName(comboBox3.Text));
            String lastOrderNumber = getInfo.GetLastOrderNumber(getInfo.GetMachineFromName(comboBox3.Text));

            String makereadyStart = dateTimePicker1.Text;
            String makereadyStop = dateTimePicker2.Text;
            String workStart = dateTimePicker3.Text;
            String workStop = dateTimePicker4.Text;
            String note = textBox6.Text;
            int done = ((int)numericUpDown4.Value);

            userBase.UpdateLastMachine(executor, getInfo.GetMachineFromName(comboBox3.Text));

            if (status == "1") // начата приладка
            {
                if (currentOrderNumber != "")
                {
                    UpdateData("timeMakereadyStop", machineCurrent, shiftStart, number, modification, counterRepeat, makereadyStop);
                    UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);
                    newStatus = "0";
                }
            }
            if (status == "3") // начата склейка
            {
                if (currentOrderNumber != "")
                {
                    GetCountOfDone orderCalc = new GetCountOfDone(dataBase, shiftStart, machineCurrent, number, modification, counterRepeat);
                    done += orderCalc.OrderCalculate(false, true);
                    UpdateData("timeToWorkStop", machineCurrent, shiftStart, number, modification, counterRepeat, workStop);
                    UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);
                    UpdateData("done", machineCurrent, shiftStart, number, modification, counterRepeat, done.ToString());
                    newStatus = "0";

                }
            }

            orders.IncrementCounterRepeat(machineCurrent, number, modification);
            orders.SetNewStatus(machineCurrent, number, modification, newStatus);
            getInfo.UpdateInfo(machineCurrent, "", "", "", "", "", false);
        }

        private void UpdateData(String nameOfColomn, String machineCurrent, String shiftStart, String number, String modification, String counterRepeat, String value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET " + nameOfColomn + " = @value " +
                    "WHERE ((machine = @machineCurrent AND startOfShift = @shiftStart) AND (numberOfOrder = @number AND modification = @modification) AND (counterRepeat = @counterRepeat))";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineCurrent", machineCurrent); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@shiftStart", shiftStart);
                Command.Parameters.AddWithValue("@number", number);
                Command.Parameters.AddWithValue("@modification", modification);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void LoadOrdersToComboBox()
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);

            String cLine = "";

            comboBox1.Items.Clear();
            comboBox1.Items.Add("<новый>");

            comboBox2.Items.Clear();

            ordersNumbers.Clear();
            ordersNumbers.Add(new Order("", ""));

            if (loadAllOrdersToCurrentMachine == true)
                cLine = " AND machine = '" + getInfo.GetMachineFromName(comboBox3.Text) + "'";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE statusOfOrder <> 4" + cLine
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    String strModification = "";
                    if (sqlReader["modification"].ToString() != "")
                        strModification = " (" + sqlReader["modification"].ToString() + ")";

                    comboBox1.Items.Add(sqlReader["numberOfOrder"].ToString() + ": " +
                        sqlReader["nameOfOrder"].ToString() + strModification + " - " + Convert.ToInt32(sqlReader["amountOfOrder"]).ToString("N0"));

                    ordersNumbers.Add(new Order(sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));
                }

                Connect.Close();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT nameOfOrder FROM orders"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    comboBox2.Items.Add(sqlReader["nameOfOrder"].ToString());
                }

                Connect.Close();
            }

            if (getInfo.GetCurrentOrderNumber(getInfo.GetMachineFromName(comboBox3.Text)) != "")
            {
                int index = 0;
                for (int i = 0; i < ordersNumbers.Count; i++)
                {
                    if (ordersNumbers[i].numberOfOrder == getInfo.GetCurrentOrderNumber(getInfo.GetMachineFromName(comboBox3.Text)) && ordersNumbers[i].modificationOfOrder == getInfo.GetCurrentOrderModification(getInfo.GetMachineFromName(comboBox3.Text)))
                    {
                        index = i;
                        break;
                    }
                }
                comboBox1.SelectedIndex = index;
                comboBox1.Enabled = false;
            }
            else if (getInfo.GetLastOrderNumber(getInfo.GetMachineFromName(comboBox3.Text)) != "")
            {
                int index = 0;
                for (int i = 0; i < ordersNumbers.Count; i++)
                {
                    if (ordersNumbers[i].numberOfOrder == getInfo.GetLastOrderNumber(getInfo.GetMachineFromName(comboBox3.Text)) && ordersNumbers[i].modificationOfOrder == getInfo.GetLastOrderModification(getInfo.GetMachineFromName(comboBox3.Text)))
                    {
                        index = i;
                        break;
                    }
                }
                comboBox1.SelectedIndex = index;
                comboBox1.Enabled = true;
            }
            else
            {
                comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = true;
            }

        }

        private void LoadOrderFromDB(String orderMachine, String orderNumber, String orderModification)
        {
            //int orderStatus = 0;
            GetDateTimeOperations totalMinToHM = new GetDateTimeOperations();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE machine = @machine AND (numberOfOrder = @number AND modification = @orderModification)"
                };
                Command.Parameters.AddWithValue("@machine", orderMachine);
                Command.Parameters.AddWithValue("@number", orderNumber);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    textBox1.Text = sqlReader["numberOfOrder"].ToString();
                    comboBox2.Text = sqlReader["nameOfOrder"].ToString();
                    numericUpDown1.Value = Convert.ToInt32(sqlReader["amountOfOrder"]);
                    numericUpDown5.Value = totalMinToHM.TotalMinutesToHoursAndMinutes(Convert.ToInt32(sqlReader["timeMakeready"])).Item1;
                    numericUpDown6.Value = totalMinToHM.TotalMinutesToHoursAndMinutes(Convert.ToInt32(sqlReader["timeMakeready"])).Item2;
                    numericUpDown7.Value = totalMinToHM.TotalMinutesToHoursAndMinutes(Convert.ToInt32(sqlReader["timeToWork"])).Item1;
                    numericUpDown8.Value = totalMinToHM.TotalMinutesToHoursAndMinutes(Convert.ToInt32(sqlReader["timeToWork"])).Item2;
                    textBox2.Text = sqlReader["orderStamp"].ToString();
                    textBox5.Text = sqlReader["modification"].ToString();
                }
                Connect.Close();
            }
            SetEnabledElements(comboBox1.SelectedIndex);
        }

        private void LoadCurrentOrderInProgressFromDB(String startOfShift, String orderNumber, String orderModification, String machine, String counterRepeat)
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);
            GetDateTimeOperations timeDif = new GetDateTimeOperations();
            GetLeadTime leadTime = new GetLeadTime(dataBase, startOfShift, orderNumber, orderModification, machine, counterRepeat);
            GetCountOfDone orderCalc = new GetCountOfDone(dataBase, startOfShift, machine, orderNumber, orderModification, counterRepeat);
            ValueOrdersBase getValue = new ValueOrdersBase(dataBase);

            String currentTime = DateTime.Now.ToString();
            String prevMakereadyStart = leadTime.GetLastDateTime("timeMakereadyStart");
            String prevMakereadyStop = leadTime.GetLastDateTime("timeMakereadyStop");

            String curMakereadyStart = leadTime.GetCurrentDateTime("timeMakereadyStart");
            String curMakereadyStop = leadTime.GetCurrentDateTime("timeMakereadyStop");
            String curWorkStart = leadTime.GetCurrentDateTime("timeToWorkStart");

            String status = getValue.GetOrderStatus(getInfo.GetMachineFromName(comboBox3.Text), orderNumber, orderModification);
            String amountOrder = getValue.GetAmountOfOrder(getInfo.GetMachineFromName(comboBox3.Text), orderNumber, orderModification);

            int amountComplete = orderCalc.OrderFullCalculate();
            int amountFull = 0;

            if (amountOrder != "")
                amountFull = Convert.ToInt32(amountOrder);

            if (curMakereadyStart == "")
            {
                if (status == "1" || status == "0")
                {
                    dateTimePicker1.Visible = true;
                    dateTimePicker1.Text = currentTime;
                }
                else
                {
                    dateTimePicker1.Visible = false;
                    //dateTimePicker1.Text = prevMakereadyStart;
                }
            }
            else
            {
                dateTimePicker1.Visible = true;
                dateTimePicker1.Text = curMakereadyStart;
            }

            if (curMakereadyStop == "")
            {
                if (status == "1")
                {
                    dateTimePicker2.Visible = true;
                    dateTimePicker2.Text = currentTime;
                }
                else
                {
                    dateTimePicker2.Visible = false;
                    //dateTimePicker1.Text = prevMakereadyStart;
                }
            }
            else
            {
                dateTimePicker2.Visible = true;
                dateTimePicker2.Text = curMakereadyStop;
            }

            if (curWorkStart == "")
            {
                if (status == "2" || status == "3")
                {
                    dateTimePicker3.Visible = true;
                    dateTimePicker3.Text = currentTime;
                }
                else
                {
                    dateTimePicker3.Visible = false;
                    //dateTimePicker1.Text = prevMakereadyStart;
                }
            }
            else
            {
                dateTimePicker3.Visible = true;
                dateTimePicker3.Text = curWorkStart;
            }

            dateTimePicker4.Text = currentTime;

            numericUpDown2.Value = amountComplete;

            if (amountComplete >= amountFull)
                numericUpDown3.Value = 0;
            else
                numericUpDown3.Value = amountFull - amountComplete;

            if (dateTimePicker1.Visible == true || dateTimePicker2.Visible == true)
                groupBox2.Visible = true;
            else
                groupBox2.Visible = false;

            if (dateTimePicker3.Visible == true || dateTimePicker4.Visible == true)
                groupBox3.Visible = true;
            else
                groupBox3.Visible = false;

        }

        private void LoadOrderInProgressFromDB(String startOfShift, String orderNumber, String orderModification, String machine, String counterRepeat)
        {
            GetDateTimeOperations timeDif = new GetDateTimeOperations();
            GetLeadTime leadTime = new GetLeadTime(dataBase, startOfShift, orderNumber, orderModification, machine, counterRepeat);
            GetCountOfDone orderCalc = new GetCountOfDone(dataBase, startOfShift, machine, orderNumber, orderModification, counterRepeat);
            GetOrdersFromBase getOrder = new GetOrdersFromBase(dataBase);
            ValueOrdersBase getValue = new ValueOrdersBase(dataBase);

            String nMakereadyStart = leadTime.GetCurrentDateTime("timeMakereadyStart");
            String nMakereadyStop = leadTime.GetCurrentDateTime("timeMakereadyStop");
            String nWorkStart = leadTime.GetCurrentDateTime("timeToWorkStart");
            String nWorkStop = leadTime.GetCurrentDateTime("timeToWorkStop");

            String amountOrder = getValue.GetAmountOfOrder(machine, orderNumber, orderModification);

            int amountComplete = orderCalc.OrderCalculate(true, false);
            int amountDone = orderCalc.OrderCalculate(false, true);
            int amountFull = 0;

            if (amountOrder != "")
                amountFull = Convert.ToInt32(amountOrder);

            if (nMakereadyStart != "")
            {
                groupBox2.Visible = true;
                dateTimePicker1.Visible = true;
                dateTimePicker1.Text = nMakereadyStart;
            }
            else
            {
                groupBox2.Visible = false;
                dateTimePicker1.Visible = false;
                dateTimePicker1.Text = "";
            }

            if (nMakereadyStop != "")
            {
                dateTimePicker2.Visible = true;
                dateTimePicker2.Text = nMakereadyStop;
                textBox3.Visible = true;
                textBox3.Text = timeDif.DateDifferent(dateTimePicker2.Text, dateTimePicker1.Text);
            }
            else
            {
                dateTimePicker2.Visible = false;
                dateTimePicker2.Text = "";
                textBox3.Visible = false;
            }

            if (nWorkStart != "")
            {
                groupBox3.Visible = true;
                dateTimePicker3.Visible = true;
                dateTimePicker3.Text = nWorkStart;
            }
            else
            {
                groupBox3.Visible = false;
                dateTimePicker3.Visible = false;
                dateTimePicker3.Text = "";
            }

            if (nWorkStop != "")
            {
                dateTimePicker4.Visible = true;
                dateTimePicker4.Text = nWorkStop;
                textBox4.Visible = true;
                textBox4.Text = timeDif.DateDifferent(dateTimePicker4.Text, dateTimePicker3.Text);
            }
            else
            {
                dateTimePicker4.Visible = false;
                dateTimePicker4.Text = "";
                textBox4.Visible = false;
            }

            numericUpDown2.Value = amountComplete;

            if (amountComplete >= amountFull)
                numericUpDown3.Value = 0;
            else
                numericUpDown3.Value = amountFull - amountComplete;

            numericUpDown4.Enabled = false;
            numericUpDown4.Value = amountDone;

            textBox6.Text = getOrder.GetNote(startOfShift, orderNumber, orderModification, counterRepeat);
        }

        private void LoadOrderForEdit(String startOfShift, String orderNumber, String orderModification, String machine, String counterRepeat)
        {
            ValueOrdersBase getOrder = new ValueOrdersBase(dataBase);
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);

            this.Text = "Детали заказа";

            String strModification = "";
            if (orderModification != "")
                strModification = " (" + orderModification + ")";

            comboBox1.Items.Add("");
            comboBox1.Items.Add(orderNumber + ": " +
                getOrder.GetOrderName(machine, orderNumber, orderModification) + strModification + " - " + Convert.ToInt32(getOrder.GetAmountOfOrder(machine, orderNumber, orderModification)).ToString("N0"));
            comboBox1.SelectedIndex = 1;
            comboBox1.Enabled = false;

            comboBox3.Items.Add(getInfo.GetMachineName(machine));
            comboBox3.SelectedIndex = 0;
            comboBox3.Enabled = false;

            button2.Visible = false;
            button3.Visible = false;

            if (startOfShift == Form1.Info.startOfShift || aMode)
            {
                button1.Visible = true;
                textBox6.Enabled = true;
            }
            else
            {
                button1.Visible = false;
                textBox6.Enabled = false;
            }


            //SetEnabledElements(1);
            LoadOrderFromDB(machine, orderNumber, orderModification);
            LoadOrderInProgressFromDB(startOfShift, orderNumber, orderModification, machine, counterRepeat);
        }

        private void SaveChanges(String startOfShift, String orderNumber, String orderModification, String machine, String counterRepeat)
        {
            if (dateTimePicker1.Visible)
            {
                UpdateData("timeMakereadyStart", machine, startOfShift, orderNumber, orderModification, counterRepeat, dateTimePicker1.Text);
            }

            if (dateTimePicker2.Visible)
            {
                UpdateData("timeMakereadyStop", machine, startOfShift, orderNumber, orderModification, counterRepeat, dateTimePicker2.Text);
            }

            if (dateTimePicker3.Visible)
            {
                UpdateData("timeToWorkStart", machine, startOfShift, orderNumber, orderModification, counterRepeat, dateTimePicker3.Text);
            }

            if (dateTimePicker4.Visible)
            {
                UpdateData("timeToWorkStop", machine, startOfShift, orderNumber, orderModification, counterRepeat, dateTimePicker4.Text);
            }

            UpdateData("note", machine, startOfShift, orderNumber, orderModification, counterRepeat, textBox6.Text);
            UpdateData("done", machine, startOfShift, orderNumber, orderModification, counterRepeat, numericUpDown4.Value.ToString());

        }

        private void AddEditCloseOrder_Load(object sender, EventArgs e)
        {
            numericUpDown2.Controls[0].Enabled = false;
            numericUpDown2.Controls[0].Visible = false;
            numericUpDown2.Controls.Remove(Controls[0]);
            numericUpDown2.BackColor = Color.White;
            numericUpDown2.Enabled = false;

            numericUpDown3.Controls[0].Enabled = false;
            numericUpDown3.Controls[0].Visible = false;
            numericUpDown3.Controls.Remove(Controls[0]);
            numericUpDown3.BackColor = Color.White;
            numericUpDown3.Enabled = false;

            if (loadOrderNumber != "")
            {
                LoadOrderForEdit(startOfShift, loadOrderNumber, loadOrderModification, loadMachine, loadCounterRepeat);
                timer1.Enabled = false;
            }
            else
            {
                LoadMachine();

                //LoadOrdersToComboBox();

                timer1.Enabled = true;
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadOrderNumber == "")
            {
                ValueInfoBase getInfo = new ValueInfoBase(dataBase);

                String number = ordersNumbers[comboBox1.SelectedIndex].numberOfOrder;
                String modification = ordersNumbers[comboBox1.SelectedIndex].modificationOfOrder;

                ValueOrdersBase getValue = new ValueOrdersBase(dataBase);

                ClearAllValue();

                LoadOrderFromDB(getInfo.GetMachineFromName(comboBox3.Text), number, modification);
                SetVisibleElements(getValue.GetOrderStatus(getInfo.GetMachineFromName(comboBox3.Text), number, modification), getInfo.GetCurrentOrderNumber(getInfo.GetMachineFromName(comboBox3.Text)));
                if (comboBox1.SelectedIndex != 0)
                {
                    LoadCurrentOrderInProgressFromDB(startOfShift, number, modification, getInfo.GetMachineFromName(comboBox3.Text), getValue.GetCounterRepeat(getInfo.GetMachineFromName(comboBox3.Text), number, modification));

                    GetOrdersFromBase getOrdersInProgressValue = new GetOrdersFromBase(dataBase);
                    textBox6.Text = getOrdersInProgressValue.GetNote(startOfShift, number, modification, getValue.GetCounterRepeat(getInfo.GetMachineFromName(comboBox3.Text), number, modification));
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);
            ValueOrdersBase getValue = new ValueOrdersBase(dataBase);
            ValueOrdersBase orders = new ValueOrdersBase(dataBase);

            if ((startOfShift == Form1.Info.startOfShift && loadOrderNumber != "") || aMode)
            {
                SaveChanges(startOfShift, loadOrderNumber, loadOrderModification, loadMachine, loadCounterRepeat);
                Close();
            }
            else
            {
                //сделать проверку статуса и в зависимости от статуса и условий выбирать
                if (CheckNotEmptyFields() == true)
                {
                    DialogResult result;

                    if (!CheckOrderAvailable(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text))
                    {
                        AddOrderToDB();
                    }
                    else if (comboBox1.SelectedIndex == 0)
                    {
                        MessageBox.Show("Заказ №" + textBox1.Text + " есть в базе, выберите из списка или проверьте введенные данные.", "Добавление заказа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    String status = getValue.GetOrderStatus(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);

                    if (numericUpDown5.Value == 0 && numericUpDown6.Value == 0 && status == "0")
                    {
                        result = MessageBox.Show("Время на приладку не задано!\r\nНачать выполнение заказа?", "Добавление заказа", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            orders.SetNewStatus(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text, "3");
                            AcceptOrderInProgressToDB();
                            Close();
                        }
                        else if (result == DialogResult.No)
                        {
                            //return;
                        }

                    }
                    else if (numericUpDown4.Value >= numericUpDown3.Value && status == "3" && getInfo.GetCurrentOrderNumber(getInfo.GetMachineFromName(comboBox3.Text)) != "")
                    {
                        result = MessageBox.Show("Выработка превышает плановую!\r\n\r\nЗавершить заказ?", "Завершение заказа", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            CloseOrderInProgressToDB();
                            Close();
                        }
                        else if (result == DialogResult.No)
                        {
                            AcceptOrderInProgressToDB();
                            Close();
                        }
                    }
                    else
                    {
                        AcceptOrderInProgressToDB();
                        Close();
                    }

                }
                else
                {
                    MessageBox.Show("Не все данные введены.", "Проверка введенных данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }



        }
        private void button2_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);
            ValueOrdersBase getValue = new ValueOrdersBase(dataBase);

            DialogResult result;

            String status = getValue.GetOrderStatus(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);

            if (numericUpDown4.Value < numericUpDown3.Value && status == "3")
            {
                result = MessageBox.Show("Выработка меньше планируемой!\r\n\r\nДа - завершить заказ\r\nНет - подтвердить заказ", "Завершение заказа", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    CloseOrderInProgressToDB();
                    Close();
                }
                else if (result == DialogResult.No)
                {
                    AcceptOrderInProgressToDB();
                    Close();
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }

            }
            else
            {
                CloseOrderInProgressToDB();
                Close();
            }


            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AbortOrderInProgressToDB();
            LoadOrdersToComboBox();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);
            ValueOrdersBase getValue = new ValueOrdersBase(dataBase);
            LoadCurrentOrderInProgressFromDB(startOfShift, textBox1.Text, textBox5.Text, getInfo.GetMachineFromName(comboBox3.Text), getValue.GetCounterRepeat(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text));
        }

        private void AddEditCloseOrder_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
        }

        private void numericUpDown1_Enter(object sender, EventArgs e)
        {
            numericUpDown1.Select(0, numericUpDown1.ToString().Length);
        }

        private void numericUpDown5_Enter(object sender, EventArgs e)
        {
            numericUpDown5.Select(0, numericUpDown5.Text.Length);
        }

        private void numericUpDown6_Enter(object sender, EventArgs e)
        {
            numericUpDown6.Select(0, numericUpDown6.Text.Length);
        }

        private void numericUpDown7_Enter(object sender, EventArgs e)
        {
            numericUpDown7.Select(0, numericUpDown7.Text.Length);
        }

        private void numericUpDown8_Enter(object sender, EventArgs e)
        {
            numericUpDown8.Select(0, numericUpDown8.Text.Length);
        }

        private void numericUpDown4_Enter(object sender, EventArgs e)
        {
            numericUpDown4.Select(0, numericUpDown4.Text.Length);
        }

        private void numericUpDown1_Click(object sender, EventArgs e)
        {
            numericUpDown1.Select(0, numericUpDown1.Text.Length);
        }

        private void numericUpDown5_Click(object sender, EventArgs e)
        {
            numericUpDown5.Select(0, numericUpDown5.Text.Length);
        }

        private void numericUpDown6_Click(object sender, EventArgs e)
        {
            numericUpDown6.Select(0, numericUpDown6.Text.Length);
        }

        private void numericUpDown7_Click(object sender, EventArgs e)
        {
            numericUpDown7.Select(0, numericUpDown7.Text.Length);
        }

        private void numericUpDown8_Click(object sender, EventArgs e)
        {
            numericUpDown8.Select(0, numericUpDown8.Text.Length);
        }

        private void numericUpDown4_Click(object sender, EventArgs e)
        {
            numericUpDown4.Select(0, numericUpDown4.Text.Length);
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            dateTimePicker1.Enabled = true;
            ((TransparentPanel)sender).Visible = false;
        }

        private void panel2_DoubleClick(object sender, EventArgs e)
        {
            dateTimePicker2.Enabled = true;
            ((TransparentPanel)sender).Visible = false;
        }

        private void panel3_DoubleClick(object sender, EventArgs e)
        {
            dateTimePicker3.Enabled = true;
            ((TransparentPanel)sender).Visible = false;
        }

        private void panel4_DoubleClick(object sender, EventArgs e)
        {
            dateTimePicker4.Enabled = true;
            ((TransparentPanel)sender).Visible = false;
        }

        private void panel5_DoubleClick(object sender, EventArgs e)
        {
            numericUpDown4.Enabled = true;
            ((TransparentPanel)sender).Visible = false;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadOrderNumber == "")
            {
                LoadOrdersToComboBox();
            }
        }

        private void numericUpDown4_DoubleClick(object sender, EventArgs e)
        {
            numericUpDown4.Enabled = true;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            if (textBox3.Visible)
                textBox3.Text = timeOperations.DateDifferent(dateTimePicker2.Text, dateTimePicker1.Text);
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            if (textBox3.Visible)
                textBox3.Text = timeOperations.DateDifferent(dateTimePicker2.Text, dateTimePicker1.Text);
        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            if (textBox4.Visible)
                textBox4.Text = timeOperations.DateDifferent(dateTimePicker4.Text, dateTimePicker3.Text);
        }

        private void dateTimePicker4_ValueChanged(object sender, EventArgs e)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            if (textBox4.Visible)
                textBox4.Text = timeOperations.DateDifferent(dateTimePicker4.Text, dateTimePicker3.Text);
        }
    }

}

/*

ввод времени приладки и работы
возможность редактирвания начала и завершения операций

*/