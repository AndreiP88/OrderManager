using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.FormAddTimeMkWork;

namespace OrderManager
{
    public partial class FormNormOrders : Form
    {
        public FormNormOrders()
        {
            InitializeComponent();

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
        private void FormFullListOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            //SaveParameterToBase("fullListForm");
        }

        private async Task GetValueFromStampNumber(String orderStamp)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase valueInfo = new ValueInfoBase();
            
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM orders WHERE orderStamp = @orderStamp"
                };
                Command.Parameters.AddWithValue("@orderStamp", orderStamp);
                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    int norm = 60 * Convert.ToInt32(sqlReader["amountOfOrder"]) / Convert.ToInt32(sqlReader["timeToWork"]);

                    ListViewItem item = new ListViewItem();

                    item.Name = orderStamp;
                    item.Text = (listView1.Items.Count + 1).ToString();
                    item.SubItems.Add(await valueInfo.GetMachineName(sqlReader["machine"].ToString()));
                    item.SubItems.Add(sqlReader["nameOfOrder"].ToString());
                    item.SubItems.Add(sqlReader["modification"].ToString());
                    item.SubItems.Add(Convert.ToDateTime(sqlReader["orderAddedDate"]).ToString("Y"));
                    item.SubItems.Add(timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeMakeready"])));
                    item.SubItems.Add(norm.ToString("N0"));

                    listView1.Items.Add(item);

                }

                Connect.Close();
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            await GetValueFromStampNumber(textBox1.Text);
        }
    }
}
