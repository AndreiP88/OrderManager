﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

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

        private void LoadCategoryes()
        {
            ValueCategory getCategory = new ValueCategory();

            List<String> categoryes = new List<String>(getCategory.GetCategoryesList());

            for (int i = 0; i < categoryes.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = getCategory.GetCategoryFromName(categoryes[i]);
                item.Text = categoryes[i];

                listView1.Items.Add(item);
            }
        }

        private String GetCategoryesFromLV()
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

            String userID = getUser.GetIDUserFromName(shortName);

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
    }
}
