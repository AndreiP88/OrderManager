using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;

namespace OrderManager
{
    public partial class FormUserProfile : Form
    {
        String idUser;

        int _wTime = 680;

        public FormUserProfile(String userID)
        {
            InitializeComponent();

            this.idUser = userID;
        }

        private void FormUserProfile_Load(object sender, EventArgs e)
        {
            LoadUser();
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

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

            List<String> shifts = (List<String>)getShifts.LoadShiftsList(date);
            List<int> shiftsDays = new List<int>();

            int fullWorkTime = 0;
            int fullOverTime = 0;
            int countFullShifts = 0;
            int countOvertimeShifts = 0;
            int countPartialShifts = 0;

            for (int i = 0; i < shifts.Count; i++)
            {
                shiftsDays.Add(Convert.ToDateTime(shifts[i]).Day);
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

            AddDaysNames();

            for (int i = 1; i <= days; i++)
            {
                string nShift = "";
                int workTimeShift = 0;
                
                int index = shiftsDays.IndexOf(i);

                if (index != -1)
                {
                    nShift = getNumberShift.NumberShift(shifts[index]);

                    if (shiftsBase.GetCheckFullShift(shifts[index]))
                    {
                        workTimeShift = _wTime;
                        countFullShifts++;
                    }
                    else
                    {
                        workTimeShift = timeOperations.totallTimeHHMMToMinutes(timeOperations.DateDifferent(shiftsBase.GetStopShift(shifts[index]), shifts[index]));
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

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveUser();
            Close();
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
            }

            LoadCalendarShifts(date);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCalendar();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCalendar();
        }
    }
}
