using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormLoadUserPasswordForm : Form
    {

        String passKey = "key";
        String loadUser = "";

        public FormLoadUserPasswordForm(String loadUser)
        {
            InitializeComponent();
            this.loadUser = loadUser;
        }

        private void LoadUserForm_Load(object sender, EventArgs e)
        {
            //LoadUsersList();
        }

        private void LoadSelectedUser(String currentUser)
        {
            Cryption pass = new Cryption();
            ValueUserBase userValue = new ValueUserBase();

            String userPass = userValue.GetPasswordUser(currentUser);
            String enterPass = textBox1.Text;

            if (enterPass == pass.DeCode(userPass, passKey))
            {
                userValue.UpdateLastUID(currentUser, pass.GetMotherBoard_ID());

                Form1.Info.nameOfExecutor = currentUser;
                FormLoadUserForm.enteredPasswordSuccess = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введен неверный пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadSelectedUser(loadUser);
            //Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                LoadSelectedUser(loadUser);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FormLoadUserForm.enteredPasswordSuccess = false;
            Close();
        }
    }
}
