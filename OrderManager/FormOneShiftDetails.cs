using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static OrderManager.Form1;

namespace OrderManager
{
    public partial class FormOneShiftDetails : Form
    {
        String timeShiftStart;
        bool adminMode;

        public FormOneShiftDetails(bool aMode, String shiftStart)
        {
            InitializeComponent();
            this.timeShiftStart = shiftStart;
            this.adminMode = aMode;
        }

        int fullTimeWorkingOut;
        int fullDone;

        List<Order> ordersCurrentShift;

        String GetParametersLine()
        {
            String pLine = "";

            if (this.WindowState == FormWindowState.Normal)
            {
                pLine += this.Location.X.ToString() + ";";
                pLine += this.Location.Y.ToString() + ";";
                pLine += this.Width.ToString() + ";";
                pLine += this.Height.ToString() + ";";
            }
            else
            {
                pLine += this.RestoreBounds.Location.X.ToString() + ";";
                pLine += this.RestoreBounds.Location.Y.ToString() + ";";
                pLine += this.RestoreBounds.Width.ToString() + ";";
                pLine += this.RestoreBounds.Height.ToString() + ";";
            }

            pLine += this.WindowState.ToString() + ";";

            for (int i = 0; i < listView1.Columns.Count; i++)
                pLine += listView1.Columns[i].Width.ToString() + ";";

            return pLine;
        }

        private void ApplyParameterLine(String pLine)
        {
            String[] parameter = pLine.Split(';');

            if (pLine != "" && parameter.Length == listView1.Columns.Count + 6)
            {
                this.Location = new Point(Convert.ToInt32(parameter[0]), Convert.ToInt32(parameter[1]));

                if (parameter[4] == "Normal")
                {
                    WindowState = FormWindowState.Normal;
                    this.Width = Convert.ToInt32(parameter[2]);
                    this.Height = Convert.ToInt32(parameter[3]);
                }

                if (parameter[4] == "Maximized")
                {
                    WindowState = FormWindowState.Maximized;
                }

                if (parameter[4] == "Minimized")
                    WindowState = FormWindowState.Minimized;

                for (int i = 0; i < listView1.Columns.Count; i++)
                    listView1.Columns[i].Width = Convert.ToInt32(parameter[5 + i]);
            }

        }

        private void SaveParameterToBase(String nameForm)
        {
            ValueSettingsBase setting = new ValueSettingsBase();

            if (Form1.Info.nameOfExecutor != "")
                setting.UpdateParameterLine(Form1.Info.nameOfExecutor, nameForm, GetParametersLine());
            else
                setting.UpdateParameterLine("0", nameForm, GetParametersLine());
        }

        private void LoadParametersFromBase(String nameForm)
        {
            ValueSettingsBase getSettings = new ValueSettingsBase();

            if (Form1.Info.nameOfExecutor != "")
                ApplyParameterLine(getSettings.GetParameterLine(Form1.Info.nameOfExecutor, nameForm));
            else
                ApplyParameterLine(getSettings.GetParameterLine("0", nameForm));
        }

        private bool PrivateData(String shiftStart, String userID)
        {
            bool result = false;

            ValueShiftsBase getUserShift = new ValueShiftsBase();

            if (getUserShift.GetNameUserFromStartShift(shiftStart) == userID)
            {
                result = true;
            }

            else
            {
                result = false;
            }

            return result;
        }

        private int CountWorkingOutOrders(int indexOrder, string machine)
        {
            int result = 0;

            for (int i = 0; i < indexOrder; i++)
            {
                if (ordersCurrentShift[i].machineOfOrder == machine || machine == "")
                {
                    result += ordersCurrentShift[i].workingOut;
                }
            }

            return result;
        }

        private int CountPreviusOutages()
        {
            int result = 0;



            return result;
        }

        private void AddOrdersToListViewFromList()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();
            ValueSettingsBase valueSettings = new ValueSettingsBase();

            ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(timeShiftStart, "");

            GetWorkingOutTime workingOutTime = new GetWorkingOutTime(timeShiftStart, ordersCurrentShift);

            fullTimeWorkingOut = 0;
            fullDone = 0;

            listView1.Items.Clear();

