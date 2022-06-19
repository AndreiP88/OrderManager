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
    public partial class FormLoadUserForm : Form
    {

        String passKey = "key";
        String dataBase;
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";

        public FormLoadUserForm(String dBase)
        {
            InitializeComponent();

            this.dataBase = dBase;
            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        private void LoadUserForm_Load(object sender, EventArgs e)
        {
            LoadUsersList();
        }

        private void LoadUsersList()
        {
            GetUserIDOrMachineFromInfoBase getMachine = new GetUserIDOrMachineFromInfoBase(dataBase);

            int counter = 0;

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users WHERE activeUser = 'True'"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    String machines = getMachine.GetMachinesStr(sqlReader["id"].ToString());

                    counter++;

                    ListViewItem item = new ListViewItem();
                    item.Name = sqlReader["id"].ToString();
                    item.Text = counter.ToString();
                    item.SubItems.Add(sqlReader["nameUser"].ToString());
                    item.SubItems.Add(machines);
                    listView1.Items.Add(item);
                }

                Connect.Close();
            }
        }

        private void LoadSelectedUser()
        {
            Cryption pass = new Cryption();
            GetValueFromUserBase userValue = new GetValueFromUserBase(dataBase);
            GetValueFromSettingsBase settingsValue = new GetValueFromSettingsBase(dataBase);
            GetValueFromInfoBase infoBase = new GetValueFromInfoBase(dataBase);

            if (listView1.SelectedItems.Count != 0)
            {
                String currentUser = listView1.SelectedItems[0].Name;
                String userPass = userValue.GetPasswordUser(currentUser);
                String enterPass = textBox1.Text;
                bool checkPass = false;
                bool checkPassword = false;

                if (settingsValue.GetPasswordChecked(currentUser) != "")
                    checkPass = Convert.ToBoolean(settingsValue.GetPasswordChecked(currentUser));

                if (userValue.GetUserWorking(currentUser) == true && checkPass == true)
                    checkPassword = true;

                if (enterPass == pass.DeCode(userPass, passKey) || checkPassword == true)
                {
                    Form1.Info.nameOfExecutor = currentUser;
                    Close();
                }
                else
                {
                    MessageBox.Show("Введен неверный пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            //Form1 form = new Form1();
            //form.Visible = true;
        }

        private void EnabledButton()
        {
            if (listView1.SelectedItems.Count != 0)
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadSelectedUser();
            //Close();
        }

        private void ShowFullOrdersForm()
        {
            FormFullListOrders form = new FormFullListOrders(dataBase, false, "", "", "");
            form.ShowDialog();
        }

        private void LoadUserForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Application.Exit();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            EnabledButton();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ShowFullOrdersForm();
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            LoadSelectedUser();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                LoadSelectedUser();
            }
        }
    }
}
