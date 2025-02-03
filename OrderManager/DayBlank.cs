using System;
using System.Drawing;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class DayBlank : UserControl
    {
        public DayBlank()
        {
            InitializeComponent();
        }

        public void Refresh(int day, string shift, string name)
        {
            dayNumber.Text = day.ToString();
            shiftLabel.Text = shift;
            firstTimeLabel.Text = name;
            secondTimeLabel.Text = "";

            if (shift != "")
            {
                this.BackColor = Color.NavajoWhite;
            }
            else
            {
                this.BackColor = Color.Turquoise;
            }
        }

        public void Refresh(int day, string shift, string firstHour, string secondHour, bool firstShiftOvertime, bool secondShiftOvertime)
        {
            dayNumber.Text = day.ToString();
            shiftLabel.Text = shift;
            firstTimeLabel.Text = firstHour;
            secondTimeLabel.Text = secondHour;

            if (firstShiftOvertime)
            {
                firstTimeLabel.ForeColor = Color.Red;
                firstTimeLabel.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
            }

            if (secondShiftOvertime)
            {
                secondTimeLabel.ForeColor = Color.Red;
                secondTimeLabel.Font = new Font("Microsoft Sans Serif", 11, FontStyle.Bold);
            }

            if (firstHour != "" || secondHour != "")
            {
                this.BackColor = Color.NavajoWhite;
            }
            else
            {
                this.BackColor = Color.Turquoise;
            }
        }

        private void DayBlank_Load(object sender, EventArgs e)
        {

        }
    }
}
