using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormPrivateNote : Form
    {
        int loadShiftID;
        int loadOrderID;
        string loadMachine;
        int loadCounterRepeat;

        public FormPrivateNote(int lShiftID, int lOrderIndex, String lMachine, int lCounterRepeat)
        {
            InitializeComponent();

            this.loadShiftID = lShiftID;
            this.loadOrderID = lOrderIndex;
            this.loadMachine = lMachine;
            this.loadCounterRepeat = lCounterRepeat;
        }

        private void UpdateData(String nameOfColomn, String machineCurrent, int shiftID, int orderIndex, int counterRepeat, String value)
        {
            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText = "UPDATE ordersInProgress SET " + nameOfColomn + " = @value " +
                    "WHERE ((machine = @machineCurrent AND shiftID = @shiftID) AND (orderID = @id AND counterRepeat = @counterRepeat))";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineCurrent", machineCurrent); // присваиваем переменной значение
                Command.Parameters.AddWithValue("@shiftID", shiftID);
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
            String pNote = getOrder.GetPrivateNote(loadShiftID, loadOrderID, loadCounterRepeat, Convert.ToInt32(loadMachine));
            textBox1.Text = pNote;
        }

        private void SaveNote()
        {
            String pNote = textBox1.Text;

            UpdateData("privateNote", loadMachine, loadShiftID, loadOrderID, loadCounterRepeat, pNote);
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
