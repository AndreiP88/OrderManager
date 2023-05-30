using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.Runtime.ConstrainedExecution;

namespace OrderManager
{
    public partial class FormUserProfile : Form
    {
        String idUser;

        int _wTime = 680;
        int _percentOvertime = 100;
        int _percentNight = 20;
        int _nightTime = 440;

        int workTime = 0;
        int overTime = 0;
        int countNightShifts = 0;

        public FormUserProfile(String userID)
        {
            InitializeComponent();

            this.idUser = userID;
        }

        private void FormUserProfile_Load(object sender, EventArgs e)
        {
            LoadUser();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();
            ValueSettingsBase settingsBase = new ValueSettingsBase();

            bool detailsSalary = false;
            string getDeteils = settingsBase.GetDeteilsSalary(idUser);

            if (getDeteils == "True")
            {
                detailsSalary = true;
            }

            if (getDeteils == "False" || getDeteils == "")
            {
                detailsSalary = false;
            }

            String[] mounts = new String[] {
            "Январь",
            "Февраль",
            "Март",
            "Апрель",
            "Май",
            "Июнь",
            "Июль",
            "Август",
            "Сентябрь",
            "Октябрь",
            "Ноябрь",
            "Декабрь"};

            comboBox1.Items.AddRange(shiftsBase.LoadYears().ToArray());
            comboBox2.Items.AddRange(mounts);

            SetItemsComboBox(DateTime.Now.Year, DateTime.Now.Month);

            if (detailsSalary)
            {
                //tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Percent;
                tableLayoutPanel1.ColumnStyles[0].Width = 52;
                tableLayoutPanel1.ColumnStyles[1].Width = 48;
                this.Width = 1050;
            }
            else
            {
                tableLayoutPanel1.ColumnStyles[0].Width = 100;
                tableLayoutPanel1.ColumnStyles[1].Width = 0;
                this.Width = 570;
            }
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
                comboBox2.SelectedIndex = month - 1;
            }


        }

        void LoadUser()
        {
            ValueUserBase userBase = new ValueUserBase();

            this.Text = userBase.GetFullNameUser(idUser);
        }

        void SaveUser()
        {

        }

        private void LoadCalendarShifts(DateTime date)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetShiftsFromBase getShifts = new GetShiftsFromBase(idUser);
            GetNumberShiftFromTimeStart getNumberShift = new GetNumberShiftFromTimeStart();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            List<int> shifts = (List<int>)getShifts.LoadShiftsList(date);
            List<int> shiftsDays = new List<int>();

            int fullWorkTime = 0;
            int fullOverTime = 0;
            int countFullShifts = 0;
            int countOvertimeShifts = 0;
            int countPartialShifts = 0;

            for (int i = 0; i < shifts.Count; i++)
            {
                shiftsDays.Add(Convert.ToDateTime(shiftsBase.GetStartShiftFromID(shifts[i])).Day);
            }

            var now = date;

            var startOfTheMonth = new DateTime(now.Year, now.Month, 1);

            int days = DateTime.DaysInMonth(now.Year, now.Month);

            int dayOfTheWeek = Convert.ToInt32(startOfTheMonth.DayOfWeek.ToString("d")) == 0
                ? 7
                : Convert.ToInt32(startOfTheMonth.DayOfWeek.ToString("d"));

            //MessageBox.Show(dayOfTheWeek.ToString());

            int dayCurrWeek = dayOfTheWeek;
            int weekOfMonth = 1;
            int oldWeekOfMonth = 0;

            tableLayoutPanel5.Controls.Clear();

            countNightShifts = 0;

            AddDaysNames();

