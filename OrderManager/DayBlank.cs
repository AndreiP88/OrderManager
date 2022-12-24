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

        private void DayBlank_Load(object sender, EventArgs e)
        {
            new List<Control> { dayNumber , AddAppointmentButton, ActiveAppointmentsLabel,ActiveAppointmentPanel,
            CompletedAppointmentsLabel,CompletedAppointmentPanel, this}.ForEach(x =>
            {
                /*x.MouseClick += DayBlankControl_MouseClick;
                x.MouseEnter += DayBlank_MouseEnter;
                x.MouseLeave += DayBlank_MouseLeave;*/
            });
        }
    }
}
