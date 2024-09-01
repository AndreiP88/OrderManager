using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.DataBaseReconnect;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OrderManager
{
    public partial class FormAddCloseEditOrder : Form
    {
        int ShiftID;
        //int OrderID = -1;
        int OrderInProgressID = -1;
        bool AdminMode = false;
        bool AdminCloseOrder = false;

        bool _editOrder;

        /*bool aMode;
        bool adminCloseOrder;
        int shiftIndex;
        string nameOfExecutor;
        int loadOrderId;
        string loadMachine;
        int loadCounterRepeat;*/

        int _userID;
        int _orderRegistrationType;
        int _typeJob;

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

        public FormAddCloseEditOrder(int loadShiftID)
        {
            InitializeComponent();

            this.ShiftID = loadShiftID;
            _editOrder = false;
        }

        public FormAddCloseEditOrder(int loadShiftID, int orderInProgressID)
        {
            InitializeComponent();

            this.ShiftID = loadShiftID;
            this.OrderInProgressID = orderInProgressID;
            _editOrder = true;
        }

        /*
         * Сделать потом для загрузки в режиме администратора и редактирования
         * public FormAddCloseEditOrder(bool adminMode, int lShiftID, int lOrderID, string lMachine, int lCounterRepeat)
        {
            InitializeComponent();

            if (adminMode)
            {
                CreateTransparentPannels();
            }
        }

        public FormAddCloseEditOrder(int lShiftID, int lOrderID, string lMachine, int lCounterRepeat)
        {
            InitializeComponent();

            if (lShiftID == Form1.Info.shiftIndex && OrderID != -1)
            {
                CreateTransparentPannels();
            }
        }*/

        class Order
        {
            public int IDOrder;
            public string numberOfOrder;
            public string modificationOfOrder;

            public Order(int idOrder, string number, string modification)
            {
                IDOrder = idOrder;
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

        private void HideNumericUpDownControls()
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
        }

        private async void AddEditCloseOrder_Load(object sender, EventArgs e)
        {
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            ValueShiftsBase shiftsValue = new ValueShiftsBase();
            ValueSettingsBase valueSettings = new ValueSettingsBase();

            HideNumericUpDownControls();

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        _userID = shiftsValue.GetNameUserFromStartShift(ShiftID);
                        _orderRegistrationType = valueSettings.GetOrderRegistrationType(_userID.ToString());

                        if (_editOrder)
                        {
                            _typeJob = getOrders.GetJobType(OrderInProgressID);
                            tabControl1.SelectTab(_typeJob);

                            switch (_typeJob)
                            {
                                case 0:

                                    break;
                                case 1:
                                    await LoadOrderForEdit(OrderInProgressID);
                                    LoadTypesFromCurrentOrder(OrderInProgressID);
                                    break;
                                default:
                                    break;
                            }
                            
                        }
                        else
                        {
                            await LoadMachine();
                            await LoadOrderCustomersToComboBox();
                            await LoadIdletimeListToComboBox();

                            if (comboBox3.Items.Count > 0)
                            {
                                await SelectLastMachineToComboBox(_userID);
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

        private async Task LoadMachine()
        {
            await Task.Run(async () =>
            {
                ValueInfoBase getInfo = new ValueInfoBase();

                Invoke(new Action(() =>
                {
                    comboBox3.Items?.Clear();
                }));
                
                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT DISTINCT id FROM machines"
                                };

                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync())
                                {
                                    Invoke(new Action(async () =>
                                    {
                                        if (CheckUserToSelectedMachine(Convert.ToInt32(sqlReader["id"]), _userID) == true)
                                            comboBox3.Items.Add(await getInfo.GetMachineName(sqlReader["id"].ToString()));
                                    }));
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
            });
        }

        private async Task LoadOrderCustomersToComboBox()
        {
            await Task.Run(async () =>
            {
                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            Invoke(new Action(() =>
                            {
                                comboBox2.Items.Clear();
                            }));
                            
                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT DISTINCT nameOfOrder FROM orders"
                                };
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync()) // считываем и вносим в комбобокс список заголовков
                                {
                                    Invoke(new Action(() =>
                                    {
                                        comboBox2.Items.Add(sqlReader["nameOfOrder"].ToString());
                                    }));
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
            });
        }

        private async Task LoadOrdersToComboBox(string machine)
        {
            await Task.Run(async () =>
            {
                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            Invoke(new Action(() =>
                            {
                                comboBox1.Items?.Clear();
                                comboBox1.Items?.Add("<новый>");
                            }));

                            ordersNumbers?.Clear();
                            ordersNumbers?.Add(new Order(-1, "", ""));

                            ordersIndexes?.Clear();
                            ordersIndexes?.Add(-1);

                            string cLine = "";

                            if (loadAllOrdersToCurrentMachine == true)
                                cLine = " AND machine = '" + machine + "'";

                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT * FROM orders WHERE statusOfOrder <> 4" + cLine
                                };
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync()) // считываем и вносим в комбобокс список заголовков
                                {
                                    string strModification = "";

                                    if (sqlReader["modification"].ToString() != "")
                                        strModification = " (" + sqlReader["modification"].ToString() + ")";

                                    Invoke(new Action(() =>
                                    {
                                        comboBox1.Items.Add(sqlReader["numberOfOrder"].ToString() + ": " +
                                        sqlReader["nameOfOrder"].ToString() + strModification + " - " + Convert.ToInt32(sqlReader["amountOfOrder"]).ToString("N0"));
                                    }));

                                    ordersNumbers.Add(new Order((int)sqlReader["count"], sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));

                                    ordersIndexes.Add((int)sqlReader["count"]);
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
            });
        }
        //string machine = await getInfo.GetMachineFromName(comboBox3.Text);
        private async Task SelectedMachineChange(string machine)
        {
            await LoadOrdersToComboBox(machine);
            await SelectActualityOrderForSelectedMachine(machine);
        }

       private async Task LoadOrderDetails()
        {
            if (!_editOrder)
            {
                ValueInfoBase getInfo = new ValueInfoBase();
                ValueOrdersBase getValue = new ValueOrdersBase();
                GetOrdersFromBase getOrdersInProgressValue = new GetOrdersFromBase();

                int orderIndex = ordersIndexes[comboBox1.SelectedIndex];

                int machine = await getInfo.GetMachineIDFromName(comboBox3.Text);
                int counterRepeat = getValue.GetCounterRepeat(orderIndex);

                ClearAllValue();

                LoadOrderFromDB(orderIndex);
                SetVisibleElements(getValue.GetOrderStatus(orderIndex), getInfo.GetCurrentOrderID(machine.ToString()).ToString());

                if (comboBox1.SelectedIndex != 0)
                {
                    LoadCurrentOrderInProgressFromDB(ShiftID, machine, orderIndex, counterRepeat);

                    //////////////////////////
                    ///
                    GetOrdersFromBase getOrders = new GetOrdersFromBase();

                    int orderInProgressID = getOrders.GetOrderInProgressID(ShiftID, orderIndex, counterRepeat, Convert.ToInt32(machine));

                    LoadTypesFromCurrentOrder(orderInProgressID);

                    textBox6.Text = getOrdersInProgressValue.GetNote(ShiftID, orderIndex, counterRepeat, machine);

                    ChangeTheStateOfTheMakereadySwitch(ShiftID, machine, orderIndex, counterRepeat);
                }
            }
        }

        private async Task LoadIdletimeListToComboBox()
        {
            await Task.Run(async () =>
            {
                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            Invoke(new Action(() =>
                            {
                                comboBox4.Items.Clear();
                            }));

                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT DISTINCT name FROM idletimeList"
                                };
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync()) // считываем и вносим в комбобокс список заголовков
                                {
                                    Invoke(new Action(() =>
                                    {
                                        comboBox4.Items.Add(sqlReader["name"].ToString());
                                    }));
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
            });
        }

        private async Task LoadIdletimeDetails(int idletimeID)
        {
            await Task.Run(async () =>
            {
                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT * FROM idletime WHERE id = @idletimeID"
                                };
                                Command.Parameters.AddWithValue("@idletimeID", idletimeID);
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync()) // считываем и вносим в комбобокс список заголовков
                                {
                                    Invoke(new Action(() =>
                                    {
                                        comboBox4.SelectedIndex = (int)sqlReader["idIdletimeList"] - 1;
                                        idletimeNumericUpDownH.Value = (int)sqlReader["normtime"] / 60;
                                        idletimeNumericUpDownM.Value = (int)sqlReader["normtime"] % 60;
                                        checkBox2.Checked = Convert.ToBoolean((int)sqlReader["checkIntoWorkingOut"]);
                                    }));
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
            });
        }

        private async Task LoadCurrentIdletimeFromDB(int shiftID, string machine, int idletimeID)
        {
            //timeToWorkStart - начало
            //timeToWorkStop - завершение
            //makereadyComplete - 0/1 не завершен/завершен

            await Task.Run(async () =>
            {
                bool reconnectionRequired = false;
                DialogResult dialog = DialogResult.Retry;

                do
                {
                    if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                    {
                        try
                        {
                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT * FROM ordersInProgress WHERE ((shiftID = @shiftID AND machine = @machine) AND (orderID = @idletimeID AND typeJob = 1))"
                                };
                                Command.Parameters.AddWithValue("@shiftID", shiftID);
                                Command.Parameters.AddWithValue("@machine", machine);
                                Command.Parameters.AddWithValue("@idletimeID", idletimeID);
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync()) // считываем и вносим в комбобокс список заголовков
                                {
                                    Invoke(new Action(() =>
                                    {
                                        dateTimePicker5.Text = sqlReader["timeToWorkStart"].ToString();

                                        if (sqlReader["timeToWorkStop"].ToString() != "")
                                        {
                                            dateTimePicker6.Text = sqlReader["timeToWorkStop"].ToString();
                                        }
                                        else
                                        {
                                            dateTimePicker6.Text = DateTime.Now.ToString();
                                        }
    

                                    }));
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
            });
        }






























        private bool CheckUserToSelectedMachine(int machine, int user)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            if (infoBase.GetIDUser(machine.ToString()) == user.ToString())
                return true;
            else
                return false;
        }

        private async Task SelectLastMachineToComboBox(int idUser)
        {
            ValueUserBase getMachine = new ValueUserBase();
            ValueInfoBase getInfo = new ValueInfoBase();

            int machine = -1;

            if (!_editOrder)// а надо ли, если мы для одного методы вызываем этот
            {
                machine = getMachine.GetLastMachineForUser(idUser.ToString());
            }

            string machineName = await getInfo.GetMachineName(machine.ToString());
            int indexMachineFromComboBobx = comboBox3.Items.IndexOf(machineName);

            if (machine != -1 && indexMachineFromComboBobx != -1)
                comboBox3.SelectedIndex = indexMachineFromComboBobx;
            else
                comboBox3.SelectedIndex = 0;
        }

        private async Task SelectActualityOrderForSelectedMachine(string machine)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            int currentJobType = getInfo.GetCurrentTypeJob(machine);

            tabControl1.SelectTab(currentJobType);

            if (currentJobType == 0)
            {
                int currentOrderID = getInfo.GetCurrentOrderID(machine);
                int lastOrderID = getInfo.GetLastOrderID(machine);

                if (currentOrderID != -1)
                {
                    int index = 0;
                    for (int i = 0; i < ordersIndexes.Count; i++)
                    {
                        if (ordersIndexes[i] == currentOrderID)
                        {
                            index = i;
                            break;
                        }
                    }
                    comboBox1.SelectedIndex = index;
                    comboBox1.Enabled = false;
                    //button7.Enabled = false;
                }
                else if (lastOrderID != -1)
                {
                    int index = 0;
                    for (int i = 0; i < ordersIndexes.Count; i++)
                    {
                        if (ordersIndexes[i] == lastOrderID)
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
            else if (currentJobType == 1)
            {
                int currentIdletimeID = getInfo.GetCurrentOrderID(machine);

                if (currentIdletimeID != -1)
                {
                    await LoadIdletimeDetails(currentIdletimeID);
                    await LoadCurrentIdletimeFromDB(ShiftID, machine, currentIdletimeID);
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
                checkBox1.Enabled = true;
                checkBox1.Checked = true;
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
                    checkBox1.Enabled = true;
                }
                else
                {
                    button1.Visible = true;
                    button2.Visible = true;
                    button3.Visible = true;

                    button6.Enabled = true;

                    /*button1.Text = "Подтвердить приладку";
                    button2.Text = "Завершить приладку";
                    button3.Text = "Прервать приладку";*/

                    dateTimePicker1.Visible = true;
                    dateTimePicker2.Visible = true;
                    textBox3.Visible = true;

                    groupBox3.Visible = false;
                    
                    if (_orderRegistrationType == 0)
                    {
                        button1.Text = "Подтвердить приладку";
                        button2.Text = "Завершить приладку";
                        button3.Text = "Прервать приладку";

                        groupBox4.Visible = false;
                    }
                    else
                    {
                        button1.Text = "Подтвердить";
                        button2.Text = "Завершить";
                        button3.Text = "Прервать";

                        groupBox4.Visible = true;
                    }

                    textBox6.Enabled = true;
                    checkBox1.Enabled = false;
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
                checkBox1.Enabled = false;
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

                checkBox1.Enabled = false;
            }
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

            if (AdminCloseOrder)
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

        private async void AddOrderToDB()
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations totalMinutes = new GetDateTimeOperations();

            String orderAddedDate = DateTime.Now.ToString();
            //String machine = mashine;
            string machine = await getInfo.GetMachineFromName(comboBox3.Text);
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

                for (int i = 0; i < items.Count; i += 2)
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

        private void AddNewOrderInProgress(string machine, string executor, int shiftID, int orderIndex, string makereadyStart,
            string makereadyStop, string workStart, string workStop, int makereadyConsider, int makereadyPartComplete, string done, int counterRepeat, string note)
        {
            ValueOrdersBase valueOrders = new ValueOrdersBase();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            //string shiftStartID = shiftsBase.GetIDFromStartShift(shiftStart);

            int result = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT COUNT(*) FROM ordersInProgress WHERE ((shiftID = @shiftID AND counterRepeat = @counterRepeat) AND (orderID = @id))"
                };

                Command.Parameters.AddWithValue("@shiftID", shiftID);
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
                    string commandText = "INSERT INTO ordersInProgress (machine, executor, shiftID, orderID, timeMakereadyStart, timeMakereadyStop, timeToWorkStart, timeToWorkStop, makereadyConsider, makereadyComplete, done, counterRepeat, note) " +
                        "VALUES(@machine, @executor, @shiftID, @orderID, @makereadyStart, @makereadyStop, @workStart, @workStop, @makereadyConsider, @makereadyComplete, @done, @counterRepeat, @note)";

                    MySqlCommand Command = new MySqlCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@machine", machine); // присваиваем переменной значение
                    Command.Parameters.AddWithValue("@executor", executor);
                    Command.Parameters.AddWithValue("@shiftID", shiftID);
                    Command.Parameters.AddWithValue("@orderID", orderIndex);
                    Command.Parameters.AddWithValue("@makereadyStart", makereadyStart);
                    Command.Parameters.AddWithValue("@makereadyStop", makereadyStop);
                    Command.Parameters.AddWithValue("@workStart", workStart);
                    Command.Parameters.AddWithValue("@workStop", workStop);
                    Command.Parameters.AddWithValue("@makereadyConsider", makereadyConsider);
                    Command.Parameters.AddWithValue("@makereadyComplete", makereadyPartComplete);
                    Command.Parameters.AddWithValue("@done", done);
                    Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                    Command.Parameters.AddWithValue("@note", note);

                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }

        private void ChangeTheStateOfTheMakereadySwitch(int shiftID, int machineID, int orderID, int counterRepeat)
        {
            /*
             - если заказ первый, то активен и включен
	         - если не первый
	         -- если добавление
	         --- если есть время на приладку и сделано 0, то активен и включен
	         --- ?если нет времени на приладку или сделано больше 0, то неактивен и выключен
	         -- если уже добавлен (подтверждение/завершение)
	         --- состоянипеп получать из бд

            1. Проверить есть ли заказ в бд в работе (проверять по orderID, machineID, counterRepeat)
            1.1. Если нет, то активно и включено
            1.2. Если есть
            1.2.1. проверка есть ли для текущей смены
            1.2.1.1. если нет
            1.2.1.1.1 то, если есть время на приладку и количество сделанного до == 0, активно и включено
            1.2.1.2. если да
            1.2.1.2.1. получить состояние из бд

             */

            GetCountOfDone orderCalc = new GetCountOfDone(shiftID, orderID, counterRepeat, machineID);
            GetLeadTime leadTime = new GetLeadTime(shiftID, machineID, orderID, counterRepeat);
            ValueOrdersBase orders = new ValueOrdersBase();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();

            int makereadyTime = Convert.ToInt32(orders.GetTimeMakeready(orderID));

            int makereadySummPreviousParts = leadTime.CalculateMakereadyParts(true, false, false);

            int makereadyLastPart = makereadyTime - makereadySummPreviousParts;
            int makereadyComplete = ordersFromBase.GetMakereadyPart(shiftID, orderID, counterRepeat, machineID);

            int amountComplete = orderCalc.OrderCalculate(true, false);

            bool isThereAnOrder = ordersFromBase.IsThereAnOrder(machineID, orderID, counterRepeat);
            bool isThereAnOrderFromCurrentShift = ordersFromBase.IsThereAnOrder(machineID, orderID, counterRepeat, shiftID);

            //MessageBox.Show("Есть ли заказ? " + isThereAnOrder + ". Есть ли в этой смене? " + isThereAnOrderFromCurrentShift + "\nОстаток приладки: " + makereadyLastPart + "; Сделано: " + amountComplete);

            if (!isThereAnOrder)
            {
                checkBox1.Checked = true;
                checkBox1.Enabled = true;
            }
            else
            {
                if (!isThereAnOrderFromCurrentShift)
                {
                    if (makereadyLastPart > 0 && amountComplete == 0)
                    {
                        checkBox1.Checked = true;
                        checkBox1.Enabled = true;
                    }
                    else
                    {
                        checkBox1.Checked = false;
                        checkBox1.Enabled = false;
                    }
                }
                else
                {
                    if (makereadyComplete < 0)
                    {
                        checkBox1.Enabled = false;
                    }
                    else
                    {
                        checkBox1.Enabled = true;
                    }

                    checkBox1.Checked = Convert.ToBoolean(ordersFromBase.GetMakereadyConsider(shiftID, orderID, counterRepeat, machineID));
                }
            }
        }

        private int ManualEnterPartMakereadyComplete(int shiftID, int machine, int orderIndex, int counterRepeat, int currentTimeMakeready)
        {
            int result = -1;

            FormEnterMakereadyPart form = new FormEnterMakereadyPart(shiftID, machine, orderIndex, counterRepeat, currentTimeMakeready);
            form.ShowDialog();

            if (form.NewValue)
            {
                result = form.NewMKPart;
            }

            return result;
        }

        private async void AcceptOrderInProgressToDB()
        {
            //ValueInfoBase getInfo = new ValueInfoBase();
            //ValueOrdersBase getValue = new ValueOrdersBase();
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();
            ValueOrdersBase orders = new ValueOrdersBase();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            //имя и время начала смены сделать через параметры
            string executor = _userID.ToString();
            int shiftID = ShiftID;
            String number = textBox1.Text;
            String modification = textBox5.Text;

            String newStatus = "0";

            string machineCurrent = await infoBase.GetMachineFromName(comboBox3.Text);
            int orderID = orders.GetOrderID(machineCurrent, number, modification);

            string status = orders.GetOrderStatus(orderID);
            int counterRepeat = orders.GetCounterRepeat(orderID);
            int currentOrderID = infoBase.GetCurrentOrderID(machineCurrent);// сделать загрузку из базы в соответствии с выбранным оборудованием
            int lastOrderID = infoBase.GetLastOrderID(machineCurrent);

            GetCountOfDone orderCalc = new GetCountOfDone(shiftID, orderID, counterRepeat);
            
            //int orderInProgressID = getOrders.GetOrderInProgressID(shiftID, orderID, counterRepeat, machineCurrent);

            String makereadyStart = dateTimePicker1.Text;
            String makereadyStop = dateTimePicker2.Text;
            String workStart = dateTimePicker3.Text;
            String workStop = dateTimePicker4.Text;
            String note = textBox6.Text;
            int done = (int)numericUpDown4.Value;
            int makereadyConsider = Convert.ToInt32(checkBox1.Checked);

            GetLeadTime leadTime = new GetLeadTime(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat);

            int makereadyTime = Convert.ToInt32(orders.GetTimeMakeready(orderID));

            int currentTimeMakeready = timeOperations.DateDifferenceToMinutes(makereadyStop, makereadyStart);
            int makereadyPart = 0;

            int makereadySummPreviousParts = leadTime.CalculateMakereadyParts(true, false, false);

            int makereadyLastPart = makereadyTime - makereadySummPreviousParts;

            /*int lastMakereadyPart = -1;

            if (!IsOrderStartedEarlier(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat))
            {
                lastMakereadyPart = makereadyLastPart;
            }*/

            userBase.UpdateLastMachine(executor, await infoBase.GetMachineFromName(comboBox3.Text));

            if (status == "0") //новая запись
            {
                AddNewOrderInProgress(machineCurrent, executor, shiftID, orderID, makereadyStart, "", "", "", makereadyConsider, makereadyPart, done.ToString(), counterRepeat, note);
                infoBase.UpdateInfo(machineCurrent, 0, counterRepeat, orderID, orderID, true);
                newStatus = "1";
            }

            if (status == "1") // начата приладка
            {
                if (currentOrderID == -1)
                {
                    AddNewOrderInProgress(machineCurrent, executor, shiftID, orderID, makereadyStart, "", "", "", makereadyConsider, makereadyPart, done.ToString(), counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, 0, counterRepeat, orderID, orderID, true);
                    newStatus = "1";
                }
                else
                {
                    if (_orderRegistrationType == 0)
                    {
                        makereadyPart = ManualEnterPartMakereadyComplete(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat, currentTimeMakeready);

                        UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, makereadyStop);
                        UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyPart);
                        newStatus = status;
                    }
                    else
                    {
                        if (done > 0)
                        {
                            done += orderCalc.OrderCalculate(false, true);

                            //int lastTimeMakeready = getOrders.LastTimeForMakeready(shiftID, orderInProgressID, Convert.ToInt32(machineCurrent), orderID, counterRepeat);
                            int lastTimeMakeready = makereadyLastPart;
                            string timeMakereadyStop = timeOperations.DateTimeAmountMunutes(makereadyStart, lastTimeMakeready);
                            string timeToWorkStart = timeOperations.DateTimeAmountMunutes(timeMakereadyStop, 1);

                            UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, timeMakereadyStop);
                            UpdateData("timeToWorkStart", machineCurrent, shiftID, orderID, counterRepeat, timeToWorkStart);

                            /*UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, makereadyStop);
                            UpdateData("timeToWorkStart", machineCurrent, shiftID, orderID, counterRepeat, timeOperations.DateTimeAmountMunutes(makereadyStop, 1));*/
                            UpdateData("timeToWorkStop", machineCurrent, shiftID, orderID, counterRepeat, workStop);
                            UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyLastPart);
                            UpdateData("done", machineCurrent, shiftID, orderID, counterRepeat, done.ToString());

                            newStatus = "3";
                        }
                        else
                        {
                            makereadyPart = ManualEnterPartMakereadyComplete(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat, currentTimeMakeready);

                            UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, makereadyStop);
                            UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyPart);

                            newStatus = status;
                        }
                    }

                    if (makereadyPart == -1)
                    {
                        makereadyConsider = 0;
                    }

                    UpdateData("makereadyConsider", machineCurrent, shiftID, orderID, counterRepeat, makereadyConsider);
                    UpdateData("note", machineCurrent, shiftID, orderID, counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, 0, counterRepeat, orderID, orderID, false);
                    //убираем заказ из активных для возможности завершить смену
                }

            }

            if (status == "2") // приладка завершена
            {
                if (currentOrderID == -1)
                {
                    AddNewOrderInProgress(machineCurrent, executor, shiftID, orderID, "", "", workStart, "", 0, -1, done.ToString(), counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, 0, counterRepeat, orderID, orderID, true);

                    newStatus = "3";
                }
                else
                {
                    UpdateData("timeToWorkStart", machineCurrent, shiftID, orderID, counterRepeat, workStart);
                    //UpdateData("makereadyConsider", machineCurrent, shiftID, orderID, counterRepeat, makereadyConsider);
                    UpdateData("note", machineCurrent, shiftID, orderID, counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, 0, counterRepeat, orderID, orderID, true);

                    newStatus = "3";
                }
            }

            if (status == "3") // начата работа
            {
                if (currentOrderID == -1)
                {
                    AddNewOrderInProgress(machineCurrent, executor, shiftID, orderID, "", "", workStart, "", 0, -1, done.ToString(), counterRepeat, note);
                    infoBase.UpdateInfo(machineCurrent, 0, counterRepeat, orderID, orderID, true);

                    newStatus = status;
                }
                else
                {
                    done += orderCalc.OrderCalculate(false, true);
                    UpdateData("timeToWorkStop", machineCurrent, shiftID, orderID, counterRepeat, workStop);
                    //UpdateData("makereadyConsider", machineCurrent, shiftID, orderID, counterRepeat, makereadyConsider);
                    UpdateData("note", machineCurrent, shiftID, orderID, counterRepeat, note);
                    UpdateData("done", machineCurrent, shiftID, orderID, counterRepeat, done.ToString());
                    //infoBase.UpdateInfo(machineCurrent, counterRepeat, number, modification, number, modification, false);
                    infoBase.UpdateInfo(machineCurrent, 0, 0, -1, orderID, false);
                    //убираем заказ из активных для возможности завершить смену

                    newStatus = status;
                }
            }

            orders.SetNewStatus(orderID, newStatus);
        }

        private async void CloseOrderInProgressToDB()
        {
            ValueInfoBase infoBase = new ValueInfoBase();
            ValueOrdersBase getValue = new ValueOrdersBase();
            ValueUserBase userBase = new ValueUserBase();
            ValueOrdersBase orders = new ValueOrdersBase();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            string executor = _userID.ToString();
            int shiftID = ShiftID;
            String number = textBox1.Text;
            String modification = textBox5.Text;
            
            String newStatus = "0";

            string machineCurrent = await infoBase.GetMachineFromName(comboBox3.Text);
            int orderID = getValue.GetOrderID(machineCurrent, number, modification);

            string status = getValue.GetOrderStatus(orderID);
            int counterRepeat = getValue.GetCounterRepeat(orderID);
            int currentOrderID = infoBase.GetCurrentOrderID(machineCurrent);
            int lastOrderID = infoBase.GetLastOrderID(machineCurrent);

            //int orderInProgressID = getOrders.GetOrderInProgressID(shiftID, orderID, counterRepeat, machineCurrent);

            String makereadyStart = dateTimePicker1.Text;
            String makereadyStop = dateTimePicker2.Text;
            String workStart = dateTimePicker2.Value.AddMinutes(1).ToString("HH:mm dd.MM.yyyy");
            String workStop = dateTimePicker4.Text;
            String note = textBox6.Text;
            int done = (int)numericUpDown4.Value;

            GetLeadTime leadTime = new GetLeadTime(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat);

            int makereadyTime = Convert.ToInt32(orders.GetTimeMakeready(orderID));

            int makereadySummPreviousParts = leadTime.CalculateMakereadyParts(true, false, false);
            int makereadyLastPart = makereadyTime - makereadySummPreviousParts;

            userBase.UpdateLastMachine(executor, await infoBase.GetMachineFromName(comboBox3.Text));

            if (status == "1") // начата приладка
            {
                if (currentOrderID != -1)
                {
                    if (_orderRegistrationType == 0)
                    {
                        DialogResult dialogResult = DialogResult.No;

                        if (!AdminCloseOrder)
                            dialogResult = MessageBox.Show("Приладка завершена. Начать выполнение заказа?", "Завершение приладки", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                        if (dialogResult == DialogResult.Cancel)
                        {
                            return;
                        }

                        UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, makereadyStop);
                        UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyLastPart);
                        UpdateData("note", machineCurrent, shiftID, orderID, counterRepeat, note);

                        if (dialogResult == DialogResult.Yes)
                        {
                            UpdateData("timeToWorkStart", machineCurrent, shiftID, orderID, counterRepeat, workStart);
                            //UpdateData("note", machineCurrent, shiftStart, number, modification, counterRepeat, note);
                            infoBase.UpdateInfo(machineCurrent, 0, counterRepeat, orderID, orderID, true);
                            //убираем заказ из активных для возможности завершить смену
                            newStatus = "3";
                        }

                        if (dialogResult == DialogResult.No || AdminCloseOrder)
                        {
                            infoBase.UpdateInfo(machineCurrent, 0, counterRepeat, orderID, orderID, false);
                            //убираем заказ из активных для возможности завершить смену
                            newStatus = "2";
                        }
                    }
                    else
                    {
                        if (done > 0)
                        {
                            GetCountOfDone orderCalc = new GetCountOfDone(shiftID, orderID, counterRepeat);

                            done += orderCalc.OrderCalculate(false, true);

                            //int lastTimeMakeready = getOrders.LastTimeForMakeready(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat);
                            int lastTimeMakeready = makereadyLastPart;
                            string timeMakereadyStop = timeOperations.DateTimeAmountMunutes(makereadyStart, lastTimeMakeready);
                            string timeToWorkStart = timeOperations.DateTimeAmountMunutes(timeMakereadyStop, 1);

                            UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, timeMakereadyStop);
                            UpdateData("timeToWorkStart", machineCurrent, shiftID, orderID, counterRepeat, timeToWorkStart);

                            /*UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, makereadyStop);
                            UpdateData("timeToWorkStart", machineCurrent, shiftID, orderID, counterRepeat, timeOperations.DateTimeAmountMunutes(makereadyStop, 1));*/
                            UpdateData("timeToWorkStop", machineCurrent, shiftID, orderID, counterRepeat, workStop);
                            UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyLastPart);
                            UpdateData("done", machineCurrent, shiftID, orderID, counterRepeat, done.ToString());
                            infoBase.UpdateInfo(machineCurrent, 0, 0, -1, orderID, false);

                            newStatus = "4";
                        }
                        else
                        {
                            UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, makereadyStop);
                            UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyLastPart);
                            infoBase.UpdateInfo(machineCurrent, 0, counterRepeat, orderID, orderID, false);
                            newStatus = "2";
                        }

                        UpdateData("note", machineCurrent, shiftID, orderID, counterRepeat, note);
                    }
                }
            }
            if (status == "3") // начата склейка
            {
                if (currentOrderID != -1)
                {
                    GetCountOfDone orderCalc = new GetCountOfDone(shiftID, orderID, counterRepeat);
                    done += orderCalc.OrderCalculate(false, true);
                    UpdateData("timeToWorkStop", machineCurrent, shiftID, orderID, counterRepeat, workStop);
                    UpdateData("note", machineCurrent, shiftID, orderID, counterRepeat, note);
                    UpdateData("done", machineCurrent, shiftID, orderID, counterRepeat, done.ToString());
                    newStatus = "4";
                    infoBase.UpdateInfo(machineCurrent, 0, 0, -1, -1, false);
                }
            }

            orders.SetNewStatus(orderID, newStatus);

            Close();
        }

        private async void AbortOrderInProgressToDB()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getValue = new ValueOrdersBase();
            ValueUserBase userBase = new ValueUserBase();
            ValueOrdersBase orders = new ValueOrdersBase();
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            string executor = _userID.ToString();
            int shiftID = ShiftID;
            String number = textBox1.Text;
            String modification = textBox5.Text;
            
            String newStatus = "0";

            string machineCurrent = await getInfo.GetMachineFromName(comboBox3.Text);
            int orderID = getValue.GetOrderID(machineCurrent, number, modification);

            string status = getValue.GetOrderStatus(orderID);
            int counterRepeat = getValue.GetCounterRepeat(orderID);
            int currentOrderID = getInfo.GetCurrentOrderID(machineCurrent);
            int lastOrderID = getInfo.GetLastOrderID(machineCurrent);

            //int orderInProgressID = getOrders.GetOrderInProgressID(shiftID, orderID, counterRepeat, machineCurrent);

            String makereadyStart = dateTimePicker1.Text;
            String makereadyStop = dateTimePicker2.Text;
            String workStart = dateTimePicker3.Text;
            String workStop = dateTimePicker4.Text;
            String note = textBox6.Text;
            int done = ((int)numericUpDown4.Value);

            int makereadyConsider = Convert.ToInt32(checkBox1.Checked);

            GetLeadTime leadTime = new GetLeadTime(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat);

            int makereadyTime = Convert.ToInt32(orders.GetTimeMakeready(orderID));

            int currentTimeMakeready = timeOperations.DateDifferenceToMinutes(makereadyStop, makereadyStart);
            int makereadyPart = 0;

            int makereadySummPreviousParts = leadTime.CalculateMakereadyParts(true, false, false);
            int makereadyLastPart = makereadyTime - makereadySummPreviousParts;

            userBase.UpdateLastMachine(executor, await getInfo.GetMachineFromName(comboBox3.Text));

            if (status == "1") // начата приладка
            {
                if (currentOrderID != -1)
                {
                    if (_orderRegistrationType == 0)
                    {
                        makereadyPart = ManualEnterPartMakereadyComplete(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat, currentTimeMakeready);

                        UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, makereadyStop);
                        UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyPart);

                        newStatus = "0";
                    }
                    else
                    {
                        if (done > 0)
                        {
                            GetCountOfDone orderCalc = new GetCountOfDone(shiftID, orderID, counterRepeat);

                            done += orderCalc.OrderCalculate(false, true);

                            //int lastTimeMakeready = getOrders.LastTimeForMakeready(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat);
                            int lastTimeMakeready = makereadyLastPart;
                            string timeMakereadyStop = timeOperations.DateTimeAmountMunutes(makereadyStart, lastTimeMakeready);
                            string timeToWorkStart = timeOperations.DateTimeAmountMunutes(timeMakereadyStop, 1);

                            UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, timeMakereadyStop);
                            UpdateData("timeToWorkStart", machineCurrent, shiftID, orderID, counterRepeat, timeToWorkStart);

                            /*UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, makereadyStop);
                            UpdateData("timeToWorkStart", machineCurrent, shiftID, orderID, counterRepeat, timeOperations.DateTimeAmountMunutes(makereadyStop, 1));*/
                            UpdateData("timeToWorkStop", machineCurrent, shiftID, orderID, counterRepeat, workStop);
                            UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyLastPart);
                            UpdateData("done", machineCurrent, shiftID, orderID, counterRepeat, done.ToString());

                            newStatus = "0";
                        }
                        else
                        {
                            makereadyPart = ManualEnterPartMakereadyComplete(shiftID, Convert.ToInt32(machineCurrent), orderID, counterRepeat, currentTimeMakeready);

                            UpdateData("timeMakereadyStop", machineCurrent, shiftID, orderID, counterRepeat, makereadyStop);
                            UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyPart);

                            newStatus = "0";
                        }
                    }

                    if (makereadyPart == -1)
                    {
                        makereadyConsider = 0;
                    }

                    UpdateData("makereadyConsider", machineCurrent, shiftID, orderID, counterRepeat, makereadyConsider);
                    UpdateData("note", machineCurrent, shiftID, orderID, counterRepeat, note);
                }
            }

            if (status == "3") // начата склейка
            {
                if (currentOrderID != -1)
                {
                    GetCountOfDone orderCalc = new GetCountOfDone(shiftID, orderID, counterRepeat);
                    done += orderCalc.OrderCalculate(false, true);
                    UpdateData("timeToWorkStop", machineCurrent, shiftID, orderID, counterRepeat, workStop);
                    UpdateData("note", machineCurrent, shiftID, orderID, counterRepeat, note);
                    UpdateData("done", machineCurrent, shiftID, orderID, counterRepeat, done.ToString());
                    newStatus = "0";

                }
            }

            orders.IncrementCounterRepeat(orderID);
            orders.SetNewStatus(orderID, newStatus);
            getInfo.UpdateInfo(machineCurrent, 0, 0, -1, -1, false);
        }

        private async Task IdleAction()
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            string machine = await getInfo.GetMachineFromName(comboBox3.Text);
            int currentIdletimeID = getInfo.GetCurrentOrderID(machine);

            string nameIdletime = comboBox4.Text;
            int normTimeIdletime = (int)(idletimeNumericUpDownH.Value * 60 + idletimeNumericUpDownM.Value);
            int checkIntoWorkingOut = checkBox2.Checked ? 1 : 0;
            string note = textBox8.Text;

            string timeStartIdletime = dateTimePicker5.Text;
            string timeStopIdletime = "";

            int idIdlitimeList = await AddIdletimeToIdletimeListDB(nameIdletime, normTimeIdletime, checkIntoWorkingOut);

            if (currentIdletimeID == -1)
            {
                int idletimeID = await AddIdletimeToIdletimeDB(idIdlitimeList, normTimeIdletime, checkIntoWorkingOut, 1, note);
                await AddNewIdletimeToOrdersInProgressBase(ShiftID, machine, _userID, idletimeID, timeStartIdletime, timeStopIdletime, 0, normTimeIdletime);
                getInfo.UpdateInfo(machine, 1, 0, idletimeID, -1, false);
            }
            else
            {
                timeStopIdletime = dateTimePicker6.Text;
                int orderInProgressID = await GetIndexOrderInProgressFromIdletime(ShiftID, machine, _userID, currentIdletimeID);

                await UpdateIdletimeFromIdletimeDB(currentIdletimeID, idIdlitimeList, normTimeIdletime, checkIntoWorkingOut, 2, note);
                //await CloseIdletime();
                UpdateData("timeToWorkStop", orderInProgressID, timeStopIdletime);
                UpdateData("makereadyConsider", orderInProgressID, 1);

                getInfo.UpdateInfo(machine, 0, 0, -1, -1, false);
            }
        }

        private async Task<int> AddIdletimeToIdletimeListDB(string nameIdletime, int normTimeIdletime, int checkIntoWorkingOut)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "INSERT INTO idletimeList (name) SELECT @name WHERE NOT EXISTS (SELECT 1 FROM idletimeList WHERE name = @name)";
                //INSERT INTO idletimeList (name) SELECT 'ТО' WHERE NOT EXISTS (SELECT 1 FROM idletimeList WHERE name = 'ТО')

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@name", nameIdletime); // присваиваем переменной значение

                await Connect.OpenAsync();
                await Command.ExecuteNonQueryAsync();
                await Connect.CloseAsync();
            }
            //Сделать возврат индекса последней  добавленной записи

            int idletimeID = await GetIdletimeIDFromIdletimeListDB(nameIdletime);

            await UpdateIdletimeFromIdletimeListDB(idletimeID, normTimeIdletime, checkIntoWorkingOut);

            return idletimeID;
        }

        private async Task<int> GetIdletimeIDFromIdletimeListDB(string idIdleTimeName)
        {
            int result = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                await Connect.OpenAsync();

                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM idletimeList WHERE name = @idIdleTimeName"
                };
                Command.Parameters.AddWithValue("@idIdleTimeName", idIdleTimeName);

                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
                {
                    result = Convert.ToInt32(sqlReader["id"]);
                }

                await Connect.CloseAsync();
            }

            return result;
        }

        private async Task UpdateIdletimeFromIdletimeListDB(int idIdleTimeList, int normTime, int checkIntoWorkingOut)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE idletimeList SET defaultNormTime = @defaultNormTime, defaultCheckIntoWorkingOut = @defaultCheckIntoWorkingOut " +
                    "WHERE id = @idIdleTimeList";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@idIdleTimeList", idIdleTimeList);
                Command.Parameters.AddWithValue("@defaultNormTime", normTime);
                Command.Parameters.AddWithValue("@defaultCheckIntoWorkingOut", checkIntoWorkingOut);

                await Connect.OpenAsync();
                await Command.ExecuteNonQueryAsync();
                await Connect.CloseAsync();
            }
        }

        private async Task<int> AddIdletimeToIdletimeDB(int idIdleTimeList, int normTime, int checkIntoWorkingOut, int status, string note)
        {
            int result = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "INSERT INTO idletime (idIdleTimeList, normTime, checkIntoWorkingOut, status, note) VALUES (@idIdleTimeList, @normTime, @checkIntoWorkingOut, @status, @note); " +
                    "SELECT LAST_INSERT_ID() 'id';";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@idIdleTimeList", idIdleTimeList); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@normTime", normTime);
                Command.Parameters.AddWithValue("@checkIntoWorkingOut", checkIntoWorkingOut);
                Command.Parameters.AddWithValue("@status", status);
                Command.Parameters.AddWithValue("@note", note);

                await Connect.OpenAsync();

                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
                {
                    result = Convert.ToInt32(sqlReader["id"]);
                }

                await Connect.CloseAsync();
            }

            return result;
        }

        private async Task<int> AddNewIdletimeToOrdersInProgressBase(int shiftID, string machine, int executor, int orderID, string timeToWorkStart, string timeToWorkStop, int makereadyConsider, int makereadyComplete)
        {
            int result = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "INSERT INTO ordersInProgress (shiftID, machine, executor, typeJob, orderID, timeMakereadyStart, timeMakereadyStop, timeToWorkStart, timeToWorkStop, makereadyConsider, makereadyComplete) " +
                    "SELECT @shiftID, @machine, @executor, @typeJob, @orderID, @timeMakereadyStart, @timeMakereadyStop, @timeToWorkStart, @timeToWorkStop, @makereadyConsider, @makereadyComplete " +
                    "WHERE NOT EXISTS (SELECT 1 FROM ordersInProgress WHERE shiftID = @shiftID AND machine = @machine AND executor = @executor AND typeJob = @typeJob AND orderID = @orderID); " +
                    "SELECT LAST_INSERT_ID() 'id';";
                //INSERT INTO idletimeList (name) SELECT 'ТО' WHERE NOT EXISTS (SELECT 1 FROM idletimeList WHERE name = 'ТО')

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@shiftID", shiftID);
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@executor", executor);
                Command.Parameters.AddWithValue("@typeJob", 1);
                Command.Parameters.AddWithValue("@orderID", orderID);
                Command.Parameters.AddWithValue("@timeMakereadyStart", "");
                Command.Parameters.AddWithValue("@timeMakereadyStop", "");
                Command.Parameters.AddWithValue("@timeToWorkStart", timeToWorkStart);
                Command.Parameters.AddWithValue("@timeToWorkStop", timeToWorkStop);
                Command.Parameters.AddWithValue("@makereadyConsider", makereadyConsider);
                Command.Parameters.AddWithValue("@makereadyComplete", makereadyComplete);

                await Connect.OpenAsync();

                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
                {
                    result = Convert.ToInt32(sqlReader["id"]);
                }

                await Connect.CloseAsync();
            }

            return result;
        }

        private async Task UpdateIdletimeFromIdletimeDB(int idIdleTime, int idIdleTimeList, int normTime, int checkIntoWorkingOut, int status, string note)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE idletime SET idIdletimeList = @idIdleTimeList, normTime = @normTime, checkIntoWorkingOut = @checkIntoWorkingOut, status = @status, note = @note " +
                    "WHERE id = @idIdleTime";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@idIdleTime", idIdleTime); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@idIdleTimeList", idIdleTimeList);
                Command.Parameters.AddWithValue("@normTime", normTime);
                Command.Parameters.AddWithValue("@checkIntoWorkingOut", checkIntoWorkingOut);
                Command.Parameters.AddWithValue("@status", status);
                Command.Parameters.AddWithValue("@note", note);

                await Connect.OpenAsync();
                await Command.ExecuteNonQueryAsync();
                await Connect.CloseAsync();
            }
        }

        private async Task<int> GetIndexOrderInProgressFromIdletime(int shiftID, string machine, int user, int idletimeID)
        {
            int result = -1;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                await Connect.OpenAsync();

                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT count FROM ordersInProgress WHERE shiftID = @shiftID AND machine = @machine AND executor = @user AND typeJob = @typeJob AND orderID = @idletimeID"
                };
                Command.Parameters.AddWithValue("@shiftID", shiftID);
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@user", user);
                Command.Parameters.AddWithValue("@typeJob", 1);
                Command.Parameters.AddWithValue("@idletimeID", idletimeID);

                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                while (await sqlReader.ReadAsync())
                {
                    result = (int)sqlReader["count"];
                }

                await Connect.CloseAsync();
            }

            return result;
        }

        private void UpdateData(string nameOfColomn, string machineCurrent, int shiftID, int orderIndex, int counterRepeat, object value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET " + nameOfColomn + " = @value " +
                    "WHERE ((machine = @machineCurrent AND shiftID = @shiftID) AND (orderID = @id AND counterRepeat = @counterRepeat))";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineCurrent", machineCurrent); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@shiftID", shiftID);
                Command.Parameters.AddWithValue("@id", orderIndex);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void UpdateData(string nameOfColomn, int orderInProgressID, object value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET " + nameOfColomn + " = @value " +
                    "WHERE count = @orderInProgressID";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@orderInProgressID", orderInProgressID); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
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

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
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

        private void LoadCurrentOrderInProgressFromDB(int shiftID, int machine, int orderID, int counterRepeat)
        {
            GetLeadTime leadTime = new GetLeadTime(shiftID, machine, orderID, counterRepeat);
            GetCountOfDone orderCalc = new GetCountOfDone(shiftID, orderID, counterRepeat);
            ValueOrdersBase getValue = new ValueOrdersBase();

            string currentTime = DateTime.Now.ToString();
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

        private void LoadOrderInProgressFromDB(int shiftID, int orderID, int machine, int counterRepeat)
        {
            GetDateTimeOperations timeDif = new GetDateTimeOperations();
            GetLeadTime leadTime = new GetLeadTime(shiftID, machine, orderID, counterRepeat);
            GetCountOfDone orderCalc = new GetCountOfDone(shiftID, orderID, counterRepeat);
            GetOrdersFromBase getOrder = new GetOrdersFromBase();
            ValueOrdersBase getValue = new ValueOrdersBase();

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        string nMakereadyStart = leadTime.GetCurrentDateTime("timeMakereadyStart");
                        string nMakereadyStop = leadTime.GetCurrentDateTime("timeMakereadyStop");
                        string nWorkStart = leadTime.GetCurrentDateTime("timeToWorkStart");
                        string nWorkStop = leadTime.GetCurrentDateTime("timeToWorkStop");

                        string amountOrder = getValue.GetAmountOfOrder(orderID);

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

                        textBox6.Text = getOrder.GetNote(shiftID, orderID, counterRepeat, machine);
                        checkBox1.Checked = Convert.ToBoolean(getOrder.GetMakereadyConsider(shiftID, orderID, counterRepeat, machine));

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

        private async Task LoadOrderForEdit(int orderInProgressID)
        {
            ValueOrdersBase getOrder = new ValueOrdersBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();

            this.Text = "Детали заказа";

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        int orderID = ordersFromBase.GetOrderID(orderInProgressID);
                        int machineID = ordersFromBase.GetMachineFromOrderInProgressID(orderInProgressID);
                        int counterRepeat = ordersFromBase.GetCounterRepeatFromOrderInProgressID(orderInProgressID);

                        string modification = getOrder.GetOrderModification(orderID);
                        string number = getOrder.GetOrderNumber(orderID);

                        string strModification = "";
                        if (modification != "")
                            strModification = " (" + modification + ")";

                        int mkComplete = ordersFromBase.GetMakereadyPart(orderInProgressID);

                        if (mkComplete == -1)
                        {
                            checkBox1.Enabled = false;
                        }
                        else
                        {
                            checkBox1.Enabled = true;
                        }

                        comboBox1.Items.Add("");
                        comboBox1.Items.Add(number + ": " +
                            getOrder.GetOrderName(orderID) + strModification + " - " + Convert.ToInt32(getOrder.GetAmountOfOrder(orderID)).ToString("N0"));
                        comboBox1.SelectedIndex = 1;
                        comboBox1.Enabled = false;

                        comboBox3.Items.Add(await getInfo.GetMachineName(machineID.ToString()));
                        comboBox3.SelectedIndex = 0;
                        comboBox3.Enabled = false;

                        button2.Visible = false;
                        button3.Visible = false;

                        if (ShiftID == Form1.Info.shiftIndex || AdminMode)
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
                        LoadOrderInProgressFromDB(ShiftID, orderID, machineID, counterRepeat);

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

        private void SaveChanges(int orderInProgressID)
        {
            if (dateTimePicker1.Visible)
            {
                UpdateData("timeMakereadyStart", orderInProgressID, dateTimePicker1.Text);
            }

            if (dateTimePicker2.Visible)
            {
                UpdateData("timeMakereadyStop", orderInProgressID, dateTimePicker2.Text);
            }

            if (dateTimePicker3.Visible)
            {
                UpdateData("timeToWorkStart", orderInProgressID, dateTimePicker3.Text);
            }

            if (dateTimePicker4.Visible)
            {
                UpdateData("timeToWorkStop", orderInProgressID, dateTimePicker4.Text);
            }

            UpdateData("note", orderInProgressID, textBox6.Text);
            UpdateData("done", orderInProgressID, numericUpDown4.Value.ToString());

            UpdateData("makereadyConsider", orderInProgressID, Convert.ToInt32(checkBox1.Checked));
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

        private async void SetNewOrder(OrdersLoad order, List<string> itemsOrder)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            string machine = await infoBase.GetMachineFromName(comboBox3.Text);

            if (order.TypeJob == 0)
            {
                tabControl1.SelectTab(0);

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
            else if (order.TypeJob == 1)
            {
                tabControl1.SelectTab(1);

                comboBox4.Text = order.nameItem;

                idletimeNumericUpDownH.Value = order.workTime / 60;
                idletimeNumericUpDownM.Value = order.workTime % 60;
            }
            
        }

        private async void LoadTypes(int orderInProgressID)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getValue = new ValueOrdersBase();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();

            int orderID = ordersFromBase.GetOrderID(orderInProgressID);

            int orderIndex = -1;

            if (orderID != -1)
            {
                orderIndex = orderID;
            }
            else
            {
                orderIndex = ordersIndexes[comboBox1.SelectedIndex];
            }

            int machine = Convert.ToInt32(await getInfo.GetMachineFromName(comboBox3.Text));
            int counterRepeat = getValue.GetCounterRepeat(orderIndex);
            

            FormTypesInTheOrder form;

            form = new FormTypesInTheOrder(ShiftID,
                orderIndex,
                counterRepeat,
                machine.ToString(),
                shiftsBase.GetNameUserFromStartShift(ShiftID).ToString());

            form.ShowDialog();

            LoadTypesFromCurrentOrder(orderInProgressID);
        }

        private void LoadTypesFromCurrentOrder(int orderInProgressID)
        {
            listView1.Items.Clear();

            ValueTypesBase typeBase = new ValueTypesBase(orderInProgressID);
            
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

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadOrderDetails();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            int typeJob = tabControl1.SelectedIndex;

            if (typeJob == 0)
            {
                ValueInfoBase getInfo = new ValueInfoBase();
                ValueOrdersBase orders = new ValueOrdersBase();

                if ((ShiftID == Form1.Info.shiftIndex && _editOrder) || AdminMode)
                {
                    SaveChanges(OrderInProgressID);
                    Close();
                }
                else
                {
                    //сделать проверку статуса и в зависимости от статуса и условий выбирать
                    if (CheckNotEmptyFields() == true)
                    {
                        DialogResult result;

                        if (!CheckOrderAvailable(await getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text))
                        {
                            AddOrderToDB();
                        }
                        else if (comboBox1.SelectedIndex == 0)
                        {
                            MessageBox.Show("Заказ №" + textBox1.Text + " есть в базе, выберите из списка или проверьте введенные данные.", "Добавление заказа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        int orderIndex = orders.GetOrderID(await getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);

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
                        else if (!checkBox1.Checked && status == "0")
                        {
                            result = MessageBox.Show("Приладка не включена в выработку!\r\nНачать выполнение заказа?", "Добавление заказа", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
                        else if (numericUpDown4.Value >= numericUpDown3.Value && numericUpDown4.Value > 0 && getInfo.GetCurrentOrderID(await getInfo.GetMachineFromName(comboBox3.Text)) != -1)
                        {
                            if (status == "1" || status == "3")
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
            else if (typeJob == 1)
            {
                await IdleAction();
                Close();
            }
            
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase orders = new ValueOrdersBase();

            MessageBoxManager.Yes = "Завершить";
            MessageBoxManager.No = "Подтвердить";
            MessageBoxManager.Cancel = "Отмена";

            DialogResult result;

            int orderIndex = orders.GetOrderID(await getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);

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

        private async void button3_ClickAsync(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            AbortOrderInProgressToDB();

            string machine = await getInfo.GetMachineFromName(comboBox3.Text);
            await SelectedMachineChange(machine);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase orders = new ValueOrdersBase();

            string machine = await getInfo.GetMachineFromName(comboBox3.Text);

            int orderIndex = orders.GetOrderID(await getInfo.GetMachineFromName(comboBox3.Text), textBox1.Text, textBox5.Text);

            LoadCurrentOrderInProgressFromDB(ShiftID, Convert.ToInt32(machine), orderIndex, orders.GetCounterRepeat(orderIndex));
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

        private async void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_editOrder)
            {
                ValueInfoBase getInfo = new ValueInfoBase();

                string machine = await getInfo.GetMachineFromName(comboBox3.Text);

                await SelectedMachineChange(machine);
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

        private async void button5_Click(object sender, EventArgs e)
        {
            ValueInfoBase valueInfoBase = new ValueInfoBase();

            string machine = await valueInfoBase.GetMachineFromName(comboBox3.Text);

            FormAddTimeMkWork fm = new FormAddTimeMkWork(machine, numericUpDown1.Value, textBox2.Text);
            fm.ShowDialog();

            if(fm.NewValue)
            {
                SetNewValue(fm.ValAmount, fm.ValStamp, fm.ValMakeready, fm.ValWork);
            }
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            LoadTypes(OrderInProgressID);
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            ValueInfoBase valueInfoBase = new ValueInfoBase();

            string machine = await valueInfoBase.GetMachineFromName(comboBox3.Text);

            FormLoadOrders fm = new FormLoadOrders(machine);
            fm.ShowDialog();

            if (fm.NewValue)
            {
                SetNewOrder(fm.SetValue, fm.Types);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            
        }
    }

}

/*

ввод времени приладки и работы
возможность редактирвания начала и завершения операций

*/