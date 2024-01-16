using libData;
using libSql;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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

        private void DrawDiagram(List<float> yValues, List<string> xValues)
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
            List<string> years = new List<string>();

            int startYear = 2017;
            int currentYear = DateTime.Now.Year;

            for (int year = startYear; year <= currentYear; year++)
            {
                years.Add(year.ToString());
            }
            
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
            cancelTokenSource?.Cancel();

            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1 && comboBox3.SelectedIndex != -1 && comboBox4.SelectedIndex != -1 && comboBox5.SelectedIndex != -1)
            {
                cancelTokenSource = new CancellationTokenSource();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex);

                int selectLoadedBase = comboBox4.SelectedIndex;
                int typeValueLoad = comboBox5.SelectedIndex;

                //Task task = new Task(() => LoadUsersFromBase(token, date));
                Task task = new Task(() => LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadedBase, typeValueLoad), cancelTokenSource.Token);
                //LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadedBase, typeValueLoad);

                task.Start();
            }
        }

        private List<int> LoadUserListFromMonthOM(CancellationToken token, List<int> equips, DateTime date)
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();
            ValueUserBase userBase = new ValueUserBase();

            List<int> usersList = new List<int>();

            List<int> usersListLoad = shiftsBase.LoadUsersListFromMonth(date);
            //Console.WriteLine(usersListLoad.Count);
            for (int i = 0; i < usersListLoad.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                
                List<int> equipsListForSelectedUser = userBase.GetEquipsListForSelectedUser(usersListLoad[i]);

                bool isEquipForUser = false;

                for (int j = 0; j < equipsListForSelectedUser.Count; j++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (equips.Contains(equipsListForSelectedUser[j]))
                    {
                        isEquipForUser = true;
                        break;
                    }
                }

                if (isEquipForUser)
                {
                    usersList.Add(usersListLoad[i]);
                }
            }

            return usersList;
        }

        private (List<int>, List<int>) LoadUserListFromMonthAS(CancellationToken token, List<int> equips, DateTime date)
        {
            ValueUsers valueUsers = new ValueUsers();
            ValueUserBase userBase = new ValueUserBase();
            ValueInfoBase infoBase = new ValueInfoBase();

            List<int> usersList = new List<int>();
            List<int> equipsAS = new List<int>();

            for (int i = 0; i < equips.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                equipsAS.Add(infoBase.GetIDEquipMachine(equips[i]));
            }

            List<int> usersListAS = valueUsers.LoadUsersListOnlyIDFromSelectMonth(equipsAS, date);

            //Добавляется в список только сотрудники, которые есть в базе данных OM
            for (int i = 0; i < usersListAS.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                int userID = userBase.GetUserIdFromASystemID(usersListAS[i]);

                if (userID != -1)
                {
                    if (!usersList.Contains(userID))
                    {
                        usersList.Add(userID);
                    }
                }
            }
            
            return (usersList, usersListAS);
        }
        private List<int> LoadUserList(CancellationToken token, int category)
        {
            ValueUserBase getUser = new ValueUserBase();

            List<int> usersList = new List<int>();

            string cLine = "";
            cLine += " WHERE activeUser = 'True'";

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
                    }
                }
                Connect.Close();
            }

            return usersList;
        }

        private void AddUsersToListView(CancellationToken token, List<int> usersList)
        {
            ValueUserBase getUser = new ValueUserBase();

            for (int i = 0; i < usersList.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                ListViewItem item = new ListViewItem();

                item.Name = usersList[i].ToString();
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(getUser.GetNameUser(usersList[i].ToString()));
                item.SubItems.Add("");
                item.SubItems.Add("");

                Invoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                }));
            }
        }

        private void AddUsersASToListView(CancellationToken token, List<int> usersList)
        {
            ValueUsers valueUsers = new ValueUsers();

            for (int i = 0; i < usersList.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                ListViewItem item = new ListViewItem();

                item.Name = usersList[i].ToString();
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(valueUsers.GetUserNameFromID(usersList[i]));
                item.SubItems.Add("");
                item.SubItems.Add("");

                Invoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                }));
            }
        }

        private void LoadUsersFromBase(CancellationToken token, DateTime date, int selectLoadBase, int typeValueLoad)
        {
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetWorkingOutSum workingOutSum = new GetWorkingOutSum();
            ValueUserBase getUser = new ValueUserBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueCategory categoryValue = new ValueCategory();
            ValueUsers valueUsers = new ValueUsers();

            List<string> usersNames = new List<string>();
            List<float> workingOut = new List<float>();
            float summWorkingOut = 0;
            int category = -1;

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                comboBox4.Enabled = false;
                comboBox5.Enabled = false;

                ClearAll();

                category = categoryValue.GetIDCategoryFromName(comboBox3.Text);
            }));

            List<int> equipsListForCategory = getInfo.GetMachinesList(category);

            List<int> usersList;

            if (selectLoadBase == 0)
            {
                //usersList = LoadUserList(token, category);
                usersList = LoadUserListFromMonthOM(token, equipsListForCategory, date);
                AddUsersToListView(token, usersList);
            }
            else
            {
                //usersList = LoadUserList(token, category);
                usersList = LoadUserListFromMonthAS(token, equipsListForCategory, date).Item2;
                AddUsersASToListView(token, usersList);
            }

            for (int i = 0; i < usersList.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                //List<int> equipsListForUser = getUser.GetEquipsListForSelectedUser(usersList[i]);//не используется

                float workingOutUser = 0;

                if (selectLoadBase == 0)
                {
                    if (typeValueLoad == 0)
                    {
                        workingOutUser = workingOutSum.CalculateWorkingOutForUserFromSelectedMonthDataBaseOM(usersList[i], equipsListForCategory, date);
                    }
                    else
                    {
                        workingOutUser = workingOutSum.CalculatePercentWorkingOutOM(usersList[i], date, token, category);
                    }

                    usersNames.Add(getUser.GetNameUser(usersList[i].ToString()));
                    workingOut.Add(workingOutUser);
                }
                else
                {
                    if (typeValueLoad == 0)
                    {
                        workingOutUser = workingOutSum.CalculateWorkingOutForUserFromSelectedMonthDataBaseASUsersFromAS(usersList[i], equipsListForCategory, date);
                    }
                    else
                    {
                        workingOutUser = workingOutSum.CalculatePercentWorkingOutAS(usersList[i], date, token, equipsListForCategory);
                    }

                    usersNames.Add(valueUsers.GetUserNameFromID(usersList[i]));
                    workingOut.Add(workingOutUser);
                }

                summWorkingOut += workingOutUser;

                Invoke(new Action(() =>
                {
                    int index = listView1.Items.IndexOfKey(usersList[i].ToString());

                    if (index >= 0)
                    {
                        ListViewItem item = listView1.Items[index];

                        if (item != null)
                        {
                            if (typeValueLoad == 0)
                            {
                                item.SubItems[2].Text = workingOutUser.ToString("N0");
                                label2.Text = summWorkingOut.ToString("N0");
                            }
                            else
                            {
                                item.SubItems[2].Text = workingOutUser.ToString("P1");
                                label2.Text = summWorkingOut.ToString("P1");
                            }
                        }
                    }

                    //label2.Text = summWorkingOut.ToString("N0");
                }));
            }

            Invoke(new Action(() =>
            {
                DrawDiagram(workingOut, usersNames);

                //label2.Text = summWorkingOut.ToString("N0");
            }));

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
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
            }));
        }

        private void FormShiftsDetails_Load(object sender, EventArgs e)
        {
            //LoadParametersFromBase("statisticForm");
            LoadYears();
            SetItemsComboBox();
            comboBox4.SelectedIndex = 0;
            comboBox5.SelectedIndex = 0;
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

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            StartLoading();
        }
    }
}
