using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormShiftsDetails : Form
    {
        bool adminMode;
        String dataBase;
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String nameOfExecutor;
        int yearOfStatistic;
        int monthOfStatistic;

        public FormShiftsDetails(bool aMode, String dBase, String executor, int yearSt, int mouthSt)
        {
            InitializeComponent();

            this.adminMode = aMode;
            this.dataBase = dBase;
            this.nameOfExecutor = executor;
            this.yearOfStatistic = yearSt;
            this.monthOfStatistic = mouthSt;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        String nameThread = "";
        bool thJob = false;

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

        private void SetItemsComboBox(int year, int month)
        {
            if (year == 0 && month == 0)
            {
                DateTime dateTime = DateTime.Now;

                //comboBox1.SelectedIndex = comboBox1.FindString(dateTime.Year.ToString());
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(dateTime.Year.ToString());
                comboBox2.SelectedIndex = dateTime.Month - 1;
            }
            else
            {
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(year.ToString());
                comboBox2.SelectedIndex = month;
            }


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

            comboBox1.Items.AddRange(years.ToArray());
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

                //Task task = new Task(LoadShiftsFromBase);
                Task task = new Task(() => LoadShiftsFromBase(token, date));

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

        private void LoadShiftsFromBase(CancellationToken token, DateTime date)
        {
            GetShiftsFromBase getShifts = new GetShiftsFromBase(dataBase, nameOfExecutor);
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();

            Invoke(new Action(() =>
            {
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
            }));

            while (!token.IsCancellationRequested)
            {
                thJob = true;

                List<Shifts> currentShift = (List<Shifts>)getShifts.LoadShiftsFromBase(date, "").Item1;// Добавить выбор категорий

                for (int i = 0; i < currentShift.Count; i++)
                {
                    ListViewItem item = new ListViewItem();

                    item.Name = currentShift[i].startShift;
                    item.Text = (i + 1).ToString();
                    item.SubItems.Add(currentShift[i].dateShift);
                    item.SubItems.Add(currentShift[i].machinesShift);
                    item.SubItems.Add(currentShift[i].workingTimeShift);
                    item.SubItems.Add(currentShift[i].countOrdersShift.ToString());
                    item.SubItems.Add(currentShift[i].amountOrdersShift.ToString("N0"));
                    item.SubItems.Add(dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift[i].workingOutShift));
                    item.SubItems.Add(getPercent.PercentString(currentShift[i].workingOutShift));

                    Invoke(new Action(() => listView1.Items.Add(item)));
                }

                List<ShiftsDetails> shiftsDetails = (List<ShiftsDetails>)getShifts.LoadShiftsFromBase(date, "").Item2; //добавить выбор категорий

                Invoke(new Action(() =>
                {
                    label7.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].countShifts * 680);
                    label8.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].allTimeShift);
                    label9.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].allTimeWorkingOutShift);

                    label13.Text = shiftsDetails[shiftsDetails.Count - 1].countOrdersShift.ToString() + "/" + shiftsDetails[shiftsDetails.Count - 1].countMakereadyShift.ToString();
                    label14.Text = shiftsDetails[shiftsDetails.Count - 1].amountAllOrdersShift.ToString("N0");
                    label15.Text = shiftsDetails[shiftsDetails.Count - 1].percentWorkingOutShift.ToString("N1") + "%";
                }));

                Invoke(new Action(() =>
                {
                    if (yearOfStatistic == 0 && monthOfStatistic == 0)
                    {
                        comboBox1.Enabled = true;
                        comboBox2.Enabled = true;
                    }

                }));

                thJob = false;
                break;
            }
        }

        private void LoadShiftdetails(String timeStartShift)
        {
            FormOneShiftDetails form = new FormOneShiftDetails(adminMode, dataBase, timeStartShift);
            form.ShowDialog();
        }

        private void FormShiftsDetails_Load(object sender, EventArgs e)
        {
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            this.Text += " - " + getUser.GetNameUser(nameOfExecutor);

            LoadParametersFromBase("shiftsForm");
            LoadYears();
            SetItemsComboBox(yearOfStatistic, monthOfStatistic);
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
                LoadShiftdetails(listView1.SelectedItems[0].Name);
            }
        }

        private void FormShiftsDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveParameterToBase("shiftsForm");
        }
    }
}
