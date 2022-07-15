using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormFullListOrders : Form
    {
        String dataBase;
        bool detailsLoad;
        String orderrMachineLoad;
        String orderNumberLoad;
        String orderModificationLoad;
        public FormFullListOrders(String dBase, bool details, String orderMachine, String orderNumber, String orderModification)
        {
            InitializeComponent();

            this.dataBase = dBase;
            this.detailsLoad = details;
            this.orderrMachineLoad = orderMachine;
            this.orderNumberLoad = orderNumber;
            this.orderModificationLoad = orderModification;
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
            ValueSettingsBase setting = new ValueSettingsBase(dataBase);

            if (Form1.Info.nameOfExecutor != "")
                setting.UpdateParameterLine(Form1.Info.nameOfExecutor, nameForm, GetParametersLine());
            else
                setting.UpdateParameterLine("0", nameForm, GetParametersLine());
        }

        private void LoadParametersFromBase(String nameForm)
        {
            ValueSettingsBase getSettings = new ValueSettingsBase(dataBase);

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
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);

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

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT startOfShift FROM ordersInProgress"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

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
            ValueOrdersBase ordersBase = new ValueOrdersBase(dataBase);
            ValueInfoBase getInfo = new ValueInfoBase(dataBase);
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

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                ValueUserBase usersBase = new ValueUserBase(dataBase);

                String commandLine;
                commandLine = "strftime('%Y,%m', date(substr(startOfShift, 7, 4) || '-' || substr(startOfShift, 4, 2) || '-' || substr(startOfShift, 1, 2))) = '";
                commandLine += comboBox1.Text + ",";
                if (comboBox2.SelectedIndex + 1 < 10)
                    commandLine += "0" + (comboBox2.SelectedIndex + 1).ToString() + "'";
                else
                    commandLine += (comboBox2.SelectedIndex + 1).ToString() + "'";

                String commandText;
                if (detailsLoad == true)
                    commandText = "SELECT * FROM ordersInProgress WHERE machine = '" + orderrMachineLoad + "' AND (numberOfOrder = '" + orderNumberLoad + "' AND modification = '" + orderModificationLoad + "')";
                else
                    commandText = "SELECT * FROM ordersInProgress WHERE " + commandLine + " AND machine = '" + getInfo.GetMachineFromName(comboBox3.Text) + "'";

                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @commandText
                    //CommandText = @"SELECT * FROM ordersInProgress WHERE " + commandLine + " AND machine = '" + comboBox3.Text + "'"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (sqlReader["numberOfOrder"].ToString().Contains(textBox1.Text))
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
                        if (tmpAmountOrder == Convert.ToInt32(ordersBase.GetAmountOfOrder(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString())) && tmpNumberOrders == sqlReader["numberOfOrder"].ToString() + ", " + sqlReader["modification"].ToString())
                        {
                            amountOrder = "";
                        }
                        else
                        {
                            amountOrder = Convert.ToInt32(ordersBase.GetAmountOfOrder(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString())).ToString("N0");
                        }

                        String modification = "";
                        if (sqlReader["modification"].ToString() != "")
                            modification = " (" + sqlReader["modification"].ToString() + ")";

                        if (tmpNumberOrders != sqlReader["numberOfOrder"].ToString() + ", " + sqlReader["modification"].ToString())
                            countOrders++;

                        tmpStartShifts = sqlReader["startOfShift"].ToString();
                        tmpNumberOrders = sqlReader["numberOfOrder"].ToString() + ", " + sqlReader["modification"].ToString();
                        tmpAmountOrder = Convert.ToInt32(ordersBase.GetAmountOfOrder(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));

                        amountAllOrders += Convert.ToInt32(sqlReader["done"]);

                        ListViewItem item = new ListViewItem();

                        item.Name = sqlReader["numberOfOrder"].ToString();
                        item.Text = (index + 1).ToString();
                        item.SubItems.Add(date);
                        item.SubItems.Add(name);
                        item.SubItems.Add(sqlReader["numberOfOrder"].ToString() + modification);
                        item.SubItems.Add(ordersBase.GetOrderName(sqlReader["machine"].ToString(), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));
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
