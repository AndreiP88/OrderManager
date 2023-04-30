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
        String loadOrderNumber;
        String loadOrderModification;
        String loadOrderCounterRepeat;
        String loadMachine;
        String loadUser;

        public FormTypesInTheOrder(String lStartOfShift, String lOrderNumber, String lOrderModification, String lOrderCounterRepeat, String lMachine, String lUser)
        {
            InitializeComponent();

            this.loadStartOfShift = lStartOfShift;
            this.loadOrderNumber = lOrderNumber;
            this.loadOrderModification = lOrderModification;
            this.loadOrderCounterRepeat = lOrderCounterRepeat;
            this.loadMachine = lMachine;
            this.loadUser = lUser;
        }

        List<TypeInTheOrder> typesCurrent = new List<TypeInTheOrder>();
        List<TypeInTheOrder> typesForAdded = new List<TypeInTheOrder>();
        List<TypeInTheOrder> typesForEdit = new List<TypeInTheOrder>();
        List<int> itemsIndex = new List<int>();

        List<string> positionForDelete = new List<string>();

        bool editedType = false;
        string indexTypeEdited = "";

        private void LoadItemsListForCurrentOrder()
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();

            comboBox1.Items.Clear();
            itemsIndex.Clear();

            int orderID = ordersBase.GetOrderID(loadMachine, loadOrderNumber, loadOrderModification);

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
                    comboBox1.Items.Add(sqlReader["name"].ToString() + " - " + (sqlReader["count"].ToString()));
                    //comboBox1.Items.Add(typesBase.GetNameItemFromID(sqlReader["typeListID"].ToString()));
                    itemsIndex.Add((int)sqlReader["id"]);
                }

                Connect.Close();
            }
        }
        
        private void LoadTypes()
        {
            ValueTypesBase typeBase = new ValueTypesBase(loadStartOfShift, loadOrderNumber, loadOrderModification, loadOrderCounterRepeat, loadMachine, loadUser);

            typesCurrent = typeBase.GetData();

            AddTypesToListView();
        }

        private void AddTypes()
        {
            typesForAdded.Add(new TypeInTheOrder(comboBox1.Text, (int)numericUpDown1.Value));
        }

        private void SaveTypes()
        {
            ValueTypesBase typeBase = new ValueTypesBase(loadStartOfShift, loadOrderNumber, loadOrderModification, loadOrderCounterRepeat, loadMachine, loadUser);

            for (int i = 0; i < typesForAdded.Count; i++)
            {
                typeBase.InsertData(typesForAdded[i].type, typesForAdded[i].done);
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
            comboBox1.Text = "";
            numericUpDown1.Value = 0;

            button1.Text = "Добавить";

            editedType = false;
        }

        private void AddTypesToListView()
        {
            int count = 0;

            listView1.Items.Clear();

            for (int i = 0; i < typesCurrent.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = typesCurrent[i].id;
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(typesCurrent[i].type);
                item.SubItems.Add(typesCurrent[i].done.ToString("N0"));

                listView1.Items.Add(item);

                count += typesCurrent[i].done;
            }

            for (int i = 0; i < typesForAdded.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = "n" + i.ToString();
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(typesForAdded[i].type);
                item.SubItems.Add(typesForAdded[i].done.ToString("N0"));

                listView1.Items.Add(item);

                count += typesForAdded[i].done;
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
                int index = Convert.ToInt32(indexTypeEdited.Replace("n", ""));

                if (indexTypeEdited.Substring(0, 1) == "n")
                {
                    typesForAdded[index].type = comboBox1.Text;
                    typesForAdded[index].done = (int)numericUpDown1.Value;
                }
                else
                {
                    int i = typesCurrent.FindLastIndex((v) => v.id == index.ToString());

                    typesCurrent[i].type = comboBox1.Text;
                    typesCurrent[i].done = (int)numericUpDown1.Value;

                    typesForEdit.Add(new TypeInTheOrder(index.ToString(), typesCurrent[i].indexTypeList, comboBox1.Text, (int)numericUpDown1.Value));
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
                comboBox1.Text = typesForAdded[index].type;
                numericUpDown1.Value = typesForAdded[index].done;
            }
            else
            {
                int i = typesCurrent.FindLastIndex((v) => v.id == index.ToString());

                comboBox1.Text = typesCurrent[i].type;
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
                typesCurrent.RemoveAt(typesCurrent.FindLastIndex((v) => v.id == index.ToString()));
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
    }
}
