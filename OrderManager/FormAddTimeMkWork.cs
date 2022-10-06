using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAddTimeMkWork : Form
    {
        internal class TimeValue
        {
            //String machine = "";
            public String orderStamp = "";
            public String name = "";
            public String mod = "";
            public String dateAddOrder = "";
            public int makeready = 0;
            public int work = 0;

            public TimeValue(string stamp, string name, string modification, string date, int mk, int wr)
            {
                this.orderStamp = stamp;
                this.name = name;
                this.mod = modification;
                this.dateAddOrder = date;
                this.makeready = mk;
                this.work = wr;
            }
        }

        string loadMachine = "";
        decimal loadAmount = 0;
        string loadStamp = "";

        List<TimeValue> value = new List<TimeValue>();

        public FormAddTimeMkWork()
        {
            InitializeComponent();
        }
        public FormAddTimeMkWork(string machine, decimal amountOfOrder, string stampOfOrder)
        {
            InitializeComponent();

            this.loadAmount = amountOfOrder;
            this.loadMachine = machine;
            this.loadStamp = stampOfOrder;

            LoadNote(loadAmount, loadStamp);

        }

        private bool newValue = false;
        private decimal amountNewValue;
        private string stampOfOrderNewValue;
        private int makereadyNewValue;
        private int workNewValue;

        public bool NewValue
        {
            get
            {
                return newValue;
            }
            set
            {
                newValue = value;
            }
        }

        public decimal ValAmount
        {
            get
            {
                return amountNewValue;
            }
            set
            {
                amountNewValue = value;
            }
        }

        public string ValStamp
        {
            get
            {
                return stampOfOrderNewValue;
            }
            set
            {
                stampOfOrderNewValue = value;
            }
        }

        public int ValMakeready
        {
            get
            {
                return makereadyNewValue;
            }
            set
            {
                makereadyNewValue = value;
            }
        }

        public int ValWork
        {
            get
            {
                return workNewValue;
            }
            set
            {
                workNewValue = value;
            }
        }

        private List<TimeValue> GetValueFromStampNumber(String machine, String orderStamp)
        {
            List<TimeValue> result = new List<TimeValue>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE machine = @machine AND orderStamp = @orderStamp"
                };
                Command.Parameters.AddWithValue("@machine", machine);
                Command.Parameters.AddWithValue("@orderStamp", orderStamp);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    int norm =  60 * Convert.ToInt32(sqlReader["amountOfOrder"]) / Convert.ToInt32(sqlReader["timeToWork"]);

                    result.Add(new TimeValue(orderStamp,
                        sqlReader["nameOfOrder"].ToString(),
                        sqlReader["modification"].ToString(),
                        sqlReader["orderAddedDate"].ToString(),
                        Convert.ToInt32(sqlReader["timeMakeready"]),
                        norm));
                }

                Connect.Close();
            }

            return result;
        }

        private void LoadNote(decimal amountOfOrder, string stampOfOrder)
        {
            numericUpDown1.Value = amountOfOrder;
            textBox2.Text = stampOfOrder;

            if (stampOfOrder != "")
            {
                LoadTimeFromStamp(stampOfOrder);
            }
        }

        private void Clear()
        {
            label6.Text = "";
            label8.Text = "";
            label10.Text = "";

            value.Clear();

            comboBox1.Items.Clear();
        }

        private void LoadTimeFromStamp(string stamp)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            
            Clear();

            value = GetValueFromStampNumber(loadMachine, stamp);

            if (value.Count > 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }

            string mkready = "";

            for (int i = 0; i < value.Count; i++)
            {
                mkready = timeOperations.TotalMinutesToHoursAndMinutesStr(value[i].makeready);
                string modStr = "";

                if (value[i].mod != "")
                    modStr = " (" + value[i].mod + ") ";
                else
                    modStr = "";

                DateTime dateTime = Convert.ToDateTime(value[i].dateAddOrder);

                comboBox1.Items.Add(value[i].name + modStr + ": " + dateTime.ToString("Y") + ". Приладка: " + mkready + ", Норма: " + value[i].work.ToString("N0") + "/ч");
            }

            label4.Text = "Найдено вариантов: " + value.Count;

            if (value.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void EnabledButton()
        {
            if (numericUpDown21.Value != 0 || numericUpDown22.Value != 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            newValue = false;

            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            newValue = true;

            ValAmount = 0;
            ValStamp = "";
            ValMakeready = 0;
            ValWork = 0;

            if (tabControl1.SelectedIndex == 0)
            {
                int mkready = value[comboBox1.SelectedIndex].makeready;
                int norm = value[comboBox1.SelectedIndex].work;
                decimal amount = numericUpDown1.Value;

                int timeToWork = TimeToWork((int)amount, norm);

                ValAmount = numericUpDown1.Value;
                ValStamp = textBox2.Text;
                ValMakeready = mkready;
                ValWork = timeToWork;
            }
            if (tabControl1.SelectedIndex == 1)
            {
                int totallTime = (int)numericUpDown21.Value * 60 + (int)numericUpDown22.Value;
                int mkreadyTime = (int)numericUpDown23.Value * 60 + (int)numericUpDown24.Value;

                if (totallTime > mkreadyTime)
                {
                    int workTime = totallTime - mkreadyTime;

                    ValAmount = loadAmount;
                    ValStamp = loadStamp;
                    ValMakeready = mkreadyTime;
                    ValWork = workTime;
                }
                else
                {
                    MessageBox.Show("Время приладки не может превышать общее время!", "Ошибка");
                    return;
                }
                
            }
            

            this.Hide();

            /*SaveNote();
            Close();*/
        }

        private void FormPrivateNote_Load(object sender, EventArgs e)
        {
            //LoadNote(loadStartOfShift);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadTimeFromStamp(textBox2.Text);
        }

        private int TimeToWork(int amount, int norm)
        {
            int timeToWork;

            if (norm > 0)
                timeToWork = 60 * amount / norm;
            else
                timeToWork = 0;

            return timeToWork;
        }

        private void UpdateShowInfo()
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            string mkready = timeOperations.TotalMinutesToHoursAndMinutesStr(value[comboBox1.SelectedIndex].makeready);
            int norm = value[comboBox1.SelectedIndex].work;
            decimal amount = numericUpDown1.Value;

            int timeToWork = TimeToWork((int)amount, norm);

            label6.Text = mkready;
            label8.Text = timeOperations.TotalMinutesToHoursAndMinutesStr(timeToWork);
            label10.Text = norm.ToString("N0") + "/ч";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateShowInfo();
        }

        private void numericUpDown1_Click(object sender, EventArgs e)
        {
            numericUpDown1.Select(0, numericUpDown1.Text.Length);
        }

        private void numericUpDown21_Click(object sender, EventArgs e)
        {
            numericUpDown21.Select(0, numericUpDown21.Text.Length);
        }

        private void numericUpDown22_Click(object sender, EventArgs e)
        {
            numericUpDown22.Select(0, numericUpDown22.Text.Length);
        }

        private void numericUpDown23_Click(object sender, EventArgs e)
        {
            numericUpDown23.Select(0, numericUpDown23.Text.Length);
        }

        private void numericUpDown24_Click(object sender, EventArgs e)
        {
            numericUpDown24.Select(0, numericUpDown24.Text.Length);
        }

        private void numericUpDown21_ValueChanged(object sender, EventArgs e)
        {
            EnabledButton();
        }

        private void numericUpDown22_ValueChanged(object sender, EventArgs e)
        {
            EnabledButton();
        }

        private void numericUpDown23_ValueChanged(object sender, EventArgs e)
        {
            EnabledButton();
        }

        private void numericUpDown24_ValueChanged(object sender, EventArgs e)
        {
            EnabledButton();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(value.Count > 0)
                UpdateShowInfo();
        }
    }
}