            for (int i = 1; i <= days; i++)
            {
                string nShift = "";
                int workTimeShift = 0;
                
                int index = shiftsDays.IndexOf(i);

                if (index != -1)
                {
                    nShift = getNumberShift.NumberShift(shiftsBase.GetStartShiftFromID(shifts[index]));

                    if (nShift == "II")
                    {
                        countNightShifts++;
                    }

                    if (shiftsBase.GetCheckFullShift(shifts[index]))
                    {
                        workTimeShift = _wTime;
                        countFullShifts++;
                    }
                    else
                    {
                        workTimeShift = timeOperations.totallTimeHHMMToMinutes(timeOperations.DateDifferent(shiftsBase.GetStopShiftFromID(shifts[index]), shiftsBase.GetStartShiftFromID(shifts[index])));
                        countPartialShifts++;
                    }

                    if (shiftsBase.GetCheckOvertimeShift(shifts[index]))
                    {
                        fullOverTime += workTimeShift;
                        countOvertimeShifts++;
                    }

                    fullWorkTime += workTimeShift;
                    
                }

                label11.Text = timeOperations.TotalMinutesToHoursAndMinutesStr(fullWorkTime);
                label13.Text = timeOperations.TotalMinutesToHoursAndMinutesStr(fullOverTime);
                label15.Text = countFullShifts.ToString();
                label17.Text = countOvertimeShifts.ToString();
                label19.Text = countPartialShifts.ToString();

                workTime = fullWorkTime;
                overTime = fullOverTime;

                string timeStr = timeOperations.TotalMinutesToHoursAndMinutesStr(workTimeShift);

                if (timeStr == "00:00")
                    timeStr = "";

                DayBlank currDay = new DayBlank();
                currDay.Refresh(i, nShift, timeStr);

                if (dayCurrWeek == 8)
                {
                    dayCurrWeek = 1;
                    weekOfMonth++;
                }

                if (weekOfMonth != oldWeekOfMonth)
                {
                    oldWeekOfMonth = weekOfMonth;

                    AddNumberWeekOfYear(new DateTime(now.Year, now.Month, i), weekOfMonth);
                }

                tableLayoutPanel5.Controls.Add(currDay, dayCurrWeek, weekOfMonth);

                //MessageBox.Show("День недели: " + dayCurrWeek.ToString() + ". Неделя месяца: " + weekOfMonth.ToString());

                dayCurrWeek++;
            }
        }

        private void AddDaysNames()
        {
            string[] dayNames = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

            for (int i = 0; i < dayNames.Length; i++)
            {
                Label label = new Label();
                label.Text = dayNames[i];
                label.TextAlign = ContentAlignment.MiddleCenter;
                //label.Font = new Font(label.Font, label.Font.Style | FontStyle.Bold);
                label.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
                label.Dock = DockStyle.Fill;

                tableLayoutPanel5.Controls.Add(label, i + 1, 0);
            }
        }

        private void AddNumberWeekOfYear(DateTime startWeekDate, int numbetWeekOfMonth)
        {
            int weekNumber = GetWeekNumber(startWeekDate);

            Label label = new Label();
            label.Text = weekNumber.ToString();
            label.TextAlign = ContentAlignment.MiddleCenter;
            //label.Font = new Font(label.Font, label.Font.Style | FontStyle.Bold);
            label.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Regular);
            label.Dock = DockStyle.Fill;

            tableLayoutPanel5.Controls.Add(label, 0, numbetWeekOfMonth);

        }

        public static int GetWeekNumber(DateTime dtPassed)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(dtPassed, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ValueUserBase setValueUsers = new ValueUserBase();

            setValueUsers.UpdateLastUID(idUser, "");
        }

        private void LoadCalendar()
        {
            DateTime date = DateTime.Now;

            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1)
            {
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex);

                LoadCalendarShifts(date);
            }
        }

        private void LoadSalaryCurrent(DateTime date)
        {
            ValueSalaryBase valueSalary = new ValueSalaryBase();

            string idSalary = valueSalary.GetIndexFromSelectedPeriod(date, idUser);

            numericUpDown1.Value = valueSalary.GetBasicSalary(idSalary);
            numericUpDown2.Value = valueSalary.GetBonusSalary(idSalary);

            numericUpDown3.Value = numericUpDown1.Value + numericUpDown2.Value;
        }

        private void LoadSalary()
        {
            DateTime date = DateTime.Now;

            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1)
            {
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex);
                string period = (comboBox2.SelectedIndex + 1).ToString("D2") + "." + comboBox1.Text.ToString();

                LoadSalaryCurrent(date);
                LoadSalaryPaid(idUser, period);
                Payroll(period);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCalendar();
            LoadSalary();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCalendar();
            LoadSalary();
        }

        private void LoadSalaryPaid(string user, string period)
        {
            ValueSalaryPaidBase valueSalaryPaid = new ValueSalaryPaidBase();

            numericUpDown10.Value = valueSalaryPaid.GetBasicSalary(user, period);
            numericUpDown11.Value = valueSalaryPaid.GetBonusSalary(user, period);
            numericUpDown12.Value = valueSalaryPaid.GetNight(user, period);
            numericUpDown13.Value = valueSalaryPaid.GetOvertime(user, period);
            numericUpDown14.Value = valueSalaryPaid.GetHoliday(user, period);
            numericUpDown9.Value = valueSalaryPaid.GetBonus(user, period);
            numericUpDown16.Value = valueSalaryPaid.GetOther(user, period);
        }

        private void CalculateFullSalary()
        {
            numericUpDown22.Value = numericUpDown4.Value +
                numericUpDown5.Value +
                numericUpDown6.Value +
                numericUpDown7.Value +
                numericUpDown8.Value +
                numericUpDown9.Value +
                numericUpDown16.Value;

            numericUpDown23.Value = numericUpDown10.Value +
                numericUpDown11.Value +
                numericUpDown12.Value +
                numericUpDown13.Value +
                numericUpDown14.Value +
                numericUpDown9.Value +
                numericUpDown16.Value;
        }

        private void RetentionFromSalary()
        {
            DateTime date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex);

            ValueSalaryBase salaryBase = new ValueSalaryBase();

            string index = salaryBase.GetIndexFromSelectedPeriod(date, idUser);

            decimal tax = salaryBase.GetTax(index);
            decimal pension = salaryBase.GetPension(index);

            numericUpDown17.Value = numericUpDown23.Value * tax / 100;
            numericUpDown19.Value = numericUpDown23.Value * pension / 100;

            numericUpDown21.Value = numericUpDown23.Value - (numericUpDown17.Value + numericUpDown19.Value);

            numericUpDown15.Value = numericUpDown22.Value * tax / 100;
            numericUpDown18.Value = numericUpDown22.Value * pension / 100;

            numericUpDown20.Value = numericUpDown22.Value - (numericUpDown15.Value + numericUpDown18.Value);
        }

        private void Payroll(string period)
        {
            ValueStandardTimeBase valueStandard = new ValueStandardTimeBase();

            int standard = valueStandard.GetStandard(period);

            decimal salaryBase = 0;
            decimal salaryBonus = 0;

            decimal salaryNight = 0;

            decimal salaryOvertime = 0;

            if (standard > 0)
            {
                decimal costPerMinuteBase = numericUpDown1.Value / (standard * 60);
                decimal costPerMinuteBonus = numericUpDown2.Value / (standard * 60);

                salaryBase = costPerMinuteBase * (workTime - overTime);
                salaryBonus = costPerMinuteBonus * (workTime - overTime);

                salaryNight = (costPerMinuteBase * _nightTime * countNightShifts) * _percentNight / 100;

                salaryOvertime = (costPerMinuteBase + costPerMinuteBonus) * overTime;
                salaryOvertime += salaryOvertime * _percentOvertime / 100;
            }

            numericUpDown4.Value = salaryBase;
            numericUpDown5.Value = salaryBonus;
            numericUpDown6.Value = salaryNight;
            numericUpDown7.Value = salaryOvertime;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ValueSettingsBase settingsBase = new ValueSettingsBase();

            if (tableLayoutPanel1.ColumnStyles[1].Width == 0)
            {
                //tableLayoutPanel1.ColumnStyles[1].SizeType = SizeType.Percent;
                tableLayoutPanel1.ColumnStyles[0].Width = 52;
                tableLayoutPanel1.ColumnStyles[1].Width = 48;
                this.Width = 1050;

                settingsBase.UpdateDeteilsSalary(idUser, "True");
            }
            else
            {
                tableLayoutPanel1.ColumnStyles[0].Width = 100;
                tableLayoutPanel1.ColumnStyles[1].Width = 0;
                this.Width = 570;

                settingsBase.UpdateDeteilsSalary(idUser, "False");
            }
            
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            ValueSalaryPaidBase valueSalaryPaid = new ValueSalaryPaidBase();

            string period = (comboBox2.SelectedIndex + 1).ToString("D2") + "." + comboBox1.Text.ToString();

            valueSalaryPaid.SetBasicSalary(idUser, period, numericUpDown10.Value);
            valueSalaryPaid.SetBonusSalary(idUser, period, numericUpDown11.Value);
            valueSalaryPaid.SetNight(idUser, period, numericUpDown12.Value);
            valueSalaryPaid.SetOvertime(idUser, period, numericUpDown13.Value);
            valueSalaryPaid.SetHoliday(idUser, period, numericUpDown14.Value);
            valueSalaryPaid.SetBonus(idUser, period, numericUpDown9.Value);
            valueSalaryPaid.SetOther(idUser, period, numericUpDown16.Value);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            string period = (comboBox2.SelectedIndex + 1).ToString("D2") + "." + comboBox1.Text.ToString();

            LoadSalaryPaid(idUser, period);
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown12_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown16_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            CalculateFullSalary();
        }

        private void numericUpDown23_ValueChanged(object sender, EventArgs e)
        {
            RetentionFromSalary();
        }

        private void numericUpDown22_ValueChanged(object sender, EventArgs e)
        {
            RetentionFromSalary();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FormSalaryForUser formSalary = new FormSalaryForUser(idUser);
            formSalary.ShowDialog();

            LoadSalary();
            RetentionFromSalary();
        }
    }
}
