﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace OrderManager
{
    public partial class FormStatisticAllUsers : Form
    {
        bool adminMode;

        public FormStatisticAllUsers(bool aMode = false)
        {
            InitializeComponent();

            this.adminMode = aMode;
        }

        bool thJob = false;
        bool calculateNullShiftsFromUser = false;

        string GetParametersLine()
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

        private void DrawDiagram(List<int> yValues, List<string> xValues)
        {
            chart1.Series.Clear();
            // Форматировать диаграмму
            //chart1.BackColor = Color.Gray;
            //chart1.BackSecondaryColor = Color.WhiteSmoke;
            chart1.BackGradientStyle = GradientStyle.DiagonalRight;

            chart1.BorderlineDashStyle = ChartDashStyle.Solid;
            //chart1.BorderlineColor = Color.Gray;
            chart1.BorderSkin.SkinStyle = BorderSkinStyle.None;

            // Форматировать область диаграммы
            chart1.ChartAreas[0].BackColor = Color.Transparent;

            // Добавить и форматировать заголовок
            /*chart1.Titles.Add("Диаграммы");
            chart1.Titles[0].Font = new Font("Utopia", 16);*/

            chart1.Series.Add(new Series("ColumnSeries")
            {
                ChartType = SeriesChartType.Pie,
                LabelBackColor = Color.Transparent,
                IsVisibleInLegend = true

            });

            // Salary series data
            //double[] yValues = { 2222, 2724, 2720, 3263, 2445 };
            //string[] xValues = { "France", "Canada", "Germany", "USA", "Italy" };
            chart1.Series["ColumnSeries"].Points.DataBindXY(xValues, yValues);

            chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
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
            label2.Text = "";
            chart1.Series.Clear();
        }

        CancellationTokenSource cancelTokenSource;
        private void StartLoading()
        {
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
                //Thread.Sleep(100);
            }

            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1 && comboBox3.SelectedIndex != -1 && comboBox4.SelectedIndex != -1)
            {
                cancelTokenSource = new CancellationTokenSource();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex);

                int selectLoadBase = comboBox4.SelectedIndex;

                //Task task = new Task(() => LoadUsersFromBase(token, date));
                Task task = new Task(() => LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadBase), cancelTokenSource.Token);
                //LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadBase);

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

        private void LoadUsersFromBase(CancellationToken token, DateTime date, int selectLoadBase)
        {
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetWorkingOutSum workingOutSum = new GetWorkingOutSum();
            ValueUserBase getUser = new ValueUserBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueCategory categoryValue = new ValueCategory();

            string cLine = " WHERE activeUser = 'True'";

            int category = -1;

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                comboBox4.Enabled = false;

                ClearAll();

                category = categoryValue.GetIDCategoryFromName(comboBox3.Text);
            }));

            List<int> usersList = new List<int>();
            List<string> usersNames = new List<string>();

            while (true)
            {
                //token.ThrowIfCancellationRequested();

                Invoke(new Action(() =>
                {
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

                    while (sqlReader.Read())
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        if (getUser.CategoryForUser(sqlReader["id"].ToString(), category.ToString()))
                        {
                            usersList.Add((int)sqlReader["id"]);
                            usersNames.Add(getUser.GetNameUser(sqlReader["id"].ToString()));

                            ListViewItem item = new ListViewItem();

                            item.Name = sqlReader["id"].ToString();
                            item.Text = (listView1.Items.Count + 1).ToString();
                            item.SubItems.Add(getUser.GetNameUser(sqlReader["id"].ToString()));
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
                
                List<int> workingOut = new List<int>();
                int summWorkingOut = 0;

                for (int i = 0; i < usersList.Count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    List<int> equipsListForUser = getUser.GetEquipsListForSelectedUser(usersList[i]);//не используется

                    List<int> equipsListForCategory = getInfo.GetMachinesList(category);

                    int workingOutUser = 0;

                    if (selectLoadBase == 0)
                    {
                        workingOutUser = workingOutSum.CalculateWorkingOutForUserFromSelectedMonthDataBaseOM(usersList[i], equipsListForCategory, date);
                    }
                    else
                    {
                        workingOutUser = workingOutSum.CalculateWorkingOutForUserFromSelectedMonthDataBaseAS(usersList[i], equipsListForCategory, date);
                    }

                    workingOut.Add(workingOutUser);
                    summWorkingOut += workingOutUser;

                    Invoke(new Action(() =>
                    {
                        int index = listView1.Items.IndexOfKey(usersList[i].ToString());

                        if (index >= 0)
                        {
                            ListViewItem item = listView1.Items[index];

                            if (item != null)
                            {
                                item.SubItems[2].Text = workingOutUser.ToString("N0");
                            }
                        }
                    }));
                }

                for (int i = 0; i < usersList.Count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    Invoke(new Action(() =>
                    {
                        int index = listView1.Items.IndexOfKey(usersList[i].ToString());

                        if (index >= 0)
                        {
                            ListViewItem item = listView1.Items[index];

                            if (item != null)
                            {
                                item.SubItems[3].Text = ((float)workingOut[i] / summWorkingOut).ToString("P2");
                            }
                        }
                    }));
                }

                Invoke(new Action(() =>
                {
                    DrawDiagram(workingOut, usersNames);

                    label2.Text = summWorkingOut.ToString("N0");
                }));

                break;
            }

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
            }));
        }

        private void FormShiftsDetails_Load(object sender, EventArgs e)
        {
            //LoadParametersFromBase("statisticForm");
            LoadYears();
            SetItemsComboBox();
            comboBox4.SelectedIndex = 0;
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

        private void FormShiftsDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            //SaveParameterToBase("statisticForm");
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartLoading();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartLoading();
        }
    }
}
