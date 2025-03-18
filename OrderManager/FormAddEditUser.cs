using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;
using System.Linq;

namespace OrderManager
{
    public partial class FormAddEditUser : Form
    {
        String userIDLoad;

        bool _loadForEdit;

        /// <summary>
        /// Добавление нового сотрудника
        /// </summary>
        /// <param name=""></param>
        public FormAddEditUser()
        {
            InitializeComponent();

            _loadForEdit = false;
        }

        /// <summary>
        /// Редактировние выбранного сотрудника
        /// </summary>
        /// <param name=""></param>
        /// <param name="userID"></param>
        public FormAddEditUser(String userID)
        {
            InitializeComponent();

            this.userIDLoad = userID;

            _loadForEdit = true;

        }

        class UserName
        {
            public string LastName;
            public string FirstName;
            public string MiddleName;
            public string indexUser;
            public UserName(string nLastName, string nFirstName, string nMiddleName, string indexUser)
            {
                this.LastName = nLastName;
                this.FirstName = nFirstName;
                this.MiddleName = nMiddleName;
                this.indexUser = indexUser;
            }
        }

        List<UserName> userNames = new List<UserName>();

        private void FormAddEditUser_Load(object sender, EventArgs e)
        {
            if (_loadForEdit)
            {
                LoadForEdit();
            }
            else
            {
                LoadForAdd();
                LoadMainNormOperation();
            }
        }

        private void LoadForEdit()
        {
            this.Text = "Редактирование сотрудника";
            button1.Text = "Сохранить";

            ValueCategory category = new ValueCategory();
            StringArray stringArray = new StringArray();

            ValueUserBase getUser = new ValueUserBase();
            UserInfo getInfo = (UserInfo)getUser.GetUserInfoFromID(userIDLoad);

            LoadCategoryes();

            String[] arrayCat = stringArray.ArrayFromTheString(getInfo.categoryesMachine);

            textBox1.Text = getInfo.surname;
            textBox2.Text = getInfo.name;
            textBox3.Text = getInfo.patronymic;
            textBox5.Text = string.Join(";", getInfo.indexUser);

            dateTimePicker1.Text = getInfo.dateOfBirth;
            dateTimePicker2.Text = getInfo.dateOfEmployment;

            if (getInfo.dateOfDismissal == "")
            {
                dateTimePicker3.Visible = false;
                label6.Visible = false;
                dateTimePicker3.Value = dateTimePicker3.MinDate;

                button3.Visible = true;
            }
            else
            {
                dateTimePicker3.Visible = true;
                label6.Visible = true;
                dateTimePicker3.Text = getInfo.dateOfDismissal;

                button3.Visible = false;
            }

            checkBox1.Checked = Convert.ToBoolean(getInfo.activeUser);

            textBox4.Text = getInfo.note;

            for (int i = 0; i < arrayCat.Length; i++)
            {
                listView1.Items[listView1.Items.IndexOfKey(arrayCat[i])].Checked = true;
            }
        }

        private void LoadForAdd()
        {
            label6.Visible = false;

            dateTimePicker3.Visible = false;
            dateTimePicker3.Value = dateTimePicker3.MinDate;

            button3.Visible = false;

            LoadCategoryes();
        }

        private void LoadMainNormOperation()
        {
            userNames.Clear();

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
                        //CommandText = @"SELECT * FROM dbo.common_employee WHERE fire_date IS null"
                        CommandText = @"SELECT
	                            * 
                            FROM
	                            dbo.common_employee
	                            INNER JOIN common_employee_speciality ON common_employee.id_common_employee_speciality = common_employee_speciality.id_common_employee_speciality 
                            WHERE
	                            fire_date IS NULL"

                    };
                    //Command.Parameters.AddWithValue("@order_num", "%" + textBox1.Text + "%");

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        userNames.Add(new UserName(
                            sqlReader["employee_lastname"].ToString(),
                            sqlReader["employee_firstname"].ToString(),
                            sqlReader["employee_middlename"].ToString(),
                            sqlReader["id_common_employee"].ToString()
                            ));

                        comboBox1.Items.Add(sqlReader["employee_speciality_name"].ToString() + ": " +
                            sqlReader["employee_lastname"].ToString() + " " +
                            sqlReader["employee_firstname"].ToString() + " " +
                            sqlReader["employee_middlename"].ToString());
                    }

