using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormLoadUserForm : Form
    {
        public static bool enteredPasswordSuccess = false;

        public FormLoadUserForm()
        {
            InitializeComponent();
        }

        List<String> users;

        private async void LoadUserForm_Load(object sender, EventArgs e)
        {
            await LoadUsersList();
            UpdateCurrentDateTime();

            timer1.Enabled = true;
        }

        private async Task LoadUsersList()
        {
            //ValueInfoBase getMachine = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();
            StringArray str = new StringArray();
            INISettings settings = new INISettings();

            string selectedCategory = settings.GetCategoryesForView();
            string[] arrayCat = str.ArrayFromTheString(selectedCategory);

            listView1.Items.Clear();
            //List<String> users = userBase.GetUserList(true);
            //users.Clear();

            users = userBase.GetUserListForCategory(true, arrayCat);

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

            await UpdateMachineFromUsers();
        }

        private async Task UpdateMachineFromUsers222()
        {
            ValueInfoBase getMachine = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();

            //int index = listView1.Items.IndexOfKey(sqlReader["id"].ToString());

            //users = userBase.GetUserListForCategory(true, loadMode);

            bool reconnectionRequired = false;

            do
            {
                reconnectionRequired = false;

                //if (!Form1._viewDatabaseRequestForm)
                {
                    try
                    {
                        for (int i = 0; i < users.Count; i++)
                        {
                            string machines = await getMachine.GetMachinesStr(users[i].ToString());

                            ListViewItem item = listView1.Items[i];

                            if (item != null)
                            {
                                item.SubItems[2].Text = machines;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException.WriteLine(ex.Message);
                        ////Form1._viewDatabaseRequestForm = true;
                        //reconnectionRequired = DataBaseReconnect.DataBaseReconnectionRequest(ex.Message);
                    }
                }
            }
            while (reconnectionRequired);
        }

        private async Task UpdateMachineFromUsers()
        {
            ValueInfoBase getMachine = new ValueInfoBase();
            ValueUserBase userBase = new ValueUserBase();

            //int index = listView1.Items.IndexOfKey(sqlReader["id"].ToString());

            //users = userBase.GetUserListForCategory(true, loadMode);

            for (int i = 0; i < users.Count; i++)
            {
                string machines = await getMachine.GetMachinesStr(users[i].ToString());

                ListViewItem item = listView1.Items[i];

                if (item != null)
                {
                    item.SubItems[2].Text = machines;
                }
            }
        }

        private void UpdateCurrentDateTime()
        {
            string currentTime = DateTime.Now.ToString("f");

            toolStripStatusLabel1.Text = "   " + currentTime;
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

        private bool ChekPassword(string loadUser)
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
            FormFullListOrders form = new FormFullListOrders(false, -1);
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

        private async void timer1_Tick(object sender, EventArgs e)
        {
            await UpdateMachineFromUsers();
            UpdateCurrentDateTime();
        }

        private async Task LoadFormDataBaseSelect()
        {
            FormAddEditTestMySQL form = new FormAddEditTestMySQL(true);
            form.ShowDialog();

            DBConnection connection = new DBConnection();

            connection.SetDBParameter();

            timer1.Enabled = false;

            users?.Clear();

            await LoadUsersList();

            timer1.Enabled = true;
        }

        private async Task LoadFormCategoryesSelect()
        {
            FormSelectCategory form = new FormSelectCategory();
            form.ShowDialog();

            timer1.Enabled = false;

            await LoadUsersList();

            timer1.Enabled = true;
        }

        private void FormLoadUserForm_DoubleClick(object sender, EventArgs e)
        {
            //LoadFormDataBaseSelect();
        }

        private async void selectDataBaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await LoadFormDataBaseSelect();
        }

        private async void selectCategoryesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await LoadFormCategoryesSelect();
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
