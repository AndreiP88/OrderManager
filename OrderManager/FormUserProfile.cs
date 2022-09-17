using System;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormUserProfile : Form
    {
        String idUser;
        String passKey = "key";

        public FormUserProfile(String userID)
        {
            InitializeComponent();

            this.idUser = userID;
        }

        private void FormUserProfile_Load(object sender, EventArgs e)
        {
            LoadUser();
        }

        void LoadUser()
        {
            Cryption pass = new Cryption();
            ValueUserBase userBase = new ValueUserBase();
            ValueSettingsBase settingsBase = new ValueSettingsBase();

            bool checkPass = false;

            if (settingsBase.GetPasswordChecked(idUser) != "")
                checkPass = Convert.ToBoolean(settingsBase.GetPasswordChecked(idUser));

            textBox1.Text = userBase.GetNameUser(idUser);
            textBox2.Text = pass.DeCode(userBase.GetPasswordUser(idUser), passKey);
            checkBox1.Checked = checkPass;
        }

        void SaveUser()
        {
            Cryption pass = new Cryption();
            ValueUserBase setUpdateUsers = new ValueUserBase();
            ValueSettingsBase updateSettingsValue = new ValueSettingsBase();

            setUpdateUsers.UpdateName(idUser, textBox1.Text);
            setUpdateUsers.UpdatePassword(idUser, pass.Code(textBox2.Text, passKey));
            updateSettingsValue.UpdateCheckPassword(idUser, checkBox1.Checked.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveUser();
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ValueUserBase setValueUsers = new ValueUserBase();

            setValueUsers.UpdateLastUID(idUser, "");
        }
    }
}
