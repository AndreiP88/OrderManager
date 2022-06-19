﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormDetailsStatistic : Form
    {
        bool adminMode;
        String dataBase;
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";

        public FormDetailsStatistic(bool aMode, String dBase)
        {
            InitializeComponent();

            this.adminMode = aMode;
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        bool thJob = false;
        bool calculateNullShiftsFromUser = false;

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
            SetUpdateSettingsValue setting = new SetUpdateSettingsValue(dataBase);

            if (Form1.Info.nameOfExecutor != "")
                setting.UpdateParameterLine(Form1.Info.nameOfExecutor, nameForm, GetParametersLine());
            else
                setting.UpdateParameterLine("0", nameForm, GetParametersLine());
        }

        private void LoadParametersFromBase(String nameForm)
        {
            GetValueFromSettingsBase getSettings = new GetValueFromSettingsBase(dataBase);

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

        private void LoadYears()
        {
            List<String> years = new List<String>();

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT startShift FROM shifts"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    if (years.IndexOf(Convert.ToDateTime(sqlReader["startShift"]).ToString("yyyy")) == -1)
                        years.Add(Convert.ToDateTime(sqlReader["startShift"]).ToString("yyyy"));
                }

                Connect.Close();
            }

            for (int i = years.Count - 1; i >= 0; i--)
                comboBox1.Items.Add(years[i].ToString());
        }

        private void ClearAll()
        {
            listView1.Items.Clear();

            label7.Text = "";
            label8.Text = "";
            label9.Text = "";

            label13.Text = "";
            label14.Text = "";
            label15.Text = "";
        }

        private void StartLoading()
        {
            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1)
            {
                ClearAll();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex);

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;

                Task task = new Task(() => LoadUsersFromBase(token, date));

                if (thJob == true)
                {
                    cancelTokenSource.Cancel();
                }
                else
                {
                    //thJob = true;
                    task.Start();
                }
            }

        }

        private void LoadUsersFromBase(CancellationToken token, DateTime date)
        {
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
            }));

            while (!token.IsCancellationRequested)
            {
                thJob = true;

                int fullCountShifts = 0;
                int fullTimeShifts = 0;
                int fullCountOrders = 0;
                int fullCountMakeready = 0;
                int fullAmountOrders = 0;
                int fullTimeWorkingOut = 0;
                float fullPercentWorkingOut = 0;
                int countActiveUser = 0;

                using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                {
                    Connect.Open();
                    SQLiteCommand Command = new SQLiteCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT * FROM users WHERE activeUser = 'True'"
                    };
                    SQLiteDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                    {
                        GetShiftsFromBase getShifts = new GetShiftsFromBase(dataBase, sqlReader["id"].ToString());

                        List<ShiftsDetails> shiftsDetails = (List<ShiftsDetails>)getShifts.LoadShiftsFromBase(date, "").Item2;

                        fullCountShifts += shiftsDetails[shiftsDetails.Count - 1].countShifts;
                        fullTimeShifts += shiftsDetails[shiftsDetails.Count - 1].allTimeShift;
                        fullCountOrders += shiftsDetails[shiftsDetails.Count - 1].countOrdersShift;
                        fullCountMakeready += shiftsDetails[shiftsDetails.Count - 1].countMakereadyShift;
                        fullAmountOrders += shiftsDetails[shiftsDetails.Count - 1].amountAllOrdersShift;
                        fullTimeWorkingOut += shiftsDetails[shiftsDetails.Count - 1].allTimeWorkingOutShift;
                        fullPercentWorkingOut += shiftsDetails[shiftsDetails.Count - 1].percentWorkingOutShift;

                        if (calculateNullShiftsFromUser)
                            countActiveUser++;
                        else
                            if (shiftsDetails[shiftsDetails.Count - 1].countShifts != 0)
                                countActiveUser++;

                        Invoke(new Action(() =>
                        {
                            ListViewItem item = new ListViewItem();

                            item.Name = sqlReader["id"].ToString();
                            item.Text = (listView1.Items.Count + 1).ToString();
                            item.SubItems.Add(getUser.GetNameUser(sqlReader["id"].ToString()));
                            item.SubItems.Add(shiftsDetails[shiftsDetails.Count - 1].countShifts.ToString());
                            item.SubItems.Add(dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].allTimeShift));
                            item.SubItems.Add(shiftsDetails[shiftsDetails.Count - 1].countOrdersShift.ToString() + "/" + shiftsDetails[shiftsDetails.Count - 1].countMakereadyShift.ToString());
                            item.SubItems.Add(shiftsDetails[shiftsDetails.Count - 1].amountAllOrdersShift.ToString("N0"));
                            item.SubItems.Add(dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].allTimeWorkingOutShift));
                            item.SubItems.Add(shiftsDetails[shiftsDetails.Count - 1].percentWorkingOutShift.ToString("N1") + "%");

                            listView1.Items.Add(item);
                        }));

                        Invoke(new Action(() =>
                        {
                            label7.Text = fullCountShifts.ToString("N0");
                            label8.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(fullTimeShifts);
                            label9.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(fullTimeWorkingOut);

                            label13.Text = fullCountOrders.ToString() + "/" + fullCountMakeready.ToString();
                            label14.Text = fullAmountOrders.ToString("N0");
                            label15.Text = (fullPercentWorkingOut / countActiveUser).ToString("N1") + "%";
                        }));

                    }

                    Connect.Close();
                }

                thJob = false;
                break;
            }

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
            }));
        }

        private void LoadShiftdetails(String dBase, String user, int yearCur, int monthCur)
        {
            FormShiftsDetails form = new FormShiftsDetails(adminMode, dBase, user, yearCur, monthCur);
            form.ShowDialog();
        }

        private void FormShiftsDetails_Load(object sender, EventArgs e)
        {
            LoadParametersFromBase("statisticForm");
            LoadYears();
            SetItemsComboBox();
            //StartLoading();
            //LoadShiftsFromBase();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartLoading();
            //LoadShiftsFromBase();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartLoading();
            //LoadShiftsFromBase();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                LoadShiftdetails(dataBase, listView1.SelectedItems[0].Name, Convert.ToInt32(comboBox1.Text), comboBox2.SelectedIndex);
            }
        }

        private void FormShiftsDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveParameterToBase("statisticForm");
        }
    }
}
