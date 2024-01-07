using libData;
using libSql;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace OrderManager
{
    public partial class FormStatisticYearForUser : Form
    {
        int UserID;

        public FormStatisticYearForUser(int userID)
        {
            InitializeComponent();

            this.UserID = userID;
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
                ChartType = SeriesChartType.Column,
                LabelBackColor = Color.Transparent,
                IsVisibleInLegend = false

            });

            // Salary series data
            //double[] yValues = { 2222, 2724, 2720, 3263, 2445 };
            //string[] xValues = { "France", "Canada", "Germany", "USA", "Italy" };
            chart1.Series["ColumnSeries"].Points.DataBindXY(xValues, yValues);

            chart1.AlignDataPointsByAxisLabel();
            
            //chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
        }

        private void SetItemsComboBox()
        {
            DateTime dateTime = DateTime.Now;

            //comboBox1.SelectedIndex = comboBox1.FindString(dateTime.Year.ToString());
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(dateTime.Year.ToString());

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
            chart1.Series.Clear();
        }

        private void ClearValues(List<int> columns)
        {
            for (int i = 0; i < 12; i++)
            {
                int index = listView1.Items.IndexOfKey((i + 1).ToString());

                if (index >= 0)
                {
                    ListViewItem item = listView1.Items[index];

                    if (item != null)
                    {
                        for (int j = 0; j < columns.Count; j++)
                        {
                            item.SubItems[columns[j]].Text = "";
                        }
                    }
                }
            }

            int indexSum = listView1.Items.IndexOfKey("sum");

            if (indexSum >= 0)
            {
                ListViewItem item = listView1.Items[indexSum];

                if (item != null)
                {
                    for (int j = 0; j < columns.Count; j++)
                    {
                        item.SubItems[columns[j]].Text = "";
                    }
                }
            }
        }

        CancellationTokenSource cancelTokenSource;

        private void StartLoading()
        {
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
            }

            if (comboBox1.SelectedIndex != -1 && comboBox4.SelectedIndex != -1)
            {
                cancelTokenSource = new CancellationTokenSource();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1);

                int selectLoadBase = comboBox4.SelectedIndex;

                AddMonthToListView();

                //Task task = new Task(() => LoadUsersFromBase(token, date));
                Task task = new Task(() => LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadBase), cancelTokenSource.Token);
                Task taskWorkingOut = new Task(() => LoadWorkingOut(cancelTokenSource.Token, date, selectLoadBase), cancelTokenSource.Token);
                //LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadBase);
                //LoadWorkingOut(cancelTokenSource.Token, date, selectLoadBase);
                  
                //await Task.WhenAny(task);
                //await Task.WhenAny(taskWorkingOut);

                task.Start();
                taskWorkingOut.Start();
            }
        }

        private void AddMonthToListView()
        {
            ClearAll();

            for (int i = 0; i < 12; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = (i + 1).ToString();
                item.Text = (i + 1).ToString();
                item.SubItems.Add(CultureInfo.GetCultureInfoByIetfLanguageTag("ru-RU").DateTimeFormat.GetMonthName(i + 1));
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");

                listView1.Items.Add(item);
            }

            ListViewItem itemSum = new ListViewItem();

            itemSum.Name = "sum";
            itemSum.Text = "";
            itemSum.SubItems.Add("ИТОГ");
            itemSum.SubItems.Add("");
            itemSum.SubItems.Add("");
            itemSum.SubItems.Add("");
            itemSum.SubItems.Add("");
            itemSum.SubItems.Add("");

            itemSum.Font = new Font(ListView.DefaultFont, FontStyle.Bold);

            listView1.Items.Add(itemSum);
        }

        private void LoadUsersFromBase(CancellationToken token, DateTime date, int selectLoadBase)
        {
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetWorkingOutSum workingOutSum = new GetWorkingOutSum();
            ValueUserBase getUser = new ValueUserBase();
            ValueInfoBase getInfo = new ValueInfoBase();

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = false;
                comboBox4.Enabled = false;

                List<int> cols = new List<int>
                {
                    2,
                    3
                };

                ClearValues(cols);
            }));

            List<string> monthNames = new List<string>();

            for (int i = 0; i < 12; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                monthNames.Add(CultureInfo.GetCultureInfoByIetfLanguageTag("ru-RU").DateTimeFormat.GetMonthName(i + 1));

                /*ListViewItem item = new ListViewItem();

                item.Name = i.ToString();
                item.Text = (i + 1).ToString();
                item.SubItems.Add(monthNames[monthNames.Count - 1]);
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add("");

                Invoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                }));*/
            }

            List<int> workingOut = new List<int>();
            int summWorkingOut = 0;

            for (int i = 0; i < 12; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                DateTime currentDateTime = date.AddMonths(i);

                List<int> equipsListForUser = getUser.GetEquipsListForSelectedUser(UserID);

                int workingOutUser = 0;

                if (selectLoadBase == 0)
                {
                    workingOutUser = workingOutSum.CalculateWorkingOutForUserFromSelectedMonthDataBaseOM(UserID, equipsListForUser, currentDateTime);
                }
                else
                {
                    workingOutUser = workingOutSum.CalculateWorkingOutForUserFromSelectedMonthDataBaseAS(UserID, equipsListForUser, currentDateTime);
                }

                workingOut.Add(workingOutUser);
                summWorkingOut += workingOutUser;

                Invoke(new Action(() =>
                {
                    int index = listView1.Items.IndexOfKey((i + 1).ToString());

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

            Invoke(new Action(() =>
            {
                DrawDiagram(workingOut, monthNames);

                //label2.Text = summWorkingOut.ToString("N0");
            }));

            for (int i = 0; i < 12; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                Invoke(new Action(() =>
                {
                    int index = listView1.Items.IndexOfKey((i + 1).ToString());

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
                int index = listView1.Items.IndexOfKey("sum");

                if (index >= 0)
                {
                    ListViewItem item = listView1.Items[index];

                    if (item != null)
                    {
                        item.SubItems[2].Text = summWorkingOut.ToString("N0");
                    }
                }
            }));

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = true;
                comboBox4.Enabled = true;
            }));

            if (token.IsCancellationRequested)
            {
                List<int> cols = new List<int>
                {
                    2,
                    3
                };

                Invoke(new Action(() =>
                {
                    ClearValues(cols);
                }));
            }
        }

        private void LoadWorkingOut(CancellationToken token, DateTime date, int selectLoadBase)
        {
            GetShiftsFromBase getShifts = new GetShiftsFromBase(UserID.ToString());
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = false;
                comboBox4.Enabled = false;

                List<int> cols = new List<int>
                {
                    4,
                    5,
                    6
                };

                ClearValues(cols);

            }));

            int countMonthEctive = 0;
            int summWorkingOutHour = 0;
            float summWorkingOutPercent = 0;
            float summBonus = 0;

            for (int i = 0; i < 12; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                DateTime currentDateTime = date.AddMonths(i);

                ShiftsDetails shiftsDetails;

                if (selectLoadBase == 0)
                {
                    shiftsDetails = getShifts.LoadCurrentDateShiftsDetails(currentDateTime, "", token);
                }
                else
                {
                    shiftsDetails = WorkingOutAS(currentDateTime, token);
                }

                if (shiftsDetails != null)
                {
                    summWorkingOutHour += shiftsDetails.allTimeWorkingOutShift;
                    summWorkingOutPercent += shiftsDetails.percentWorkingOutShift;
                    summBonus += shiftsDetails.percentBonusShift;


                    if (shiftsDetails.allTimeWorkingOutShift > 0)
                    {
                        countMonthEctive++;
                    }


                    Invoke(new Action(() =>
                    {
                        int index = listView1.Items.IndexOfKey((i + 1).ToString());

                        if (index >= 0)
                        {
                            ListViewItem item = listView1.Items[index];

                            if (item != null)
                            {
                                item.SubItems[4].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails.allTimeWorkingOutShift);
                                item.SubItems[5].Text = shiftsDetails.percentWorkingOutShift.ToString("P2");
                                item.SubItems[6].Text = shiftsDetails.percentBonusShift.ToString("P0");
                            }
                        }
                    }));
                }
            }

            Invoke(new Action(() =>
            {
                int index = listView1.Items.IndexOfKey("sum");

                if (index >= 0)
                {
                    ListViewItem item = listView1.Items[index];

                    if (item != null)
                    {
                        item.SubItems[4].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(summWorkingOutHour);
                        item.SubItems[5].Text = (summWorkingOutPercent / countMonthEctive).ToString("P2");
                        item.SubItems[6].Text = summBonus.ToString("P0");
                    }
                }
            }));

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = true;
                comboBox4.Enabled = true;
            }));

            if (token.IsCancellationRequested)
            {
                List<int> cols = new List<int>
                {
                    4,
                    5,
                    6
                };

                Invoke(new Action(() =>
                {
                    ClearValues(cols);
                }));
            }
        }

        private ShiftsDetails WorkingOutAS(DateTime selectDate, CancellationToken token)
        {
            ShiftsDetails shiftsDetails = null;

            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            ValueShifts valueShifts = new ValueShifts();
            ValueUserBase userBase = new ValueUserBase();

            List<int> userIndexFromAS = userBase.GetIndexUserFromASBase(UserID);
            
            List<User> usersList = new List<User>();

            for (int i = 0; i < userIndexFromAS.Count; i++)
            {
                usersList.Add(new User(userIndexFromAS[i]));
                usersList[usersList.Count - 1].Shifts = new List<UserShift>();
            }
            try
            {
                usersList = valueShifts.LoadShiftsForSelectedMonth(usersList, selectDate, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }

            float totalTimeWorkigOut = 0;
            //float totalPercentWorkingOut = 0;
            float totalBonusWorkingOut = 0;
            List<float> totalPercentWorkingOutList = new List<float>();

            for (int i = 0; i < usersList.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                for (int j = 0; j < usersList[i].Shifts.Count; j++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    
                    UserShift shift = usersList[i].Shifts[j];

                    bool currentShift;// = CheckCurrentShift(shiftDate, shiftNumber);

                    if (shift.ShiftDateEnd == "")
                    {
                        currentShift = true;
                    }
                    else
                    {
                        currentShift = false;
                    }

                    if (!currentShift)
                    {
                        float timeWorkigOut = CalculateWorkTime(shift.Orders);

                        totalTimeWorkigOut += timeWorkigOut;
                        totalPercentWorkingOutList.Add(getPercent.Percent((int)timeWorkigOut));
                        totalBonusWorkingOut += getPercent.GetBonusWorkingOutF((int)timeWorkigOut);
                    }
                }

                float percentWorkingOutAverage = totalPercentWorkingOutList.Sum() / totalPercentWorkingOutList.Count;

                shiftsDetails = new ShiftsDetails(
                -1,
                -1,
                -1,
                (int)totalTimeWorkigOut,
                -1,
                -1,
                -1,
                percentWorkingOutAverage,
                totalBonusWorkingOut
                );
            }

            return shiftsDetails;
        }

        private float CalculateWorkTime(List<UserShiftOrder> order)
        {
            float workingOut = 0;

            for (int i = 0; i < order.Count; i++)
            {
                workingOut += CalculateWorkTimeForOneOrder(order[i]);
            }

            return workingOut;
        }

        private float CalculateWorkTimeForOneOrder(UserShiftOrder order)
        {
            float workingOut = 0;

            if (order.Normtime > 0)
            {
                if (order.PlanOutQty > 0)
                {
                    workingOut += ((float)order.FactOutQty * (float)order.Normtime) / (float)order.PlanOutQty;
                }
                else
                {
                    if (order.FactOutQty > 0)
                    {
                        workingOut += (float)order.Normtime;
                    }
                }
            }

            return workingOut;
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
