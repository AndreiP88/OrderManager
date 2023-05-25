using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormPrivateNote : Form
    {
        String loadStartOfShift;
        int loadOrderID;
        String loadMachine;
        String loadCounterRepeat;

        public FormPrivateNote(String lStartOfShift, int lOrderIndex, String lMachine, String lCounterRepeat)
        {
            InitializeComponent();

            this.loadStartOfShift = lStartOfShift;
            this.loadOrderID = lOrderIndex;
            this.loadMachine = lMachine;
            this.loadCounterRepeat = lCounterRepeat;
        }

        private void UpdateData(String nameOfColomn, String machineCurrent, String shiftStart, int orderIndex, String counterRepeat, String value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET " + nameOfColomn + " = @value " +
                    "WHERE ((machine = @machineCurrent AND startOfShift = @shiftStart) AND (orderID = @id AND counterRepeat = @counterRepeat))";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineCurrent", machineCurrent); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@shiftStart", shiftStart);
                Command.Parameters.AddWithValue("@id", orderIndex);
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
            String pNote = getOrder.GetPrivateNote(loadStartOfShift, loadOrderID, loadCounterRepeat, loadMachine);
            textBox1.Text = pNote;
        }

        private void SaveNote()
        {
            String pNote = textBox1.Text;

            UpdateData("privateNote", loadMachine, loadStartOfShift, loadOrderID, loadCounterRepeat, pNote);
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
