using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormLoadUserPasswordForm : Form
    {
        bool passworEdit = false;

        String passKey = "key";
        String loadUser = "";

        public FormLoadUserPasswordForm(String loadUser)
        {
            InitializeComponent();
            this.loadUser = loadUser;
        }

        public FormLoadUserPasswordForm(bool editPass, String loadUser)
        {
            InitializeComponent();
            this.passworEdit = editPass;
            this.loadUser = loadUser;
        }

        private void LoadUserForm_Load(object sender, EventArgs e)
        {
            if (passworEdit)
            {
                Cryption pass = new Cryption();
                ValueUserBase userBase = new ValueUserBase();

                textBox1.Text = pass.DeCode(userBase.GetPasswordUser(loadUser), passKey);

                this.Text = "Установка/изменение пароля";
                button1.Text = "Применить";
            }
            //LoadUsersList();
        }

        private void ApplyButton(String currentUser)
        {
            if (passworEdit)
            {
                SetPassword(currentUser, textBox1.Text);
                Close();
            }
            else
            {
                LoadSelectedUser(currentUser);
            }
        }

        private void SetPassword(String currentUser, String password)
        {
            Cryption pass = new Cryption();
            ValueUserBase setUpdateUsers = new ValueUserBase();

            setUpdateUsers.UpdatePassword(currentUser, pass.Code(password, passKey));
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
            ApplyButton(loadUser);
            //Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                ApplyButton(loadUser);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!passworEdit)
            {
                FormLoadUserForm.enteredPasswordSuccess = false;
            }
            
            Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox1.PasswordChar = '\0';
            }
            else
            {
                textBox1.PasswordChar = '*';
            }
        }
    }
}
