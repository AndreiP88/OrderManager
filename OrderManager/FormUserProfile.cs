using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormUserProfile : Form
    {
        String idUser;
        String passKey = "key";

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
            Cryption pass = new Cryption();
            ValueUserBase userBase = new ValueUserBase();
            ValueSettingsBase settingsBase = new ValueSettingsBase();

            bool checkPass = false;

            if (settingsBase.GetPasswordChecked(idUser) != "")
                checkPass = Convert.ToBoolean(settingsBase.GetPasswordChecked(idUser));

            textBox1.Text = userBase.GetNameUser(idUser);
            textBox2.Text = pass.DeCode(userBase.GetPasswordUser(idUser), passKey);
            checkBox1.Checked = checkPass;
        }

        void SaveUser()
        {
            Cryption pass = new Cryption();
            ValueUserBase setUpdateUsers = new ValueUserBase();
            ValueSettingsBase updateSettingsValue = new ValueSettingsBase();

            setUpdateUsers.UpdateName(idUser, textBox1.Text);
            setUpdateUsers.UpdatePassword(idUser, pass.Code(textBox2.Text, passKey));
            updateSettingsValue.UpdateCheckPassword(idUser, checkBox1.Checked.ToString());
        }

        private void LoadCalendarShifts(DateTime date)
        {
            GetShiftsFromBase getShifts = new GetShiftsFromBase(idUser);
            GetNumberShiftFromTimeStart getNumberShift = new GetNumberShiftFromTimeStart();

            List<String> shifts = (List<String>)getShifts.LoadShiftsList(date);
            List<int> shiftsDays = new List<int>();

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

            tableLayoutPanel5.Controls.Clear();

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

            for (int i = 1; i <= days; i++)
            {
                string nShift = "";
                int index = shiftsDays.IndexOf(i);

                if (index != -1)
                {
                    nShift = getNumberShift.NumberShift(shifts[index]);
                }

                DayBlank currDay = new DayBlank();
                currDay.Refresh(i, nShift, "");

                if (dayCurrWeek == 8)
                {
                    dayCurrWeek = 1;
                    weekOfMonth++;
                }

                tableLayoutPanel5.Controls.Add(currDay, dayCurrWeek, weekOfMonth);

                //MessageBox.Show("День недели: " + dayCurrWeek.ToString() + ". Неделя месяца: " + weekOfMonth.ToString());

                dayCurrWeek++;
            }
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
