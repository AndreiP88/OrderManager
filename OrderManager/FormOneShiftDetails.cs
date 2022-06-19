using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormOneShiftDetails : Form
    {
        readonly String timeShiftStart;
        String dataBase;
        bool adminMode;

        public FormOneShiftDetails(bool aMode, String dBase, String shiftStart)
        {
            InitializeComponent();
            this.dataBase = dBase;
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
            SetUpdateSettingsValue setting = new SetUpdateSettingsValue(dataBase);

            if (Form1.Info.nameOfExecutor != "")
                setting.UpdateParameterLine(Form1.Info.nameOfExecutor, nameForm, GetParametersLine());
            else
                setting.UpdateParameterLine("0", nameForm, GetParametersLine());
        }

        private void LoadParametersFromBase(String nameForm)
        {
            GetValueFromSettingsBase getSettings = new GetValueFromSettingsBase(dataBase);

            if (Form1.Info.nameOfExecutor != "")
                ApplyParameterLine(getSettings.GetParameterLine(Form1.Info.nameOfExecutor, nameForm));
            else
                ApplyParameterLine(getSettings.GetParameterLine("0", nameForm));
        }
        
        private bool PrivateData (String shiftStart, String userID)
        {
            bool result = false;

            GetValueFromShiftsBase getUserShift = new GetValueFromShiftsBase(dataBase);

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

        private void AddOrdersToListViewFromList()
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase(dataBase);

            ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(timeShiftStart, "");

            fullTimeWorkingOut = 0;
            fullDone = 0;

            listView1.Items.Clear();

            for (int index = 0; index < ordersCurrentShift.Count; index++)
            {
                String modification = "";
                if (ordersCurrentShift[index].modificationOfOrder != "")
                    modification = " (" + ordersCurrentShift[index].modificationOfOrder + ")";

                ListViewItem item = new ListViewItem();

                item.Name = ordersCurrentShift[index].numberOfOrder.ToString();
                item.Text = (index + 1).ToString();
                item.SubItems.Add(getInfo.GetMachineName(ordersCurrentShift[index].machineOfOrder.ToString()));
                item.SubItems.Add(ordersCurrentShift[index].numberOfOrder.ToString() + modification);
                item.SubItems.Add(ordersCurrentShift[index].nameOfOrder.ToString());
                item.SubItems.Add(ordersCurrentShift[index].amountOfOrder.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].lastCount.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].plannedTimeMakeready.ToString() + ", " + ordersCurrentShift[index].plannedTimeWork.ToString());
                item.SubItems.Add(ordersCurrentShift[index].facticalTimeMakeready.ToString() + ", " + ordersCurrentShift[index].facticalTimeWork.ToString());
                item.SubItems.Add(ordersCurrentShift[index].done.ToString("N0"));
                item.SubItems.Add(ordersCurrentShift[index].norm.ToString("N0"));
                item.SubItems.Add(timeOperations.TotalMinutesToHoursAndMinutesStr(ordersCurrentShift[index].workingOut));
                item.SubItems.Add(ordersCurrentShift[index].note.ToString());

                if (PrivateData(timeShiftStart, Form1.Info.nameOfExecutor))
                    item.SubItems.Add(ordersCurrentShift[index].notePrivate.ToString());

                listView1.Items.Add(item);

                fullTimeWorkingOut += ordersCurrentShift[index].workingOut;
                fullDone += ordersCurrentShift[index].done;
            }
        }

        private void ViewDetailsForUser()
        {
            GetDateTimeOperations dtOperations = new GetDateTimeOperations();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);
            GetValueFromShiftsBase getShift = new GetValueFromShiftsBase(dataBase);


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
                GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

                FormAddCloseOrder form;

                form = new FormAddCloseOrder(adminMode, dataBase, timeShiftStart,
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
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            FormPrivateNote form;

            form = new FormPrivateNote(dataBase, timeShiftStart,
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
            if(e.Item != null)
            {

                //toolTip1.Show(e.Item.Index + ": " + listView1.Items[e.Item.Index].SubItems[12].Text, listView1);
                //toolTip1.Show(e.Item.SubItems[12].Text, listView1);
            }
        }
    }
}
