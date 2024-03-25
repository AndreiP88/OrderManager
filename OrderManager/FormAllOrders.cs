using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using static OrderManager.DataBaseReconnect;

namespace OrderManager
{
    public partial class FormAllOrders : Form
    {
        int DefaultMachine;

        public FormAllOrders(int defaultMachine = -1)
        {
            InitializeComponent();

            DefaultMachine = defaultMachine;
        }

        class Order
        {
            public string numberOfOrder;
            public string modificationOfOrder;

            public Order(String number, string modification)
            {
                numberOfOrder = number;
                modificationOfOrder = modification;
            }
        }

        List<int> ordersIndexes = new List<int>();

        private void SetStartPeriodDTPicker()
        {
            dateTimePicker1.Value = DateTime.Now.AddMonths(-2);
        }

        private void LoadSelectedOrderFromID(int id)
        {
            FormFullListOrders form = new FormFullListOrders(true, id);
            form.ShowDialog();
        }

        private async void ShowFullOrdersForm(bool editOrder)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            FormAddNewOrder form;

            if (editOrder)
                form = new FormAddNewOrder(await getInfo.GetMachineFromName(comboBox1.Text), ordersIndexes[listView1.SelectedIndices[0]]);
            else
                form = new FormAddNewOrder(await getInfo.GetMachineFromName(comboBox1.Text));

            form.ShowDialog();
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

        private void SaveParameterToBase()
        {
            ValueSettingsBase setting = new ValueSettingsBase();

            if (Form1.Info.nameOfExecutor != "")
                setting.UpdateParameterLine(Form1.Info.nameOfExecutor, "allOrdersForm", GetParametersLine());
            else
                setting.UpdateParameterLine("0", "allOrdersForm", GetParametersLine());
        }

        private void LoadParametersFromBase()
        {
            ValueSettingsBase getSettings = new ValueSettingsBase();

            if (Form1.Info.nameOfExecutor != "")
                ApplyParameterLine(getSettings.GetParameterLine(Form1.Info.nameOfExecutor, "allOrdersForm"));
            else
                ApplyParameterLine(getSettings.GetParameterLine("0", "allOrdersForm"));
        }

        private async Task LoadMachineOLD()
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
                    comboBox1.Items.Add(await getInfo.GetMachineName(sqlReader["id"].ToString()));
                }