                    connection.Close();
                }

                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                //сделать выбор для редактирования
                comboBox1.SelectedIndex = -1;
            }
            catch
            {
                comboBox1.Items.Clear();
            }
        }

        private void LoadCategoryes()
        {
            ValueCategory getCategory = new ValueCategory();

            List<string> categoryes = new List<string>(getCategory.GetCategoryesList());

            for (int i = 0; i < categoryes.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = getCategory.GetCategoryFromName(categoryes[i]);
                item.Text = categoryes[i];

                listView1.Items.Add(item);
            }
        }

        private string GetCategoryesFromLV()
        {
            StringArray str = new StringArray();

            String result = "";

            String[] indexes = new String[listView1.CheckedIndices.Count];
            //listView1.CheckedIndices[]
            //indexes.Concat<int>(listView1.CheckedIndices);

            for (int i = 0; i < listView1.CheckedIndices.Count; i++)
            {
                indexes[i] = listView1.Items[listView1.CheckedIndices[i]].Name;
            }

            result = str.StringFromTheArray(indexes);

            return result;
        }

        private bool CheckCorrectInput()
        {
            bool result = true;

            if (textBox1.Text == "")
            {
                MessageBox.Show("Не введена фамилия", "Ошибка", MessageBoxButtons.OK);
                return false;
            }


            if (textBox2.Text == "")
            {
                MessageBox.Show("Не введено имя", "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            if (textBox3.Text == "")
            {
                MessageBox.Show("Не введено отчество", "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            if (listView1.CheckedIndices.Count < 1)
            {
                MessageBox.Show("Необходимо выбрать хотябы один участок работы", "Ошибка", MessageBoxButtons.OK);
                return false;
            }

            return result;
        }

        private void AddNewUser()
        {
            ValueUserBase getUser = new ValueUserBase();

            String shortName = textBox2.Text + " " + textBox1.Text;

            String surname = textBox1.Text;
            String name = textBox2.Text;
            String patronymic = textBox3.Text;

            String dateOfBirth = dateTimePicker1.Value.ToString("dd.MM.yyyy");
            String dateOfEmployment = dateTimePicker2.Value.ToString("dd.MM.yyyy");
            String dateOfDismissal = "";
            String activeUser = checkBox1.Checked.ToString();

            String categoryesMachine = GetCategoryesFromLV();

            if (dateTimePicker3.Value != dateTimePicker3.MinDate)
                dateOfDismissal = dateTimePicker3.Value.ToString("dd.MM.yyyy");

            String note = textBox4.Text;

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;
                if (!_loadForEdit)
                    commandText = "INSERT INTO users (nameUser, surname, name, patronymic, categoryesMachine, dateOfBirth, dateOfEmployment, activeUser, note) " +
                        "VALUES (@nameUser, @surname, @name, @patronymic, @categoryesMachine, @dateOfBirth, @dateOfEmployment, @activeUser, @note)";
                else
                    commandText = "UPDATE users SET nameUser = @nameUser, surname = @surname, name = @name, patronymic = @patronymic, categoryesMachine = @categoryesMachine, " +
                    "dateOfBirth = @dateOfBirth, dateOfEmployment = @dateOfEmployment, activeUser = @activeUser, dateOfDismissal = @dateOfDismissal, note = @note " +
                    "WHERE id = @userIDLoad";

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@userIDLoad", userIDLoad);
                Command.Parameters.AddWithValue("@nameUser", shortName);
                Command.Parameters.AddWithValue("@surname", surname);
                Command.Parameters.AddWithValue("@name", name);
                Command.Parameters.AddWithValue("@patronymic", patronymic);
                Command.Parameters.AddWithValue("@categoryesMachine", categoryesMachine);
                Command.Parameters.AddWithValue("@dateOfBirth", dateOfBirth);
                Command.Parameters.AddWithValue("@dateOfEmployment", dateOfEmployment);
                Command.Parameters.AddWithValue("@activeUser", activeUser);
                Command.Parameters.AddWithValue("@dateOfDismissal", dateOfDismissal);
                Command.Parameters.AddWithValue("@note", note);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            int userID = getUser.GetIDUserFromName(shortName);

            string indexUserFromAS = textBox5.Text;
            List<int> indexes = indexUserFromAS?.Split(';')?.Select(Int32.Parse)?.ToList();

            getUser.AddNewASUsersIndexes(userID, indexes);

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;

                commandText = "INSERT INTO usersSettings (userID) " +
                    "SELECT * FROM (SELECT @userID) " +
                    "AS tmp WHERE NOT EXISTS(SELECT userID FROM usersSettings WHERE userID = @userID) LIMIT 1";

                /*commandText = "INSERT INTO settings (userID) VALUES (@userID)";*/

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@userID", userID);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;

                commandText = "INSERT INTO usersInfo (user) " +
                    "SELECT * FROM (SELECT @userID) " +
                    "AS tmp WHERE NOT EXISTS(SELECT user FROM usersInfo WHERE user = @userID) LIMIT 1";

                /*commandText = "INSERT INTO settings (userID) VALUES (@userID)";*/

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@userID", userID);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                string commandText;

                commandText = "INSERT INTO usersWindowState (userID) " +
                    "SELECT * FROM (SELECT @userID) " +
                    "AS tmp WHERE NOT EXISTS(SELECT userID FROM usersWindowState WHERE userID = @userID) LIMIT 1";

                /*commandText = "INSERT INTO settings (userID) VALUES (@userID)";*/

                MySqlCommand Command = new MySqlCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@userID", userID);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CheckCorrectInput())
            {
                AddNewUser();
                Close();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dateTimePicker3.Visible = true;
            dateTimePicker3.Value = DateTime.Now;

            label6.Visible = true;

            button3.Visible = false;

            checkBox1.Checked = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = userNames[comboBox1.SelectedIndex].LastName;
            textBox2.Text = userNames[comboBox1.SelectedIndex].FirstName;
            textBox3.Text = userNames[comboBox1.SelectedIndex].MiddleName;
            textBox5.Text = userNames[comboBox1.SelectedIndex].indexUser.ToString();
        }
    }
}
