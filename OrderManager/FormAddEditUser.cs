using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAddEditUser : Form
    {
        String dataBase;
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String userIDLoad;

        bool _loadForEdit;

        /// <summary>
        /// Добавление нового сотрудника
        /// </summary>
        /// <param name="dataBase"></param>
        public FormAddEditUser(String dataBase)
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
        public FormAddEditUser(String dataBase, String userID)
        {
            InitializeComponent();

            this.dataBase = dataBase;
            this.userIDLoad = userID;

            _loadForEdit = true;

            DataBasePatch(dataBase);
        }

        private String DataBasePatch (String dBase)
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
            this.Text = "Редактирование сотрудника";
            button1.Text = "Сохранить";

            ValueCategory category = new ValueCategory(dataBase);
            StringArray stringArray = new StringArray();

            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);
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
            }
            else
            {
                dateTimePicker3.Visible = true;
                label6.Visible = true;
                dateTimePicker3.Text = getInfo.dateOfDismissal;
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

            LoadCategoryes();
        }

        private void LoadCategoryes()
        {
            ValueCategory getCategory = new ValueCategory(dataBase);

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
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            String shortName = textBox1.Text + " " + textBox2.Text;

            String surname = textBox1.Text;
            String name = textBox2.Text;
            String patronymic = textBox3.Text;

            String dateOfBirth = dateTimePicker1.Value.ToString("dd.MM.yyyy");
            String dateOfEmployment = dateTimePicker2.Value.ToString("dd.MM.yyyy"); ;
            String activeUser = checkBox1.Checked.ToString();

            String categoryesMachine = GetCategoryesFromLV();

            String note = textBox4.Text;

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText;
                if (!_loadForEdit)
                    commandText = "INSERT INTO users (nameUser, surname, name, patronymic, categoryesMachine, dateOfBirth, dateOfEmployment, activeUser, note) " +
                        "VALUES (@nameUser, @surname, @name, @patronymic, @categoryesMachine, @dateOfBirth, @dateOfEmployment, @activeUser, @note)";
                else
                    commandText = "UPDATE users SET nameUser = @nameUser, surname = @surname, name = @name, patronymic = @patronymic, categoryesMachine = @categoryesMachine, " +
                    "dateOfBirth = @dateOfBirth, dateOfEmployment = @dateOfEmployment, activeUser = @activeUser, note = @note " +
                    "WHERE id = @userIDLoad";


                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@userIDLoad", userIDLoad);
                Command.Parameters.AddWithValue("@nameUser", shortName);
                Command.Parameters.AddWithValue("@surname", surname);
                Command.Parameters.AddWithValue("@name", name);
                Command.Parameters.AddWithValue("@patronymic", patronymic);
                Command.Parameters.AddWithValue("@categoryesMachine", categoryesMachine);
                Command.Parameters.AddWithValue("@dateOfBirth", dateOfBirth);
                Command.Parameters.AddWithValue("@dateOfEmployment", dateOfEmployment);
                Command.Parameters.AddWithValue("@activeUser", activeUser);
                Command.Parameters.AddWithValue("@note", note);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }

            String userID = getUser.GetIDUserFromName(shortName);

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText;
                
                commandText = "INSERT INTO settings (userID) SELECT * FROM (SELECT @userID) " +
                    "AS tmp WHERE NOT EXISTS(SELECT userID FROM settings WHERE (userID = @userID) LIMIT 1";

                /*commandText = "INSERT INTO settings (userID) VALUES (@userID)";*/

                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
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

        }
    }
}