                Connect.Close();
            }

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
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
                            ValueInfoBase getInfo = new ValueInfoBase();

                            int indexDeafaultMachineItem = -1;

                            using (MySqlConnection Connect = DBConnection.GetDBConnection())
                            {
                                await Connect.OpenAsync();
                                MySqlCommand Command = new MySqlCommand
                                {
                                    Connection = Connect,
                                    CommandText = @"SELECT DISTINCT id FROM machines"
                                };
                                DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                                while (await sqlReader.ReadAsync()) // считываем и вносим в комбобокс список заголовков
                                {
                                    if (token.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    int idMachine = (int)sqlReader["id"];

                                    Invoke(new Action(async () =>
                                    {
                                        comboBox1.Items.Add(await getInfo.GetMachineName(idMachine.ToString()));

                                        if (DefaultMachine == idMachine)
                                            indexDeafaultMachineItem = comboBox1.Items.Count - 1;
                                    }));
                                }

                                Connect.Close();
                            }
                            Invoke(new Action(() =>
                            {
                                if (comboBox1.Items.Count > 0)
                                {
                                    if (indexDeafaultMachineItem != -1 && indexDeafaultMachineItem < comboBox1.Items.Count)
                                    {
                                        comboBox1.SelectedIndex = indexDeafaultMachineItem;
                                    }
                                    else
                                    {
                                        comboBox1.SelectedIndex = 0;
                                    }
                                }
                            }));

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

        private void LoadOrdersFromBase()
        {

        }

        private async Task<List<string>> LoadIndexesOrdersFromBase(string key)
        {
            List<string> result = new List<string>();

            ValueInfoBase getInfo = new ValueInfoBase();

            listView1.Items.Clear();

            ordersIndexes.Clear();

            int index = 0;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                String commandLine;
                //commandLine = "strftime('%Y-%m-%d 00:00:00', date(substr(orderAddedDate, 7, 4) || '-' || substr(orderAddedDate, 4, 2) || '-' || substr(orderAddedDate, 1, 2))) >= '";
                commandLine = "DATE_FORMAT(STR_TO_DATE(orderAddedDate,'%d.%m.%Y %H:%i:%S'), '%Y-%m-%d 00:00:00') >= '";
                commandLine += dateTimePicker1.Value.ToString("yyyy-MM-dd 00:00:00") + "'";

                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE " + commandLine + " AND machine = '" + await getInfo.GetMachineFromName(comboBox1.Text) + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    if (sqlReader["numberOfOrder"].ToString().Contains(key))
                    {
                        result.Add(sqlReader["count"].ToString());

                        ordersIndexes.Add((int)sqlReader["count"]);

                        String modification = "";
                        if (sqlReader["modification"].ToString() != "")
                            modification = " (" + sqlReader["modification"].ToString() + ")";

                        ListViewItem item = new ListViewItem();

                        item.Name = sqlReader["count"].ToString();
                        item.Text = (index + 1).ToString();
                        item.SubItems.Add(sqlReader["numberOfOrder"].ToString() + modification);
                        item.SubItems.Add(sqlReader["nameOfOrder"].ToString());
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        //item.SubItems.Add(orderCalc.OrderCalculate(true, true).ToString("N0"));
                        item.SubItems.Add("");
                        item.SubItems.Add("");

                        listView1.Items.Add(item);

                        index++;
                    }
                }

                Connect.Close();
            }
            return result;
        }

        private async Task LoadOrdersFromTheKey(string key)
        {
            List<string> indexes = new List<string>(await LoadIndexesOrdersFromBase(key));

            StartLoading(indexes);
        }

        CancellationTokenSource cancelTokenSource;

        private void StartLoading(List<string> indexes)
        {
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
            }

            cancelTokenSource = new CancellationTokenSource();

            //Task task = new Task(() => LoadUsersFromBase(token, date));
            Task task = new Task(() => LoadOrdersDetailsFromBase(cancelTokenSource.Token, indexes), cancelTokenSource.Token);
            task.Start();
        }

        private void LoadOrdersDetailsFromBase(CancellationToken token, List<string> indexes)
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            
            for (int i = 0; i < indexes.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    Connect.Open();
                    MySqlCommand Command = new MySqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT * FROM orders WHERE count = '" + indexes[i] + "'"
                    };
                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                    {
                        GetCountOfDone orderCalc = new GetCountOfDone(-1, (int)sqlReader["count"], 0);
                        GetLeadTime leadTimeFirst = new GetLeadTime(-1, (int)sqlReader["machine"], (int)sqlReader["count"], 0);
                        GetLeadTime leadTimeLast = new GetLeadTime(-1, (int)sqlReader["machine"], (int)sqlReader["count"], (int)sqlReader["counterRepeat"]);


                        Invoke(new Action(() =>
                        {
                            int index = listView1.Items.IndexOfKey(indexes[i]);

                            if (index >= 0)
                            {
                                ListViewItem item = listView1.Items[index];
                                if (item != null)
                                {
                                    item.SubItems[3].Text = timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeMakeready"]));
                                    item.SubItems[4].Text = timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeToWork"]));
                                    item.SubItems[5].Text = Convert.ToInt32(sqlReader["amountOfOrder"]).ToString("N0");
                                    item.SubItems[6].Text = leadTimeFirst.GetFirstValue("timeMakereadyStart").ToString();
                                    item.SubItems[7].Text = leadTimeLast.GetLastValue("timeToWorkStop").ToString();
                                    //item.SubItems.Add(orderCalc.OrderCalculate(true, true).ToString("N0"));
                                    item.SubItems[8].Text = orderCalc.OrderFullCalculate().ToString("N0");
                                    item.SubItems[9].Text = ordersBase.GetOrderStatusName((int)sqlReader["count"]);
                                }
                            }
                        }));
                    }

                    Connect.Close();
                }
            }
        }

        private void SetNewStatus(int orderIndex, string newStatus)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE orders SET statusOfOrder = @status " +
                    "WHERE count = @id";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@status", newStatus);
                Command.Parameters.AddWithValue("@id", orderIndex);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private async void FormFullListOrders_Load(object sender, EventArgs e)
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new CancellationTokenSource();

            await LoadMachine(cancelTokenSource.Token);
            SetStartPeriodDTPicker();
            LoadParametersFromBase();
        }

        private void FormAllOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveParameterToBase();
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
            await LoadOrdersFromTheKey(textBox1.Text);
        }

        private async void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
            await LoadOrdersFromTheKey(textBox1.Text);
        }

        private async void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
            await LoadOrdersFromTheKey(textBox1.Text);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ShowFullOrdersForm(false);
            LoadOrdersFromBase();
            await LoadOrdersFromTheKey(textBox1.Text);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            if (listView1.SelectedItems.Count != 0)
            {
                LoadSelectedOrderFromID(Convert.ToInt32(listView1.Items[listView1.SelectedIndices[0]].Name));
                //LoadSelectedOrder(true, getInfo.GetMachineFromName(comboBox1.Text), ordersNumbers[listView1.SelectedIndices[0]].numberOfOrder, ordersNumbers[listView1.SelectedIndices[0]].modificationOfOrder);
            }
                
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = listView1.SelectedItems.Count == 0;
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            if (listView1.SelectedItems.Count != 0)
            {
                LoadSelectedOrderFromID(Convert.ToInt32(listView1.Items[listView1.SelectedIndices[0]].Name));
                //LoadSelectedOrder(true, getInfo.GetMachineFromName(comboBox1.Text), ordersNumbers[listView1.SelectedIndices[0]].numberOfOrder, ordersNumbers[listView1.SelectedIndices[0]].modificationOfOrder);
            }
                
        }

        private async void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowFullOrdersForm(true);
            LoadOrdersFromBase();
            await LoadOrdersFromTheKey(textBox1.Text);
        }

        private async void deactivateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            SetNewStatus(ordersIndexes[listView1.SelectedIndices[0]], "4");
            LoadOrdersFromBase();
            await LoadOrdersFromTheKey(textBox1.Text);
        }
    }
}
