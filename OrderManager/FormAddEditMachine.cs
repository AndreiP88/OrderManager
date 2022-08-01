using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAddEditMachine : Form
    {
        String dataBase;
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String machineIDLoad;

        bool _loadForEdit;

        /// <summary>
        /// Добавление нового сотрудника
        /// </summary>
        /// <param name="dataBase"></param>
        public FormAddEditMachine(String dataBase)
        {
            InitializeComponent();

            this.dataBase = dataBase;

            _loadForEdit = false;

            DataBasePatch(dataBase);
        }

        /// <summary>
        /// Редактировние выбранного сотрудника
        /// </summary>
        /// <param name="dataBase"></param>
        /// <param name="userID"></param>
        public FormAddEditMachine(String dataBase, String machineID)
        {
            InitializeComponent();

            this.dataBase = dataBase;
            this.machineIDLoad = machineID;

            _loadForEdit = true;

            DataBasePatch(dataBase);
        }

        private String DataBasePatch(String dBase)
        {
            String dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;

            return dataBase;
        }

        private void FormAddEditUser_Load(object sender, EventArgs e)
        {
            if (_loadForEdit)
            {
                LoadForEdit();
            }
            else
            {
                LoadForAdd();
            }
        }

        private void LoadForEdit()
        {
            this.Text = "Редактирование оборудование";
            button1.Text = "Сохранить";

            ValueCategory category = new ValueCategory(dataBase);
            ValueInfoBase getMachine = new ValueInfoBase(dataBase);

            LoadCategoryes();

            textBox1.Text = getMachine.GetMachineName(machineIDLoad);

            dateTimePicker1.Text = getMachine.GetMachineStartWork(machineIDLoad);

            textBox4.Text = getMachine.GetMachineNote(machineIDLoad);

            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(category.GetCategoryName(getMachine.GetCategoryMachine(machineIDLoad)));
        }

        private void LoadForAdd()
        {
            LoadCategoryes();

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void LoadCategoryes()
        {
            ValueCategory getCategory = new ValueCategory(dataBase);

            List<String> categoryes = new List<String>(getCategory.GetCategoryesList());

            comboBox1.Items.AddRange(categoryes.ToArray());
        }

        private bool CheckCorrectInput()
        {
            bool result = true;

            if (textBox1.Text == "")
            {
                MessageBox.Show("Не введено название", "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрана категория", "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            return result;
        }

        private void AddNewMachine()
        {
            ValueInfoBase getMachine = new ValueInfoBase(dataBase);
            ValueCategory valueCategory = new ValueCategory(dataBase);

            String name = textBox1.Text;

            String dateStartWork = dateTimePicker1.Value.ToString("dd.MM.yyyy");

            String category = valueCategory.GetCategoryFromName(comboBox1.Text);

            String note = textBox4.Text;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;
                if (!_loadForEdit)
                    commandText = "INSERT INTO machines (category, name, dateStartWork, note) " +
                        "VALUES (@category, @name, @dateStartWork, @note)";
                else
                    commandText = "UPDATE machines SET category = @category, name = @name, dateStartWork = @dateStartWork, note = @note " +
                    "WHERE id = @machineIDLoad";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineIDLoad", machineIDLoad);
                Command.Parameters.AddWithValue("@category", category);
                Command.Parameters.AddWithValue("@name", name);
                Command.Parameters.AddWithValue("@dateStartWork", dateStartWork);
                Command.Parameters.AddWithValue("@note", note);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            String machineID = getMachine.GetMachineFromName(name);

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;

                commandText = "INSERT INTO machinesInfo (machine) " +
                    "SELECT * FROM (SELECT @machineID) " +
                    "AS tmp WHERE NOT EXISTS(SELECT machine FROM machinesInfo WHERE machine = @machineID) LIMIT 1";

                /*commandText = "INSERT INTO settings (userID) VALUES (@userID)";*/

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineID", machineID);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckCorrectInput())
            {
                AddNewMachine();
                Close();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
