using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormCloseShift : Form
    {
        String loadStartOfShift = "";

        public FormCloseShift()
        {
            InitializeComponent();
        }
        public FormCloseShift(String lStartOfShift)
        {
            InitializeComponent();

            this.loadStartOfShift = lStartOfShift;

            this.Text = "Комментарий";

            label1.Text = "";

            //textBox1.Enabled = false;
            textBox1.ReadOnly = true;

            button1.Enabled = false;
            button1.Text = "Сохранить";

            LoadNote(loadStartOfShift);
            LoadCheckFullShift(loadStartOfShift);
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

        private void button2_Click(object sender, EventArgs e)
        {
            ShiftVal = false;
            NoteVal = textBox1.Text;
            fullShift = checkBox1.Checked;

            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShiftVal = true;
            NoteVal = textBox1.Text;
            fullShift = checkBox1.Checked;

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
    }
}
