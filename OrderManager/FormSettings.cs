using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormSettings : Form
    {
        string user = "";

        public FormSettings(string userID)
        {
            InitializeComponent();

            this.user = userID;
        }

        private void LoadValue()
        {
            ValueSettingsBase valueSettings = new ValueSettingsBase();

            comboBox1.SelectedIndex = valueSettings.GetTypeLoadOrderDetails(user);
            comboBox2.SelectedIndex = valueSettings.GetTypeLoadItemMouseHover(user);
            comboBox3.SelectedIndex = valueSettings.GetTypeLoadDeviationToMainLV(user);
            comboBox4.SelectedIndex = valueSettings.GetTypeViewDeviationToMainLV(user);
            comboBox5.SelectedIndex = valueSettings.GetOrderRegistrationType(user);
        }

        private void SaveValue()
        {
            ValueSettingsBase valueSettings = new ValueSettingsBase();

            valueSettings.UpdateTypeLoadOrderDetails(user, comboBox1.SelectedIndex.ToString());
            valueSettings.UpdateTypeLoadItemMouseHover(user, comboBox2.SelectedIndex.ToString());
            valueSettings.UpdateTypeLoadDeviationToMainLV(user, comboBox3.SelectedIndex.ToString());
            valueSettings.UpdateTypeViewDeviationToMainLV(user, comboBox4.SelectedIndex.ToString());
            valueSettings.UpdateOrderRegistrationType(user, comboBox5.SelectedIndex.ToString());
            
        }
        private void FormSettings_Load(object sender, EventArgs e)
        {
            LoadValue();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveValue();
            Close();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox5.SelectedIndex == 0)
            {
                comboBox4.Enabled = true;
            }
            else
            {
                comboBox4.Enabled = false;
            }
        }
    }
}
