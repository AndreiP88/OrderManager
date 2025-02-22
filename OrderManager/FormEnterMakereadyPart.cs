using System;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormEnterMakereadyPart : Form
    {
        bool _edit = false;
        int _type = 1;
        int makereadyTime = -1;

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

        public FormEnterMakereadyPart(int shiftID, int machine, int orderIndex, int counterRepeat, int currentTimeMakeready, int typeMakeready)
        {
            InitializeComponent();

            _edit = false;

            this.ShiftID = shiftID;
            this.Machine = machine;
            this.OrderIndex = orderIndex;
            this.CounterRepeat = counterRepeat;
            this.CurrentTimeMakeready = currentTimeMakeready;


            this._type = typeMakeready;

            this.OrderInProgressID = -1;
        }

        private void LoadOtherParameters(int orderInProgressID)
        {
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            ValueOrdersBase orders = new ValueOrdersBase();

            this.ShiftID = getOrders.GetShiftIDFromOrderInProgressID(orderInProgressID);
            this.Machine = getOrders.GetMachineFromOrderInProgressID(orderInProgressID);
            this.OrderIndex = getOrders.GetOrderID(orderInProgressID);
            this.CounterRepeat = getOrders.GetCounterRepeatFromOrderInProgressID(orderInProgressID);
            this._type = orders.GetMakereadyType(OrderIndex);
        }

        int maxValueTrackBox = 1000;

        private void SetValueForEdit()
        {
            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetLeadTime leadTime = new GetLeadTime(ShiftID, Machine, OrderIndex, CounterRepeat);

            makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(OrderIndex));
            int makereadySummPreviousParts = leadTime.CalculateMakereadyParts(true, false, true);
            int currentMakereadyPart = getOrders.GetMakereadyPartFromOrderID(OrderInProgressID);

            int lastTimeMakeready = -2;

            if (_type == 0)
            {
                lastTimeMakeready = makereadyTime - makereadySummPreviousParts;
                trackBar1.Maximum = makereadyTime;

                SetTimeValue(currentMakereadyPart);
            }
            else if (_type == 1)
            {
                lastTimeMakeready = 100 - makereadySummPreviousParts;
                trackBar1.Maximum = 100;

                SetTimeValue(currentMakereadyPart * makereadyTime / 100);
            }

            maxValueTrackBox = lastTimeMakeready;

            SetTrackBarValue(currentMakereadyPart);
            SetPercentValue(trackBar1.Value, trackBar1.Maximum);
        }
        private void SetValueForAdd()
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            GetLeadTime leadTime = new GetLeadTime(ShiftID, Machine, OrderIndex, CounterRepeat);

            makereadyTime = Convert.ToInt32(ordersBase.GetTimeMakeready(OrderIndex));

            int makereadySummPreviousParts = leadTime.CalculateMakereadyParts(true, false, false);
            
            int lastTimeMakeready = -2;

            if (_type == 0)
            {
                lastTimeMakeready = makereadyTime - makereadySummPreviousParts;
                trackBar1.Maximum = makereadyTime;

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
            }
            else if (_type == 1)
            {
                lastTimeMakeready = 100 - makereadySummPreviousParts;
                trackBar1.Maximum = 100;

                int currentMakereadyPart = CurrentTimeMakeready * 100 / makereadyTime;

                if (currentMakereadyPart > lastTimeMakeready)
                {
                    SetTrackBarValue(lastTimeMakeready);
                    SetTimeValue(lastTimeMakeready * makereadyTime / 100);
                }
                else
                {
                    SetTrackBarValue(currentMakereadyPart);
                    SetTimeValue(CurrentTimeMakeready);
                }
            }

            maxValueTrackBox = lastTimeMakeready;

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

        private int GetPercentValue()
        {
            int result = 0;

            int currentValue = trackBar1.Value;
            int maxValue = trackBar1.Maximum;

            result = 100 * currentValue / maxValue;

            return result;
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

        public int Type
        {
            get => _type;
            set => _type = value;
        }

        private bool newValue = false;

        public bool NewValue
        {
            get => newValue;
            set => newValue = value;
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
            //Сделать часть приладки, передавать тип (0 - время, 1 - часть приладки (0 - 100))

            newValue = true;

            switch (_type)
            {
                case 0:
                    mkPart = GetTimeValue();
                    break;
                case 1:
                    mkPart = GetPercentValue();
                    break;
                default:
                    newValue = false;
                    break;
            }

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
                button2.Enabled = true;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value > maxValueTrackBox)
            {
                trackBar1.Value = maxValueTrackBox;
            }
            
            if (_type == 0)
            {
                SetTimeValue(trackBar1.Value);
            }
            else if (_type == 1)
            {
                SetTimeValue(trackBar1.Value * makereadyTime / 100);
            }

            SetPercentValue(trackBar1.Value, trackBar1.Maximum);
        }

    }
}
