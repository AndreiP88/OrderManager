using System;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormUserProfile : Form
    {
        String dataBase;
        String idUser;
        String passKey = "key";

        public FormUserProfile(String dBase, String userID)
        {
            InitializeComponent();

            this.dataBase = dBase;
            this.idUser = userID;
        }

        private void FormUserProfile_Load(object sender, EventArgs e)
        {
            LoadUser();
        }

        void LoadUser()
        {
            Cryption pass = new Cryption();
            ValueUserBase userBase = new ValueUserBase(dataBase);
            ValueSettingsBase settingsBase = new ValueSettingsBase(dataBase);

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
            ValueUserBase setUpdateUsers = new ValueUserBase(dataBase);
            ValueSettingsBase updateSettingsValue = new ValueSettingsBase(dataBase);

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
    }
}
