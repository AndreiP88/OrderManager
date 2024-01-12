using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormEnterMakereadyPart : Form
    {
        bool _edit = false;

        int ShiftID;
        int Machine;
        int OrderIndex;
        int CounterRepeat;
        int CurrentTimeMakeready;

        int OrderInProgressID;

        public FormEnterMakereadyPart(int orderInProgressID)
        {
            InitializeComponent();

            _edit = true;

            LoadOtherParameters(orderInProgressID);

            this.OrderInProgressID = orderInProgressID;
        }

        public FormEnterMakereadyPart(int shiftID, int machine, int orderIndex, int counterRepeat, int currentTimeMakeready)
        {
            InitializeComponent();

            _edit = false;

            this.ShiftID = shiftID;
            this.Machine = machine;
            this.OrderIndex = orderIndex;
            this.CounterRepeat = counterRepeat;
            this.CurrentTimeMakeready = currentTimeMakeready;

            this.OrderInProgressID = -1;
        }

        private void LoadOtherParameters(int orderInProgressID)
        {
            GetOrdersFromBase getOrders = new GetOrdersFromBase();

            this.ShiftID = getOrders.GetShiftIDFromOrderInProgressID(orderInProgressID);
            this.Machine = getOrders.GetMachineFromOrderInProgressID(orderInProgressID);
            this.OrderIndex = getOrders.GetOrderID(orderInProgressID);
            this.CounterRepeat = getOrders.GetCounterRepeatFromOrderInProgressID(orderInProgressID);
        }

        int maxValueTrackBox = 1000;

        private void SetValueForEdit()
        {
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetLeadTime leadTime = new GetLeadTime(ShiftID, Machine, OrderIndex, CounterRepeat);

            int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(OrderIndex));

            trackBar1.Maximum = makereadyTime;

            int makereadySummPreviousParts = leadTime.CalculateMakereadyParts(true, false, true);
            int lastTimeMakeready = makereadyTime - makereadySummPreviousParts;

            maxValueTrackBox = lastTimeMakeready;

            int currentMakereadyPart = getOrders.GetMakereadyPartFromOrderID(OrderInProgressID);

            SetTrackBarValue(currentMakereadyPart);
            SetTimeValue(currentMakereadyPart);

            SetPercentValue(trackBar1.Value, trackBar1.Maximum);
        }
        private void SetValueForAdd()
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetLeadTime leadTime = new GetLeadTime(ShiftID, Machine, OrderIndex, CounterRepeat);

            int makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(OrderIndex));

            trackBar1.Maximum = makereadyTime;

            int makereadySummPreviousParts = leadTime.CalculateMakereadyParts(true, false, false);
            int lastTimeMakeready = makereadyTime - makereadySummPreviousParts;

            maxValueTrackBox = lastTimeMakeready;

            if (CurrentTimeMakeready > lastTimeMakeready)
            {
                SetTrackBarValue(lastTimeMakeready);
                SetTimeValue(lastTimeMakeready);
            }
            else
            {
                SetTrackBarValue(CurrentTimeMakeready);
                SetTimeValue(CurrentTimeMakeready);
            }

            SetPercentValue(trackBar1.Value, trackBar1.Maximum);
        }

        private void SetTimeValue(int time)
        {
            int hour = time / 60;

            int minute = time % 60;

            label2.Text = hour.ToString("D2") + ":" + minute.ToString("D2");
        }

        private void SetPercentValue(int currentValue, int maxValue)
        {
            label3.Text = ((float)currentValue / maxValue).ToString("P0");
        }

        private int GetTimeValue()
        {
            //return (int)(numericUpDown21.Value * 60 + numericUpDown22.Value);
            return trackBar1.Value;
        }

        private void SetTrackBarValue(int value)
        {
            trackBar1.Value = value;
        }

        private void SetTrackBarValue(int makereadyTime, int currentMKTime)
        {
            int value = trackBar1.Maximum;

            if (currentMKTime < makereadyTime)
            {
                value = (int)(trackBar1.Maximum * currentMKTime / makereadyTime);
            }

            SetTrackBarValue(value);
        }

        private bool newValue = false;

        public bool NewValue
        {
            get
            {
                return newValue;
            }
            set
            {
                newValue = value;
            }
        }

        private int mkPart = 0;

        public int NewMKPart
        {
            get
            {
                return mkPart;
            }
            set
            {
                mkPart = value;
            }
        }

        private void Clear()
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            newValue = false;

            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            newValue = true;
            mkPart = GetTimeValue();

            this.Hide();
        }

        private void FormPrivateNote_Load(object sender, EventArgs e)
        {
            if (_edit)
            {
                SetValueForEdit();
                button2.Enabled = true;
            }
            else
            {
                SetValueForAdd();
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value > maxValueTrackBox)
            {
                trackBar1.Value = maxValueTrackBox;
            }

            SetTimeValue(trackBar1.Value);
            SetPercentValue(trackBar1.Value, trackBar1.Maximum);
        }

    }
}
