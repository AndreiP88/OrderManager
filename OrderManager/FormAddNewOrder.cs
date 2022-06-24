using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAddNewOrder : Form
    {
        bool editOrderLoad;
        String dataBase;
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String orderrMachineLoad;
        String orderNumberLoad;
        String orderModificationLoad;

        List<String> numbersOrdersInProgress;
        String numbersOrder;

        public FormAddNewOrder(String dBase, String orderMachine, String orderNumber, String orderModification)
        {
            InitializeComponent();

            this.editOrderLoad = true;
            this.dataBase = dBase;
            this.orderrMachineLoad = orderMachine;
            this.orderNumberLoad = orderNumber;
            this.orderModificationLoad = orderModification;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        public FormAddNewOrder(String dBase, String orderMachine)
        {
            InitializeComponent();

            this.editOrderLoad = false;
            this.dataBase = dBase;
            this.orderrMachineLoad = orderMachine;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        private void SelectCurrentMachineToComboBox(String machine)
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            if (machine != "" && comboBox1.Items.IndexOf(getInfo.GetMachineName(machine)) != -1)
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(getInfo.GetMachineName(machine));
            else
                comboBox1.SelectedIndex = 0;
        }

        private void LoadMachine()
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT id FROM machines"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

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

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT nameOfOrder FROM orders"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

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

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE machine = @machine AND (numberOfOrder = @number AND modification = @orderModification)"
                };
                Command.Parameters.AddWithValue("@machine", orderMachine);
                Command.Parameters.AddWithValue("@number", orderNumber);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
                SQLiteDataReader sqlReader = Command.ExecuteReader();
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
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetDateTimeOperations totalMinutes = new GetDateTimeOperations();
            GetValueFromOrdersBase getOrderCount = new GetValueFromOrdersBase(dataBase, orderrMachineLoad, orderNumberLoad, orderModificationLoad);

            String orderCount = getOrderCount.GetOrderCount();
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


            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText;

                if (!editOrderLoad)
                commandText = "INSERT INTO orders (orderAddedDate, machine, numberOfOrder, nameOfOrder, modification, amountOfOrder, timeMakeready, timeToWork, orderStamp, statusOfOrder, counterRepeat) " +
                    "SELECT * FROM (SELECT @orderAddedDate, @machine, @number, @name, @modification, @amount, @timeM, @timeW, @stamp, @status, @counterR) " +
                    "AS tmp WHERE NOT EXISTS(SELECT numberOfOrder FROM orders WHERE (numberOfOrder = @number AND modification = @modification) AND machine = @machine) LIMIT 1";
                else
                    commandText = "UPDATE orders SET orderAddedDate = @orderAddedDate, machine = @machine, numberOfOrder = @number, nameOfOrder = @name, modification = @modification, " +
                    "amountOfOrder = @amount, timeMakeready = @timeM, timeToWork = @timeW, orderStamp = @stamp " +
                    "WHERE count = @orderCount";

                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
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
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            String machine = getInfo.GetMachineFromName(comboBox1.Text);
            String number = textBox1.Text;
            String modification = textBox5.Text;            

            for(int i = 0; i < numbersOrdersInProgress.Count; i++)
            {
                using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                {
                    string commandText;
                    commandText = "UPDATE ordersInProgress SET machine = @machine, numberOfOrder = @number, modification = @modification " +
                        "WHERE count = @count";

                    SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
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
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            String machine = getInfo.GetMachineFromName(comboBox1.Text);

            SetUpdateInfoBase setInfo = new SetUpdateInfoBase(dataBase, machine);

            String number = textBox1.Text;
            String modification = textBox5.Text;

            if (getInfo.GetCurrentOrderNumber(machine) == orderNumberLoad && getInfo.GetCurrentOrderModification(machine) == orderModificationLoad)
            {
                setInfo.UpdateCurrentOrder(number, modification);
            }

        }

        private void LoadOrderFromDB(String orderMachine, String orderNumber, String orderModification)
        {
            //int orderStatus = 0;
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetValueFromOrdersBase getOrderValue = new GetValueFromOrdersBase(dataBase, orderMachine, orderNumber, orderModification);
            GetDateTimeOperations totalMinToHM = new GetDateTimeOperations();

            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase(dataBase);
            numbersOrdersInProgress = (List<String>)ordersFromBase.GetNumbersOrders(orderMachine, orderNumber, orderModification);
            numbersOrder = getOrderValue.GetOrderCount();

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE machine = @machine AND (numberOfOrder = @number AND modification = @orderModification)"
                };
                Command.Parameters.AddWithValue("@machine", orderMachine);
                Command.Parameters.AddWithValue("@number", orderNumber);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
                SQLiteDataReader sqlReader = Command.ExecuteReader();

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
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetValueFromOrdersBase getValue = new GetValueFromOrdersBase(dataBase, getInfo.GetMachineFromName(comboBox1.Text), textBox1.Text, textBox5.Text);

            if (CheckNotEmptyFields() == true)
            {
                if (CheckOrderAvailable(getInfo.GetMachineFromName(comboBox1.Text), textBox1.Text, textBox5.Text) && numbersOrder != getValue.GetOrderCount())
                {
                    MessageBox.Show("Заказ №" + textBox1.Text + " есть в базе, проверьте введенные данные.", "Добавление заказа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (CheckOrderAvailable(getInfo.GetMachineFromName(comboBox1.Text), textBox1.Text, textBox5.Text) && numbersOrder == getValue.GetOrderCount())
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
    }

}

/*

Проверка на пустые поля
ввод времени приладки и работы
возможность редактирвания начала и завершения операций

*/