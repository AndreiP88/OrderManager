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
    public partial class FormSettings : Form
    {
        String dataBase;

        public FormSettings(String dBase)
        {
            InitializeComponent();

            this.dataBase = dBase;
        }

        private void loadMachines()
        {

        }

        private void loadUsers()
        {

        }

        private void FormSettings_Load(object sender, EventArgs e)
        {

        }
    }
}
