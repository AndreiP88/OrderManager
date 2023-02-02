using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace OrderManager
{
    public partial class FormSalaryForUser : Form
    {
        String loadUser;

        public FormSalaryForUser(string lUser)
        {
            InitializeComponent();

            this.loadUser = lUser;
        }

        List<SalaryForUser> salaryCurrent = new List<SalaryForUser>();
        List<SalaryForUser> salaryForAdded = new List<SalaryForUser>();
        List<SalaryForUser> salaryForEdit = new List<SalaryForUser>();

        List<string> positionForDelete = new List<string>();

        bool editedSalary = false;
        string indexSalaryEdited = "";
        
        private void LoadTypes()
        {
            ValueSalaryBase salaryBase = new ValueSalaryBase();

            salaryCurrent = salaryBase.GetData(loadUser);

            AddSalaryToListView();
        }

        private void AddTypes()
        {
            salaryForAdded.Add(new SalaryForUser("01." + dateTimePicker1.Value.ToString("MM.yyyy"),
                        numericUpDown1.Value,
                        numericUpDown2.Value,
                        numericUpDown3.Value,
                        numericUpDown4.Value));
        }

        private void SaveTypes()
        {
            ValueSalaryBase salaryBase = new ValueSalaryBase();

            for (int i = 0; i < salaryForAdded.Count; i++)
            {
                salaryBase.InsertData(loadUser ,salaryForAdded[i]);
            }

            for (int i = 0; i < salaryForEdit.Count; i++)
            {
                salaryBase.UpdateData(salaryForEdit[i]);
            }

            for (int i = 0; i < positionForDelete.Count; i++)
            {
                salaryBase.DeleteSalary(positionForDelete[i]);
            }
        }

        private void Clear()
        {
            dateTimePicker1.Value = DateTime.Now;
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            numericUpDown3.Value = 0;
            numericUpDown4.Value = 0;

            button1.Text = "Добавить";

            editedSalary = false;
        }

        private void AddSalaryToListView()
        {
            listView1.Items.Clear();

            for (int i = 0; i < salaryCurrent.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = salaryCurrent[i].id;
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(Convert.ToDateTime(salaryCurrent[i].period).ToString("MM.yyyy"));
                item.SubItems.Add(salaryCurrent[i].basicSalary.ToString("N2"));
                item.SubItems.Add(salaryCurrent[i].bonusSalary.ToString("N2"));
                item.SubItems.Add(salaryCurrent[i].tax.ToString("N5"));
                item.SubItems.Add(salaryCurrent[i].pension.ToString("N5"));

                listView1.Items.Add(item);
            }

            for (int i = 0; i < salaryForAdded.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = "n" + i.ToString();
                item.Text = (listView1.Items.Count + 1).ToString();
                item.SubItems.Add(Convert.ToDateTime(salaryForAdded[i].period).ToString("MM.yyyy"));
                item.SubItems.Add(salaryForAdded[i].basicSalary.ToString("N2"));
                item.SubItems.Add(salaryForAdded[i].bonusSalary.ToString("N2"));
                item.SubItems.Add(salaryForAdded[i].tax.ToString("N5"));
                item.SubItems.Add(salaryForAdded[i].pension.ToString("N5"));

                listView1.Items.Add(item);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!editedSalary)
            {
                AddTypes();
            }
            else
            {
                int index = Convert.ToInt32(indexSalaryEdited.Replace("n", ""));
                string periodCurr = "01." + dateTimePicker1.Value.ToString("MM.yyyy");

                if (indexSalaryEdited.Substring(0, 1) == "n")
                {
                    salaryForAdded[index].period = periodCurr;
                    salaryForAdded[index].basicSalary = numericUpDown1.Value;
                    salaryForAdded[index].bonusSalary = numericUpDown2.Value;
                    salaryForAdded[index].tax = numericUpDown3.Value;
                    salaryForAdded[index].pension = numericUpDown4.Value;
                }
                else
                {
                    int i = salaryCurrent.FindLastIndex((v) => v.id == index.ToString());

                    salaryCurrent[i].period = periodCurr;
                    salaryCurrent[i].basicSalary = numericUpDown1.Value;
                    salaryCurrent[i].bonusSalary = numericUpDown2.Value;
                    salaryCurrent[i].tax = numericUpDown3.Value;
                    salaryCurrent[i].pension = numericUpDown4.Value;

                    salaryForEdit.Add(new SalaryForUser(index.ToString(), 
                        periodCurr,
                        numericUpDown1.Value,
                        numericUpDown2.Value,
                        numericUpDown3.Value,
                        numericUpDown4.Value
                        ));
                }
            }

            AddSalaryToListView();

            Clear();
        }

        private void RunEdit()
        {
            button1.Text = "Сохранить";
            editedSalary = true;

            indexSalaryEdited = listView1.SelectedItems[0].Name;

            int index = Convert.ToInt32(indexSalaryEdited.Replace("n", ""));

            if (indexSalaryEdited.Substring(0, 1) == "n")
            {
                dateTimePicker1.Value = Convert.ToDateTime(salaryForAdded[index].period);
                numericUpDown1.Value = salaryForAdded[index].basicSalary;
                numericUpDown2.Value = salaryForAdded[index].bonusSalary;
                numericUpDown3.Value = salaryForAdded[index].tax;
                numericUpDown4.Value = salaryForAdded[index].pension;
            }
            else
            {
                int i = salaryCurrent.FindLastIndex((v) => v.id == index.ToString());

                dateTimePicker1.Value = Convert.ToDateTime(salaryCurrent[i].period);
                numericUpDown1.Value = salaryCurrent[i].basicSalary;
                numericUpDown2.Value = salaryCurrent[i].bonusSalary;
                numericUpDown3.Value = salaryCurrent[i].tax;
                numericUpDown4.Value = salaryCurrent[i].pension;
            }
        }

        private void FormPrivateNote_Load(object sender, EventArgs e)
        {
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
                salaryForAdded.RemoveAt(index);
            }
            else
            {
                salaryCurrent.RemoveAt(salaryCurrent.FindLastIndex((v) => v.id == index.ToString()));
                positionForDelete.Add(index.ToString());
            }

            AddSalaryToListView();
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

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
