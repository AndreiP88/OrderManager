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
        private LoadOrder loadOrder;

        public FormLoadOrderOperations(LoadOrder loadOrder)
        {
            InitializeComponent();

            this.loadOrder = loadOrder;
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
    }
}
