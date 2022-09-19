using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OrderManager.Form1;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace OrderManager
{
    public partial class FormAddEditTestMySQL : Form
    {
        public FormAddEditTestMySQL()
        {
            InitializeComponent();
        }

        bool availableDB = false;
        bool edit = false;
        bool edited = false;

        

        String selectedItemForEdit = "";

        /*public bool IsServerConnected()
        {
            string host = textBox1.Text;
            int port = Convert.ToInt32(textBox2.Text);
            string database = textBox3.Text;
            string username = textBox4.Text;
            string password = textBox5.Text;

            using (MySqlConnection Connect = DBConnection.GetDBConnection(host, port, database, username, password))
            {
                try
                {
                    Connect.Open();
                    Connect.Close();
                    return true;
                }
                catch
                {
                    return false;
                }

            }
        }*/

        private void EnabledTextBoxes(bool enabled)
        {
            textBox1.Enabled = enabled;
            textBox2.Enabled = enabled;
            textBox3.Enabled = enabled;
            textBox4.Enabled = enabled;
            textBox5.Enabled = enabled;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            IniFile ini = new IniFile(Form1.connectionFile);

            if (comboBox1.SelectedIndex == 0)
            {
                EnabledTextBoxes(true);

                comboBox1.DropDownStyle = ComboBoxStyle.DropDown;
                button1.Text = "Добавить";

                button2.Text = "Редактировать";
                button2.Enabled = false;

                button3.Text = "Применить";
                button3.Enabled = false;
                
                Clear();
            }
            else
            {
                edit = false;

                EnabledTextBoxes(false);

                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                button1.Text = "Удалить";

                button2.Text = "Редактировать";
                button2.Enabled = true;

                if (ini.ReadString("selected", "general").Substring(5) == comboBox1.Text)
                {
                    if (edited)
                    {
                        button3.Text = "Применить";
                        button3.Enabled = true;

                        button4.Enabled = false;
                    }
                    else
                    {
                        button3.Text = "Активен";
                        button3.Enabled = false;
                    }
                }
                else
                {
                    button3.Text = "Применить";
                    button3.Enabled = true;
                }

                LoadSelected("mysql" + comboBox1.Text);
            }
                
        }

        private void LoadSelected(String section)
        {
            IniFile ini = new IniFile(Form1.connectionFile);

            Clear();

            String host = "host";
            String port = "port";
            String database = "database";
            String username = "username";
            String password = "password";

            if (ini.KeyExists(host, section))
                textBox1.Text = ini.ReadString(host, section);

            if (ini.KeyExists(port, section))
                textBox2.Text = ini.ReadString(port, section);

            if (ini.KeyExists(database, section))
                textBox3.Text = ini.ReadString(database, section);

            if (ini.KeyExists(username, section))
                textBox4.Text = ini.ReadString(username, section);

            if (ini.KeyExists(password, section))
                textBox5.Text = ini.ReadString(password, section);
        }

        private void Clear()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
        }

        private void SelectCurerntConnection(String currentConnection)
        {
            if (comboBox1.Items.Contains(currentConnection))
                comboBox1.SelectedItem = currentConnection;
            else
                comboBox1.SelectedIndex = 0;
        }

        private void LoadConnectionsList()
        {
            comboBox1.Items.Clear();

            IniFile ini = new IniFile(Form1.connectionFile);

            List<String> conections = new List<String>();
            String[] sections = ini.GetAllSections();

            conections.Add("<новое соединение>");

            for (int i = 0; i < sections.Length; i++)
            {
                if (sections[i].StartsWith("mysql"))
                    conections.Add(sections[i].Substring(5));
            }
            comboBox1.Items.AddRange(conections.ToArray());
        }

        private void FormAddEditTestMySQL_Load(object sender, EventArgs e)
        {
            IniFile ini = new IniFile(connectionFile);

            LoadConnectionsList();
            SelectCurerntConnection(ini.ReadString("selected", "general").Substring(5));
        }

        private void AddSection()
        {
            /*StreamWriter f = new StreamWriter(connectionFile, true, Encoding.GetEncoding(1251));
            f.WriteLine("[mysql" + comboBox1.Text + "]");
            f.WriteLine("host=" + textBox1.Text);
            f.WriteLine("port=" + textBox2.Text);
            f.WriteLine("database=" + textBox3.Text);
            f.WriteLine("username=" + textBox4.Text);
            f.WriteLine("password=" + textBox5.Text);
            f.Close();*/

            IniFile ini = new IniFile(connectionFile);

            String section = "mysql" + comboBox1.Text;

            ini.Write("host", textBox1.Text, section);
            ini.Write("port", textBox2.Text, section);
            ini.Write("database", textBox3.Text, section);
            ini.Write("username", textBox4.Text, section);
            ini.Write("password", textBox5.Text, section);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IniFile ini = new IniFile(connectionFile);

            String selectedItem = "";

            if (comboBox1.SelectedIndex == 0 || comboBox1.SelectedIndex == -1)
            {
                //String[] sect = ini.GetAllSections();
                List<String> sections = ini.GetAllSections().ToList();

                selectedItem = "mysql" + comboBox1.Text;

                if (!sections.Contains(selectedItem))
                {
                    AddSection();
                    LoadConnectionsList();
                    SelectCurerntConnection(selectedItem.Substring(5));
                }
                else
                    MessageBox.Show("Имя уже испрользуется", "Ошибка", MessageBoxButtons.OK);
            }
            else if (comboBox1.SelectedIndex > 0)
            {
                DialogResult result;
                result = MessageBox.Show("Вы действительно хотите удалить профиль соединения '" + comboBox1.Text + "'?", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    ini.DeleteSection("mysql" + comboBox1.Text);
                    LoadConnectionsList();
                    SelectCurerntConnection(ini.ReadString("selected", "general").Substring(5));
                }
            }

            
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DBConnection connection = new DBConnection();

            string host = textBox1.Text;
            int port = Convert.ToInt32(textBox2.Text);
            string database = textBox3.Text;
            string username = textBox4.Text;
            string password = textBox5.Text;

            if (connection.IsServerConnected(host, port, database, username, password))
            {
                IniFile ini = new IniFile(connectionFile);

                String section = "mysql" + comboBox1.Text;

                ini.Write("selected", section, "general");

                Close();
            }
            else
            {
                MessageBox.Show("Соединение не установлено!", "Ошибка", MessageBoxButtons.OK);
            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IniFile ini = new IniFile(connectionFile);

            if (!edit)
            {
                edit = true;
                EnabledTextBoxes(true);

                button1.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;

                comboBox1.DropDownStyle = ComboBoxStyle.Simple;

                button2.Text = "Сохранить";

                selectedItemForEdit = "mysql" + comboBox1.Text;
            }
            else if (edit)
            {
                String section = comboBox1.Text;

                edit = false;
                EnabledTextBoxes(false);

                button1.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;

                button2.Text = "Редактировать";

                edited = true;

                ini.DeleteSection(selectedItemForEdit);

                AddSection();

                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

                LoadConnectionsList();
                SelectCurerntConnection(section);

                selectedItemForEdit = "";
            }
            


        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
