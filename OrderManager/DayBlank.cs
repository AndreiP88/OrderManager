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

        public void Refresh(int day, string shift, string hour)
        {
            dayNumber.Text = day.ToString();
            shiftLabel.Text = shift;
            firstTimeLabel.Text = hour.ToString();

            secondTimeLabel.Text = hour.ToString();

            if (shift == "")
            {
                this.BackColor = Color.Turquoise;
            }
            else
            {
                this.BackColor = Color.NavajoWhite;
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
