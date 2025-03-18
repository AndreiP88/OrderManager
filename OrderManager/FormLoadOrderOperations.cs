using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace OrderManager
{
    public partial class FormLoadOrderOperations : Form
    {
        private List<LoadShift> loadShift;
        private List<LoadShift> loadShiftOrders;

        public FormLoadOrderOperations(List<LoadShift> loadShift)
        {
            InitializeComponent();

            this.loadShift = loadShift;
        }

        private void LoadShiftListToListView(List<LoadShift> shifts)
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

                listViewItem.Checked = shift.IsNewShift;
                listViewItem.Name = i.ToString();

                listViewItem.Text = (i + 1).ToString();
                listViewItem.SubItems.Add(userBase.GetSmalNameUser(shift.UserIDBaseOM.ToString()));
                listViewItem.SubItems.Add(shift.ShiftDate);
                listViewItem.SubItems.Add(shift.ShiftNumber.ToString());

                if (shift.Order.Count > 0)
                {
                    listViewItem.SubItems.Add(shift.Order[0].OrderNumber);
                    listViewItem.SubItems.Add(shift.Order[0].NameCustomer);
                    listViewItem.SubItems.Add(shift.Order[0].AmountOfOrder.ToString("N0"));
                    listViewItem.SubItems.Add(((float)shift.Order[0].OrderOperations[0].MakereadyComplete / 100).ToString("P0"));
                    listViewItem.SubItems.Add(shift.Order[0].OrderOperations[0].Done.ToString("N0"));
                }

                listViewItem.BackColor = color;

                listView1.Items.Add(listViewItem);

                for (int j = 1; j < shift.Order.Count; j++)
                {
                    LoadOrder order = shift.Order[j];

                    ListViewItem listViewItemOrders = new ListViewItem();

                    listViewItemOrders.Name = i.ToString();
                    //listViewItemOrders.Text = i.ToString();
                    //listViewItemOrders.Text = shift.IDFbcBrigade.ToString();
                    //listViewItemOrders.Checked = shift.IsNewShift;
                    listViewItemOrders.SubItems.Add("");
                    listViewItemOrders.SubItems.Add("");
                    listViewItemOrders.SubItems.Add("");

                    listViewItemOrders.SubItems.Add(shift.Order[j].OrderNumber);
                    listViewItemOrders.SubItems.Add(shift.Order[j].NameCustomer);
                    listViewItemOrders.SubItems.Add(shift.Order[j].AmountOfOrder.ToString("N0"));
                    listViewItemOrders.SubItems.Add(((float)shift.Order[j].OrderOperations[0].MakereadyComplete / 100).ToString("P0"));
                    listViewItemOrders.SubItems.Add(shift.Order[j].OrderOperations[0].Done.ToString("N0"));

                    listViewItemOrders.BackColor = color;

                    listView1.Items.Add(listViewItemOrders);
                }
                //listViewItem.SubItems.Add(shift.

                
            }
        }


        private void AddTypesToListView()
        {
            int count = 0;

            listView1.Items.Clear();

            /*for (int i = 0; i < itemsCurrentOrder.Count; i++)
            {
                //int index = itemsCurrentOrder.FindLastIndex((v) => v.indexTypeList == itemsCurrentOrder[i].indexTypeList);

                ListViewItem item = new ListViewItem();

                item.Name = itemsCurrentOrder[i].indexTypeList.ToString();
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(itemsCurrentOrder[i].name);
                item.SubItems.Add(itemsCurrentOrder[i].count.ToString("N0"));

                listView1.Items.Add(item);

                count += itemsCurrentOrder[i].count;
            }*/
        }

        private void FormLoadOrderOperations_Load(object sender, EventArgs e)
        {
            GetOrderOperations orderOperations = new GetOrderOperations();

            loadShiftOrders = orderOperations.OperationsForOrder(loadShift);

            LoadShiftListToListView(loadShiftOrders);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
