using libData;
using libSql;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace OrderManager
{
    public partial class FormStatisticAllUsers : Form
    {
        public FormStatisticAllUsers()
        {
            InitializeComponent();

            lvwColumnSorter = new ListViewColumnSorter();
            this.listView1.ListViewItemSorter = lvwColumnSorter;
        }

        private ListViewColumnSorter lvwColumnSorter;

        List<UserWorkingOutput> userWorkingOutputs = new List<UserWorkingOutput>();
        List<Category> categoriesList;

        bool loadCategoryStatistic = false;
        bool loadCategoryList = false;

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
                ChartType = SeriesChartType.Column,
                //ChartType = SeriesChartType.Pie,
                LabelBackColor = Color.Transparent,
                IsVisibleInLegend = false

            });

            chart1.Series["ColumnSeries"].Points.DataBindXY(xValues, yValues);

            chart1.ChartAreas[0].AxisX.LabelStyle.Angle = -30;
            //chart1.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            //chart1.ChartAreas[0].AxisX.IsLabelAutoFit = false;
            chart1.ChartAreas[0].AxisX.Interval = 1;


            //chart1.ChartAreas[0].Area3DStyle.Enable3D = true;
            chart1.AlignDataPointsByAxisLabel();
        }

        private void LoadCategoryToStatisticComboBox()
        {
            ValueCategory valueCategoryes = new ValueCategory();

            categoriesList?.Clear();
            categoriesList = valueCategoryes.GetCategoriesList();

            loadCategoryList = true;

            comboBox3.Items.Clear();

            comboBox3.Items.Add("<выберите участок>");

            for (int i = 0; i < categoriesList.Count; i++)
            {
                comboBox3.Items.Add(categoriesList[i].Name);
            }

            loadCategoryList = false;
        }

        private void GetEquipListFromTypeBaseLoad(int categoryID, int typebaseLoad)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            //for (int i = 0; i < categoriesList.Count; i++)
            {
                categoriesList[categoryID].Equips = getInfo.GetEquipsList(categoriesList[categoryID].Id, typebaseLoad);
            }
        }

        private void SetItemsComboBox()
        {
            //ValueCategory category = new ValueCategory();

            //List<string> categoryes = new List<string>(category.GetCategoryesList());

            //comboBox3.Items.AddRange(categoryes.ToArray());

            LoadCategoryToStatisticComboBox();

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

            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1 && comboBox3.SelectedIndex != -1 && comboBox4.SelectedIndex != -1)
            {
                cancelTokenSource = new CancellationTokenSource();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex);

                int selectLoadedBase = comboBox4.SelectedIndex;
                int typeValueLoad = comboBox5.SelectedIndex;

                //Task task = new Task(() => LoadUsersFromBase(token, date));
                //Task task = new Task(() => LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadedBase, typeValueLoad), cancelTokenSource.Token);
                //LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadedBase, typeValueLoad);

                Task task = new Task(() => LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadedBase), cancelTokenSource.Token);
                //LoadUsersFromBase(cancelTokenSource.Token, date, selectLoadedBase);

                task.Start();
            }
        }

        private List<int> LoadUserListFromMonthOM(CancellationToken token, List<int> equips, DateTime date)
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();
            ValueUserBase userBase = new ValueUserBase();

            List<int> usersList = new List<int>();

            List<int> usersListLoad = shiftsBase.LoadUsersListFromMonth(date);

            Console.WriteLine(usersListLoad.Count);
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

        private async Task<List<int>> LoadUserListFromMonthAS(CancellationToken token, List<int> equips, DateTime date, bool includeASuserID = false)
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

                equipsAS.Add(await infoBase.GetIDEquipMachine(equips[i]));
            }
            //Теперь передаётся 
            List<int> usersListAS = valueUsers.LoadUsersListOnlyIDFromSelectMonth(equips, date);

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

            if (includeASuserID)
            {
                return usersListAS;
            }
            else
            {
                return usersList;
            }
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

        private async void LoadUsersFromBase(CancellationToken token, DateTime date, int selectLoadBase)
        {
            CalculateWorkingOutput workingOutSum = new CalculateWorkingOutput();
            GetWorkingOutSum workingOutSumOM = new GetWorkingOutSum();
            ValueUsers valueUsers = new ValueUsers();
            ValueUserBase getUser = new ValueUserBase();
            ValueInfoBase getInfo = new ValueInfoBase();

            userWorkingOutputs?.Clear();

            int category = -1;
            int categoryComboBoxSelectedIndex = -1;

            Invoke(new Action(() =>
            {
                categoryComboBoxSelectedIndex = comboBox3.SelectedIndex;
            }));

            if (categoryComboBoxSelectedIndex < 1)
            {
                Invoke(new Action(() =>
                {
                    ClearAll();
                }));

                return;
            }

            Invoke(new Action(() =>
            {
                loadCategoryStatistic = true;
                buttonUpdateCancel.Text = "Отмена";

                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                comboBox4.Enabled = false;
                comboBox5.Enabled = false;
            }));

            category = categoryComboBoxSelectedIndex - 1;

            List<int> equipsListForCategory = new List<int>();//getInfo.GetMachinesList(categoriesList[category].Id);

            GetEquipListFromTypeBaseLoad(category, selectLoadBase);

            foreach (Equip equip in categoriesList[category].Equips)
            {
                equipsListForCategory.Add(equip.Id);
            }

            List<int> usersList;// = await LoadUserListFromMonthAS(token, equipsListForCategory, date, true);

            if (selectLoadBase == 0)
            {
                usersList = LoadUserListFromMonthOM(token, equipsListForCategory, date);
            }
            else
            {
                usersList = await LoadUserListFromMonthAS(token, equipsListForCategory, date, true);
            }

            Invoke(new Action(() =>
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = usersList.Count;
                progressBar1.Value = 0;
            }));

            for (int i = 0; i < usersList.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                UserWorkingOutput userWorkingOutput;

                string name;

                if (selectLoadBase == 0)
                {
                    userWorkingOutput = await workingOutSumOM.FullWorkingOutputOMAsync(usersList[i], date, token, categoriesList[category].Id);

                    name = getUser.GetNameUser(usersList[i].ToString()); 
                }
                else
                {
                    userWorkingOutput = workingOutSum.FullWorkingOutput(usersList[i], date, token, equipsListForCategory);

                    name = valueUsers.GetUserNameFromID(usersList[i]);
                }

                userWorkingOutputs.Add(new UserWorkingOutput(
                    usersList[i],
                    name,
                    userWorkingOutput.Amount,
                    userWorkingOutput.Worktime,
                    userWorkingOutput.Percent,
                    userWorkingOutput.Makeready,
                    userWorkingOutput.Bonus,
                    userWorkingOutput.CountShifts
                    ));

                Invoke(new Action(() =>
                {
                    progressBar1.Value++;
                }));
            }

            Invoke(new Action(() =>
            {
                loadCategoryStatistic = false;
                buttonUpdateCancel.Text = "Обновить";

                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;

                progressBar1.Value = 0;

                if (comboBox5.SelectedIndex == -1)
                {
                    comboBox5.SelectedIndex = 0;
                }
                else
                {
                    ChangeTypeWorkingOutput();
                }
            }));
        }
        private void ChangeTypeWorkingOutput()
        {
            /*cancelTokenSource?.Cancel();

            cancelTokenSource = new CancellationTokenSource();*/

            AddUserWorkingOutputValues(cancelTokenSource.Token, comboBox5.SelectedIndex);
        }

        private void AddUserWorkingOutputValues(CancellationToken token, int typeValueLoad)
        {
            List<UserWorkingOutput> userOutputs = userWorkingOutputs;
            List<string> names = new List<string>();
            List<float> values = new List<float>();
            float summWorkingOut = 0;

            Invoke(new Action(() =>
            {
                ClearAll();
            }));

            if (typeValueLoad == 0)
            {
                userOutputs.Sort((b1, b2) => b2.Amount.CompareTo(b1.Amount));
                label1.Text = "Всего сделано продукции:";
            }
            else if (typeValueLoad == 1)
            {
                userOutputs.Sort((b1, b2) => b2.Percent.CompareTo(b1.Percent));
                label1.Text = "Средняя выработка:";
            }
            else
            {
                userOutputs.Sort((b1, b2) => b2.Makeready.CompareTo(b1.Makeready));
                label1.Text = "Всего сделано приладок:";
            }

            for (int i = 0; i < userOutputs.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                float wOutValue = 0;

                if (typeValueLoad == 0)
                {
                    wOutValue = userOutputs[i].Amount;
                    values.Add(wOutValue);
                }
                else if (typeValueLoad == 1)
                {
                    wOutValue = userOutputs[i].Percent;
                    values.Add(wOutValue * 100);
                }
                else
                {
                    wOutValue = userOutputs[i].Makeready;
                    values.Add(wOutValue);
                }

                ListViewItem item = new ListViewItem();

                int inrdexCurrentItem = listView1.Items.Count;

                item.Name = userOutputs[i].UserID.ToString();
                item.Text = (inrdexCurrentItem + 1).ToString();
                item.SubItems.Add(userOutputs[i].UserName);
                //item.SubItems.Add(userOutputs[i].CountShifts.ToString());

                if (typeValueLoad == 1)
                {
                    item.SubItems.Add(wOutValue.ToString("P1"));
                }
                else
                {
                    item.SubItems.Add(wOutValue.ToString("N0"));
                }

                item.SubItems.Add("");
                //item.SubItems.Add(userOutputs[i].Bonus.ToString("P0"));

                Invoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                }));

                summWorkingOut += wOutValue;

                string add = "";

                if (names.Contains(userOutputs[i].UserName))
                {
                    add += " ";
                }

                names.Add(userOutputs[i].UserName + add);
            }

            for (int i = 0; i < userOutputs.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                float wOutValue = 0;

                if (typeValueLoad == 0)
                {
                    wOutValue = userOutputs[i].Amount;
                }
                else if (typeValueLoad == 1)
                {
                    wOutValue = userOutputs[i].Percent;
                }
                else
                {
                    wOutValue = userOutputs[i].Makeready;
                }

                Invoke(new Action(() =>
                {
                    int index = listView1.Items.IndexOfKey(userOutputs[i].UserID.ToString());

                    if (index >= 0)
                    {
                        ListViewItem item = listView1.Items[index];

                        if (item != null)
                        {
                            item.SubItems[3].Text = ((float)wOutValue / summWorkingOut).ToString("P2");
                        }
                    }
                }));
            }

            if (!token.IsCancellationRequested)
            {
                if (typeValueLoad == 0)
                {
                    label2.Text = summWorkingOut.ToString("N0");
                }
                else if (typeValueLoad == 1)
                {
                    label2.Text = (summWorkingOut / userOutputs.Count).ToString("P1");
                }
                else
                {
                    label2.Text = summWorkingOut.ToString("N0");
                }

                Invoke(new Action(() =>
                {
                    DrawDiagram(values, names);

                    //label2.Text = summWorkingOut.ToString("N0");
                }));
            }
        }

        /*private async void LoadUsersFromBase(CancellationToken token, DateTime date, int selectLoadBase, int typeValueLoad)
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
                //AddUsersToListView(token, usersList);
            }
            else
            {
                //usersList = LoadUserList(token, category);
                usersList = await LoadUserListFromMonthAS(token, equipsListForCategory, date, true);
                //AddUsersASToListView(token, usersList);
            }

            Invoke(new Action(() =>
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = usersList.Count * 3;
                progressBar1.Value = 0;
            }));

            for (int i = 0; i < usersList.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                //List<int> equipsListForUser = getUser.GetEquipsListForSelectedUser(usersList[i]);//не используется

                float workingOutUser = 0;
                string name = "";

                if (selectLoadBase == 0)
                {
                    if (typeValueLoad == 0)
                    {
                        workingOutUser = workingOutSum.CalculateWorkingOutForUserFromSelectedMonthDataBaseOM(usersList[i], equipsListForCategory, date);
                    }
                    else if (typeValueLoad == 1)
                    {
                        workingOutUser = await workingOutSum.CalculatePercentWorkingOutOM(usersList[i], date, token, category) * 100;
                    }
                    else
                    {
                        workingOutUser = await workingOutSum.CalculateCountMakeReadyOM(usersList[i], date, token, category);
                    }

                    name = getUser.GetNameUser(usersList[i].ToString());

                    workingOut.Add(workingOutUser);
                }
                else if (selectLoadBase == 1)
                {
                    if (typeValueLoad == 0)
                    {
                        workingOutUser = await workingOutSum.CalculateWorkingOutForUserFromSelectedMonthDataBaseASUsersFromAS(usersList[i], equipsListForCategory, date);
                    }
                    else if (typeValueLoad == 1)
                    {
                        workingOutUser = await workingOutSum.CalculatePercentWorkingOutAS(usersList[i], date, token, equipsListForCategory) * 100;
                    }
                    else
                    {
                        workingOutUser = await workingOutSum.CalculateCountMakeReadyAS(usersList[i], date, token, equipsListForCategory);
                    }

                    name = valueUsers.GetUserNameFromID(usersList[i]);

                    workingOut.Add(workingOutUser);
                }

                string add = "";

                if (usersNames.Contains(name))
                {
                    add += " ";
                }

                usersNames.Add(name + add);

                Invoke(new Action(() =>
                {
                    progressBar1.Value++;
                }));
            }

            ///Sort
            ///

            int tmpUsersList;
            float tmpWorkimgOut;
            string tmpUserNames;

            for (int i = 0; i < workingOut.Count - 1; i++)
            {
                for (int j = i + 1; j < workingOut.Count; j++)
                {
                    if (workingOut[i] < workingOut[j])
                    {
                        tmpUsersList = usersList[i];
                        usersList[i] = usersList[j];
                        usersList[j] = tmpUsersList;

                        tmpUserNames = usersNames[i];
                        usersNames[i] = usersNames[j];
                        usersNames[j] = tmpUserNames;

                        tmpWorkimgOut = workingOut[i];
                        workingOut[i] = workingOut[j];
                        workingOut[j] = tmpWorkimgOut;
                    }
                }
            }

            if (selectLoadBase == 0)
            {
                AddUsersToListView(token, usersList);
            }
            else
            {
                AddUsersASToListView(token, usersList);
            }

            for (int i = 0; i < usersList.Count; i++)
            {
                summWorkingOut += workingOut[i];

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
                                item.SubItems[2].Text = workingOut[i].ToString("N0");
                                label2.Text = summWorkingOut.ToString("N0");
                            }
                            else if (typeValueLoad == 1)
                            {
                                item.SubItems[2].Text = (workingOut[i] / 100).ToString("P1");
                                label2.Text = ((summWorkingOut / 100) / (i + 1)).ToString("P1");
                            }
                            else
                            {
                                item.SubItems[2].Text = workingOut[i].ToString("N0");
                                label2.Text = summWorkingOut.ToString("N0");
                            }
                        }
                    }

                    //label2.Text = summWorkingOut.ToString("N0");
                }));

                Invoke(new Action(() =>
                {
                    progressBar1.Value++;
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

                Invoke(new Action(() =>
                {
                    progressBar1.Value++;
                }));
            }

            Invoke(new Action(() =>
            {
                DrawDiagram(workingOut, usersNames);

                //label2.Text = summWorkingOut.ToString("N0");
            }));

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
            }));
        }*/

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

            if (loadCategoryStatistic)
            {
                cancelTokenSource?.Cancel();

                Thread.Sleep(100);
            }
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
            ChangeTypeWorkingOutput();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView1.Sort();
        }

        private void buttonUpdateCancel_Click(object sender, EventArgs e)
        {
            if (loadCategoryStatistic)
            {
                cancelTokenSource?.Cancel();

                Thread.Sleep(100);

                ClearAll();
            }
            else
            {
                StartLoading();
            }
        }
    }
}
