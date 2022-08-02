using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormPrivateNote : Form
    {
        String loadStartOfShift;
        String loadOrderNumber;
        String loadOrderModification;
        String loadMachine;
        String loadCounterRepeat;

        public FormPrivateNote(String lStartOfShift, String lOrderNumber, String lOrderModification, String lMachine, String lCounterRepeat)
        {
            InitializeComponent();

            this.loadStartOfShift = lStartOfShift;
            this.loadOrderNumber = lOrderNumber;
            this.loadOrderModification = lOrderModification;
            this.loadMachine = lMachine;
            this.loadCounterRepeat = lCounterRepeat;
        }

        private void UpdateData(String nameOfColomn, String machineCurrent, String shiftStart, String number, String modification, String counterRepeat, String value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET " + nameOfColomn + " = @value " +
                    "WHERE ((machine = @machineCurrent AND startOfShift = @shiftStart) AND (numberOfOrder = @number AND modification = @modification) AND (counterRepeat = @counterRepeat))";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineCurrent", machineCurrent); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@shiftStart", shiftStart);
                Command.Parameters.AddWithValue("@number", number);
                Command.Parameters.AddWithValue("@modification", modification);
                Command.Parameters.AddWithValue("@counterRepeat", counterRepeat);
                Command.Parameters.AddWithValue("@value", value);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void LoadNote()
        {
            GetOrdersFromBase getOrder = new GetOrdersFromBase();
            String pNote = getOrder.GetPrivateNote(loadStartOfShift, loadOrderNumber, loadOrderModification, loadCounterRepeat);
            textBox1.Text = pNote;
        }

        private void SaveNote()
        {
            String pNote = textBox1.Text;

            UpdateData("privateNote", loadMachine, loadStartOfShift, loadOrderNumber, loadOrderModification, loadCounterRepeat, pNote);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveNote();
            Close();
        }

        private void FormPrivateNote_Load(object sender, EventArgs e)
        {
            LoadNote();
        }
    }
}
