using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAllOrders : Form
    {
        public FormAllOrders()
        {
            InitializeComponent();

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

        private void SetStartPeriodDTPicker()
        {
            dateTimePicker1.Value = DateTime.Now.AddMonths(-2);
        }

        private void LoadSelectedOrder(bool detailsLoad, String orderMachine, String orderNumberm, String orderModification)
        {
            FormFullListOrders form = new FormFullListOrders(detailsLoad, orderMachine, orderNumberm, orderModification);
            form.ShowDialog();
        }

        private void ShowFullOrdersForm(bool editOrder)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            FormAddNewOrder form;

            if (editOrder)
                form = new FormAddNewOrder(getInfo.GetMachineFromName(comboBox1.Text), ordersNumbers[listView1.SelectedIndices[0]].numberOfOrder, ordersNumbers[listView1.SelectedIndices[0]].modificationOfOrder);
            else
                form = new FormAddNewOrder(getInfo.GetMachineFromName(comboBox1.Text));

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

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void LoadOrdersFromBase()
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            String machine = getInfo.GetMachineFromName(comboBox1.Text);

            listView1.Items.Clear();

            ordersNumbers.Clear();
            //ordersNumbers.Add(new Order("", ""));

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
                    CommandText = @"SELECT * FROM orders WHERE " + commandLine + " AND machine = '" + getInfo.GetMachineFromName(comboBox1.Text) + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    if (sqlReader["numberOfOrder"].ToString().Contains(textBox1.Text))
                    {
                        GetCountOfDone orderCalc = new GetCountOfDone("", machine, sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), "");
                        GetLeadTime leadTimeFirst = new GetLeadTime("", sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["machine"].ToString(), "0");
                        GetLeadTime leadTimeLast = new GetLeadTime("", sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["machine"].ToString(), sqlReader["counterRepeat"].ToString());

                        ordersNumbers.Add(new Order(sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));

                        String modification = "";
                        if (sqlReader["modification"].ToString() != "")
                            modification = " (" + sqlReader["modification"].ToString() + ")";

                        ListViewItem item = new ListViewItem();

                        item.Name = sqlReader["numberOfOrder"].ToString();
                        item.Text = (index + 1).ToString();
                        item.SubItems.Add(sqlReader["numberOfOrder"].ToString() + modification);
                        item.SubItems.Add(sqlReader["nameOfOrder"].ToString());
                        item.SubItems.Add(timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeMakeready"])));
                        item.SubItems.Add(timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeToWork"])));
                        item.SubItems.Add(Convert.ToInt32(sqlReader["amountOfOrder"]).ToString("N0"));
                        item.SubItems.Add(leadTimeFirst.GetFirstValue("timeMakereadyStart").ToString());
                        item.SubItems.Add(leadTimeLast.GetLastValue("timeToWorkStop").ToString());
                        //item.SubItems.Add(orderCalc.OrderCalculate(true, true).ToString("N0"));
                        item.SubItems.Add(orderCalc.OrderFullCalculate().ToString("N0"));
                        item.SubItems.Add(ordersBase.GetOrderStatusName(getInfo.GetMachineFromName(comboBox1.Text), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));

                        listView1.Items.Add(item);

                        index++;
                    }

                }

                Connect.Close();
            }

        }

        private void SetNewStatus(String orderMachine, String numberOfOrder, String orderModification, String newStatus)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE orders SET statusOfOrder = @status " +
                    "WHERE machine = @orderMachine AND (numberOfOrder = @number AND modification = @orderModification)";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@status", newStatus);
                Command.Parameters.AddWithValue("@orderMachine", orderMachine);
                Command.Parameters.AddWithValue("@number", numberOfOrder);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void FormFullListOrders_Load(object sender, EventArgs e)
        {
            LoadMachine();
            SetStartPeriodDTPicker();
            LoadParametersFromBase();
        }

        private void FormAllOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveParameterToBase();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShowFullOrdersForm(false);
            LoadOrdersFromBase();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            if (listView1.SelectedItems.Count != 0)
                LoadSelectedOrder(true, getInfo.GetMachineFromName(comboBox1.Text), ordersNumbers[listView1.SelectedIndices[0]].numberOfOrder, ordersNumbers[listView1.SelectedIndices[0]].modificationOfOrder);
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
                LoadSelectedOrder(true, getInfo.GetMachineFromName(comboBox1.Text), ordersNumbers[listView1.SelectedIndices[0]].numberOfOrder, ordersNumbers[listView1.SelectedIndices[0]].modificationOfOrder);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowFullOrdersForm(true);
            LoadOrdersFromBase();
        }

        private void deactivateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            SetNewStatus(getInfo.GetMachineFromName(comboBox1.Text), ordersNumbers[listView1.SelectedIndices[0]].numberOfOrder, ordersNumbers[listView1.SelectedIndices[0]].modificationOfOrder, "4");
            LoadOrdersFromBase();
        }
    }
}
