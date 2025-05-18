using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace OrderManager
{
    public partial class FormCloseShift : Form
    {
        int loadStartOfShift = -1;
        bool _edit = false;

        public FormCloseShift(int lStartOfShift)
        {
            InitializeComponent();

            _edit = false;
        }
        public FormCloseShift(int lStartOfShift, bool edit)
        {
            InitializeComponent();

            _edit = edit;

            this.loadStartOfShift = lStartOfShift;

            if (edit)
            {
                this.Text = "Комментарий";

                label1.Text = "";

                //textBox1.Enabled = false;
                textBox1.ReadOnly = true;

                button1.Enabled = false;
                button1.Text = "Сохранить";

                LoadNote(loadStartOfShift);
                LoadCheckFullShift(loadStartOfShift);                
            }
            else
            {
                CheckedFullShift(lStartOfShift);
            }

            LoadCheckOvertimeShift(loadStartOfShift);
            
        }

        public bool ShiftVal { get; set; }
        public String NoteVal { get; set; }
        public int TimeShift { get; set; }
        public bool FullShiftVal { get; set; }
        public bool OvertimeShiftVal { get; set; }

        private void LoadNote(int shiftID)
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            String pNote = shiftsBase.GetNoteShift(shiftID);

            textBox1.Text = pNote;
        }

        private void LoadCheckFullShift(int shiftID)
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            bool check = shiftsBase.GetCheckFullShift(shiftID);

            checkBox1.Checked = check;

            SetTimeValue(shiftID, check);
        }

        private void LoadCheckOvertimeShift(int shiftID)
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            bool check = shiftsBase.GetCheckOvertimeShift(shiftID);

            checkBox2.Checked = check;
        }

        private void CheckedFullShift(int shiftID)
        {
            int time = GetShiftTime(shiftID);

            if (time > 650)
            {
                checkBox1.Checked = true;
                dateTimePicker1.Value = Convert.ToDateTime(DateTime.Now.ToString("dd.MM.yyyy") + " 0:00").AddMinutes(680);
                dateTimePicker1.Enabled = false;
            }
            else
            {
                checkBox1.Checked = false;
                dateTimePicker1.Value = Convert.ToDateTime(DateTime.Now.ToString("dd.MM.yyyy") + " 0:00").AddMinutes(time);
                dateTimePicker1.Enabled = true;
            }
        }

        private void SetTimeValue(int shiftID, bool checkeBoxValue)
        {
            int time = GetShiftTime(shiftID);

            if (checkeBoxValue)
            {
                dateTimePicker1.Value = Convert.ToDateTime(DateTime.Now.ToString("dd.MM.yyyy") + " 0:00").AddMinutes(680);
                dateTimePicker1.Enabled = false;
            }
            else
            {
                dateTimePicker1.Value = Convert.ToDateTime(DateTime.Now.ToString("dd.MM.yyyy") + " 0:00").AddMinutes(time);
                dateTimePicker1.Enabled = true;
            }
        }

        private int GetShiftTime(int shiftID)
        {
            int result = 0;

            ValueShiftsBase shiftsBase = new ValueShiftsBase();
            GetNumberShiftFromTimeStart shiftFromTimeStart = new GetNumberShiftFromTimeStart();

            LoadShift shift = shiftsBase.GetShiftFromID(shiftID);

            if (shift != null)
            {
                if (_edit)
                {
                    result = shiftsBase.GetTimeShift(shiftID);
                }
                else
                {
                    DateTime timeStartShiftPlaned = Convert.ToDateTime(shiftFromTimeStart.PlanedStartShift(shift.ShiftStart, shift.ShiftNumber));

                    result = (int)DateTime.Now.Subtract(timeStartShiftPlaned).TotalMinutes;
                }
            }

            return result;
        }

        private void ChangeEnabledButton()
        {
            if (_edit)
            {
                ValueShiftsBase shiftsBase = new ValueShiftsBase();

                bool valueFullShift = shiftsBase.GetCheckFullShift(loadStartOfShift);

                bool valueOvertimeShift = shiftsBase.GetCheckOvertimeShift(loadStartOfShift);

                int timeShiftValue = shiftsBase.GetTimeShift(loadStartOfShift);
                int time = Convert.ToDateTime(dateTimePicker1.Value).Hour * 60 + Convert.ToDateTime(dateTimePicker1.Value).Minute;

                if (checkBox1.Checked != valueFullShift || checkBox2.Checked != valueOvertimeShift || time != timeShiftValue)
                {
                    button1.Enabled = true;
                }
                else
                {
                    button1.Enabled = false;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShiftVal = false;
            NoteVal = textBox1.Text;
            TimeShift = Convert.ToDateTime(dateTimePicker1.Value).Hour * 60 + Convert.ToDateTime(dateTimePicker1.Value).Minute;
            FullShiftVal = checkBox1.Checked;
            OvertimeShiftVal = checkBox2.Checked;

            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShiftVal = true;
            NoteVal = textBox1.Text;
            TimeShift = Convert.ToDateTime(dateTimePicker1.Value).Hour * 60 + Convert.ToDateTime(dateTimePicker1.Value).Minute;
            FullShiftVal = checkBox1.Checked;
            OvertimeShiftVal = checkBox2.Checked;

            this.Hide();

            /*SaveNote();
            Close();*/
        }

        private void FormPrivateNote_Load(object sender, EventArgs e)
        {
            //LoadNote(loadStartOfShift);
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            textBox1.ReadOnly = false;
            button1.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ChangeEnabledButton();

            SetTimeValue(loadStartOfShift, checkBox1.Checked);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
           ChangeEnabledButton();
        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            ChangeEnabledButton();
        }
    }
}
