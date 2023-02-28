using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormCloseShift : Form
    {
        String loadStartOfShift = "";
        bool _edit = false;

        public FormCloseShift()
        {
            InitializeComponent();

            _edit = false;
        }
        public FormCloseShift(String lStartOfShift)
        {
            InitializeComponent();

            _edit = true;

            this.loadStartOfShift = lStartOfShift;

            this.Text = "Комментарий";

            label1.Text = "";

            //textBox1.Enabled = false;
            textBox1.ReadOnly = true;

            button1.Enabled = false;
            button1.Text = "Сохранить";

            LoadNote(loadStartOfShift);
            LoadCheckFullShift(loadStartOfShift);
            LoadCheckOvertimeShift(loadStartOfShift);
        }

        private bool closeShiftVal;
        public bool ShiftVal
        {
            get
            {
                return closeShiftVal;
            }
            set
            {
                closeShiftVal = value;
            }
        }

        private String noteShiftVal;
        public String NoteVal
        {
            get
            {
                return noteShiftVal;
            }
            set
            {
                noteShiftVal = value;
            }
        }

        private bool fullShift;
        public bool FullShiftVal
        {
            get
            {
                return fullShift;
            }
            set
            {
                fullShift = value;
            }
        }

        private bool overtimeShift;
        public bool OvertimeShiftVal
        {
            get
            {
                return overtimeShift;
            }
            set
            {
                overtimeShift = value;
            }
        }

        private void LoadNote(String shiftStart)
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            String pNote = shiftsBase.GetNoteShift(shiftStart);

            textBox1.Text = pNote;
        }

        private void LoadCheckFullShift(String shiftStart)
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            bool check = shiftsBase.GetCheckFullShift(shiftStart);

            checkBox1.Checked = check;
        }

        private void LoadCheckOvertimeShift(String shiftStart)
        {
            ValueShiftsBase shiftsBase = new ValueShiftsBase();

            bool check = shiftsBase.GetCheckOvertimeShift(shiftStart);

            checkBox2.Checked = check;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShiftVal = false;
            NoteVal = textBox1.Text;
            fullShift = checkBox1.Checked;
            overtimeShift = checkBox2.Checked;

            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShiftVal = true;
            NoteVal = textBox1.Text;
            fullShift = checkBox1.Checked;
            overtimeShift = checkBox2.Checked;

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
            if (_edit)
            {
                ValueShiftsBase shiftsBase = new ValueShiftsBase();

                bool value = shiftsBase.GetCheckFullShift(loadStartOfShift);

                if (checkBox1.Checked != value)
                {
                    button1.Enabled = true;
                }
                else
                {
                    button1.Enabled = false;
                }
            }
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (_edit)
            {
                ValueShiftsBase shiftsBase = new ValueShiftsBase();

                bool value = shiftsBase.GetCheckOvertimeShift(loadStartOfShift);

                if (checkBox2.Checked != value)
                {
                    button1.Enabled = true;
                }
                else
                {
                    button1.Enabled = false;
                }
            }
                
        }
    }
}
