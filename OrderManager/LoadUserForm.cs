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
    public partial class LoadUserForm : Form
    {
        public LoadUserForm()
        {
            InitializeComponent();
        }

        String dataBase = Directory.GetCurrentDirectory() + "\\data.db";

        private void LoadUserForm_Load(object sender, EventArgs e)
        {
            LoadUsersList();
            LoadMachine();
        }

        private void LoadUsersList()
        {
            int counter = 0;

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM users"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    counter++;

                    ListViewItem item = new ListViewItem();
                    item.Name = sqlReader["id"].ToString();
                    item.Text = counter.ToString();
                    item.SubItems.Add(sqlReader["nameUser"].ToString());
                    listView1.Items.Add(item);

                }

                Connect.Close();
            }
        }

        private void LoadMachine()
        {
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT machine FROM Info"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    comboBox1.Items.Add(sqlReader["machine"].ToString());
                }

                Connect.Close();
            }
            
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
        }

        private void LoadSelectedUser(String typeOfLoad)
        {
            DateTime date = DateTime.Now;

            if (listView1.SelectedItems.Count != 0)
            {
                String currentUser = listView1.SelectedItems[0].Text;
                String machine = comboBox1.Text;
                String startOfShift = "";

                if (typeOfLoad == "new")
                    startOfShift = date.ToString();
                if (typeOfLoad == "continued")
                    startOfShift = "";

                Form1.Info.mashine = machine;

                SaveParametersFromSelectionUserToBase(machine, currentUser, startOfShift);
            }
        }

        private void SaveParametersFromSelectionUserToBase(String machine, String currentUser, String startOfShift)
        {
            String cLine;
            if (startOfShift == "")
                cLine = "";
            else
                cLine = ", startOfShift = @startOfShift";

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText = "UPDATE Info SET nameOfExecutor = @currentUser" + cLine + " " +
                    "WHERE (machine = @machine)";

                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@machine", machine); // присваиваем переменной значение
                //if (startOfShift != "")
                    Command.Parameters.AddWithValue("@startOfShift", startOfShift);
                Command.Parameters.AddWithValue("@currentUser", currentUser);

                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private String GetIDUserFromMachine(String machine)
        {
            String result = "";

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM Info WHERE machine = '" + machine + "'"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result = sqlReader["nameOfExecutor"].ToString();
                }

                Connect.Close();
            }

            return result;
        }

        private void EnabledButton()
        {
            if (listView1.SelectedItems.Count != 0)
                button1.Enabled = true;
            else
                button1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (GetIDUserFromMachine(comboBox1.Text) == "")
            {
                LoadSelectedUser("new");
                Close();
            }
            else
            {
                LoadSelectedUser("continued");
                Close();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (GetIDUserFromMachine(comboBox1.Text) != "")
            {
                //int idUser = Convert.ToInt16(GetIDUserFromMachine(comboBox1.Text));
                int idUser = listView1.Items.IndexOfKey(GetIDUserFromMachine(comboBox1.Text));
                listView1.Items[idUser].Selected = true;
                listView1.Enabled = false;
                listView1.Items[idUser].EnsureVisible();
            }
            else
            {
                listView1.Items[0].Selected = true;
                listView1.Items[0].Selected = false;
                listView1.Enabled = true;
            }

            EnabledButton();

        }

        private void LoadUserForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Application.Exit();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            EnabledButton();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
