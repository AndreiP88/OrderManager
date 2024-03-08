using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.DataBaseReconnect;

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

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
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

                        reconnectionRequired = false;
                    }
                    catch (Exception ex)
                    {
                        LogException.WriteLine(ex.StackTrace + "; " + ex.Message);

                        dialog = DataBaseReconnectionRequest(ex.Message);

                        if (dialog == DialogResult.Retry)
                        {
                            reconnectionRequired = true;
                        }
                        if (dialog == DialogResult.Abort || dialog == DialogResult.Cancel)
                        {
                            reconnectionRequired = false;
                            Application.Exit();
                        }
                    }
                }
            }
            while (reconnectionRequired);
        }

        private async Task UpdateMachineFromUsers()
        {
            ValueInfoBase getMachine = new ValueInfoBase();

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
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

                        reconnectionRequired = false;
                    }
                    catch (Exception ex)
                    {
                        LogException.WriteLine(ex.StackTrace + "; " + ex.Message);

                        dialog = DataBaseReconnectionRequest(ex.Message);

                        if (dialog == DialogResult.Retry)
                        {
                            reconnectionRequired = true;
                        }
                        if (dialog == DialogResult.Abort || dialog == DialogResult.Cancel)
                        {
                            reconnectionRequired = false;
                            Application.Exit();
                        }
                    }
                }
            }
            while (reconnectionRequired);
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

            bool reconnectionRequired = false;
            DialogResult dialog = DialogResult.Retry;

            do
            {
                if (!Form1._viewDatabaseRequestForm && dialog == DialogResult.Retry)
                {
                    try
                    {
                        if (listView1.SelectedItems.Count != 0)
                        {
                            string currentUser = listView1.SelectedItems[0].Name;
                            string userPass = userValue.GetPasswordUser(currentUser);
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

                        reconnectionRequired = false;
                    }
                    catch (Exception ex)
                    {
                        LogException.WriteLine(ex.StackTrace + "; " + ex.Message);

                        dialog = DataBaseReconnectionRequest(ex.Message);

                        if (dialog == DialogResult.Retry)
                        {
                            reconnectionRequired = true;
                        }
                        if (dialog == DialogResult.Abort || dialog == DialogResult.Cancel)
                        {
                            reconnectionRequired = false;
                            Application.Exit();
                        }
                    }
                }
            }
            while (reconnectionRequired);
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

        private void FormLoadUserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
        }
    }
}
