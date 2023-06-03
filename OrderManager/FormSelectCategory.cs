using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormSelectCategory : Form
    {
        public FormSelectCategory()
        {
            InitializeComponent();
        }

        private void LoadCategoryes()
        {
            ValueCategory getCategory = new ValueCategory();
            StringArray str = new StringArray();
            INISettings settings = new INISettings();

            string selectedCategory = settings.GetCategoryesForView();
            string[] arrayCat = str.ArrayFromTheString(selectedCategory);

            List<string> categoryes = new List<string>(getCategory.GetCategoryesList());

            for (int i = 0; i < categoryes.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = getCategory.GetCategoryFromName(categoryes[i]);
                item.Text = categoryes[i];

                listView1.Items.Add(item);
            }

            for (int i = 0; i < arrayCat.Length; i++)
            {
                int index = listView1.Items.IndexOfKey(arrayCat[i]);

                if (index >= 0)
                {
                    listView1.Items[index].Checked = true;
                }
                
            }
        }

        private string GetCategoryesFromLV()
        {
            StringArray str = new StringArray();

            string result = "";

            string[] indexes = new string[listView1.CheckedIndices.Count];
            //listView1.CheckedIndices[]
            //indexes.Concat<int>(listView1.CheckedIndices);

            for (int i = 0; i < listView1.CheckedIndices.Count; i++)
            {
                indexes[i] = listView1.Items[listView1.CheckedIndices[i]].Name;
            }

            result = str.StringFromTheArray(indexes);

            return result;
        }

        private void SaveSelectedCategory()
        {
            INISettings settings = new INISettings();

            string categoryString = GetCategoryesFromLV();

            settings.SetCategoryesForView(categoryString);
        }

        private void LoadUserForm_Load(object sender, EventArgs e)
        {
            LoadCategoryes();
        }

        private void ApplyButton()
        {
            SaveSelectedCategory();
            Close();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            ApplyButton();
            //Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
