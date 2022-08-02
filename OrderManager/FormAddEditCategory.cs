using MySql.Data.MySqlClient;
using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAddEditCategory : Form
    {
        String categoryIDLoad;

        bool _loadForEdit;

        /// <summary>
        /// Добавление нового сотрудника
        /// </summary>
        /// <param name=""></param>
        public FormAddEditCategory()
        {
            InitializeComponent();

            _loadForEdit = false;
        }

        /// <summary>
        /// Редактировние выбранного сотрудника
        /// </summary>
        /// <param name=""></param>
        /// <param name="userID"></param>
        public FormAddEditCategory(String categoryID)
        {
            InitializeComponent();

            this.categoryIDLoad = categoryID;

            _loadForEdit = true;
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
            this.Text = "Редактирование участок";
            button1.Text = "Сохранить";

            ValueCategory category = new ValueCategory();

            textBox1.Text = category.GetCategoryName(categoryIDLoad);
        }

        private void LoadForAdd()
        {

        }

        private bool CheckCorrectInput()
        {
            bool result = true;

            if (textBox1.Text == "")
            {
                MessageBox.Show("Не введено название участка", "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            return result;
        }

        private void AddNewCategory()
        {
            String category = textBox1.Text;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;
                if (!_loadForEdit)
                    commandText = "INSERT INTO machinesCategoryes (category) " +
                        "VALUES (@category)";
                else
                    commandText = "UPDATE machinesCategoryes SET category = @category " +
                    "WHERE id = @categoryIDLoad";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@categoryIDLoad", categoryIDLoad);
                Command.Parameters.AddWithValue("@category", category);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckCorrectInput())
            {
                AddNewCategory();
                Close();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
