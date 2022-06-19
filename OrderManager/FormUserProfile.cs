using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            GetValueFromUserBase userBase = new GetValueFromUserBase(dataBase);
            GetValueFromSettingsBase settingsBase = new GetValueFromSettingsBase(dataBase);

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
            SetUpdateUsersBase setUpdateUsers = new SetUpdateUsersBase(dataBase);
            SetUpdateSettingsValue updateSettingsValue = new SetUpdateSettingsValue(dataBase);

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
