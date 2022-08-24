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
    public partial class FormAddEditTestMySQL : Form
    {
        public FormAddEditTestMySQL()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                comboBox1.DropDownStyle = ComboBoxStyle.DropDown;
            else
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void FormAddEditTestMySQL_Load(object sender, EventArgs e)
        {
            IniFile ini = new IniFile("settings.ini");

            comboBox1.Items.AddRange(ini.GetAllSections());
        }
    }
}
