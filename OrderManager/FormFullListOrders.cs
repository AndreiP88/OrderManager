﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormFullListOrders : Form
    {
        bool detailsLoad;
        int orderID;
        /*String orderrMachineLoad;
        String orderNumberLoad;
        String orderModificationLoad;
        public FormFullListOrders(bool details, String orderMachine, String orderNumber, String orderModification)
        {
            InitializeComponent();

            this.orderID = "";
            this.detailsLoad = details;
            this.orderrMachineLoad = orderMachine;
            this.orderNumberLoad = orderNumber;
            this.orderModificationLoad = orderModification;
        }*/

        public FormFullListOrders(bool details, int orderIDFromOrdersBase)
        {
            InitializeComponent();

            this.detailsLoad = details;
            this.orderID = orderIDFromOrdersBase;
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

        private void SetItemsComboBox()
        {
            DateTime dateTime = DateTime.Now;

            //comboBox1.SelectedIndex = comboBox1.FindString(dateTime.Year.ToString());
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(dateTime.Year.ToString());
            comboBox2.SelectedIndex = dateTime.Month - 1;
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
                    comboBox3.Items.Add(getInfo.GetMachineName(sqlReader["id"].ToString()));
                }

                Connect.Close();
            }

            if (comboBox3.Items.Count > 0)
                comboBox3.SelectedIndex = 0;
        }

        private void LoadYears()
        {
            List<String> years = new List<String>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT startOfShift FROM ordersInProgress"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    if (years.IndexOf(Convert.ToDateTime(sqlReader["startOfShift"]).ToString("yyyy")) == -1)
                        years.Add(Convert.ToDateTime(sqlReader["startOfShift"]).ToString("yyyy"));
                }

                Connect.Close();
            }

            for (int i = years.Count - 1; i >= 0; i--)
                comboBox1.Items.Add(years[i].ToString());
        }

        private void LoadOrdersFromBase()
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            ValueUserBase usersBase = new ValueUserBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetNumberShiftFromTimeStart getNumberShift = new GetNumberShiftFromTimeStart();

            listView1.Items.Clear();

            String tmpStartShifts = "", tmpNumberOrders = "";
            int tmpAmountOrder = 0;
            int index = 0;
            int countOrders = 0;
            int amountAllOrders = 0;

            String year = "";
            String month = "";
            String machine = "";

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                String commandLine;
                //commandLine = "strftime('%Y,%m', date(substr(startOfShift, 7, 4) || '-' || substr(startOfShift, 4, 2) || '-' || substr(startOfShift, 1, 2))) = '";
                commandLine = "DATE_FORMAT(STR_TO_DATE(startOfShift,'%d.%m.%Y %H:%i:%S'), '%Y,%m') = '";
                //commandLine = "DATE_FORMAT(startOfShift, '%Y,%m') = '";
                commandLine += comboBox1.Text + ",";
                if (comboBox2.SelectedIndex + 1 < 10)
                    commandLine += "0" + (comboBox2.SelectedIndex + 1).ToString() + "'";
                else
                    commandLine += (comboBox2.SelectedIndex + 1).ToString() + "'";

                String commandText;
                if (detailsLoad == true)
                {
                    //if (orderID != "")
                        commandText = "SELECT * FROM ordersInProgress WHERE orderID = '" + orderID + "'";
                    //else
                        //commandText = "SELECT * FROM ordersInProgress WHERE machine = '" + orderrMachineLoad + "' AND (numberOfOrder = '" + orderNumberLoad + "' AND modification = '" + orderModificationLoad + "')";
                }
                else
                    commandText = "SELECT * FROM ordersInProgress WHERE " + commandLine + " AND machine = '" + getInfo.GetMachineFromName(comboBox3.Text) + "'";

                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @commandText
                    //CommandText = @"SELECT * FROM ordersInProgress WHERE " + commandLine + " AND machine = '" + comboBox3.Text + "'"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    string orderNumber = ordersBase.GetOrderNumber((int)sqlReader["orderID"]);

                    if (orderNumber.Contains(textBox1.Text))
                    {
                        year = Convert.ToDateTime(sqlReader["startOfShift"]).ToString("yyyy");
                        month = Convert.ToDateTime(sqlReader["startOfShift"]).ToString("MMMM");
                        machine = sqlReader["machine"].ToString();

                        //отображение имени исполнителя не в каждой строке, а только в начале смены
                        //возможно сделать, как опцию
                        String date, name;
                        if (tmpStartShifts == sqlReader["startOfShift"].ToString())
                        {
                            date = "";
                            name = "";
                        }
                        else
                        {
                            date = Convert.ToDateTime(sqlReader["startOfShift"]).ToString("d");
                            date += ", " + getNumberShift.NumberShift(sqlReader["startOfShift"].ToString());
                            name = usersBase.GetNameUser(sqlReader["executor"].ToString());
                        }

                        //отображение общего количества тиража не в каждой строке, а только в первой
                        String amountOrder;
                        if (tmpAmountOrder == Convert.ToInt32(ordersBase.GetAmountOfOrder((int)sqlReader["orderID"])) && tmpNumberOrders == sqlReader["orderID"].ToString())
                        {
                            amountOrder = "";
                        }
                        else
                        {
                            amountOrder = Convert.ToInt32(ordersBase.GetAmountOfOrder((int)sqlReader["orderID"])).ToString("N0");
                        }

                        
                        string modification = ordersBase.GetOrderModification((int)sqlReader["orderID"]);

                        if (modification != "")
                            modification = " (" + modification + ")";

                        if (tmpNumberOrders != sqlReader["orderID"].ToString())
                            countOrders++;

                        tmpStartShifts = sqlReader["startOfShift"].ToString();
                        tmpNumberOrders = sqlReader["orderID"].ToString();
                        tmpAmountOrder = Convert.ToInt32(ordersBase.GetAmountOfOrder((int)sqlReader["orderID"]));

                        amountAllOrders += Convert.ToInt32(sqlReader["done"]);

                        ListViewItem item = new ListViewItem();

                        item.Name = sqlReader["orderID"].ToString();
                        item.Text = (index + 1).ToString();
                        item.SubItems.Add(date);
                        item.SubItems.Add(name);
                        item.SubItems.Add(ordersBase.GetOrderNumber((int)sqlReader["orderID"]) + modification);
                        item.SubItems.Add(ordersBase.GetOrderName((int)sqlReader["orderID"]));
                        item.SubItems.Add(timeOperations.DateDifferent(sqlReader["timeMakereadyStop"].ToString(), sqlReader["timeMakereadyStart"].ToString()));
                        item.SubItems.Add(timeOperations.DateDifferent(sqlReader["timeToWorkStop"].ToString(), sqlReader["timeToWorkStart"].ToString()));
                        item.SubItems.Add(amountOrder);
                        item.SubItems.Add(Convert.ToInt32(sqlReader["done"]).ToString("N0"));
                        item.SubItems.Add(sqlReader["note"].ToString());

                        listView1.Items.Add(item);

                        index++;
                    }

                }

                Connect.Close();
            }

            if (detailsLoad == true)
            {
                comboBox1.Text = year;
                comboBox2.Text = month;
                comboBox3.Text = getInfo.GetMachineName(machine);
            }

            label7.Text = countOrders.ToString();
            label8.Text = amountAllOrders.ToString("N0");
        }

        private void FormFullListOrders_Load(object sender, EventArgs e)
        {
            LoadParametersFromBase("fullListForm");

            if (detailsLoad == true)
            {
                this.Text = "Детали заказа";

                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;

                comboBox2.SelectedIndex = -1;

                label6.Visible = false;
                textBox1.Visible = false;

                LoadYears();
                LoadMachine();

                //LoadOrdersFromBase();
            }
            else
            {
                LoadYears();
                LoadMachine();
                SetItemsComboBox();
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadOrdersFromBase();
        }

        private void FormFullListOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveParameterToBase("fullListForm");
        }
    }
}
