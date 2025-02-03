using System;
using System.Drawing;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class DayBlankFull : UserControl
    {
        public DayBlankFull()
        {
            InitializeComponent();
        }

        private string _day = "";

        private Color[] _colors = { Color.FromArgb(-1), Color.FromArgb(-1), Color.FromArgb(-1) };
        private bool _planedDay = false;
        private bool _planedNight = false;
        private bool _factDay = false;
        private bool _factNight = false;

        private bool _overtimeDay = false;
        private bool _overtimeNight = false;

        /*private static int h = 100;
        private static int w = 120;

        private Point[] pointsDayShiftPlanned = { new Point(0, h - 4), new Point(0, 0), new Point(w - 4, 0) };
        private Point[] pointsDayShiftFact = { new Point(10, h - 25), new Point(10, 10), new Point(w - 30, 10) };

        private Point[] pointsNightShiftPlanned = { new Point(w - 1, 0), new Point(w, h), new Point(0, h - 1) };
        private Point[] pointsNightShiftFact = { new Point(w - 13, 25), new Point(w - 13, h - 13), new Point(30, h - 13) };*/


        public void Refresh(int day, string shift, string name)
        {
            //DrawRectangle();
            /*dayNumber.Text = day.ToString();
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
            }*/
        }

        public void Refresh(int day, bool planedDay, bool planedNight, bool factDay, bool factNight, bool overtimeDay, bool overtimeNight, Color[] colors)
        {
            _day = day.ToString();

            _colors = colors;

            _planedDay = planedDay;
            _factDay = factDay;

            _planedNight = planedNight;
            _factNight = factNight;

            _overtimeDay = overtimeDay;
            _overtimeNight = overtimeNight;
        }

        public void Refresh(int day, bool planedDay, bool planedNight, bool factDay, bool factNight, Color[] colors)
        {
            _day = day.ToString();

            _colors = colors;

            _planedDay = planedDay;
            _factDay = factDay;

            _planedNight = planedNight;
            _factNight = factNight;
        }

        private void DrawDayShiftPlaned(Color color)
        {
            int h = this.panel1.Height;
            int w = this.panel1.Width;

            Point[] pointsDayShiftPlanned = { new Point(0, h - 2), new Point(0, 0), new Point(w - 2, 0) };

            DrawShift(color, pointsDayShiftPlanned, _day);
        }

        private void DrawDayShiftFact(Color color)
        {
            int h = this.panel1.Height;
            int w = this.panel1.Width;

            Point[] pointsDayShiftFact = { new Point(6, h - 15), new Point(6, 6), new Point(w - 22, 6) };

            DrawShift(color, pointsDayShiftFact, _day, _overtimeDay);
        }

        private void DrawNightShiftPlaned(Color color)
        {
            int h = this.panel1.Height;
            int w = this.panel1.Width;

            Point[] pointsNightShiftPlanned = { new Point(w, 1), new Point(w, h), new Point(1, h) };

            DrawShift(color, pointsNightShiftPlanned, _day);
        }

        private void DrawNightShiftFact(Color color)
        {
            int h = this.panel1.Height;
            int w = this.panel1.Width;

            Point[] pointsNightShiftFact = { new Point(w - 6, 12), new Point(w - 6, h - 6), new Point(20, h - 6) };

            DrawShift(color, pointsNightShiftFact, _day, _overtimeNight);
        }

        private void DrawShift(Color color, Point[] points, string day, bool overtimeShift = false)
        {
            Graphics formGraphics = panel1.CreateGraphics();

            Brush brush = new SolidBrush(color);
            formGraphics.FillPolygon(brush, points);

            if (overtimeShift)
            {
                Pen pen = new Pen(Color.Red, 2);
                formGraphics.DrawPolygon(pen, points);
            }
            
            Font font = new Font(FontFamily.GenericSansSerif, 13f);
            formGraphics.DrawString(day, font, Brushes.DarkBlue, 4, 4);

            brush.Dispose();
            formGraphics.Dispose();
        }

        private void DayBlankFull_Load(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (_planedDay)
            {
                DrawDayShiftPlaned(_colors[0]);
            }
            else
            {
                DrawDayShiftPlaned(_colors[2]);
            }

            if (_factDay)
            {
                DrawDayShiftFact(_colors[0]);
            }
            else
            {
                DrawDayShiftFact(_colors[2]);
            }

            if (_planedNight)
            {
                DrawNightShiftPlaned(_colors[1]);
            }
            else
            {
                DrawNightShiftPlaned(_colors[2]);
            }

            if (_factNight)
            {
                DrawNightShiftFact(_colors[1]);
            }
            else
            {
                DrawNightShiftFact(_colors[2]);
            }
        }
    }
}
