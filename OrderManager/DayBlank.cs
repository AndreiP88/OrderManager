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
    public partial class DayBlank : UserControl
    {
        public DayBlank()
        {
            InitializeComponent();
        }

        public void Refresh(int day, string shift, string hour)
        {
            dayNumber.Text = day.ToString();
            shiftNumber.Text = shift;
            hourCurr.Text = hour.ToString();

            if (shift == "")
            {
                this.BackColor = Color.Turquoise;
            }
            else
            {
                this.BackColor = Color.NavajoWhite;
            }
        }

        private void DayBlank_Load(object sender, EventArgs e)
        {

        }
    }
}
