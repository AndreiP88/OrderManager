﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.DataBaseReconnect;

namespace OrderManager
{
    public partial class FormShiftsDetails : Form
    {
        bool adminMode;

        string nameOfExecutor;
        int yearOfStatistic;
        int monthOfStatistic;

        public FormShiftsDetails(bool aMode, string executor, int yearSt, int mouthSt)
        {
            InitializeComponent();

            this.adminMode = aMode;
            this.nameOfExecutor = executor;
            this.yearOfStatistic = yearSt;
            this.monthOfStatistic = mouthSt;
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

        private void SetItemsComboBox(int year, int month)
        {
            if (year == 0 && month == 0)
            {
                DateTime dateTime = DateTime.Now;

                //comboBox1.SelectedIndex = comboBox1.FindString(dateTime.Year.ToString());
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(dateTime.Year.ToString());
                comboBox2.SelectedIndex = dateTime.Month - 1;

                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
            }
            else
            {
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(year.ToString());
                comboBox2.SelectedIndex = month;

                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
            }
        }

        private void LoadYears()
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            List<string> years = shiftsBase.LoadYears();

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

        CancellationTokenSource cancelTokenSource;
        private void StartLoading()
        {
            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1)
            {
                cancelTokenSource?.Cancel();
                cancelTokenSource = new CancellationTokenSource();

                ClearAll();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex);

                //Task task = new Task(LoadShiftsFromBase);
                Task task = new Task(() => LoadShiftsFromBase(cancelTokenSource.Token, date), cancelTokenSource.Token);
                task.Start();
            }
        }

        private async void LoadShiftsFromBase(CancellationToken token, DateTime date)
        {
            GetShiftsFromBase getShifts = new GetShiftsFromBase(nameOfExecutor);
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            GetNumberShiftFromTimeStart getNumberShift = new GetNumberShiftFromTimeStart();
            ValueShiftsBase shiftValue = new ValueShiftsBase();

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        List<int> shifts = getShifts.LoadShiftsList(date);

                        for (int i = 0; i < shifts.Count; i++)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            string shiftStart = shiftValue.GetStartShiftFromID(shifts[i]);
                            string dateStr;

                            dateStr = Convert.ToDateTime(shiftStart).ToString("d");
                            dateStr += ", " + getNumberShift.NumberShift(shiftStart);

                            ListViewItem item = new ListViewItem();

                            item.Name = shifts[i].ToString();
                            item.Text = (i + 1).ToString();
                            item.SubItems.Add(dateStr);
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            if (nameOfExecutor == Form1.Info.nameOfExecutor)
                                item.SubItems.Add(shiftValue.GetNoteShift(shifts[i]));

                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            Invoke(new Action(() => listView1.Items.Add(item)));
                        }

                        for (int i = 0; i < shifts.Count; i++)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            Shifts currentShift = await getShifts.LoadCurrentShift(shifts[i]);

                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            if (!token.IsCancellationRequested)
                            {
                                try
                                {
                                    Invoke(new Action(() =>
                                    {
                                        int index = listView1.Items.IndexOfKey(shifts[i].ToString());

                                        ListViewItem item = listView1.Items[index];

                                        if (item != null)
                                        {
                                            item.SubItems[2].Text = currentShift.machinesShift;
                                            item.SubItems[3].Text = currentShift.workingTimeShift;
                                            item.SubItems[4].Text = currentShift.countOrdersShift.ToString() + " / " + currentShift.countMakeReadyShift.ToString();
                                            item.SubItems[5].Text = currentShift.amountOrdersShift.ToString("N0");
                                            item.SubItems[6].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift.workingOutShift) + " (" + getPercent.GetBonusWorkingOut(currentShift.workingOutShift) + ")";
                                            item.SubItems[7].Text = getPercent.PercentString(currentShift.workingOutShift);

                                            if (shiftValue.GetCheckOvertimeShift(currentShift.startShiftID))
                                            {
                                                item.Font = new Font(listView1.Font, FontStyle.Bold);
                                            }

                                            if (getPercent.Percent(currentShift.workingOutShift) >= 0.8)
                                            {
                                                item.ForeColor = Color.SeaGreen;
                                            }
                                            else
                                            {
                                                item.ForeColor = Color.DarkRed;
                                            }
                                        }
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    LogException.WriteLine("LoadShiftsFromBase: " + ex.Message);
                                }
                            }
                        }

                        if (!token.IsCancellationRequested)
                        {
                            ShiftsDetails shiftsDetails = await getShifts.LoadCurrentDateShiftsDetails(date, "", token); //добавить выбор категорий

                            Invoke(new Action(() =>
                            {
                                label7.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails.shiftsWorkingTime);
                                label8.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails.allTimeShift);
                                label9.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails.allTimeWorkingOutShift);

                                label13.Text = shiftsDetails.countOrdersShift.ToString() + "/" + shiftsDetails.countMakereadyShift.ToString();
                                label14.Text = shiftsDetails.amountAllOrdersShift.ToString("N0");
                                label15.Text = shiftsDetails.percentWorkingOutShift.ToString("P1");
                            }));
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

        private void UpdateNote(int shiftID)
        {
            ValueShiftsBase valueShifts = new ValueShiftsBase();

            int index = listView1.Items.IndexOfKey(shiftID.ToString());

            ListViewItem item = listView1.Items[index];
            if (item != null)
            {
                item.SubItems[8].Text = valueShifts.GetNoteShift(shiftID);
            }
        }

        private void LoadShiftdetails(int shiftID)
        {
            FormOneShiftDetails form = new FormOneShiftDetails(adminMode, shiftID);
            form.ShowDialog();
        }

        private void LoadShiftNote(int shiftID)
        {
            FormCloseShift form = new FormCloseShift(shiftID, true);
            form.ShowDialog();

            bool result = form.ShiftVal;

            if (result)
            {
                ValueShiftsBase getShift = new ValueShiftsBase();

                getShift.SetTimeShift(shiftID, form.TimeShift);
                getShift.SetNoteShift(shiftID, form.NoteVal);
                getShift.SetCheckFullShift(shiftID, form.FullShiftVal);
                getShift.SetCheckOvertimeShift(shiftID, form.OvertimeShiftVal);

                UpdateNote(shiftID);

                StartLoading();
            }
        }

        private void FormShiftsDetails_Load(object sender, EventArgs e)
        {
            ValueUserBase getUser = new ValueUserBase();

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
                LoadShiftdetails(Convert.ToInt32(listView1.SelectedItems[0].Name));
            }
        }

        private void FormShiftsDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancelTokenSource?.Cancel();

            SaveParameterToBase("shiftsForm");
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                LoadShiftdetails(Convert.ToInt32(listView1.SelectedItems[0].Name));
            }
        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                LoadShiftNote(Convert.ToInt32(listView1.SelectedItems[0].Name));
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = (listView1.SelectedItems.Count == 0 || nameOfExecutor != Form1.Info.nameOfExecutor);
        }

    }
}
