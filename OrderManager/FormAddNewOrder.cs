using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OrderManager
{
    public partial class FormAddNewOrder : Form
    {
        bool editOrderLoad;
        String orderrMachineLoad;
        String orderNumberLoad;
        String orderModificationLoad;

        List<String> numbersOrdersInProgress;
        String numbersOrder;

        public FormAddNewOrder(String orderMachine, String orderNumber, String orderModification)
        {
            InitializeComponent();

            this.editOrderLoad = true;
            this.orderrMachineLoad = orderMachine;
            this.orderNumberLoad = orderNumber;
            this.orderModificationLoad = orderModification;

        }

        public FormAddNewOrder(String orderMachine)
        {
            InitializeComponent();

            this.editOrderLoad = false;
            this.orderrMachineLoad = orderMachine;

        }

        private void SelectCurrentMachineToComboBox(String machine)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            if (machine != "" && comboBox1.Items.IndexOf(getInfo.GetMachineName(machine)) != -1)
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(getInfo.GetMachineName(machine));
            else
                comboBox1.SelectedIndex = 0;
        }

        private void LoadMachine()
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT id FROM machines"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    comboBox1.Items.Add(getInfo.GetMachineName(sqlReader["id"].ToString()));
                }

                Connect.Close();
            }

            SelectCurrentMachineToComboBox(orderrMachineLoad);
        }

        private void LoadOrdersToComboBox()
        {
            comboBox2.Items.Clear();

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

        private void AddOrderToDB()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations totalMinutes = new GetDateTimeOperations();
            ValueOrdersBase getOrderCount = new ValueOrdersBase();

            String orderCount = getOrderCount.GetOrderCount(orderrMachineLoad, orderNumberLoad, orderModificationLoad);
            String orderAddedDate = DateTime.Now.ToString();
            String machine = getInfo.GetMachineFromName(comboBox1.Text);
            String number = textBox1.Text;
            String name = comboBox2.Text;
            String modification = textBox5.Text;
            String amount = numericUpDown1.Value.ToString();
            String timeM = totalMinutes.TotalHoursToMinutesTS(TimeSpan.FromHours((int)numericUpDown5.Value).Add(TimeSpan.FromMinutes((int)numericUpDown6.Value))).ToString();
            String timeW = totalMinutes.TotalHoursToMinutesTS(TimeSpan.FromHours((int)numericUpDown7.Value).Add(TimeSpan.FromMinutes((int)numericUpDown8.Value))).ToString();
            String stamp = textBox2.Text;
            String status = "0";
            String counterR = "0";

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

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;

                if (!editOrderLoad && result == 0)
                    commandText = "INSERT INTO orders (orderAddedDate, machine, numberOfOrder, nameOfOrder, modification, amountOfOrder, timeMakeready, timeToWork, orderStamp, statusOfOrder, counterRepeat) " +
                        "VALUES (@orderAddedDate, @machine, @number, @name, @modification, @amount, @timeM, @timeW, @stamp, @status, @counterR)";
                else
                    commandText = "UPDATE orders SET machine = @machine, numberOfOrder = @number, nameOfOrder = @name, modification = @modification, " +
                    "amountOfOrder = @amount, timeMakeready = @timeM, timeToWork = @timeW, orderStamp = @stamp " +
                    "WHERE count = @orderCount";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@orderCount", orderCount); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@orderAddedDate", orderAddedDate);
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

        private void EditOrderInProgress()
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            String machine = getInfo.GetMachineFromName(comboBox1.Text);
            String number = textBox1.Text;
            String modification = textBox5.Text;

            for (int i = 0; i < numbersOrdersInProgress.Count; i++)
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    string commandText;
                    commandText = "UPDATE ordersInProgress SET machine = @machine, numberOfOrder = @number, modification = @modification " +
                        "WHERE count = @count";

                    MySqlCommand Command = new MySqlCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@machine", machine);
                    Command.Parameters.AddWithValue("@number", number);
                    Command.Parameters.AddWithValue("@modification", modification);
                    Command.Parameters.AddWithValue("@count", numbersOrdersInProgress[i].ToString());

                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }

        private void EditCurrentOrderInfo()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            String machine = getInfo.GetMachineFromName(comboBox1.Text);

            ValueInfoBase setInfo = new ValueInfoBase();

            String number = textBox1.Text;
            String modification = textBox5.Text;

            if (getInfo.GetCurrentOrderNumber(machine) == orderNumberLoad && getInfo.GetCurrentOrderModification(machine) == orderModificationLoad)
            {
                setInfo.UpdateCurrentOrder(machine, number, modification);
            }

        }

        private void LoadOrderFromDB(String orderMachine, String orderNumber, String orderModification)
        {
            //int orderStatus = 0;
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getOrderValue = new ValueOrdersBase();
            GetDateTimeOperations totalMinToHM = new GetDateTimeOperations();

            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();
            numbersOrdersInProgress = (List<String>)ordersFromBase.GetNumbersOrders(orderMachine, orderNumber, orderModification);
            numbersOrder = getOrderValue.GetOrderCount(orderrMachineLoad, orderNumberLoad, orderModificationLoad);

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
                    comboBox1.Text = getInfo.GetMachineName(sqlReader["machine"].ToString());
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getValue = new ValueOrdersBase();

            if (CheckNotEmptyFields() == true)
            {
                if (CheckOrderAvailable(getInfo.GetMachineFromName(comboBox1.Text), textBox1.Text, textBox5.Text) && numbersOrder != getValue.GetOrderCount(getInfo.GetMachineFromName(comboBox1.Text), textBox1.Text, textBox5.Text))
                {
                    MessageBox.Show("Заказ №" + textBox1.Text + " есть в базе, проверьте введенные данные.", "Добавление заказа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (CheckOrderAvailable(getInfo.GetMachineFromName(comboBox1.Text), textBox1.Text, textBox5.Text) && numbersOrder == getValue.GetOrderCount(getInfo.GetMachineFromName(comboBox1.Text), textBox1.Text, textBox5.Text))
                {
                    AddOrderToDB();

                    if (editOrderLoad == true && numbersOrdersInProgress.Count > 0)
                    {
                        EditOrderInProgress();
                        EditCurrentOrderInfo();
                    }
                }
                else if (!CheckOrderAvailable(getInfo.GetMachineFromName(comboBox1.Text), textBox1.Text, textBox5.Text))
                {
                    AddOrderToDB();

                    if (editOrderLoad == true && numbersOrdersInProgress.Count > 0)
                    {
                        EditOrderInProgress();
                        EditCurrentOrderInfo();
                    }
                }
                Close();
            }
            else
            {
                MessageBox.Show("Не все данные введены.", "Проверка введенных данных", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
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

        private void FormAddNewOrder_Load(object sender, EventArgs e)
        {
            LoadMachine();
            LoadOrdersToComboBox();
            if (editOrderLoad)
            {
                LoadOrderFromDB(orderrMachineLoad, orderNumberLoad, orderModificationLoad);
                button1.Text = "Редактировать";
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ValueInfoBase valueInfoBase = new ValueInfoBase();

            string machine = valueInfoBase.GetMachineFromName(orderrMachineLoad);

            FormAddTimeMkWork fm = new FormAddTimeMkWork(orderrMachineLoad, numericUpDown1.Value, textBox2.Text);
            fm.ShowDialog();

            if (fm.NewValue)
            {
                SetNewValue(fm.ValAmount, fm.ValStamp, fm.ValMakeready, fm.ValWork);
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
    }

}

/*

Проверка на пустые поля
ввод времени приладки и работы
возможность редактирвания начала и завершения операций

*/