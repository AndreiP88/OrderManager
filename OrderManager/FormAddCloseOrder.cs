﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows.Forms;
using static OrderManager.Form1;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace OrderManager
{
    public partial class FormAddCloseOrder : Form
    {
        bool aMode;
        bool adminCloseOrder;
        String startOfShift;
        String nameOfExecutor;
        int loadOrderId;
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
            this. = dBase;
            this.startOfShift = lStartOfShift;
            this.nameOfExecutor = getUser.GetCurrentUserIDFromShiftStart(lStartOfShift);
            this.loadOrderNumber = "";
            this.loadOrderModification = "";
            this.loadMachine = "";
            this.loadCounterRepeat = "";

        }*/

        public FormAddCloseOrder(String lStartOfShift, String lNameOfExecutor)
        {
            InitializeComponent();

            this.aMode = false;
            this.adminCloseOrder = false;

            this.startOfShift = lStartOfShift;
            this.nameOfExecutor = lNameOfExecutor;
            this.loadOrderId = -1;
            this.loadMachine = "";
            this.loadCounterRepeat = "";

        }

        public FormAddCloseOrder(String lStartOfShift, String lNameOfExecutor, String lMachine)
        {
            InitializeComponent();

            this.aMode = false;
            this.adminCloseOrder = true;

            this.startOfShift = lStartOfShift;
            this.nameOfExecutor = lNameOfExecutor;
            this.loadOrderId = -1;
            this.loadMachine = lMachine;
            this.loadCounterRepeat = "";

        }

        public FormAddCloseOrder(bool adminMode, String lStartOfShift, int lOrderID, String lMachine, String lCounterRepeat)
        {
            InitializeComponent();

            this.aMode = adminMode;
            this.adminCloseOrder = false;

            this.startOfShift = lStartOfShift;
            this.loadOrderId = lOrderID;
            this.loadMachine = lMachine;
            this.loadCounterRepeat = lCounterRepeat;

            if (adminMode)
            {
                CreateTransparentPannels();
            }
        }

        public FormAddCloseOrder(String lStartOfShift, int lOrderID, String lMachine, String lCounterRepeat)
        {
            InitializeComponent();

            this.aMode = false;
            this.adminCloseOrder = false;

            this.startOfShift = lStartOfShift;
            this.loadOrderId = lOrderID;
            this.loadMachine = lMachine;
            this.loadCounterRepeat = lCounterRepeat;

            if (lStartOfShift == Form1.Info.startOfShift && loadOrderId != -1)
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
        List<int> ordersIndexes = new List<int>();

        bool loadAllOrdersToCurrentMachine = true;

        List<string> items = new List<string>();

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

        private void SetVisibleElements(string status, string currentOrder)
        {
            if (status == "0" || status == "-1") //новая запись
            {
                button1.Visible = true;
                button2.Visible = false;
                button3.Visible = false;

                button6.Enabled = false;

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
                if (currentOrder == "-1")
                {
                    button1.Visible = true;
                    button2.Visible = false;
                    button3.Visible = false;

                    button6.Enabled = false;

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

                    button6.Enabled = true;

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

                button6.Enabled = false;

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
                if (currentOrder == "-1")
                {
                    button1.Visible = true;
                    button2.Visible = false;
                    button3.Visible = false;

                    button6.Enabled = false;

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

                    button6.Enabled = true;

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
                button5.Enabled = true;
                button7.Enabled = true;
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
                button5.Enabled = false;
                button7.Enabled = false;
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
            ValueInfoBase getInfo = new ValueInfoBase();

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
            ValueInfoBase getUserID = new ValueInfoBase();
            if (getUserID.GetIDUser(machine) == user)
                return true;
            else
                return false;
        }

        private void SelectLastMschineToComboBox(String idUser)
        {
            ValueUserBase getMachine = new ValueUserBase();
            ValueInfoBase getInfo = new ValueInfoBase();

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
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations totalMinutes = new GetDateTimeOperations();

            String orderAddedDate = DateTime.Now.ToString();
            //String machine = mashine;
            string machine = getInfo.GetMachineFromName(comboBox3.Text);
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

                int orderID = ordersBase.GetOrderID(machine, number, modification);

                for (int i = 0; i < items.Count; i = i + 2)
                {
                    using (MySqlConnection Connect = DBConnection.GetDBConnection())
                    {
                        /*string commandText = "INSERT INTO orders (orderAddedDate, machine, numberOfOrder, nameOfOrder, modification, amountOfOrder, timeMakeready, timeToWork, orderStamp, statusOfOrder, counterRepeat) " +
                            "SELECT * FROM (SELECT @orderAddedDate, @machine, @number, @name, @modification, @amount, @timeM, @timeW, @stamp, @status, @counterR) " +
                            "AS tmp WHERE NOT EXISTS(SELECT numberOfOrder FROM orders WHERE (numberOfOrder = @number AND modification = @modification) AND machine = @machine) LIMIT 1";*/

                        string commandText = "INSERT INTO typesList (orderID, name, count) " +
                        "VALUES (@orderID, @name, @count)";

                        MySqlCommand Command = new MySqlCommand(commandText, Connect);
                        Command.Parameters.AddWithValue("@orderID", orderID); // присваиваем переменной значение
                        Command.Parameters.AddWithValue("@name", items[i]);
                        Command.Parameters.AddWithValue("@count", items[i + 1]);

                        Connect.Open();
                        Command.ExecuteNonQuery();
                        Connect.Close();
                    }
                }
            }
        }

        private void AddNewOrderInProgress(String machine, String executor, String shiftStart, int orderIndex, String makereadyStart,
            String makereadyStop, String workStart, String workStop, String done, String counterRepeat, String note)
        {
            ValueOrdersBase valueOrders = new ValueOrdersBase();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            string shiftStartID = shiftsBase.GetIDFromStartShift(shiftStart);

            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(*) FROM ordersInProgress WHERE ((startOfShift = @shiftStart AND counterRepeat = @counterRepeat) AND (orderID = @id))"
                };

                Command.Parameters.AddWithValue("@shiftStart", shiftStart);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@id", orderIndex);

                Connect.Open();
                result = Convert.ToInt32(Command.ExecuteScalar());
                Connect.Close();
            }

            if (result == 0)
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    string commandText = "INSERT INTO ordersInProgress (machine, executor, startOfShift, startOfShiftID, orderID, timeMakereadyStart, timeMakereadyStop, timeToWorkStart, timeToWorkStop, done, counterRepeat, note) " +
                        "VALUES(@machine, @executor, @shiftStart, @shiftStartID, @orderID, @makereadyStart, @makereadyStop, @workStart, @workStop, @done, @counterRepeat, @note)";

                    MySqlCommand Command = new MySqlCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@machine", machine); // присваиваем переменной значение
                    Command.Parameters.AddWithValue("@executor", executor);
                    Command.Parameters.AddWithValue("@shiftStart", shiftStart);
                    Command.Parameters.AddWithValue("@shiftStartID", shiftStartID);
                    Command.Parameters.AddWithValue("@orderID", orderIndex);
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
            //ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getValue = new ValueOrdersBase();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();
            ValueOrdersBase orders = new ValueOrdersBase();

            //имя и время начала смены сделать через параметры
            String executor = nameOfExecutor;
            String shiftStart = startOfShift;
            String number = textBox1.Text;
            String modification = textBox5.Text;
            
            String newStatus = "0";

            String machineCurrent = infoBase.GetMachineFromName(comboBox3.Text);
            int orderID = getValue.GetOrderID(machineCurrent, number, modification);

            string status = getValue.GetOrderStatus(orderID);
            String counterRepeat = getValue.GetCounterRepeat(orderID);
            string currentOrderID = infoBase.GetCurrentOrderID(infoBase.GetMachineFromName(comboBox3.Text));// сделать загрузку из базы в соответствии с выбранным оборудованием
            string lastOrderID = infoBase.GetLastOrderID(infoBase.GetMachineFromName(comboBox3.Text));

            String makereadyStart = dateTimePicker1.Text;
            String makereadyStop = dateTimePicker2.Text;
            String workStart = dateTimePicker3.Text;
            String workStop = dateTimePicker4.Text;
            String note = textBox6.Text;
            int done = (int)numericUpDown4.Value;

            userBase.UpdateLastMachine(executor, infoBase.GetMachineFromName(comboBox3.Text));

            if (status == "0") //новая запись
            {
                AddNewOrderInProgress(machineCurrent, executor, shiftStart, orderID, makereadyStart, "", "", "", done.ToString(), counterRepeat, note);
                infoBase.UpdateInfo(machineCurrent, counterRepeat, orderID.ToString(), orderID.ToString(), true);
                newStatus = "1";
            }
            if (status == "1") // начата приладка
            {
                if (currentOrderID == "-1")
                {
                    AddNewOrderInProgress(machineCurrent, executor, shiftStart, orderID, makereadyStart, "", "", "", done.ToString(), counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, orderID.ToString(), orderID.ToString(), true);
                    newStatus = "1";
                }
                else
                {
                    UpdateData("timeMakereadyStop", machineCurrent, shiftStart, orderID, counterRepeat, makereadyStop);
                    UpdateData("note", machineCurrent, shiftStart, orderID, counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, orderID.ToString(), orderID.ToString(), false);
                    //убираем заказ из активных для возможности завершить смену
                    newStatus = status;
                }

            }
            if (status == "2") // приладка завершена
            {
                if (currentOrderID == "-1")
                {
                    AddNewOrderInProgress(machineCurrent, executor, shiftStart, orderID, "", "", workStart, "", done.ToString(), counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, orderID.ToString(), orderID.ToString(), true);
                    newStatus = "3";
                }
                else
                {
                    UpdateData("timeToWorkStart", machineCurrent, shiftStart, orderID, counterRepeat, workStart);
                    UpdateData("note", machineCurrent, shiftStart, orderID, counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, orderID.ToString(), orderID.ToString(), true);
                    newStatus = "3";
                }
            }
            if (status == "3") // начата склейка
            {
                if (currentOrderID == "-1")
                {
                    AddNewOrderInProgress(machineCurrent, executor, shiftStart, orderID, "", "", workStart, "", done.ToString(), counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, counterRepeat, orderID.ToString(), orderID.ToString(), true);
                    newStatus = status;
                }
                else
                {
                    GetCountOfDone orderCalc = new GetCountOfDone(shiftStart, orderID, counterRepeat);
                    done += orderCalc.OrderCalculate(false, true);
                    UpdateData("timeToWorkStop", machineCurrent, shiftStart, orderID, counterRepeat, workStop);
                    UpdateData("note", machineCurrent, shiftStart, orderID, counterRepeat, note);
                    UpdateData("done", machineCurrent, shiftStart, orderID, counterRepeat, done.ToString());
                    //infoBase.UpdateInfo(machineCurrent, counterRepeat, number, modification, number, modification, false);
                    infoBase.UpdateInfo(machineCurrent, "", "", orderID.ToString(), false);
                    //убираем заказ из активных для возможности завершить смену
                    newStatus = status;
                }
            }

            orders.SetNewStatus(orderID, newStatus);
        }

        private void CloseOrderInProgressToDB()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getValue = new ValueOrdersBase();
            ValueUserBase userBase = new ValueUserBase();
            ValueOrdersBase orders = new ValueOrdersBase();

            String executor = nameOfExecutor;
            String shiftStart = startOfShift;
            String number = textBox1.Text;
            String modification = textBox5.Text;
            
            String newStatus = "0";

            String machineCurrent = getInfo.GetMachineFromName(comboBox3.Text);
            int orderID = getValue.GetOrderID(machineCurrent, number, modification);

            string status = getValue.GetOrderStatus(orderID);
            string counterRepeat = getValue.GetCounterRepeat(orderID);
            string currentOrderID = getInfo.GetCurrentOrderID(getInfo.GetMachineFromName(comboBox3.Text));
            string lastOrderID = getInfo.GetLastOrderID(getInfo.GetMachineFromName(comboBox3.Text));

            String makereadyStart = dateTimePicker1.Text;
            String makereadyStop = dateTimePicker2.Text;
            String workStart = dateTimePicker2.Value.AddMinutes(1).ToString();
            String workStop = dateTimePicker4.Text;
            String note = textBox6.Text;
            int done = (int)numericUpDown4.Value;


            userBase.UpdateLastMachine(executor, getInfo.GetMachineFromName(comboBox3.Text));

            if (status == "1") // начата приладка
            {
                if (currentOrderID != "-1")
                {
                    DialogResult dialogResult = DialogResult.No;

                    if (!adminCloseOrder)
                        dialogResult = MessageBox.Show("Приладка завершена. Начать выполнение заказа?", "Завершение приладки", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Cancel)
                    {
                        return;
                    }

                    UpdateData("timeMakereadyStop", machineCurrent, shiftStart, orderID, counterRepeat, makereadyStop);
                    UpdateData("note", machineCurrent, shiftStart, orderID, counterRepeat, note);

                    if (dialogResult == DialogResult.Yes)
                    {
                        UpdateData("timeToWorkStart", machineCurrent, shiftStart, orderID, counterRepeat, workStart);
                        //UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);
                        getInfo.UpdateInfo(getInfo.GetMachineFromName(comboBox3.Text), counterRepeat, orderID.ToString(), orderID.ToString(), true);
                        //убираем заказ из активных для возможности завершить смену
                        newStatus = "3";
                    }

                    if (dialogResult == DialogResult.No || adminCloseOrder)
                    {
                        getInfo.UpdateInfo(getInfo.GetMachineFromName(comboBox3.Text), counterRepeat, orderID.ToString(), orderID.ToString(), false);
                        //убираем заказ из активных для возможности завершить смену
                        newStatus = "2";
                    }
                }
            }
            if (status == "3") // начата склейка
            {
                if (currentOrderID != "-1")
                {
                    GetCountOfDone orderCalc = new GetCountOfDone(shiftStart, orderID, counterRepeat);
                    done += orderCalc.OrderCalculate(false, true);
                    UpdateData("timeToWorkStop", machineCurrent, shiftStart, orderID, counterRepeat, workStop);
                    UpdateData("note", machineCurrent, shiftStart, orderID, counterRepeat, note);
                    UpdateData("done", machineCurrent, shiftStart, orderID, counterRepeat, done.ToString());
                    newStatus = "4";
                    getInfo.UpdateInfo(machineCurrent, "", "", "", false);
                }
            }

            orders.SetNewStatus(orderID, newStatus);

            Close();
        }

        private void AbortOrderInProgressToDB()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getValue = new ValueOrdersBase();
            ValueUserBase userBase = new ValueUserBase();
            ValueOrdersBase orders = new ValueOrdersBase();

            String executor = nameOfExecutor;
            String shiftStart = startOfShift;
            String number = textBox1.Text;
            String modification = textBox5.Text;
            
            String newStatus = "0";

            String machineCurrent = getInfo.GetMachineFromName(comboBox3.Text);
            int orderID = getValue.GetOrderID(machineCurrent, number, modification);

            string status = getValue.GetOrderStatus(orderID);
            string counterRepeat = getValue.GetCounterRepeat(orderID);
            string currentOrderID = getInfo.GetCurrentOrderID(getInfo.GetMachineFromName(comboBox3.Text));
            string lastOrderID = getInfo.GetLastOrderID(getInfo.GetMachineFromName(comboBox3.Text));

            String makereadyStart = dateTimePicker1.Text;
            String makereadyStop = dateTimePicker2.Text;
            String workStart = dateTimePicker3.Text;
            String workStop = dateTimePicker4.Text;
            String note = textBox6.Text;
            int done = ((int)numericUpDown4.Value);

            userBase.UpdateLastMachine(executor, getInfo.GetMachineFromName(comboBox3.Text));

            if (status == "1") // начата приладка
            {
                if (currentOrderID != "-1")
                {
                    UpdateData("timeMakereadyStop", machineCurrent, shiftStart, orderID, counterRepeat, makereadyStop);
                    UpdateData("note", machineCurrent, shiftStart, orderID, counterRepeat, note);
                    newStatus = "0";
                }
            }
            if (status == "3") // начата склейка
            {
                if (currentOrderID != "-1")
                {
                    GetCountOfDone orderCalc = new GetCountOfDone(shiftStart, orderID, counterRepeat);
                    done += orderCalc.OrderCalculate(false, true);
                    UpdateData("timeToWorkStop", machineCurrent, shiftStart, orderID, counterRepeat, workStop);
                    UpdateData("note", machineCurrent, shiftStart, orderID, counterRepeat, note);
                    UpdateData("done", machineCurrent, shiftStart, orderID, counterRepeat, done.ToString());
                    newStatus = "0";

                }
            }

            orders.IncrementCounterRepeat(orderID);
            orders.SetNewStatus(orderID, newStatus);
            getInfo.UpdateInfo(machineCurrent, "", "", "", false);
        }

        private void UpdateData(String nameOfColomn, String machineCurrent, String shiftStart, int orderIndex, String counterRepeat, String value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET " + nameOfColomn + " = @value " +
                    "WHERE ((machine = @machineCurrent AND startOfShift = @shiftStart) AND (orderID = @id AND counterRepeat = @counterRepeat))";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineCurrent", machineCurrent); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@shiftStart", shiftStart);
                Command.Parameters.AddWithValue("@id", orderIndex);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void LoadOrdersToComboBox()
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            String cLine = "";

            comboBox1.Items.Clear();
            comboBox1.Items.Add("<новый>");

            comboBox2.Items.Clear();

            ordersNumbers.Clear();
            ordersNumbers.Add(new Order("", ""));

            ordersIndexes.Clear();
            ordersIndexes.Add(-1);

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

                    ordersIndexes.Add((int)sqlReader["count"]);
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

            if (getInfo.GetCurrentOrderID(getInfo.GetMachineFromName(comboBox3.Text)) != "-1")
            {
                int index = 0;
                for (int i = 0; i < ordersIndexes.Count; i++)
                {
                    if (ordersIndexes[i].ToString() == getInfo.GetCurrentOrderID(getInfo.GetMachineFromName(comboBox3.Text)))
                    {
                        index = i;
                        break;
                    }
                }
                comboBox1.SelectedIndex = index;
                comboBox1.Enabled = false;
                //button7.Enabled = false;
            }
            else if (getInfo.GetLastOrderID(getInfo.GetMachineFromName(comboBox3.Text)) != "-1")
            {
                int index = 0;
                for (int i = 0; i < ordersIndexes.Count; i++)
                {
                    if (ordersIndexes[i].ToString() == getInfo.GetLastOrderID(getInfo.GetMachineFromName(comboBox3.Text)))
                    {
                        index = i;
                        break;
                    }
                }
                comboBox1.SelectedIndex = index;
                comboBox1.Enabled = true;
                //button7.Enabled = false;
            }
            else
            {
                comboBox1.SelectedIndex = 0;
                comboBox1.Enabled = true;
                //button7.Enabled = true;
            }

        }

        private bool OrderSelectionIfItExists(string machine, string number, string modification)
        {
            bool exist = false;

            ValueOrdersBase ordersBase = new ValueOrdersBase();

            int orderIndex = ordersBase.GetOrderID(machine, number, modification);

            int itemIndex = ordersIndexes.IndexOf(orderIndex);

            if (itemIndex != -1)
            {
                comboBox1.SelectedIndex = itemIndex;
                exist = true;
            }

            /*int itemIndex = .FindLastIndex((v) => v.indexTypeList == typesCurrent[i].indexTypeList,
                                             (v) => v.indexTypeList == typesCurrent[i].indexTypeList);*/

            return exist;
        }

        private void LoadOrderFromDB(int orderIndex)
        {
            //int orderStatus = 0;
            GetDateTimeOperations totalMinToHM = new GetDateTimeOperations();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE count = @id"
                };
                Command.Parameters.AddWithValue("@id", orderIndex);

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

        private void LoadCurrentOrderInProgressFromDB(String startOfShift, int orderID, String counterRepeat)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations timeDif = new GetDateTimeOperations();
            GetLeadTime leadTime = new GetLeadTime(startOfShift, orderID, counterRepeat);
            GetCountOfDone orderCalc = new GetCountOfDone(startOfShift, orderID, counterRepeat);
            ValueOrdersBase getValue = new ValueOrdersBase();

            String currentTime = DateTime.Now.ToString();
            String prevMakereadyStart = leadTime.GetLastDateTime("timeMakereadyStart");
            String prevMakereadyStop = leadTime.GetLastDateTime("timeMakereadyStop");

            String curMakereadyStart = leadTime.GetCurrentDateTime("timeMakereadyStart");
            String curMakereadyStop = leadTime.GetCurrentDateTime("timeMakereadyStop");
            String curWorkStart = leadTime.GetCurrentDateTime("timeToWorkStart");

            String status = getValue.GetOrderStatus(orderID);
            String amountOrder = getValue.GetAmountOfOrder(orderID);

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

        private void LoadOrderInProgressFromDB(String startOfShift, int orderID, String machine, String counterRepeat)
        {
            GetDateTimeOperations timeDif = new GetDateTimeOperations();
            GetLeadTime leadTime = new GetLeadTime(startOfShift, orderID, counterRepeat);
            GetCountOfDone orderCalc = new GetCountOfDone(startOfShift, orderID, counterRepeat);
            GetOrdersFromBase getOrder = new GetOrdersFromBase();
            ValueOrdersBase getValue = new ValueOrdersBase();

            String nMakereadyStart = leadTime.GetCurrentDateTime("timeMakereadyStart");
            String nMakereadyStop = leadTime.GetCurrentDateTime("timeMakereadyStop");
            String nWorkStart = leadTime.GetCurrentDateTime("timeToWorkStart");
            String nWorkStop = leadTime.GetCurrentDateTime("timeToWorkStop");

            String amountOrder = getValue.GetAmountOfOrder(orderID);

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

            textBox6.Text = getOrder.GetNote(startOfShift, orderID, counterRepeat, machine);
        }

        private void LoadOrderForEdit(string startOfShift, int orderID, string machine, string counterRepeat)
        {
            ValueOrdersBase getOrder = new ValueOrdersBase();
            ValueInfoBase getInfo = new ValueInfoBase();

            this.Text = "Детали заказа";

            string modification = getOrder.GetOrderModification(orderID);
            string number = getOrder.GetOrderNumber(orderID);

            String strModification = "";
            if (modification != "")
                strModification = " (" + modification + ")";

            comboBox1.Items.Add("");
            comboBox1.Items.Add(number + ": " +
                getOrder.GetOrderName(orderID) + strModification + " - " + Convert.ToInt32(getOrder.GetAmountOfOrder(orderID)).ToString("N0"));
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

                button6.Enabled = true;
            }
            else
            {
                button1.Visible = false;
                textBox6.Enabled = false;

                button6.Enabled = false;
            }


            //SetEnabledElements(1);
            LoadOrderFromDB(orderID);
            LoadOrderInProgressFromDB(startOfShift, orderID, machine, counterRepeat);
        }

        private void SaveChanges(String startOfShift, int orderIndex, String machine, String counterRepeat)
        {
            if (dateTimePicker1.Visible)
            {
                UpdateData("timeMakereadyStart", machine, startOfShift, orderIndex, counterRepeat, dateTimePicker1.Text);
            }

            if (dateTimePicker2.Visible)
            {
                UpdateData("timeMakereadyStop", machine, startOfShift, orderIndex, counterRepeat, dateTimePicker2.Text);
            }

            if (dateTimePicker3.Visible)
            {
                UpdateData("timeToWorkStart", machine, startOfShift, orderIndex, counterRepeat, dateTimePicker3.Text);
            }

            if (dateTimePicker4.Visible)
            {
                UpdateData("timeToWorkStop", machine, startOfShift, orderIndex, counterRepeat, dateTimePicker4.Text);
            }

            UpdateData("note", machine, startOfShift, orderIndex, counterRepeat, textBox6.Text);
            UpdateData("done", machine, startOfShift, orderIndex, counterRepeat, numericUpDown4.Value.ToString());

        }

        private void AddEditCloseOrder_Load(object sender, EventArgs e)
        {
            ValueShiftsBase shiftsValue = new ValueShiftsBase();

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

            if (loadOrderId != -1)
            {
                LoadOrderForEdit(startOfShift, loadOrderId, loadMachine, loadCounterRepeat);
                LoadTypesFromCurrentOrder(loadOrderId, loadCounterRepeat, loadMachine, shiftsValue.GetNameUserFromStartShift(startOfShift));

                timer1.Enabled = false;
            }
            else
            {
                LoadMachine();

                //LoadOrdersToComboBox();

                timer1.Enabled = true;
            }

        }

        private void SetNewValue(decimal amountOrder, string stampOrder, int makereadyOrder, int workOrder)
        {
            numericUpDown1.Value = amountOrder;
            textBox2.Text = stampOrder;

            int makereadyH = makereadyOrder / 60;
            int makereadyM = makereadyOrder % 60;

            int workH = workOrder / 60;
            int workM = workOrder % 60;

            numericUpDown5.Value = makereadyH;
            numericUpDown6.Value = makereadyM;

            numericUpDown7.Value = workH;
            numericUpDown8.Value = workM;

        }

        /*private void SetNewOrder(string number, string customer, string item, int mkTime, int wkTime, decimal amount, string stamp, List<string> itemsOrder)
        {
            textBox1.Text = number;
            comboBox2.Text = customer;

            textBox5.Text = item;

            numericUpDown1.Value = amount;
            textBox2.Text = stamp;

            int makereadyH = mkTime / 60;
            int makereadyM = mkTime % 60;

            int workH = wkTime / 60;
            int workM = wkTime % 60;

            numericUpDown5.Value = makereadyH;
            numericUpDown6.Value = makereadyM;

            numericUpDown7.Value = workH;
            numericUpDown8.Value = workM;

            items = itemsOrder;
        }*/

        private void SetNewOrder(OrdersLoad order, List<string> itemsOrder)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            string machine = infoBase.GetMachineFromName(comboBox3.Text);

            if (!OrderSelectionIfItExists(machine, order.numberOfOrder, order.nameItem))
            {
                textBox1.Text = order.numberOfOrder;
                comboBox2.Text = order.nameCustomer;

                textBox5.Text = order.nameItem;

                numericUpDown1.Value = order.amountOfOrder;
                textBox2.Text = order.stamp;

                int mkTime = order.makereadyTime;
                int wkTime = order.workTime;

                int makereadyH = mkTime / 60;
                int makereadyM = mkTime % 60;

                int workH = wkTime / 60;
                int workM = wkTime % 60;

                numericUpDown5.Value = makereadyH;
                numericUpDown6.Value = makereadyM;

                numericUpDown7.Value = workH;
                numericUpDown8.Value = workM;

                items = itemsOrder;
            }
        }

        private void LoadTypes()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getValue = new ValueOrdersBase();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            int orderIndex = -1;

            if (loadOrderId != -1)
            {
                orderIndex = loadOrderId;
            }
            else
            {
                orderIndex = ordersIndexes[comboBox1.SelectedIndex];
            }

            String machine = getInfo.GetMachineFromName(comboBox3.Text);
            String counterRepeat = getValue.GetCounterRepeat(orderIndex);
            

            FormTypesInTheOrder form;

            form = new FormTypesInTheOrder(startOfShift,
                orderIndex,
                counterRepeat,
                machine,
                shiftsBase.GetNameUserFromStartShift(startOfShift));

            form.ShowDialog();

            LoadTypesFromCurrentOrder(orderIndex, counterRepeat, machine, getInfo.GetIDUser(machine));
        }

        private void LoadTypesFromCurrentOrder(int orderIndex, string counterRepeat, string machine, string user)
        {
            listView1.Items.Clear();

            ValueTypesBase typeBase = new ValueTypesBase(startOfShift, orderIndex, counterRepeat, machine, user);
            
            List<TypeInTheOrder> typesCurrent = typeBase.GetData();

            for (int i = 0; i < typesCurrent.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = typesCurrent[i].id.ToString();
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(typeBase.GetNameItemFromID(typesCurrent[i].indexTypeList));
                item.SubItems.Add(typesCurrent[i].done.ToString("N0"));

                listView1.Items.Add(item);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadOrderId == -1)
            {
                ValueInfoBase getInfo = new ValueInfoBase();
                ValueOrdersBase getValue = new ValueOrdersBase();

                int orderIndex = ordersIndexes[comboBox1.SelectedIndex];
                
                String machine = getInfo.GetMachineFromName(comboBox3.Text);
                String counterRepeat = getValue.GetCounterRepeat(orderIndex);
                
                ClearAllValue();

                LoadOrderFromDB(orderIndex);
                SetVisibleElements(getValue.GetOrderStatus(orderIndex), getInfo.GetCurrentOrderID(machine));
                if (comboBox1.SelectedIndex != 0)
                {
                    LoadCurrentOrderInProgressFromDB(startOfShift, orderIndex, counterRepeat);

                    LoadTypesFromCurrentOrder(orderIndex, counterRepeat, machine, getInfo.GetIDUser(machine));

                    GetOrdersFromBase getOrdersInProgressValue = new GetOrdersFromBase();
                    textBox6.Text = getOrdersInProgressValue.GetNote(startOfShift, orderIndex, counterRepeat, machine);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase orders = new ValueOrdersBase();

            if ((startOfShift == Form1.Info.startOfShift && loadOrderId != -1) || aMode)
            {
                SaveChanges(startOfShift, loadOrderId, loadMachine, loadCounterRepeat);
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

                    int orderIndex = orders.GetOrderID(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);

                    String status = orders.GetOrderStatus(orderIndex);

                    if (numericUpDown5.Value == 0 && numericUpDown6.Value == 0 && status == "0")
                    {
                        result = MessageBox.Show("Время на приладку не задано!\r\nНачать выполнение заказа?", "Добавление заказа", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            orders.SetNewStatus(orderIndex, "3");
                            AcceptOrderInProgressToDB();
                            Close();
                        }
                        else if (result == DialogResult.No)
                        {
                            //return;
                        }

                    }
                    else if (numericUpDown4.Value >= numericUpDown3.Value && status == "3" && getInfo.GetCurrentOrderID(getInfo.GetMachineFromName(comboBox3.Text)) != "")
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
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase orders = new ValueOrdersBase();

            MessageBoxManager.Yes = "Завершить";
            MessageBoxManager.No = "Подтвердить";
            MessageBoxManager.Cancel = "Отмена";

            DialogResult result;

            int orderIndex = orders.GetOrderID(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);

            String status = orders.GetOrderStatus(orderIndex);

            if (numericUpDown4.Value < numericUpDown3.Value && status == "3")
            {
                MessageBoxManager.Register();
                //result = MessageBox.Show("Выработка меньше планируемой!\r\n\r\nДа - завершить заказ\r\nНет - подтвердить заказ", "Завершение заказа", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                result = MessageBox.Show("Выработка меньше планируемой!", "Завершение заказа", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                MessageBoxManager.Unregister();

                if (result == DialogResult.Yes)
                {
                    CloseOrderInProgressToDB();
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
                //Close();
            }
            //Close();
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
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase orders = new ValueOrdersBase();

            int orderIndex = orders.GetOrderID(getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);

            LoadCurrentOrderInProgressFromDB(startOfShift, orderIndex, orders.GetCounterRepeat(orderIndex));
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
            if (loadOrderId == -1)
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

        private void button5_Click(object sender, EventArgs e)
        {
            ValueInfoBase valueInfoBase = new ValueInfoBase();

            string machine = valueInfoBase.GetMachineFromName(comboBox3.Text);

            FormAddTimeMkWork fm = new FormAddTimeMkWork(machine, numericUpDown1.Value, textBox2.Text);
            fm.ShowDialog();

            if(fm.NewValue)
            {
                SetNewValue(fm.ValAmount, fm.ValStamp, fm.ValMakeready, fm.ValWork);
            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            LoadTypes();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ValueInfoBase valueInfoBase = new ValueInfoBase();

            string machine = valueInfoBase.GetMachineFromName(comboBox3.Text);

            FormLoadOrders fm = new FormLoadOrders(machine);
            fm.ShowDialog();

            if (fm.NewValue)
            {
                SetNewOrder(fm.SetValue, fm.Types);
            }
        }
    }

}

/*

ввод времени приладки и работы
возможность редактирвания начала и завершения операций

*/