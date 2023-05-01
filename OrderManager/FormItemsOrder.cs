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
    public partial class FormItemsOrder : Form
    {
        string orderID;

        public FormItemsOrder(string orderIndex)
        {
            InitializeComponent();

            this.orderID = orderIndex;
        }

        List<TypeInTheOrder> itemsForAdded = new List<TypeInTheOrder>();
        List<TypeInTheOrder> itemsForEdit = new List<TypeInTheOrder>();
        List<TypeInTheOrder> itemsCurrentOrder = new List<TypeInTheOrder>();

        List<string> positionForDelete = new List<string>();

        bool editedType = false;
        string indexTypeEdited = "";

        private void LoadItemsListForCurrentOrder()
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            textBox1.Text = "";
            itemsCurrentOrder.Clear();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM typesList WHERE orderID = @orderID"

                };
                Command.Parameters.AddWithValue("@orderID", orderID);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    //comboBox1.Items.Add(typesBase.GetNameItemFromID(sqlReader["typeListID"].ToString()));
                    itemsCurrentOrder.Add(new TypeInTheOrder(
                        (int)sqlReader["id"],
                        sqlReader["name"].ToString(),
                        (int)sqlReader["count"]
                        ));
                }

                Connect.Close();
            }
        }
        
        private void LoadTypes()
        {
            AddTypesToListView();
            /*ValueTypesBase typeBase = new ValueTypesBase(loadStartOfShift, loadOrderNumber, loadOrderModification, loadOrderCounterRepeat, loadMachine, loadUser);

            typesCurrent = typeBase.GetData();

            AddTypesToListView();*/
        }

        private void AddTypes()
        {
            itemsForAdded.Add(new TypeInTheOrder(Convert.ToInt32(orderID), textBox1.Text, (int)numericUpDown1.Value));
        }

        private void SaveTypes()
        {
            ValueTypesBase typeBase = new ValueTypesBase();

            for (int i = 0; i < itemsForAdded.Count; i++)
            {
                typeBase.InsertItem(itemsForAdded[i]);
            }

            for (int i = 0; i < itemsForEdit.Count; i++)
            {
                typeBase.UpdateItem(itemsForEdit[i]);
            }

            for (int i = 0; i < positionForDelete.Count; i++)
            {
                typeBase.DeleteItem(positionForDelete[i]);
            }
        }

        private void Clear()
        {
            textBox1.Text = "";
            numericUpDown1.Value = 0;

            button1.Text = "Добавить";

            editedType = false;
        }

        private void AddTypesToListView()
        {
            int count = 0;

            listView1.Items.Clear();

            for (int i = 0; i < itemsCurrentOrder.Count; i++)
            {
                //int index = itemsCurrentOrder.FindLastIndex((v) => v.indexTypeList == itemsCurrentOrder[i].indexTypeList);

                ListViewItem item = new ListViewItem();

                item.Name = itemsCurrentOrder[i].indexTypeList.ToString();
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(itemsCurrentOrder[i].name);
                item.SubItems.Add(itemsCurrentOrder[i].count.ToString("N0"));

                listView1.Items.Add(item);

                count += itemsCurrentOrder[i].count;
            }

            for (int i = 0; i < itemsForAdded.Count; i++)
            {
                //int index = itemsForAdded.FindLastIndex((v) => v.indexTypeList == itemsForAdded[i].indexTypeList);
                //переделать добавление и тут поправить
                ListViewItem item = new ListViewItem();

                item.Name = "n" + i.ToString();
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(itemsForAdded[i].name);
                item.SubItems.Add(itemsForAdded[i].count.ToString("N0"));

                listView1.Items.Add(item);

                count += itemsForAdded[i].done;
            }

            label2.Text = count.ToString("N0");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!editedType)
            {
                AddTypes();
            }
            else
            {
                //сделать проверку на соответствие названия с тем, что есть в наименованиях для заказа и, если нету, то добавлять новую позицию
                int index = Convert.ToInt32(indexTypeEdited.Replace("n", ""));

                if (indexTypeEdited.Substring(0, 1) == "n")
                {
                    itemsForAdded[index].name = textBox1.Text;
                    itemsForAdded[index].count = (int)numericUpDown1.Value;
                }
                else
                {
                    int i = itemsCurrentOrder.FindLastIndex((v) => v.indexTypeList == index);

                    itemsCurrentOrder[i].name = textBox1.Text;
                    itemsCurrentOrder[i].count = (int)numericUpDown1.Value;

                    itemsForEdit.Add(new TypeInTheOrder(index, itemsCurrentOrder[i].name, itemsCurrentOrder[i].count));
                }
            }

            AddTypesToListView();

            Clear();
        }

        private void RunEdit()
        {
            button1.Text = "Сохранить";
            editedType = true;

            indexTypeEdited = listView1.SelectedItems[0].Name;

            int index = Convert.ToInt32(indexTypeEdited.Replace("n", ""));

            if (indexTypeEdited.Substring(0, 1) == "n")
            {
                //int itemIndex = itemsForAdded.FindLastIndex((v) => v.id == itemsForAdded[index].id);

                textBox1.Text = itemsForAdded[index].name;
                numericUpDown1.Value = itemsForAdded[index].done;
            }
            else
            {
                int i = itemsCurrentOrder.FindLastIndex((v) => v.indexTypeList == index);
                //int itemIndex = itemsCurrentOrder.FindLastIndex((v) => v.id == itemsCurrentOrder[i].id);

                textBox1.Text = itemsCurrentOrder[i].name;
                numericUpDown1.Value = itemsCurrentOrder[i].count;
            }
        }

        private void FormPrivateNote_Load(object sender, EventArgs e)
        {
            LoadItemsListForCurrentOrder();
            LoadTypes();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveTypes();
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunEdit();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string lvName = listView1.SelectedItems[0].Name;
            int index = Convert.ToInt32(lvName.Replace("n", ""));

            if (lvName.Substring(0, 1) == "n")
            {
                itemsForAdded.RemoveAt(index);
            }
            else
            {
                itemsCurrentOrder.RemoveAt(itemsCurrentOrder.FindLastIndex((v) => v.indexTypeList == index));
                positionForDelete.Add(index.ToString());
            }

            AddTypesToListView();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = listView1.SelectedItems.Count == 0;
        }

        private void numericUpDown1_Click(object sender, EventArgs e)
        {
            numericUpDown1.Select(0, numericUpDown1.Text.Length);
        }

        private void numericUpDown1_Enter(object sender, EventArgs e)
        {
            numericUpDown1.Select(0, numericUpDown1.Text.Length);
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                RunEdit();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }
    }
}
