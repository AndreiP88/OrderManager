using System;
using System.Drawing;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormShiftSchedule : Form
    {
        int userID;

        public FormShiftSchedule(int loadUserID)
        {
            InitializeComponent();

            tableLayoutPanelShiftShedule.ColumnCount = 0;
            this.userID = loadUserID;
        }

        ShiftShedule shiftShedule/* = new ShiftShedule()*/;

        private void LoadShiftShedule()
        {
            ValueUserBase userBase = new ValueUserBase();

            shiftShedule = userBase.GetUserShiftShedule(userID);

            UpdateShiftShedule();
        }

        private void UpdateShiftShedule()
        {
            if (shiftShedule != null)
            {
                if (shiftShedule.ShiftStartDate != "")
                {
                    dateTimePicker1.Value = Convert.ToDateTime(shiftShedule.ShiftStartDate);
                }
                else
                {
                    dateTimePicker1.Value = DateTime.Now;
                }

                Clear();

                foreach (ShiftBlank shift in shiftShedule.ShiftBlanks)
                {
                    AddShiftBlankToPanel(shift.Shift, shift.Name);
                }

                for (int i = 0; i < shiftShedule.ShiftColors.Length; i++)
                {
                    AddColorToPanel(i, shiftShedule.ShiftColors[i]);
                }
            }
        }

        private void SaveShiftShedule()
        {
            ValueUserBase userBase = new ValueUserBase();

            shiftShedule.UserID = userID;
            shiftShedule.ShiftStartDate = dateTimePicker1.Value.ToString("dd.MM.yyyy");

            userBase.SetUserShiftShedule(shiftShedule);
        }

        private void AddShiftBlankToPanel(string shift, string name)
        {
            tableLayoutPanelShiftShedule.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tableLayoutPanelShiftShedule.ColumnCount++;

            int index = tableLayoutPanelShiftShedule.ColumnCount - 1;

            bool planedDay = false, factDay = false, planedNight = false, factNight = false;

            if (shift == "I")
            {
                planedDay = true;
                factDay = true;
            }

            if (shift == "II")
            {
                planedNight = true;
                factNight = true;
            }

            //DayBlank currDay = new DayBlank();
            DayBlankFull currDay = new DayBlankFull();
            currDay.Refresh(index + 1, planedDay, planedNight, factDay, factNight, shiftShedule.ShiftColors);

            tableLayoutPanelShiftShedule.Controls.Add(currDay, index, 0);
        }
        private void Clear()
        {
            for (int i = tableLayoutPanelShiftShedule.ColumnCount - 1; i >= 0 ; i--)
            {
                tableLayoutPanelShiftShedule.Controls?.RemoveAt(i);
            }

            tableLayoutPanelShiftShedule.ColumnCount = 0;
        }

        private void AddNewShiftBlank(int shiftIndex, string name)
        {
;           string shift;

            switch (shiftIndex)
            {
                case 0:
                    shift = "I";
                    break;
                case 1:
                    shift = "II";
                    break;
                default:
                    shift = "";
                    break;
            }

            shiftShedule.ShiftBlanks.Add(new ShiftBlank(shift, name));

            AddShiftBlankToPanel(shift, name);

            //if (shiftBlanks.Count > 1)
            /*{
                tableLayoutPanelShiftShedule.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                tableLayoutPanelShiftShedule.ColumnCount++;
            }

            int index = shiftBlanks.Count - 1;

            DayBlank currDay = new DayBlank();
            currDay.Refresh(index + 1, shift, name, "", false, false);

            tableLayoutPanelShiftShedule.Controls.Add(currDay, index, 0);*/

            ViewShiftBlank();
        }

        private void ViewShiftBlank()
        {
            /*tableLayoutPanelShiftShedule.ColumnCount = 0;

            for (int i = 0; i < shiftBlanks.Count; i++)
            {
                if (i > 0)
                {
                    tableLayoutPanelShiftShedule.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                    tableLayoutPanelShiftShedule.ColumnCount++;
                }

                DayBlank currDay = new DayBlank();
                currDay.Refresh(i + 1, shiftBlanks[i].Shift, shiftBlanks[i].Name, "", false, false);

                tableLayoutPanelShiftShedule.Controls.Add(currDay, i, 0);
            }*/
        }

        private void DeleteLastShiftBlank()
        {
            if (shiftShedule.ShiftBlanks.Count > 0 && tableLayoutPanelShiftShedule.ColumnCount > 0)
            {
                shiftShedule.ShiftBlanks?.RemoveAt(shiftShedule.ShiftBlanks.Count - 1);

                tableLayoutPanelShiftShedule.Controls?.RemoveAt(tableLayoutPanelShiftShedule.ColumnCount - 1);
                tableLayoutPanelShiftShedule.ColumnCount--;
            }
            
            //tableLayoutPanelShiftShedule.ColumnCount--;

            ViewShiftBlank();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddNewShiftBlank(comboBox1.SelectedIndex, textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DeleteLastShiftBlank();
        }

        private void FormShiftSchedule_Load(object sender, EventArgs e)
        {
            LoadShiftShedule();
            comboBox1.SelectedIndex = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveShiftShedule();
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    textBox1.Text = "1 смена";
                    break;
                case 1:
                    textBox1.Text = "2 смена";
                    break;
                case 2:
                    textBox1.Text = "Выходной";
                    break;
            }
        }

        private void SelectColor(int index)
        {
            colorDialog1.Color = shiftShedule.ShiftColors[index];

            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color color = colorDialog1.Color;

                SetColor(index, color);

                AddColorToPanel(index, color);

                UpdateShiftShedule();
            }
        }

        private void SetColor(int index, Color color)
        {
            shiftShedule.ShiftColors[index] = color;
        }

        private void AddColorToPanel(int index, Color color)
        {
            switch (index)
            {
                case 0:
                    pictureBox1.BackColor = color;
                    break;
                case 1:
                    pictureBox2.BackColor = color;
                    break;
                case 2:
                    pictureBox3.BackColor = color;
                    break;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SelectColor(0);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SelectColor(1);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SelectColor(2);
        }
    }
}
