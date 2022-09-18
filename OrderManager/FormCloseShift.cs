using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormCloseShift : Form
    {
        String loadStartOfShift;

        public FormCloseShift()
        {
            InitializeComponent();
        }
        public FormCloseShift(String lStartOfShift)
        {
            InitializeComponent();

            this.loadStartOfShift = lStartOfShift;
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



        private void LoadNote()
        {
            GetOrdersFromBase getOrder = new GetOrdersFromBase();
            String pNote = "";
            textBox1.Text = pNote;
        }

        private void SaveNote()
        {
            String pNote = textBox1.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShiftVal = false;
            NoteVal = textBox1.Text;

            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ShiftVal = true;
            NoteVal = textBox1.Text;

            this.Hide();

            /*SaveNote();
            Close();*/
        }

        private void FormPrivateNote_Load(object sender, EventArgs e)
        {
            LoadNote();
        }
    }
}
