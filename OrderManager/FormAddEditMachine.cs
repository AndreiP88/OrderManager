using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OrderManager
{
    public partial class FormAddEditMachine : Form
    {
        String machineIDLoad;

        bool _loadForEdit;

        /// <summary>
        /// Добавление нового сотрудника
        /// </summary>
        /// <param name=""></param>
        public FormAddEditMachine()
        {
            InitializeComponent();

            _loadForEdit = false;
        }

        /// <summary>
        /// Редактировние выбранного сотрудника
        /// </summary>
        /// <param name=""></param>
        /// <param name="userID"></param>
        public FormAddEditMachine(String machineID)
        {
            InitializeComponent();

            this.machineIDLoad = machineID;

            _loadForEdit = true;
        }

        class EquipName
        {
            public int id;
            public string name;
            public EquipName(int index, string nameEquip)
            {
                id = index;
                name = nameEquip;
            }
        }

        bool loadedFromBase = false;

        List<EquipName> eNames = new List<EquipName>();

        private void FormAddEditUser_Load(object sender, EventArgs e)
        {
            LoadMainNormOperation();

            if (_loadForEdit)
            {
                LoadForEdit();
            }
            else
            {
                LoadForAdd();
            }
        }

        private async void LoadForEdit()
        {
            this.Text = "Редактирование оборудование";
            button1.Text = "Сохранить";

            ValueCategory category = new ValueCategory();
            ValueInfoBase getMachine = new ValueInfoBase();

            LoadCategoryes();

            textBox1.Text = await getMachine.GetMachineName(machineIDLoad);

            dateTimePicker1.Text = await getMachine.GetMachineStartWork(machineIDLoad);

            textBox4.Text = await getMachine.GetMachineNote(machineIDLoad);

            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(category.GetCategoryName(await getMachine.GetCategoryMachine(machineIDLoad)));

            SelectMainNormOperationIndex(machineIDLoad);
        }

        private void SelectMainNormOperationIndex(string machine)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            int mainID = Convert.ToInt32(infoBase.GetIDEquipMachine(machine));

            int mainIndex = eNames.FindIndex(v => v.id == mainID);

            comboBox2.SelectedIndex = mainIndex;
        }

        private void LoadForAdd()
        {
            LoadCategoryes();

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

            textBox1.Text = "";
        }

        private void LoadCategoryes()
        {
            ValueCategory getCategory = new ValueCategory();

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

            if (comboBox2.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбрано используемое оборудование", "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            return result;
        }

        private void LoadMainNormOperation()
        {
            eNames.Clear();

            string connectionString = @"Data Source = SRV-ACS\DSACS; Initial Catalog = asystem; Persist Security Info = True; User ID = ds; Password = 1";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = connection,
                        //CommandText = @"SELECT * FROM dbo.order_head WHERE status = '1' AND order_num LIKE '@order_num'"
                        CommandText = @"SELECT * FROM dbo.common_equip_directory"
                    };
                    //Command.Parameters.AddWithValue("@order_num", "%" + textBox1.Text + "%");

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        eNames.Add(new EquipName(
                            Convert.ToInt32(sqlReader["id_common_equip_directory"].ToString()),
                            sqlReader["equip_name"].ToString()
                            ));

                        comboBox2.Items.Add(sqlReader["equip_name"].ToString());
                    }

                    connection.Close();
                }

                comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                //сделать выбор для редактирования
                comboBox2.SelectedIndex = 0;

                loadedFromBase = true;
            }
            catch
            {
                comboBox2.Text = "1";

                loadedFromBase = false;
            }
        }

        private async void AddNewMachine()
        {
            ValueInfoBase getMachine = new ValueInfoBase();
            ValueCategory valueCategory = new ValueCategory();

            String name = textBox1.Text;

            String dateStartWork = dateTimePicker1.Value.ToString("dd.MM.yyyy");

            String category = valueCategory.GetCategoryFromName(comboBox1.Text);

            String note = textBox4.Text;

            string equip = comboBox2.Text;

            if (loadedFromBase)
            {
                equip = eNames[comboBox2.SelectedIndex].id.ToString();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;
                if (!_loadForEdit)
                    commandText = "INSERT INTO machines (category, name, idEquip, dateStartWork, note) " +
                        "VALUES (@category, @name, @idEquip, @dateStartWork, @note)";
                else
                    commandText = "UPDATE machines SET category = @category, name = @name, idEquip = @idEquip, dateStartWork = @dateStartWork, note = @note " +
                    "WHERE id = @machineIDLoad";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machineIDLoad", machineIDLoad);
                Command.Parameters.AddWithValue("@category", category);
                Command.Parameters.AddWithValue("@name", name);
                Command.Parameters.AddWithValue("@idEquip", equip);
                Command.Parameters.AddWithValue("@dateStartWork", dateStartWork);
                Command.Parameters.AddWithValue("@note", note);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            string machineID = await getMachine.GetMachineFromName(name);

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

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = comboBox2.Text;
            }
        }
    }
}
