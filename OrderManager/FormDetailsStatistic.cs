﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.DataBaseReconnect;

namespace OrderManager
{
    public partial class FormDetailsStatistic : Form
    {
        bool adminMode;

        public FormDetailsStatistic(bool aMode)
        {
            InitializeComponent();

            this.adminMode = aMode;
        }

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
            ValueCategory category = new ValueCategory();

            List<String> categoryes = new List<String>(category.GetCategoryesList());

            comboBox3.Items.AddRange(categoryes.ToArray());

            DateTime dateTime = DateTime.Now;

            //comboBox1.SelectedIndex = comboBox1.FindString(dateTime.Year.ToString());
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(dateTime.Year.ToString());
            comboBox2.SelectedIndex = dateTime.Month - 1;

            if (comboBox3.Items.Count > 0)
            {
                comboBox3.SelectedIndex = 0;
            }
        }

        private void LoadYears()
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            List<String> years = shiftsBase.LoadYears();

            comboBox1.Items.AddRange(years.ToArray());

            /*for (int i = years.Count - 1; i >= 0; i--)
                comboBox1.Items.Add(years[i].ToString());*/
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

        CancellationTokenSource cancelTokenSource;
        private void StartLoading()
        {
            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1 && comboBox3.SelectedIndex != -1)
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                ClearAll();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex);

                Task task = new Task(() => LoadUsersFromBase(cancelTokenSource.Token, date), cancelTokenSource.Token);
                //LoadUsersFromBase(cancelTokenSource.Token, date);

                task.Start();
            }
        }

        private async void LoadUsersFromBase(CancellationToken token, DateTime date)
        {
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            ValueUserBase getUser = new ValueUserBase();
            ValueCategory categoryValue = new ValueCategory();

            string cLine = " WHERE activeUser = 'True'";

            string category = "";

            int fullCountShifts = 0;
            int fullTimeShifts = 0;
            int fullCountOrders = 0;
            int fullCountMakeready = 0;
            int fullAmountOrders = 0;
            int fullTimeWorkingOut = 0;
            float fullPercentWorkingOut = 0;
            int countActiveUser = 0;

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
                            category = categoryValue.GetCategoryFromName(comboBox3.Text);

                            listView1.Items.Clear();
                        }));

                        using (MySqlConnection Connect = DBConnection.GetDBConnection())
                        {
                            Connect.Open();
                            MySqlCommand Command = new MySqlCommand
                            {
                                Connection = Connect,
                                CommandText = @"SELECT * FROM users" + cLine
                            };
                            DbDataReader sqlReader = Command.ExecuteReader();

                            while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                            {
                                if (token.IsCancellationRequested)
                                {
                                    break;
                                }

                                if (getUser.CategoryForUser(sqlReader["id"].ToString(), category))
                                {
                                    ListViewItem item = new ListViewItem();

                                    item.Name = sqlReader["id"].ToString();
                                    item.Text = (listView1.Items.Count + 1).ToString();
                                    item.SubItems.Add(getUser.GetNameUser(sqlReader["id"].ToString()));
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");

                                    Invoke(new Action(() =>
                                    {
                                        listView1.Items.Add(item);
                                    }));
                                }
                            }
                            Connect.Close();
                        }

                        using (MySqlConnection Connect = DBConnection.GetDBConnection())
                        {
                            Connect.Open();
                            MySqlCommand Command = new MySqlCommand
                            {
                                Connection = Connect,
                                CommandText = @"SELECT * FROM users" + cLine
                            };
                            DbDataReader sqlReader = Command.ExecuteReader();

                            while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                            {
                                if (token.IsCancellationRequested)
                                {
                                    break;
                                }

                                if (getUser.CategoryForUser(sqlReader["id"].ToString(), category))
                                {
                                    GetShiftsFromBase getShifts = new GetShiftsFromBase(sqlReader["id"].ToString());

                                    ShiftsDetails shiftsDetails = await getShifts.LoadCurrentDateShiftsDetails(date, category, token);

                                    fullCountShifts += shiftsDetails.countShifts;
                                    fullTimeShifts += shiftsDetails.allTimeShift;
                                    fullCountOrders += shiftsDetails.countOrdersShift;
                                    fullCountMakeready += shiftsDetails.countMakereadyShift;
                                    fullAmountOrders += shiftsDetails.amountAllOrdersShift;
                                    fullTimeWorkingOut += shiftsDetails.allTimeWorkingOutShift;
                                    fullPercentWorkingOut += shiftsDetails.percentWorkingOutShift;

                                    if (calculateNullShiftsFromUser)
                                        countActiveUser++;
                                    else
                                        if (shiftsDetails.countShifts != 0)
                                        countActiveUser++;

                                    if (token.IsCancellationRequested)
                                    {
                                        break;
                                    }

                                    Invoke(new Action(() =>
                                    {
                                        int index = listView1.Items.IndexOfKey(sqlReader["id"].ToString());

                                        if (index >= 0)
                                        {
                                            ListViewItem item = listView1.Items[index];
                                            if (item != null)
                                            {
                                                item.SubItems[2].Text = shiftsDetails.countShifts.ToString();
                                                item.SubItems[3].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails.shiftsWorkingTime);
                                                item.SubItems[4].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails.allTimeShift);
                                                item.SubItems[5].Text = shiftsDetails.countOrdersShift.ToString() + "/" + shiftsDetails.countMakereadyShift.ToString();
                                                item.SubItems[6].Text = shiftsDetails.amountAllOrdersShift.ToString("N0");
                                                item.SubItems[7].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails.allTimeWorkingOutShift);
                                                item.SubItems[8].Text = shiftsDetails.percentWorkingOutShift.ToString("P1");

                                                if (shiftsDetails.percentWorkingOutShift >= 0.8)
                                                {
                                                    item.ForeColor = Color.SeaGreen;
                                                }
                                                else
                                                {
                                                    item.ForeColor = Color.DarkRed;
                                                }
                                            }
                                        }

                                        label7.Text = fullCountShifts.ToString("N0");
                                        label8.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(fullTimeShifts);
                                        label9.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(fullTimeWorkingOut);

                                        label13.Text = fullCountOrders.ToString() + "/" + fullCountMakeready.ToString();
                                        label14.Text = fullAmountOrders.ToString("N0");
                                        label15.Text = (fullPercentWorkingOut / countActiveUser).ToString("P1");
                                    }));
                                }
                            }

                            Connect.Close();
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

        private void LoadShiftdetails(String user, int yearCur, int monthCur)
        {
            FormShiftsDetails form = new FormShiftsDetails(adminMode, user, yearCur, monthCur);
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
                LoadShiftdetails(listView1.SelectedItems[0].Name, Convert.ToInt32(comboBox1.Text), comboBox2.SelectedIndex);
            }
        }

        private void FormShiftsDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancelTokenSource?.Cancel();
            SaveParameterToBase("statisticForm");
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartLoading();
        }
    }
}