            for (int index = 0; index < ordersCurrentShift.Count; index++)
            {
                String modification = "";
                if (ordersCurrentShift[index].modificationOfOrder != "")
                    modification = " (" + ordersCurrentShift[index].modificationOfOrder + ")";

                string deviation = "<>";

                Color color = Color.DarkRed;

                int typeLoad = valueSettings.GetTypeLoadDeviationToMainLV(Info.nameOfExecutor);
                int typeView = valueSettings.GetTypeViewDeviationToMainLV(Info.nameOfExecutor);

                if (typeLoad == 0)
                {
                    OrderStatusValue statusValue = workingOutTime.GetWorkingOutTimeForSelectedOrder(index, true);

                    color = statusValue.color;

                    if (typeView == 0)
                    {
                        deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent) + ", " + timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                    }
                    else
                    {
                        //deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent + statusValue.wkTimeDifferent);
                        deviation = timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                    }
                }
                else if (typeLoad == 1)
                {
                    OrderStatusValue statusValue = workingOutTime.GetWorkingOutTimeForSelectedOrder(index, false);

                    color = statusValue.color;

                    if (typeView == 0)
                    {
                        deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent) + ", " + timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                    }
                    else
                    {
                        //deviation = timeOperations.MinuteToTimeString(statusValue.mkTimeDifferent + statusValue.wkTimeDifferent);
                        deviation = timeOperations.MinuteToTimeString(statusValue.wkTimeDifferent);
                    }
                }
                else if (typeLoad == 2)
                {
                    if ((ordersCurrentShift[index].mkDeviation + ordersCurrentShift[index].wkDeviation) > 0)
                    {
                        color = Color.SeaGreen;
                    }
                    else
                    {
                        color = Color.DarkRed;
                    }

                    if (typeView == 0)
                    {
                        deviation = timeOperations.MinuteToTimeString(ordersCurrentShift[index].mkDeviation) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].wkDeviation);
                    }
                    else
                    {
                        deviation = timeOperations.MinuteToTimeString(ordersCurrentShift[index].mkDeviation + ordersCurrentShift[index].wkDeviation);
                    }
                }

                ListViewItem item = new ListViewItem();

                item.Name = ordersCurrentShift[index].numberOfOrder.ToString();
                item.Text = (index + 1).ToString();
                item.SubItems.Add(getInfo.GetMachineName(ordersCurrentShift[index].machineOfOrder.ToString()));
                item.SubItems.Add(ordersCurrentShift[index].numberOfOrder.ToString() + modification);
                item.SubItems.Add(ordersCurrentShift[index].nameOfOrder.ToString());
                item.SubItems.Add(ordersCurrentShift[index].amountOfOrder.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].lastCount.ToString("N0"));
                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].plannedTimeMakeready) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].plannedTimeWork));
                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].facticalTimeMakeready) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].facticalTimeWork));
                //item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].mkDeviation) + ", " + timeOperations.MinuteToTimeString(ordersCurrentShift[index].wkDeviation));
                item.SubItems.Add(deviation);
                item.SubItems.Add(ordersCurrentShift[index].done.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].norm.ToString("N0"));
                item.SubItems.Add(timeOperations.MinuteToTimeString(ordersCurrentShift[index].workingOut));
                item.SubItems.Add(ordersCurrentShift[index].note.ToString());

                item.ForeColor = color;

                if (PrivateData(timeShiftStart, Form1.Info.nameOfExecutor))
                    item.SubItems.Add(ordersCurrentShift[index].notePrivate);

                listView1.Items.Add(item);

                fullTimeWorkingOut += ordersCurrentShift[index].workingOut;
                fullDone += ordersCurrentShift[index].done;
            }
        }

        private void ViewDetailsForUser()
        {
            GetDateTimeOperations dtOperations = new GetDateTimeOperations();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            ValueUserBase getUser = new ValueUserBase();
            ValueShiftsBase getShift = new ValueShiftsBase();


            label4.Text = getUser.GetNameUser(getShift.GetNameUserFromStartShift(timeShiftStart));
            label5.Text = timeShiftStart;
            label6.Text = getShift.GetStopShift(timeShiftStart);

            label10.Text = dtOperations.TotalMinutesToHoursAndMinutesStr(fullTimeWorkingOut);
            label11.Text = getPercent.PercentString(fullTimeWorkingOut);
            label12.Text = fullDone.ToString("N0");


        }

        private void LoadOrdersFromBase()
        {
            ClearAll();
            AddOrdersToListViewFromList();
            ViewDetailsForUser();
        }

        private void ClearAll()
        {
            listView1.Items.Clear();

            label4.Text = "";
            label5.Text = "";
            label6.Text = "";

            label10.Text = "";
            label11.Text = "";
            label12.Text = "";
        }

        private void DetailsOrder()
        {
            if (listView1.SelectedItems.Count != 0)
            {
                ValueInfoBase getInfo = new ValueInfoBase();

                FormAddCloseOrder form;

                form = new FormAddCloseOrder(adminMode, timeShiftStart,
                ordersCurrentShift[listView1.SelectedIndices[0]].numberOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].modificationOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].counterRepeat);

                form.ShowDialog();
                LoadOrdersFromBase();
            }
        }

        private void LoadOrderNote()
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            FormPrivateNote form;

            form = new FormPrivateNote(timeShiftStart,
                ordersCurrentShift[listView1.SelectedIndices[0]].numberOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].modificationOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].machineOfOrder,
                ordersCurrentShift[listView1.SelectedIndices[0]].counterRepeat);

            form.ShowDialog();
            LoadOrdersFromBase();
        }

        private void FormOneShiftDetails_Load(object sender, EventArgs e)
        {
            LoadParametersFromBase("oneShiftDetails");
            LoadOrdersFromBase();
        }

        private void FormOneShiftDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveParameterToBase("oneShiftDetails");
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            DetailsOrder();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = listView1.SelectedItems.Count == 0;

            if (!PrivateData(timeShiftStart, Form1.Info.nameOfExecutor))
                e.Cancel = true;
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DetailsOrder();
        }

        private void noteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadOrderNote();
        }

        private void listView1_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (e.Item != null)
            {

                //toolTip1.Show(e.Item.Index + ": " + listView1.Items[e.Item.Index].SubItems[12].Text, listView1);
                //toolTip1.Show(e.Item.SubItems[12].Text, listView1);
            }
        }
    }
}
