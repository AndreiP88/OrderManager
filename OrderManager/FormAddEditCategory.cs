using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAddEditCategory : Form
    {
        string categoryIDLoad;

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

        class NormOperation
        {
            public int id;
            public string name;
            public NormOperation(int index, string nameNormOperation)
            {
                id = index;
                name = nameNormOperation;
            }
        }

        bool loadedFromBase = false;

        List<NormOperation> mainOperation = new List<NormOperation>();
        List<NormOperation> mkOperation = new List<NormOperation>();
        List<NormOperation> wkOperation = new List<NormOperation>();

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

        private void LoadForEdit()
        {
            this.Text = "Редактирование участок";
            button1.Text = "Сохранить";

            ValueCategory category = new ValueCategory();

            textBox1.Text = category.GetCategoryName(categoryIDLoad);

            SelectMainNormOperationIndex(categoryIDLoad);
        }

        private void SelectMainNormOperationIndex(string caatyegoryID)
        {
            ValueCategory category = new ValueCategory();

            int mainID = Convert.ToInt32(category.GetMainIDNormOperation(caatyegoryID));

            int mainIndex = mainOperation.FindIndex(v => v.id == mainID);

            int optionID = Convert.ToInt32(category.GetIDOptionView(caatyegoryID));

            int optionIndex = mainOperation.FindIndex(v => v.id == optionID);

            comboBox1.SelectedIndex = mainIndex;

            comboBox4.SelectedIndex = optionIndex;
        }

        private void SelectSubNormOperationIndex(string caatyegoryID)
        {
            ValueCategory category = new ValueCategory();

            int mkID = Convert.ToInt32(category.GetMKIDNormOperation(caatyegoryID));
            int wkID = Convert.ToInt32(category.GetWKIDNormOperation(caatyegoryID));

            int mkIndex = mkOperation.FindIndex(v => v.id == mkID);
            int wkIndex = wkOperation.FindIndex(v => v.id == wkID);

            if (mkIndex != -1)
            {
                comboBox2.SelectedIndex = mkIndex;
            }

            if (wkIndex != -1)
            {
                comboBox3.SelectedIndex = wkIndex;
            }
        }

        private void LoadMainNormOperation()
        {
            mainOperation.Clear();

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
                        CommandText = @"SELECT * FROM dbo.norm_operation WHERE (parentid is null)"
                    };
                    //Command.Parameters.AddWithValue("@order_num", "%" + textBox1.Text + "%");

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        mainOperation.Add(new NormOperation(
                            Convert.ToInt32(sqlReader["id_norm_operation"].ToString()),
                            sqlReader["operation_name"].ToString()
                            ));

                        comboBox1.Items.Add(sqlReader["operation_name"].ToString());
                        comboBox4.Items.Add(sqlReader["operation_name"].ToString());
                    }

                    connection.Close();
                }

                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                //сделать выбор для редактирования
                comboBox1.SelectedIndex = 0;

                comboBox4.DropDownStyle = ComboBoxStyle.DropDownList;
                //сделать выбор для редактирования
                comboBox4.SelectedIndex = 0;

                loadedFromBase = true;
            }
            catch
            {
                comboBox1.Text = "1";
                comboBox4.Text = "12";

                loadedFromBase = false;
            }
        }

        private void LoadSubNormOperation(int parentID)
        {
            mkOperation.Clear();
            wkOperation.Clear();

            comboBox2.Items.Clear();
            comboBox3.Items.Clear();

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
                        CommandText = @"SELECT * FROM dbo.norm_operation WHERE (parentid = @parentid)"
                    };
                    Command.Parameters.AddWithValue("@parentid", parentID);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        mkOperation.Add(new NormOperation(
                            Convert.ToInt32(sqlReader["id_norm_operation"].ToString()),
                            sqlReader["operation_name"].ToString()
                            ));

                        wkOperation.Add(new NormOperation(
                            Convert.ToInt32(sqlReader["id_norm_operation"].ToString()),
                            sqlReader["operation_name"].ToString()
                            ));

                        comboBox2.Items.Add(sqlReader["operation_name"].ToString());
                        comboBox3.Items.Add(sqlReader["operation_name"].ToString());
                    }

                    connection.Close();
                }

                comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
                //сделать выбор для редактирования
                comboBox2.SelectedIndex = 0;

                if (comboBox3.Items.Count > 0)
                {
                    comboBox3.SelectedIndex = 1;
                }
                else
                {
                    comboBox3.SelectedIndex = 0;
                }

                SelectSubNormOperationIndex(categoryIDLoad);
            }
            catch
            {
                //comboBox1.Text = "1";
            }


        }

        private void LoadForAdd()
        {

        }

        private bool CheckCorrectInput()
        {
            bool result = true;

            if (textBox1.Text == "" || comboBox1.SelectedIndex == -1 || comboBox2.SelectedIndex == -1 || comboBox3.SelectedIndex == -1 || comboBox4.SelectedIndex == -1)
            {
                MessageBox.Show("Не все поля заполнены и выбраны", "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            return result;
        }

        private void AddNewCategory()
        {
            string category = textBox1.Text;

            string mainNormOp = comboBox1.Text;
            string mkNormOp = comboBox2.Text;
            string wkNormOp = comboBox3.Text;
            string optionView = comboBox4.Text;

            if (loadedFromBase)
            {
                mainNormOp = mainOperation[comboBox1.SelectedIndex].id.ToString();
                mkNormOp = mkOperation[comboBox2.SelectedIndex].id.ToString();
                wkNormOp = wkOperation[comboBox3.SelectedIndex].id.ToString();
                optionView = mainOperation[comboBox4.SelectedIndex].id.ToString();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;
                if (!_loadForEdit)
                    commandText = "INSERT INTO machinesCategoryes (category, mainIdNormOperation, mkIdNormOperation, wkIdNormOperation, idOptionForView) " +
                        "VALUES (@category, @mainNormOp, @mkNormOp, @wkNormOp, @optionView)";
                else
                    commandText = "UPDATE machinesCategoryes SET category = @category, mainIdNormOperation = @mainNormOp, mkIdNormOperation = @mkNormOp, wkIdNormOperation = @wkNormOp, idOptionForView = @optionView " +
                    "WHERE id = @categoryIDLoad";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@categoryIDLoad", categoryIDLoad);
                Command.Parameters.AddWithValue("@category", category);
                Command.Parameters.AddWithValue("@mainNormOp", mainNormOp);
                Command.Parameters.AddWithValue("@mkNormOp", mkNormOp);
                Command.Parameters.AddWithValue("@wkNormOp", wkNormOp);
                Command.Parameters.AddWithValue("@optionView", optionView);

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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSubNormOperation(mainOperation[comboBox1.SelectedIndex].id);
        }
    }
}
