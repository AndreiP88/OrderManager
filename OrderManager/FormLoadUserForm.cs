using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace OrderManager
{
    public partial class FormLoadUserForm : Form
    {
        public static bool enteredPasswordSuccess = false;

        String loadMode = "";

        public FormLoadUserForm(String loadMode)
        {
            InitializeComponent();
            this.loadMode = loadMode;   
        }

        List<String> users;

        private void LoadUserForm_Load(object sender, EventArgs e)
        {
            LoadUsersList();
            timer1.Enabled = true;
        }

        private void LoadUsersList()
        {
            ValueInfoBase getMachine = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();

            //List<String> users = userBase.GetUserList(true);
            //users.Clear();

            users = userBase.GetUserListForCategory(true, loadMode);

            int counter = 0;

            for (int i = 0; i < users.Count; i++) // считываем и вносим в комбобокс список заголовков
            {
                //String machines = getMachine.GetMachinesStr(users[i].ToString());

                counter++;

                ListViewItem item = new ListViewItem();
                item.Name = users[i].ToString();
                item.Text = counter.ToString();
                item.SubItems.Add(userBase.GetNameUser(users[i]));
                //item.SubItems.Add(machines);
                item.SubItems.Add("");
                listView1.Items.Add(item);
            }

            UpdateMachineFromUsers();
        }

        private void UpdateMachineFromUsers()
        {
            ValueInfoBase getMachine = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();

            //int index = listView1.Items.IndexOfKey(sqlReader["id"].ToString());

            //users = userBase.GetUserListForCategory(true, loadMode);

            for (int i = 0; i < users.Count; i++)
            {
                String machines = getMachine.GetMachinesStr(users[i].ToString());

                ListViewItem item = listView1.Items[i];

                if (item != null)
                {
                    item.SubItems[2].Text = machines;
                }
            }
        }

        private void LoadSelectedUser()
        {
            Cryption pass = new Cryption();
            ValueUserBase userValue = new ValueUserBase();
            ValueSettingsBase settingsValue = new ValueSettingsBase();
            ValueInfoBase infoBase = new ValueInfoBase();

            if (listView1.SelectedItems.Count != 0)
            {
                String currentUser = listView1.SelectedItems[0].Name;
                String userPass = userValue.GetPasswordUser(currentUser);
                bool checkPass = false;
                bool checkPassword = false;

                if (settingsValue.GetPasswordChecked(currentUser) != "")
                    checkPass = Convert.ToBoolean(settingsValue.GetPasswordChecked(currentUser));

                if (userValue.GetUserWorking(currentUser) == true && checkPass == true)
                    checkPassword = true;

                if ((checkPassword == true && pass.GetMotherBoard_ID() == userValue.GetLastUID(currentUser)) || userPass == "")
                {
                    Form1.Info.nameOfExecutor = currentUser;
                    Close();
                }
                else if (ChekPassword(currentUser))
                {
                    Close();
                }
            }

            //Form1 form = new Form1();
            //form.Visible = true;
        }

        private bool ChekPassword(String loadUser)
        {
            bool result = false;

            FormLoadUserPasswordForm form = new FormLoadUserPasswordForm(loadUser);
            form.ShowDialog();

            if (enteredPasswordSuccess)
                result = true;

            return result;
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
            FormFullListOrders form = new FormFullListOrders(false, "", "", "");
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateMachineFromUsers();
        }
    }
}
