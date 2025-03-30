using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormLoadOrderOperations : Form
    {
        private List<LoadShift> loadShift;
        private List<LoadShift> loadShiftOrders;

        private bool viewAllButtons;

        private bool _loadShiftList = false;

        public FormLoadOrderOperations(List<LoadShift> loadShift, bool viewAllButtons = false)
        {
            InitializeComponent();

            this.loadShift = loadShift;
            this.viewAllButtons = viewAllButtons;
        }

        private int typeAcceptedOrder = 1;

        public int TypeAcceptedOrder
        {
            get
            {
                return typeAcceptedOrder;
            }
            set
            {
                typeAcceptedOrder = value;
            }
        }

        private async Task LoadShiftListToListViewAsync(List<LoadShift> shifts)
        {
            ValueUserBase userBase = new ValueUserBase();
            ValueInfoBase infoBase = new ValueInfoBase();

            _loadShiftList = true;

            listView1.Items.Clear();

            for (int i = 0; i < shifts.Count; i++)
            {
                Color color = Color.WhiteSmoke;

                if (i % 2 == 0)
                {
                    //color = Color.Silver;
                }

                LoadShift shift = shifts[i];

                string shiftDetail = userBase.GetSmalNameUser(shift.UserIDBaseOM.ToString()) + ". Дата: " + shift.ShiftDate + ", Смена: " + shift.ShiftNumber + ". Время: " + shift.ShiftStart + " - " + shift.ShiftEnd + "; ID: " + shift.IndexOMShift;

                ListViewGroup listViewGroup = new ListViewGroup(shiftDetail);
                listViewGroup.Name = i.ToString();//shift.IDFbcBrigade.ToString();

                listView1.Groups.Add(listViewGroup);

                for (int j = 0; j < shift.Order.Count; j++)
                {
                    LoadOrder order = shift.Order[j];

                    string makereadyCompleteView = ((float)shift.Order[j].OrderOperations[0].MakereadyComplete / 100).ToString("P0");

                    if (shift.Order[j].OrderOperations[0].OLDValueMakereadyComplete != -1)
                    {
                        makereadyCompleteView = ((float)shift.Order[j].OrderOperations[0].OLDValueMakereadyComplete / 100).ToString("P0") + " ➞ " + makereadyCompleteView;
                    }

                    string doneView = shift.Order[j].OrderOperations[0].Done.ToString("N0");

                    if (shift.Order[j].OrderOperations[0].OLDValueDone != -1)
                    {
                        doneView = shift.Order[j].OrderOperations[0].OLDValueDone.ToString("N0") + " ➞ " + doneView;
                    }

                    ListViewItem listViewItemOrders = new ListViewItem(listViewGroup);
                    
                    listViewItemOrders.Checked = shift.Order[j].IsOrderLoad;
                    listViewItemOrders.Name = j.ToString();//shift.Order[j].IdManOrderJobItem.ToString();
                    listViewItemOrders.Text = (j + 1).ToString();

                    listViewItemOrders.SubItems.Add(shift.Order[j].EquipName);
                    listViewItemOrders.SubItems.Add(shift.Order[j].OrderNumber);
                    listViewItemOrders.SubItems.Add(shift.Order[j].NameCustomer);
                    listViewItemOrders.SubItems.Add(shift.Order[j].AmountOfOrder.ToString("N0"));
                    listViewItemOrders.SubItems.Add(shift.Order[j].LastAmount.ToString("N0"));
                    listViewItemOrders.SubItems.Add(makereadyCompleteView);
                    listViewItemOrders.SubItems.Add(doneView);

                    listViewItemOrders.BackColor = color;

                    listView1.Items.Add(listViewItemOrders);
                }
                //listViewItem.SubItems.Add(shift.

                
            }

            _loadShiftList = false;
        }

        private void LoadShiftListToListViewOLD(List<LoadShift> shifts)
        {
            ValueUserBase userBase = new ValueUserBase();

            listView1.Items.Clear();

            for (int i = 0; i < shifts.Count; i++)
            {
                Color color = Color.Silver;

                if (i % 2 != 0)
                {
                    color = Color.WhiteSmoke;
                }

                LoadShift shift = shifts[i];

                ListViewItem listViewItem = new ListViewItem();

                listViewItem.Text = (i + 1).ToString();
                listViewItem.SubItems.Add(userBase.GetSmalNameUser(shift.UserIDBaseOM.ToString()));
                listViewItem.SubItems.Add(shift.ShiftDate);
                listViewItem.SubItems.Add(shift.ShiftNumber.ToString());

                if (shift.Order.Count > 0)
                {
                    string makereadyCompleteView = ((float)shift.Order[0].OrderOperations[0].MakereadyComplete / 100).ToString("P0");

                    if (shift.Order[0].OrderOperations[0].OLDValueMakereadyComplete != -1)
                    {
                        makereadyCompleteView = ((float)shift.Order[0].OrderOperations[0].OLDValueMakereadyComplete / 100).ToString("P0") + " ➞ " + makereadyCompleteView;
                    }

                    string doneView = shift.Order[0].OrderOperations[0].Done.ToString("N0");

                    if (shift.Order[0].OrderOperations[0].OLDValueDone != -1)
                    {
                        doneView = shift.Order[0].OrderOperations[0].OLDValueDone.ToString("N0") + " ➞ " + doneView;
                    }

                    listViewItem.Checked = shift.Order[0].IsOrderLoad;
                    listViewItem.Name = shift.Order[0].IdManOrderJobItem.ToString();

                    listViewItem.SubItems.Add(shift.Order[0].OrderNumber);
                    listViewItem.SubItems.Add(shift.Order[0].NameCustomer);
                    listViewItem.SubItems.Add(shift.Order[0].AmountOfOrder.ToString("N0"));
                    listViewItem.SubItems.Add(makereadyCompleteView);
                    listViewItem.SubItems.Add(doneView);
                }

                listViewItem.BackColor = color;

                listView1.Items.Add(listViewItem);

                for (int j = 1; j < shift.Order.Count; j++)
                {
                    LoadOrder order = shift.Order[j];

                    string makereadyCompleteView = ((float)shift.Order[j].OrderOperations[0].MakereadyComplete / 100).ToString("P0");

                    if (shift.Order[j].OrderOperations[0].OLDValueMakereadyComplete != -1)
                    {
                        makereadyCompleteView = ((float)shift.Order[j].OrderOperations[0].OLDValueMakereadyComplete / 100).ToString("P0") + " 🠒 " + makereadyCompleteView;
                    }

                    string doneView = shift.Order[j].OrderOperations[0].Done.ToString("N0");

                    if (shift.Order[j].OrderOperations[0].OLDValueDone != -1)
                    {
                        doneView = shift.Order[j].OrderOperations[0].OLDValueDone.ToString("N0") + " ➞ " + doneView;
                    }

                    ListViewItem listViewItemOrders = new ListViewItem();

                    listViewItemOrders.Checked = shift.Order[j].IsOrderLoad;
                    listViewItemOrders.Name = shift.Order[j].IdManOrderJobItem.ToString();
                    //listViewItemOrders.Text = i.ToString();
                    //listViewItemOrders.Text = shift.IDFbcBrigade.ToString();
                    //listViewItemOrders.Checked = shift.IsNewShift;
                    listViewItemOrders.SubItems.Add("");
                    listViewItemOrders.SubItems.Add("");
                    listViewItemOrders.SubItems.Add("");

                    listViewItemOrders.SubItems.Add(shift.Order[j].OrderNumber);
                    listViewItemOrders.SubItems.Add(shift.Order[j].NameCustomer);
                    listViewItemOrders.SubItems.Add(shift.Order[j].AmountOfOrder.ToString("N0"));
                    listViewItemOrders.SubItems.Add(makereadyCompleteView);
                    listViewItemOrders.SubItems.Add(doneView);

                    listViewItemOrders.BackColor = color;

                    listView1.Items.Add(listViewItemOrders);
                }
                //listViewItem.SubItems.Add(shift.


            }
        }

        private void CheckChengedForOrder(int shiftID, int orderID, bool value)
        {
            loadShiftOrders[shiftID].Order[orderID].IsOrderLoad = value;

            loadShiftOrders[shiftID].IsLoadShift = false;

            for (int i = 0; i < loadShiftOrders[shiftID].Order.Count; i++)
            {
                if (loadShiftOrders[shiftID].Order[i].IsOrderLoad)
                {
                    loadShiftOrders[shiftID].IsLoadShift = true;
                    break;
                }
            }
        }
        private async Task<bool> AcceptNewValuesAsync()
        {
            bool ordersLoaded = false;

            for (int i = 0; i < loadShiftOrders.Count; i++)
            {
                if (loadShiftOrders[i].IsLoadShift)
                {
                    loadShiftOrders[i].IndexOMShift = await AddShiftAsync(loadShiftOrders[i]);

                    for (int j = 0; j <  loadShiftOrders[i].Order.Count; j++)
                    {
                        if (loadShiftOrders[i].Order[j].IsOrderLoad)
                        {
                            loadShiftOrders[i].Order[j].OrderOMIndex = await AddNewOrderAsync(loadShiftOrders[i].Order[j]);

                            await AddNewOrderInProgress(loadShiftOrders[i], loadShiftOrders[i].Order[j]);

                            ordersLoaded = true;
                        }
                    }
                }
            }

            return ordersLoaded;
        }
        private async Task<int> AddShiftAsync(LoadShift shift)
        {
            int shiftID = -1;

            if (shift.IndexOMShift == 0)
            {
                ValueShiftsBase shiftsBase = new ValueShiftsBase();

                shiftID = await shiftsBase.AddClosedShiftAsync(shift);
            }
            else
            {
                shiftID = shift.IndexOMShift;
            }

            return shiftID;
        }

        private async Task<int> AddNewOrderAsync(LoadOrder order)
        {
            int newOrderID = -1;

            if (order.OrderOMIndex == -1)
            {
                ValueOrdersBase valueOrders = new ValueOrdersBase();

                newOrderID = await valueOrders.AddOrderToDB(order.EquipID, order.OrderNumber, order.NameCustomer, order.ItemOrder, order.AmountOfOrder, order.MakereadyTime, order.WorkTime, order.StampOrder, order.Items);

                //newOrderID = valueOrders.GetOrderID(order.EquipID.ToString(), order.OrderNumber, order.ItemOrder);
            }
            else
            {
                newOrderID = order.OrderOMIndex;
            }

            return newOrderID;
        }
        private async Task AddNewOrderInProgress(LoadShift shift, LoadOrder order)
        {
            ValueOrdersBase valueOrders = new ValueOrdersBase();
            ValueInfoBase infoBase = new ValueInfoBase();

            int typeJob = 0;

            int shiftID = shift.IndexOMShift;
            int orderID = order.OrderOMIndex;
            int machineCurrent = order.EquipID;

            int amount = order.AmountOfOrder;
            //Сделать подсчет остатка тиража для отслеживания завершения заказа
            string makereadyStart = order.OrderOperations[0].MakereadyStart;
            string makereadyStop = order.OrderOperations[0].MakereadyStop;
            string workStart = order.OrderOperations[0].WorkStart;
            string workStop = order.OrderOperations[0].WorkStop;
            int makereadyComplete = order.OrderOperations[0].MakereadyComplete;
            int done = order.OrderOperations[0].Done;
            int lastAmount = order.LastAmount;
            int counterRepeat = 0;

            int makereadyConsider = 0;
            int newStatus = 0;

            if (makereadyStart == "" && makereadyStop == "")
            {
                if (workStart == "" && workStop == "")
                {
                    newStatus = 0;
                }
                else
                {
                    if (done > lastAmount)
                    {
                        newStatus = 4;
                    }
                    else
                    {
                        newStatus = 3;
                    }
                }

                    makereadyConsider = 0;
            }
            else
            {
                if (workStart == "" && workStop == "")
                {
                    newStatus = 2;
                }
                else
                {
                    if (done > lastAmount)
                    {
                        newStatus = 4;
                    }
                    else
                    {
                        newStatus = 3;
                    }
                }

                makereadyConsider = 1;
            }

            if (order.OrderOperations[0].OrderOperationID == 0)
            {
                await valueOrders.AddNewOrderInProgressAsync(machineCurrent, shift.UserIDBaseOM, typeJob, shiftID, orderID, makereadyStart, makereadyStop, workStart, workStop, makereadyConsider, makereadyComplete, done, counterRepeat, "");

                valueOrders.SetNewStatus(orderID, newStatus.ToString());
                //infoBase.UpdateInfo(machineCurrent.ToString(), 0, 0, -1, orderID, false);
            }
            else
            {
                if (order.OrderOperations[0].OLDValueMakereadyComplete != -1)
                {
                    valueOrders.UpdateData("makereadyComplete", machineCurrent, shiftID, orderID, counterRepeat, makereadyComplete);

                }

                if (order.OrderOperations[0].OLDValueDone != -1)
                {
                    valueOrders.UpdateData("done", machineCurrent, shiftID, orderID, counterRepeat, done.ToString());
                }
            }

            infoBase.UpdateInfo(machineCurrent.ToString(), 0, 0, -1, orderID, false);
        }

        private async void FormLoadOrderOperations_LoadAsync(object sender, EventArgs e)
        {
            if (viewAllButtons)
            {
                button1.Visible = true;
                button2.Visible = true;

                button3.Text = "Отменить и продолжить";
                button4.Text = "Отменить и вернуться";
            }
            else
            {
                button1.Visible = false;
                button2.Visible = false;

                button3.Text = "Сохранить";
                button4.Text = "Отменить";
            }

            GetOrderOperations orderOperations = new GetOrderOperations();

            loadShiftOrders = await orderOperations.OperationsForOrder(loadShift);

            await LoadShiftListToListViewAsync(loadShiftOrders);
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!_loadShiftList)
            {
                //MessageBox.Show(e.Item.Group.Name + ", " + e.Item.Name + " - " + listView1.Items[e.Item.Index].Checked + "");
                CheckChengedForOrder(Convert.ToInt32(e.Item.Group.Name), Convert.ToInt32(e.Item.Name), e.Item.Checked);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (await AcceptNewValuesAsync())
            {
                TypeAcceptedOrder = 1;
            }
            else
            {
                TypeAcceptedOrder = 4;
            }

            Close();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (await AcceptNewValuesAsync())
            {
                TypeAcceptedOrder = 2;
            }
            else
            {
                TypeAcceptedOrder = 4;
            }

            Close();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            TypeAcceptedOrder = 3;

            Close();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            TypeAcceptedOrder = 4;

            Close();
        }

        private void FormLoadOrderOperations_FormClosing(object sender, FormClosingEventArgs e)
        {
            TypeAcceptedOrder = 4;
        }
    }
}
