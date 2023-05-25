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
    public partial class FormTypesInTheOrder : Form
    {
        String loadStartOfShift;
        int loadOrderID;
        String loadOrderCounterRepeat;
        String loadMachine;
        String loadUser;

        public FormTypesInTheOrder(String lStartOfShift, int lOrderID, String lOrderCounterRepeat, String lMachine, String lUser)
        {
            InitializeComponent();

            this.loadStartOfShift = lStartOfShift;
            this.loadOrderID = lOrderID;
            this.loadOrderCounterRepeat = lOrderCounterRepeat;
            this.loadMachine = lMachine;
            this.loadUser = lUser;
        }

        List<TypeInTheOrder> typesCurrent = new List<TypeInTheOrder>();
        List<TypeInTheOrder> typesForAdded = new List<TypeInTheOrder>();
        List<TypeInTheOrder> typesForEdit = new List<TypeInTheOrder>();
        List<TypeInTheOrder> itemsCurrentOrder = new List<TypeInTheOrder>();

        List<string> positionForDelete = new List<string>();

        bool editedType = false;
        string indexTypeEdited = "";

        private void LoadItemsListForCurrentOrder()
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            comboBox1.Items.Clear();
            itemsCurrentOrder.Clear();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM typesList WHERE orderID = @orderID"

                };
                Command.Parameters.AddWithValue("@orderID", loadOrderID);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    comboBox1.Items.Add(sqlReader["name"].ToString());
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
            ValueTypesBase typeBase = new ValueTypesBase(loadStartOfShift, loadOrderID, loadOrderCounterRepeat, loadMachine, loadUser);

            typesCurrent = typeBase.GetData();

            AddTypesToListView();
        }

        private void AddTypes()
        {
            int itemListIndex = itemsCurrentOrder[comboBox1.SelectedIndex].indexTypeList;

            typesForAdded.Add(new TypeInTheOrder(itemListIndex, (int)numericUpDown1.Value));
        }

        private void SaveTypes()
        {
            ValueTypesBase typeBase = new ValueTypesBase(loadStartOfShift, loadOrderID, loadOrderCounterRepeat, loadMachine, loadUser);

            for (int i = 0; i < typesForAdded.Count; i++)
            {
                typeBase.InsertData(typesForAdded[i]);
            }

            for (int i = 0; i < typesForEdit.Count; i++)
            {
                typeBase.UpdateData(typesForEdit[i]);
            }

            for (int i = 0; i < positionForDelete.Count; i++)
            {
                typeBase.DeleteTypes(positionForDelete[i]);
            }
        }

        private void Clear()
        {
            comboBox1.SelectedIndex = -1;
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;

            button1.Text = "Добавить";
            button5.Enabled = true;

            editedType = false;
        }

        private void AddTypesToListView()
        {
            ValueTypesBase typesBase = new ValueTypesBase(loadStartOfShift, loadOrderID, loadOrderCounterRepeat, loadMachine, loadUser);

            int count = 0;

            listView1.Items.Clear();

            for (int i = 0; i < typesCurrent.Count; i++)
            {
                int index = itemsCurrentOrder.FindLastIndex((v) => v.indexTypeList == typesCurrent[i].indexTypeList);

                ListViewItem item = new ListViewItem();

                item.Name = typesCurrent[i].id.ToString();
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(itemsCurrentOrder[index].name);
                item.SubItems.Add(itemsCurrentOrder[index].count.ToString("N0"));
                item.SubItems.Add(typesCurrent[i].done.ToString("N0"));

                listView1.Items.Add(item);

                count += typesCurrent[i].done;
            }

            for (int i = 0; i < typesForAdded.Count; i++)
            {
                int index = itemsCurrentOrder.FindLastIndex((v) => v.indexTypeList == typesForAdded[i].indexTypeList);

                if (index != -1)
                {
                    ListViewItem item = new ListViewItem();

                    item.Name = "n" + i.ToString();
                    item.Text = (listView1.Items.Count + 1).ToString();
                    item.SubItems.Add(itemsCurrentOrder[index].name);
                    item.SubItems.Add(itemsCurrentOrder[index].count.ToString("N0"));
                    item.SubItems.Add(typesForAdded[i].done.ToString("N0"));

                    listView1.Items.Add(item);

                    count += typesForAdded[i].done;
                }
                else
                {
                    typesForAdded.RemoveAt(i);
                }
                
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
                    typesForAdded[index].indexTypeList = itemsCurrentOrder[comboBox1.SelectedIndex].indexTypeList;
                    typesForAdded[index].done = (int)numericUpDown1.Value;
                }
                else
                {
                    int i = typesCurrent.FindLastIndex((v) => v.id == index);

                    typesCurrent[i].indexTypeList = itemsCurrentOrder[comboBox1.SelectedIndex].indexTypeList;
                    typesCurrent[i].done = (int)numericUpDown1.Value;

                    typesForEdit.Add(new TypeInTheOrder(index, typesCurrent[i].indexTypeList, (int)numericUpDown1.Value));
                }
            }

            AddTypesToListView();

            Clear();
        }

        private void RunEdit()
        {
            button1.Text = "Сохранить";
            editedType = true;
            button5.Enabled = false;

            indexTypeEdited = listView1.SelectedItems[0].Name;

            int index = Convert.ToInt32(indexTypeEdited.Replace("n", ""));

            if (indexTypeEdited.Substring(0, 1) == "n")
            {
                int itemIndex = itemsCurrentOrder.FindLastIndex((v) => v.indexTypeList == typesForAdded[index].indexTypeList);

                comboBox1.SelectedIndex = itemIndex;
                numericUpDown1.Value = typesForAdded[index].done;
            }
            else
            {
                int i = typesCurrent.FindLastIndex((v) => v.id == index);
                int itemIndex = itemsCurrentOrder.FindLastIndex((v) => v.indexTypeList == typesCurrent[i].indexTypeList);

                comboBox1.SelectedIndex = itemIndex;
                numericUpDown1.Value = typesCurrent[i].done;
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
                typesForAdded.RemoveAt(index);
            }
            else
            {
                typesCurrent.RemoveAt(typesCurrent.FindLastIndex((v) => v.id == index));
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
            if (comboBox1.Text.Length > 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text.Length > 0)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                numericUpDown2.Value = itemsCurrentOrder[comboBox1.SelectedIndex].count;
            }
            else
            {
                numericUpDown2.Value = 0;
            }
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            FormItemsOrder form = new FormItemsOrder(loadOrderID.ToString());
            form.ShowDialog();

            LoadItemsListForCurrentOrder();
            LoadTypes();
        }
    }
}
