﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAdmin : Form
    {
        String dataBase;
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        bool adminMode;

        bool allOrderTHRead = false;

        public FormAdmin(bool aMode, String dBase)
        {
            InitializeComponent();

            this.adminMode = aMode;
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }

        public class MyListView : ListView
        {
            public MyListView()
            {
                DoubleBuffered = true;
            }

            public event EventHandler<int> GroupHeaderClick;
            protected virtual void OnGroupHeaderClick(int e)
            {
                var handler = GroupHeaderClick;
                if (handler != null) handler(this, e);
            }
            private const int LVM_HITTEST = 0x1000 + 18;
            private const int LVHT_EX_GROUP_HEADER = 0x10000000;
            [StructLayout(LayoutKind.Sequential)]
            private struct LVHITTESTINFO
            {
                public int pt_x;
                public int pt_y;
                public int flags;
                public int iItem;
                public int iSubItem;
                public int iGroup;
            }
            [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
            private static extern int SendMessage(IntPtr hWnd, int msg,
                int wParam, ref LVHITTESTINFO ht);
            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                var ht = new LVHITTESTINFO() { pt_x = e.X, pt_y = e.Y };
                var value = SendMessage(this.Handle, LVM_HITTEST, -1, ref ht);
                if (value != -1 && (ht.flags & LVHT_EX_GROUP_HEADER) != 0)
                    OnGroupHeaderClick(value);
            }
        }

        class OrderNM
        {
            public String numberOfOrder;
            public String modificationOfOrder;
            public OrderNM(String number, string modification)
            {
                numberOfOrder = number;
                modificationOfOrder = modification;
            }

        }

        //List<ColumnHeader> columnHeadersMain = new List<ColumnHeader>();
        List<OrderNM> ordersNumbers = new List<OrderNM>();

        int currentPage = 1;
        int selectedYear = 0;
        int selectedMonth = 0;
        String selectedCategory = "Все";
        String selectedMachine  = "";
        String selectedUser = "";
        DateTime selectedDateTime = DateTime.Now.AddMonths(-2);

        bool thJob = false;
        bool calculateNullShiftsFromUser = false;

        String[] columnHeadersMain = { 
            "Исполнитель;200",
            "Оборудование;200",
            "Активный заказ;300",
            "Статус заказ;200",
            "Начало выполнения;150",
            "Остаток времени;150"
        };

        String[] columnHeadersStatistic = {
            "№;30",
            "Исполнитель;200",
            "Смен отработано;150",
            "Фактически отработано;150",
            "Сделано заказов/приладок;170",
            "Сделано продукции;150",
            "Выработка, ч.;120",
            "Выработка, %;120"
        };

        String[] columnHeadersShifts = {
            "№;30",
            "Дата, смена;90",
            "Оборудование;200",
            "Отработано;80",
            "Сделано заказов;100",
            "Сделано продукции;100",
            "Выработка, ч.;120",
            "Выработка, %;120"
        };

        String[] columnHeadersStatisticMachines = {
            "№;30",
            "Участок;200",
            "Оборудование;150",
            "Сделано заказов за месяц (год);200",
            "Сделано продукции за месяц (год);200"
        };

        String[] columnHeadersAllOrders = {
            "№;30",
            "Заказ;180",
            "Звквзчик;160",
            "Приладка;80",
            "Работа;80",
            "Тираж;80",
            "Начато выполнение;120",
            "Завершено выполнени;120",
            "Сделано;80",
            "Статус выполнения;120"
        };

        String[] columnHeadersNorm = {
            "№;30",
            "Штамп;180",
            "Оборудование;180",
            "Звказчик;160",
            "Приладка;80",
            "Работа;80"
        };

        String[] columnHeadersUsers = {
            "№;30",
            "Имя;180",
            "Полное имя;180",
            "Рабочий участок;200",
            "Дата трудоустройства;160",
            "Дата увольнения;160",
            "Стаж;120",
            "Дата рождения;160",
            "Возраст;90",
            "Примечание;180"
        };

        String[] columnHeadersMachines = {
            "№;30",
            "Оборудование;180",
            "Дата начала работы;200",
            "Время работы;160",
            "Примечание;360",
        };

        private void FormAdmin_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Enabled = false;
            Application.Exit();
        }

        private List<ColumnHeader> ListHeaders(String[] headersWidth)
        {
            List<ColumnHeader> columnHeaders = new List<ColumnHeader>();

            for (int i = 0; i < headersWidth.Length; i++)
            {
                String[] head = headersWidth[i].Split(';');

                columnHeaders.Add(new ColumnHeader()
                {
                    Text = head[0].ToString(),
                    Width = Convert.ToInt32(head[1])
                });
            }
            
            return columnHeaders;
        }

        private List<ColumnHeader> Headers(int loadPage)
        {
            List<ColumnHeader> columnHeaders = new List<ColumnHeader>();

            StringArray stringArray = new StringArray('|');
            INISettings ini = new INISettings();

            String inStr = "";

            switch (loadPage)
            {
                case 1:
                    inStr = ini.GetColumnHeadersMain();
                    break;
                case 2:
                    inStr = ini.GetColumnHeadersStatistic();
                    break;
                case 3:
                    inStr = ini.GetColumnHeadersShifts();
                    break;
                case 4:
                    inStr = ini.GetColumnHeadersStatisticMachines();
                    break;
                case 5:
                    inStr = ini.GetColumnHeadersAllOrders();
                    break;
                case 6:
                    inStr = ini.GetColumnHeadersNorm();
                    break;
                case 7:
                    inStr = ini.GetColumnHeadersUsers();
                    break;
                case 8:
                    inStr = ini.GetColumnHeadersMachines();
                    break;
                case 9:

                    break;

                default:
                    break;
            }

            String[] headersWidth = stringArray.ArrayFromTheString(inStr);

            for (int i = 0; i < headersWidth.Length; i++)
            {
                String[] head = headersWidth[i].Split(';');

                columnHeaders.Add(new ColumnHeader()
                {
                    Text = head[0].ToString(),
                    Width = Convert.ToInt32(head[1])
                });
            }

            //MessageBox.Show(currentPage.ToString() + ": " + inStr);

            return columnHeaders;
        }

        private void SaveHeaders(ListView listView)
        {
            INISettings ini = new INISettings();

            String hedersStr = "";

                for (int i = 0; i < listView.Columns.Count - 1; i++)
                {
                    hedersStr += listView.Columns[i].Text + ";" + listView.Columns[i].Width + "|";
                }
                hedersStr += listView.Columns[listView.Columns.Count - 1].Text + ";" + listView.Columns[listView.Columns.Count - 1].Width;
            if (hedersStr.Length > 0)
            switch (currentPage)
            {
                case 1:
                    ini.SetColumnHeadersMain(hedersStr);
                    break;
                case 2:
                    ini.SetColumnHeadersStatistic(hedersStr);
                    break;
                case 3:
                    ini.SetColumnHeadersShifts(hedersStr);
                    break;
                case 4:
                    ini.SetColumnHeadersStatisticMachines(hedersStr);
                    break;
                case 5:
                    ini.SetColumnHeadersAllOrders(hedersStr);
                    break;
                case 6:
                    ini.SetColumnHeadersNorm(hedersStr);
                    break;
                case 7:
                    ini.SetColumnHeadersUsers(hedersStr);
                    break;
                case 8:
                    ini.SetColumnHeadersMachines(hedersStr);
                    break;
                case 9:

                    break;

                default:
                    break;
            }
        }

        private List<String> LoadYears()
        {
            List<String> years = new List<String>();

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT startShift FROM shifts"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    if (years.IndexOf(Convert.ToDateTime(sqlReader["startShift"]).ToString("yyyy")) == -1)
                        years.Add(Convert.ToDateTime(sqlReader["startShift"]).ToString("yyyy"));
                }

                Connect.Close();
            }

            return years;
        }

        private List<String> LoadMachine()
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            List<String> machine = new List<String>();

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT id FROM machines"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    machine.Add(getInfo.GetMachineName(sqlReader["id"].ToString()));
                }

                Connect.Close();
            }

            return machine;
        }

        private void LoadSelectedOrder(bool detailsLoad, String orderMachine, String orderNumberm, String orderModification)
        {
            FormFullListOrders form = new FormFullListOrders(dataBase, detailsLoad, orderMachine, orderNumberm, orderModification);
            form.ShowDialog();
        }

        private void ShowFullOrdersForm(bool editOrder, int selectedIndex)
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            FormAddNewOrder form;

            if (editOrder)
                form = new FormAddNewOrder(dataBase, getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[selectedIndex].numberOfOrder, ordersNumbers[selectedIndex].modificationOfOrder);
            else
                form = new FormAddNewOrder(dataBase, getInfo.GetMachineFromName(comboBoxMachine.Text));

            form.ShowDialog();
        }

        private void ShowAddUserForm()
        {
            FormAddEditUser form = new FormAddEditUser(dataBase);

            form.ShowDialog();
        }

        private void ShowEditUserForm(String userID)
        {
            FormAddEditUser form = new FormAddEditUser(dataBase, userID);

            form.ShowDialog();
        }

        private void ShowSelectDataBaseFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            INISettings ini = new INISettings();

            String path = "";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                openFileDialog.Filter = "Файл базы данных *.db|*.db";
                
                openFileDialog.Multiselect = false;
                openFileDialog.Title = "Открыть базу данных";

                path = openFileDialog.FileName;

                ini.SetDataBasePath(path);
            }
            
        }

        private void DeleteUser(String id)
        {
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            DialogResult result;
            result = MessageBox.Show("Вы действительно хотите удалить сотрудника " + getUser.GetNameUser(id) + "?", "Удаление сотрудника", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                {
                    string commandText = "DELETE FROM users WHERE id = @id";

                    SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@id", id);
                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }

                using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                {
                    string commandText = "DELETE FROM usersInfo WHERE user = @id";

                    SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@id", id);
                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }

                using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                {
                    string commandText = "DELETE FROM usersSettings WHERE userID = @id";

                    SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@id", id);
                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }

        private void ShowAddCategoryForm()
        {
            FormAddEditCategory form = new FormAddEditCategory(dataBase);

            form.ShowDialog();
        }

        private void ShowEditCategoryForm(String categoryID)
        {
            FormAddEditCategory form = new FormAddEditCategory(dataBase, categoryID);

            form.ShowDialog();
        }

        private void DeleteCategory(String id)
        {
            ValueCategory getCategory = new ValueCategory(dataBase);

            DialogResult result;
            result = MessageBox.Show("Вы действительно хотите удалить участок '" + getCategory.GetCategoryName(id) + "'?", "Удаление участка", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                {
                    string commandText = "DELETE FROM machinesCategoryes WHERE id = @id";

                    SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@id", id);
                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }

        private void ShowAddMachineForm()
        {
            FormAddEditMachine form = new FormAddEditMachine(dataBase);

            form.ShowDialog();
        }

        private void ShowEditMachineForm(String machineID)
        {
            FormAddEditMachine form = new FormAddEditMachine(dataBase, machineID);

            form.ShowDialog();
        }

        private void DeleteMachine(String id)
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            DialogResult result;
            result = MessageBox.Show("Вы действительно хотите удалить оборудование '" + getInfo.GetMachineName(id) + "'?", "Удаление оборудования", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                {
                    string commandText = "DELETE FROM machines WHERE id = @id";

                    SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@id", id);
                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }

                using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                {
                    string commandText = "DELETE FROM machinesInfo WHERE machine = @id";

                    SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                    Command.Parameters.AddWithValue("@id", id);
                    Connect.Open();
                    Command.ExecuteNonQuery();
                    Connect.Close();
                }
            }
        }

        private void DeactivateOrder(String machine, String orderNumber, String orderModification, String newStatus)
        {
            DialogResult result;
            result = MessageBox.Show("Вы действительно хотите деактивировать заказ?", "Завершение заказа", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                SetNewStatus(machine, orderNumber, orderModification, newStatus);
        }

        private void SetNewStatus(String orderMachine, String numberOfOrder, String orderModification, String newStatus)
        {
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                string commandText = "UPDATE orders SET statusOfOrder = @status " +
                    "WHERE machine = @orderMachine AND (numberOfOrder = @number AND modification = @orderModification)";

                SQLiteCommand Command = new SQLiteCommand(commandText, Connect);
                Command.Parameters.AddWithValue("@status", newStatus);
                Command.Parameters.AddWithValue("@orderMachine", orderMachine);
                Command.Parameters.AddWithValue("@number", numberOfOrder);
                Command.Parameters.AddWithValue("@orderModification", orderModification);
                Connect.Open();
                Command.ExecuteNonQuery();
                Connect.Close();
            }
        }

        private void CreatActivityControl()
        {
            var nameTab = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(nameTab))
            {
                var control = tableLayoutPanel1.Controls.Find(nameTab, true);
                tableLayoutPanel1.Controls.Remove(control[0]);
            }


        }

        private void CreateListView(List<ColumnHeader> headers)
        {
            var name = "listView";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);
                tableLayoutPanel1.Controls.Remove(control[0]);
            }

            MyListView listView = new MyListView();

            listView.Visible = false;

            listView.Columns.AddRange(headers.ToArray());

            listView.Name = "listView";
            listView.Dock = System.Windows.Forms.DockStyle.Fill;
            listView.ForeColor = System.Drawing.Color.Black;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            listView.HideSelection = false;
            listView.Location = new System.Drawing.Point(3, 98);
            listView.MultiSelect = false;
            listView.Size = new System.Drawing.Size(1258, 316);
            listView.TabIndex = 2;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = System.Windows.Forms.View.Details;

            listView.Visible = true;

            tableLayoutPanel1.Controls.Add(listView, 0, 2);

            //listView.Tag = listView.SelectedItems[0].Name;
            listView.DoubleClick += new EventHandler(ListViewDoubleClick);
            listView.SelectedIndexChanged += new EventHandler(listView_SelectedIndexChanged);
            listView.GroupHeaderClick += myListView_GroupHeaderClick;
            //Controls.Add(listView);
        }

        private void ContextMenuToLV()
        {
            ListView listV = new ListView();

            var name = "listView";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);

                listV = (ListView)control[0];
                //tableLayoutPanel1.Controls.Remove(control[0]);
            }

            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem viewToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "viewToolStripMenuItem",
                Text = "Просмотр"
            }; 

            ToolStripMenuItem editToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "editToolStripMenuItem",
                Text = "Редактировать"
            };

            ToolStripMenuItem deactivateToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "deactivateToolStripMenuItem",
                Text = "Деактивировать"
            };
            //Список заказов

            ToolStripMenuItem editCategoryItem = new ToolStripMenuItem
            {
                Name = "editCategoryItem",
                Text = "Редактировать участок"
            };

            ToolStripMenuItem editMachineItem = new ToolStripMenuItem
            {
                Name = "editMachineItem",
                Text = "Редактировать оборудование"
            };

            ToolStripMenuItem deleteditCategoryItem = new ToolStripMenuItem
            {
                Name = "editCategoryItem",
                Text = "Удалить участок"
            };

            ToolStripMenuItem deleteMachineItem = new ToolStripMenuItem
            {
                Name = "editMachineItem",
                Text = "Удалить оборудование"
            };
            //Оборудование по категориям

            ToolStripMenuItem editUserItem = new ToolStripMenuItem
            {
                Name = "editUserItem",
                Text = "Редактировать"
            };

            ToolStripMenuItem deleteUserItem = new ToolStripMenuItem
            {
                Name = "deleteUserItem",
                Text = "Удалить"
            };
            //Сотрудники

            ToolStripMenuItem detailsShift = new ToolStripMenuItem
            {
                Name = "detailsShift",
                Text = "Детали смены"
            };

            ToolStripMenuItem closeShift = new ToolStripMenuItem
            {
                Name = "closeShift",
                Text = "Завершить смену"
            };
            //Активные исполнители

            ToolStripMenuItem deleteItem = new ToolStripMenuItem
            {
                Name = "deleteToolStripMenuItem1",
                Text = "Удалить"
            };

            ToolStripItem[] items = new ToolStripItem[] {};

            switch (currentPage)
            {
                case 1:
                    items = new ToolStripItem[]
                    {
                        detailsShift,
                        closeShift
                    };
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    items = new ToolStripItem[] 
                    {
                        viewToolStripMenuItem,
                        editToolStripMenuItem,
                        deactivateToolStripMenuItem
                    };
                   break;
                case 6:

                    break;
                case 7:
                    items = new ToolStripItem[]
                    {
                        editUserItem,
                        deleteUserItem
                    };
                    break;

                case 8:
                    items = new ToolStripItem[]
                    {
                        editCategoryItem,
                        editMachineItem,
                        deleteditCategoryItem,
                        deleteMachineItem
                    };
                    break;

                default:
                    break;
            }

            menu.Items.AddRange(items);

            menu.Name = "contextMenuStrip";
            menu.Size = new Size(163, 92);
            menu.Opening += new CancelEventHandler(contextMenuStrip_Opening);
            menu.Closed += new ToolStripDropDownClosedEventHandler(contextMenuStrip_Closed);

            viewToolStripMenuItem.Click += new EventHandler(viewToolStripMenuItem_Click);
            editToolStripMenuItem.Click += new EventHandler(editToolStripMenuItem_Click);
            deactivateToolStripMenuItem.Click += new EventHandler(deactivateToolStripMenuItem_Click);

            editCategoryItem.Click += new EventHandler(editToolStripMenuItem_Click);
            editMachineItem.Click += new EventHandler(editToolStripMenuItem_Click);
            deleteditCategoryItem.Click += new EventHandler(deleteToolStripMenuItem_Click);
            deleteMachineItem.Click += new EventHandler(deleteToolStripMenuItem_Click);

            editUserItem.Click += new EventHandler(editToolStripMenuItem_Click);
            deleteUserItem.Click += new EventHandler(deleteToolStripMenuItem_Click);

            detailsShift.Click += new EventHandler(viewToolStripMenuItem_Click);
            closeShift.Click += new EventHandler(closeShift_Click);

            deleteItem.Click += new EventHandler(deleteToolStripMenuItem_Click);

            listV.ContextMenuStrip = menu;
        }

        private void CreateStatisticUsersControls()
        {
            ValueCategory category = new ValueCategory(dataBase);

            List<String> categoryes = new List<String>(category.GetCategoryesList());
            List<String> years = new List<String>(LoadYears());
            String[] mounts = new String[] {
            "Январь",
            "Февраль",
            "Март",
            "Апрель",
            "Май",
            "Июнь",
            "Июль",
            "Август",
            "Сентябрь",
            "Октябрь",
            "Ноябрь",
            "Декабрь"};

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);
                tableLayoutPanel1.Controls.Remove(control[0]);
            }

            TableLayoutPanel tableLayoutPanelControl = new TableLayoutPanel();

            tableLayoutPanelControl.Dock = DockStyle.Fill;
            tableLayoutPanelControl.Name = "tableLayoutPanelControl";

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

            tableLayoutPanelControl.RowCount = 3;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 1);

            ComboBox comboBoxYear = new ComboBox();
            comboBoxYear.Name = "comboBoxYear";
            comboBoxYear.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxYear.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxYear.FormattingEnabled = true;
            comboBoxYear.Location = new System.Drawing.Point(3, 3);
            comboBoxYear.Size = new System.Drawing.Size(144, 21);
            comboBoxYear.TabIndex = 0;
            comboBoxYear.Items.AddRange(years.ToArray());
            comboBoxYear.SelectedIndex = comboBoxYear.Items.IndexOf(selectedYear.ToString());
            comboBoxYear.Visible = true;
            tableLayoutPanelControl.Controls.Add(comboBoxYear, 0, 0);
            
            comboBoxYear.SelectedIndexChanged += new System.EventHandler(comboBoxYear_SelectedIndexChanged);


            ComboBox comboBoxMount = new ComboBox();
            comboBoxMount.Name = "comboBoxMount";
            comboBoxMount.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxMount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxMount.FormattingEnabled = true;
            comboBoxMount.Location = new System.Drawing.Point(3, 3);
            comboBoxMount.Size = new System.Drawing.Size(144, 21);
            comboBoxMount.TabIndex = 1;
            comboBoxMount.Items.AddRange(mounts);
            comboBoxMount.SelectedIndex = selectedMonth - 1;
            comboBoxMount.Visible = true;
            tableLayoutPanelControl.Controls.Add(comboBoxMount, 0, 1);

            comboBoxMount.SelectedIndexChanged += new System.EventHandler(comboBoxMount_SelectedIndexChanged);

            ComboBox comboBoxCategory = new ComboBox();
            comboBoxCategory.Name = "comboBoxCategory";
            comboBoxCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxCategory.FormattingEnabled = true;
            comboBoxCategory.Location = new System.Drawing.Point(3, 3);
            comboBoxCategory.Size = new System.Drawing.Size(144, 21);
            comboBoxCategory.TabIndex = 1;
            comboBoxCategory.Items.Add("Все");
            comboBoxCategory.Items.AddRange(categoryes.ToArray());
            comboBoxCategory.SelectedIndex = comboBoxCategory.Items.IndexOf(selectedCategory.ToString());
            comboBoxCategory.Visible = true;
            tableLayoutPanelControl.Controls.Add(comboBoxCategory, 0, 2);

            comboBoxCategory.SelectedIndexChanged += new System.EventHandler(comboBoxCategory_SelectedIndexChanged);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "- год", ContentAlignment.MiddleLeft), 1, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "- месяц", ContentAlignment.MiddleLeft), 1, 1);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "Сумма отработанных смен:", ContentAlignment.MiddleRight), 2, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label1", "", ContentAlignment.MiddleLeft), 3, 0);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label02", "Сумма отработанных часов:", ContentAlignment.MiddleRight), 2, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label2", "", ContentAlignment.MiddleLeft), 3, 1);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label03", "Сумма эффективных часов:", ContentAlignment.MiddleRight), 2, 2);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label3", "", ContentAlignment.MiddleLeft), 3, 2);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label04", "Сделано заказов/приладок:", ContentAlignment.MiddleRight), 4, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label4", "", ContentAlignment.MiddleLeft), 5, 0);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label05", "Сделано продукции:", ContentAlignment.MiddleRight), 4, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label5", "", ContentAlignment.MiddleLeft), 5, 1);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label06", "Средняя выработка:", ContentAlignment.MiddleRight), 4, 2);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label6", "", ContentAlignment.MiddleLeft), 5, 2);

            /*
            Label label01 = new Label();
            label01.AutoSize = true;
            label01.Dock = System.Windows.Forms.DockStyle.Fill;
            label01.Location = new System.Drawing.Point(1, 1);
            label01.Name = "label01";
            label01.Size = new System.Drawing.Size(64, 24);
            label01.TabIndex = 10;
            label01.Text = "Сумма отработанных смен:";
            label01.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            label01.Visible = true;
            tableLayoutPanelControl.Controls.Add(label01, 1, 0);

            Label label1 = new Label();
            label1.AutoSize = true;
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.Location = new System.Drawing.Point(1, 1);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(64, 24);
            label1.TabIndex = 10;
            label1.Text = "";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label1.Visible = true;
            tableLayoutPanelControl.Controls.Add(label1, 2, 0);
            */


        }

        private void CreateShiftsControls()
        {
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            List<String> years = new List<String>(LoadYears());
            String[] mounts = new String[] {
            "Январь",
            "Февраль",
            "Март",
            "Апрель",
            "Май",
            "Июнь",
            "Июль",
            "Август",
            "Сентябрь",
            "Октябрь",
            "Ноябрь",
            "Декабрь"};
            List<String> users = new List<String>(getUser.GetUserList(true));
            //List<String> usersNames = new List<String>();

            //users.ForEach(u => usersNames.Add(getUser.GetNameUser(u)));

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);
                tableLayoutPanel1.Controls.Remove(control[0]);
            }

            TableLayoutPanel tableLayoutPanelControl = new TableLayoutPanel();

            tableLayoutPanelControl.Dock = DockStyle.Fill;
            tableLayoutPanelControl.Name = "tableLayoutPanelControl";

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 85));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

            tableLayoutPanelControl.RowCount = 3;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 1);

            ComboBox comboBoxYear = new ComboBox();
            comboBoxYear.Name = "comboBoxYear";
            comboBoxYear.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxYear.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxYear.FormattingEnabled = true;
            comboBoxYear.Location = new System.Drawing.Point(3, 3);
            comboBoxYear.Size = new System.Drawing.Size(144, 21);
            comboBoxYear.TabIndex = 0;
            comboBoxYear.Items.AddRange(years.ToArray());
            comboBoxYear.SelectedIndex = comboBoxYear.Items.IndexOf(selectedYear.ToString());
            comboBoxYear.Visible = true;
            tableLayoutPanelControl.Controls.Add(comboBoxYear, 0, 0);

            comboBoxYear.SelectedIndexChanged += new System.EventHandler(comboBoxYear_SelectedIndexChanged);


            ComboBox comboBoxMount = new ComboBox();
            comboBoxMount.Name = "comboBoxMount";
            comboBoxMount.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxMount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxMount.FormattingEnabled = true;
            comboBoxMount.Location = new System.Drawing.Point(3, 3);
            comboBoxMount.Size = new System.Drawing.Size(144, 21);
            comboBoxMount.TabIndex = 1;
            comboBoxMount.Items.AddRange(mounts);
            comboBoxMount.SelectedIndex = selectedMonth - 1;
            comboBoxMount.Visible = true;
            tableLayoutPanelControl.Controls.Add(comboBoxMount, 0, 1);

            comboBoxMount.SelectedIndexChanged += new System.EventHandler(comboBoxMount_SelectedIndexChanged);


            ComboBox comboBoxUser = new ComboBox();
            comboBoxUser.Name = "comboBoxUser";
            comboBoxUser.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxUser.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxUser.FormattingEnabled = true;
            comboBoxUser.Location = new System.Drawing.Point(3, 3);
            comboBoxUser.Size = new System.Drawing.Size(144, 21);
            comboBoxUser.TabIndex = 1;
            //comboBoxUser.Items.AddRange(usersNames.ToArray());
            users.ForEach(u => comboBoxUser.Items.Add(getUser.GetNameUser(u)));
            comboBoxUser.SelectedIndex = comboBoxUser.Items.IndexOf(selectedUser.ToString());
            comboBoxUser.Visible = true;
            tableLayoutPanelControl.Controls.Add(comboBoxUser, 0, 2);

            comboBoxUser.SelectedIndexChanged += new System.EventHandler(comboBoxUser_SelectedIndexChanged);


            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label001", "- год", ContentAlignment.MiddleLeft), 1, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label002", "- месяц", ContentAlignment.MiddleLeft), 1, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label003", "- сотрудник", ContentAlignment.MiddleLeft), 1, 2);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "Рабочие часы:", ContentAlignment.MiddleRight), 2, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label1", "", ContentAlignment.MiddleLeft), 3, 0);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label02", "Фактически отработано:", ContentAlignment.MiddleRight), 2, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label2", "", ContentAlignment.MiddleLeft), 3, 1);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label03", "Эффективные часы:", ContentAlignment.MiddleRight), 2, 2);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label3", "", ContentAlignment.MiddleLeft), 3, 2);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label04", "Сделано заказов/приладок:", ContentAlignment.MiddleRight), 4, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label4", "", ContentAlignment.MiddleLeft), 5, 0);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label05", "Сделано продукции:", ContentAlignment.MiddleRight), 4, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label5", "", ContentAlignment.MiddleLeft), 5, 1);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label06", "Средняя выработка:", ContentAlignment.MiddleRight), 4, 2);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label6", "", ContentAlignment.MiddleLeft), 5, 2);

        }

        private void CreateStatisticMachinesControls()
        {
            ValueCategory category = new ValueCategory(dataBase);

            List<String> categoryes = new List<String>(category.GetCategoryesList());
            List<String> years = new List<String>(LoadYears());
            String[] mounts = new String[] {
            "Январь",
            "Февраль",
            "Март",
            "Апрель",
            "Май",
            "Июнь",
            "Июль",
            "Август",
            "Сентябрь",
            "Октябрь",
            "Ноябрь",
            "Декабрь"};

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);
                tableLayoutPanel1.Controls.Remove(control[0]);
            }

            TableLayoutPanel tableLayoutPanelControl = new TableLayoutPanel();

            tableLayoutPanelControl.Dock = DockStyle.Fill;
            tableLayoutPanelControl.Name = "tableLayoutPanelControl";

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

            tableLayoutPanelControl.RowCount = 3;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 1);

            ComboBox comboBoxYear = new ComboBox();
            comboBoxYear.Name = "comboBoxYear";
            comboBoxYear.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxYear.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxYear.FormattingEnabled = true;
            comboBoxYear.Location = new System.Drawing.Point(3, 3);
            comboBoxYear.Size = new System.Drawing.Size(144, 21);
            comboBoxYear.TabIndex = 0;
            comboBoxYear.Items.AddRange(years.ToArray());
            comboBoxYear.SelectedIndex = comboBoxYear.Items.IndexOf(selectedYear.ToString());
            comboBoxYear.Visible = true;
            tableLayoutPanelControl.Controls.Add(comboBoxYear, 0, 0);

            comboBoxYear.SelectedIndexChanged += new System.EventHandler(comboBoxYear_SelectedIndexChanged);


            ComboBox comboBoxMount = new ComboBox();
            comboBoxMount.Name = "comboBoxMount";
            comboBoxMount.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxMount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxMount.FormattingEnabled = true;
            comboBoxMount.Location = new System.Drawing.Point(3, 3);
            comboBoxMount.Size = new System.Drawing.Size(144, 21);
            comboBoxMount.TabIndex = 1;
            comboBoxMount.Items.AddRange(mounts);
            comboBoxMount.SelectedIndex = selectedMonth - 1;
            comboBoxMount.Visible = true;
            tableLayoutPanelControl.Controls.Add(comboBoxMount, 0, 1);

            comboBoxMount.SelectedIndexChanged += new System.EventHandler(comboBoxMount_SelectedIndexChanged);

            ComboBox comboBoxCategory = new ComboBox();
            comboBoxCategory.Name = "comboBoxCategory";
            comboBoxCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxCategory.FormattingEnabled = true;
            comboBoxCategory.Location = new System.Drawing.Point(3, 3);
            comboBoxCategory.Size = new System.Drawing.Size(144, 21);
            comboBoxCategory.TabIndex = 1;
            comboBoxCategory.Items.Add("Все");
            comboBoxCategory.Items.AddRange(categoryes.ToArray());
            comboBoxCategory.SelectedIndex = comboBoxCategory.Items.IndexOf(selectedCategory.ToString());
            comboBoxCategory.Visible = true;
            tableLayoutPanelControl.Controls.Add(comboBoxCategory, 0, 2);

            comboBoxCategory.SelectedIndexChanged += new System.EventHandler(comboBoxCategory_SelectedIndexChanged);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "- год", ContentAlignment.MiddleLeft), 1, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "- месяц", ContentAlignment.MiddleLeft), 1, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "- участок", ContentAlignment.MiddleLeft), 1, 2);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "Сделано заказов за месяц (год):", ContentAlignment.MiddleRight), 2, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label1", "", ContentAlignment.MiddleLeft), 3, 0);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label02", "Сделано продукции за месяц (год):", ContentAlignment.MiddleRight), 2, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label2", "", ContentAlignment.MiddleLeft), 3, 1);
        }

        private void CreateAllOrdersControls()
        {
            GetValueFromInfoBase getMachine = new GetValueFromInfoBase(dataBase);

            List<String> machine = new List<String>(LoadMachine());

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);
                tableLayoutPanel1.Controls.Remove(control[0]);
            }

            TableLayoutPanel tableLayoutPanelControl = new TableLayoutPanel();

            tableLayoutPanelControl.Dock = DockStyle.Fill;
            tableLayoutPanelControl.Name = "tableLayoutPanelControl";

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

            tableLayoutPanelControl.RowCount = 3;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 1);

            Button addButton = new Button();
            addButton.Name = "addButton";
            addButton.Dock = DockStyle.Fill;
            addButton.Location = new System.Drawing.Point(3, 3);
            addButton.Size = new System.Drawing.Size(144, 21);
            addButton.TabIndex = 0;
            addButton.Text = "Добавить";
            addButton.Visible = true;
            tableLayoutPanelControl.SetRowSpan(addButton, 3);
            tableLayoutPanelControl.Controls.Add(addButton, 0, 0);
            addButton.Click += new System.EventHandler(addButton_Click);


            DateTimePicker dateTime = new DateTimePicker();
            dateTime.Name = "dateTime";
            dateTime.Dock = DockStyle.Fill;
            dateTime.Location = new System.Drawing.Point(3, 3);
            dateTime.Size = new System.Drawing.Size(144, 21);
            dateTime.TabIndex = 0;
            dateTime.Value = selectedDateTime;
            dateTime.Visible = true;
            tableLayoutPanelControl.Controls.Add(dateTime, 2, 0);
            dateTime.ValueChanged += new System.EventHandler(dateTime_ValueChanged);


            ComboBox comboBoxMachine = new ComboBox();
            comboBoxMachine.Name = "comboBoxMachine";
            comboBoxMachine.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxMachine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxMachine.FormattingEnabled = true;
            comboBoxMachine.Location = new System.Drawing.Point(3, 3);
            comboBoxMachine.Size = new System.Drawing.Size(144, 21);
            comboBoxMachine.TabIndex = 1;
            comboBoxMachine.Items.AddRange(machine.ToArray());
            comboBoxMachine.SelectedIndex = 0;
            
            tableLayoutPanelControl.Controls.Add(comboBoxMachine, 2, 1);

            comboBoxMachine.Visible = true;
            if (comboBoxMachine.Items.IndexOf(getMachine.GetMachineName(selectedMachine)) != -1)
                comboBoxMachine.SelectedIndex = comboBoxMachine.Items.IndexOf(getMachine.GetMachineName(selectedMachine));

            comboBoxMachine.SelectedIndexChanged += new System.EventHandler(comboBoxMachine_SelectedIndexChanged);


            TextBox textBox = new TextBox();
            textBox.Name = "textBoxFilter";
            textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox.Location = new System.Drawing.Point(3, 3);
            textBox.Size = new System.Drawing.Size(144, 21);
            textBox.TabIndex = 2;
            textBox.Text = "";
            textBox.Visible = true;
            tableLayoutPanelControl.Controls.Add(textBox, 2, 2);
            textBox.TextChanged += new System.EventHandler(textBoxFilter_TextChanged);


            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label001", "- начало периода", ContentAlignment.MiddleLeft), 3, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label002", "- оборудование", ContentAlignment.MiddleLeft), 3, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label003", "- фильтр", ContentAlignment.MiddleLeft), 3, 2);
        }

        private void CreateUsersControls()
        {
            GetValueFromInfoBase getMachine = new GetValueFromInfoBase(dataBase);

            List<String> machine = new List<String>(LoadMachine());

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);
                tableLayoutPanel1.Controls.Remove(control[0]);
            }

            TableLayoutPanel tableLayoutPanelControl = new TableLayoutPanel();

            tableLayoutPanelControl.Dock = DockStyle.Fill;
            tableLayoutPanelControl.Name = "tableLayoutPanelControl";

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

            tableLayoutPanelControl.RowCount = 1;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 1);

            Button addButton = new Button();
            addButton.Name = "addButton";
            addButton.Dock = DockStyle.Fill;
            addButton.Location = new System.Drawing.Point(3, 3);
            addButton.Size = new System.Drawing.Size(144, 21);
            addButton.TabIndex = 0;
            addButton.Text = "Добавить";
            addButton.Visible = true;
            tableLayoutPanelControl.Controls.Add(addButton, 0, 0);
            addButton.Click += new System.EventHandler(addButton_Click);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "", ContentAlignment.MiddleRight), 1, 0);
        }

        private void CreateMachineControls()
        {
            GetValueFromInfoBase getMachine = new GetValueFromInfoBase(dataBase);

            List<String> machine = new List<String>(LoadMachine());

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);
                tableLayoutPanel1.Controls.Remove(control[0]);
            }

            TableLayoutPanel tableLayoutPanelControl = new TableLayoutPanel();

            tableLayoutPanelControl.Dock = DockStyle.Fill;
            tableLayoutPanelControl.Name = "tableLayoutPanelControl";

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

            tableLayoutPanelControl.RowCount = 1;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 1);

            Button addCategoryButton = new Button();
            addCategoryButton.Name = "addCategoryButton";
            addCategoryButton.Dock = DockStyle.Fill;
            addCategoryButton.Location = new System.Drawing.Point(3, 3);
            addCategoryButton.Size = new System.Drawing.Size(144, 21);
            addCategoryButton.TabIndex = 0;
            addCategoryButton.Text = "Добавить участок";
            addCategoryButton.Visible = true;
            tableLayoutPanelControl.Controls.Add(addCategoryButton, 0, 0);
            addCategoryButton.Click += new System.EventHandler(addButton_Click);

            Button addMachineButton = new Button();
            addMachineButton.Name = "addMachineButton";
            addMachineButton.Dock = DockStyle.Fill;
            addMachineButton.Location = new System.Drawing.Point(1, 0);
            addMachineButton.Size = new System.Drawing.Size(144, 21);
            addMachineButton.TabIndex = 0;
            addMachineButton.Text = "Добавить оборудование";
            addMachineButton.Visible = true;
            tableLayoutPanelControl.Controls.Add(addMachineButton, 1, 0);
            addMachineButton.Click += new System.EventHandler(addButton_Click);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "", ContentAlignment.MiddleRight), 2, 0);
        }

        private void CreateNormControls()
        {
            GetValueFromInfoBase getMachine = new GetValueFromInfoBase(dataBase);

            List<String> machine = new List<String>(LoadMachine());

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);
                tableLayoutPanel1.Controls.Remove(control[0]);
            }

            TableLayoutPanel tableLayoutPanelControl = new TableLayoutPanel();

            tableLayoutPanelControl.Dock = DockStyle.Fill;
            tableLayoutPanelControl.Name = "tableLayoutPanelControl";

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

            tableLayoutPanelControl.RowCount = 1;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 1);

            TextBox textBox = new TextBox();
            textBox.Name = "textBoxFilter";
            textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox.Location = new System.Drawing.Point(3, 3);
            textBox.Size = new System.Drawing.Size(144, 21);
            textBox.TabIndex = 2;
            textBox.Text = "";
            textBox.Visible = true;
            tableLayoutPanelControl.Controls.Add(textBox, 0, 0);
            textBox.TextChanged += new System.EventHandler(textBoxFilter_TextChanged);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label001", "- фильтр", ContentAlignment.MiddleLeft), 1, 0);
        }

        private void CreateSettingsControls()
        {
            GetValueFromOrdersBase getOrders = new GetValueFromOrdersBase(dataBase);
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);
            GetValueFromInfoBase getMachine = new GetValueFromInfoBase(dataBase);
            INISettings ini = new INISettings();

            List<String> machine = new List<String>(LoadMachine());

            String path = ini.DataBasePath();

            String[] name = { "listView", "tableLayoutPanelControl" };

            for (int i = 0; i < name.Length; i++)
            {
                if (tableLayoutPanel1.Controls.ContainsKey(name[i]))
                {
                    var control = tableLayoutPanel1.Controls.Find(name[i], true);
                    tableLayoutPanel1.Controls.Remove(control[0]);
                }
            }

            /*for (int i = 0; i < tableLayoutPanel1.Controls.Count; i++)
            {
                MessageBox.Show(tableLayoutPanel1.Controls[i].Name);

                if (tableLayoutPanel1.Controls[i].Name != "tableLayoutPanel2")
                    tableLayoutPanel1.Controls.RemoveAt(i);
            }*/

            

            TableLayoutPanel tableLayoutPanelControl = new TableLayoutPanel();

            tableLayoutPanelControl.Dock = DockStyle.Fill;
            tableLayoutPanelControl.Name = "tableLayoutPanelControl";

            tableLayoutPanelControl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tableLayoutPanelControl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            tableLayoutPanelControl.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            tableLayoutPanelControl.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            tableLayoutPanelControl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tableLayoutPanelControl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tableLayoutPanelControl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));


            tableLayoutPanelControl.RowCount = 5;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 2);

            TableLayoutPanel tableLayoutPanelPath = new TableLayoutPanel();
            tableLayoutPanelPath.Dock = DockStyle.Fill;
            tableLayoutPanelPath.Name = "tableLayoutPanelPath";
            tableLayoutPanelPath.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanelPath.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tableLayoutPanelPath.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tableLayoutPanelPath.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tableLayoutPanelPath.RowCount = 1;
            tableLayoutPanelPath.Visible = true;

            TextBox textBox = new TextBox();
            textBox.Name = "textBoxDBPath";
            textBox.Dock = DockStyle.Fill;
            textBox.Location = new Point(3, 3);
            textBox.Size = new Size(144, 21);
            textBox.TabIndex = 2;
            textBox.Text = path;
            textBox.Visible = true;
            tableLayoutPanelPath.Controls.Add(textBox, 0, 0);

            Button addButton = new Button();
            addButton.Name = "addButtonDBPath";
            addButton.Dock = DockStyle.Top;
            addButton.Location = new System.Drawing.Point(0, 0);
            addButton.Size = new System.Drawing.Size(144, 21);
            addButton.TabIndex = 0;
            addButton.Text = "Обзор";
            addButton.Visible = true;
            tableLayoutPanelPath.Controls.Add(addButton, 1, 0);
            addButton.Click += new System.EventHandler(addButton_Click);

            CheckBox checkBox = new CheckBox();
            checkBox.Name = "checkBoxDBLoacacion";
            checkBox.Dock = DockStyle.Fill;
            checkBox.Location = new Point(3, 3);
            checkBox.Size = new Size(144, 21);
            checkBox.TabIndex = 2;
            checkBox.Text = "Использовать локальную базу данныx";
            checkBox.Checked = ini.GetCheckDBLocalPath();
            checkBox.Visible = true;
            checkBox.CheckedChanged += checkBox_CheckedChanged;
            tableLayoutPanelControl.Controls.Add(checkBox, 0, 0);

            //Первая колонка
            tableLayoutPanelControl.Controls.Add(tableLayoutPanelPath, 0, 1);

            EnabledPathControls(checkBox.Checked);

            //Вторая колонка
            String countOrders = String.Format("{0,-35}{1,-25:f4}", "Всего заказов в базе:", getOrders.GetCountOrders().ToString("N0"));
            String countUsers = String.Format("{0,-35}{1,-25:f4}", "Всего сотрудников:", getUser.GetCountUsers().ToString("N0"));

            tableLayoutPanelControl.Controls.Add(CreateLabel("countOrders", countOrders, ContentAlignment.MiddleLeft, new Font("Consolas", 12)), 1, 0);
            tableLayoutPanelControl.Controls.Add(CreateLabel("countUsers", countUsers, ContentAlignment.MiddleLeft, new Font("Consolas", 12)), 1, 1);

            //tableLayoutPanelControl.Controls.Add(CreateLabel("label001", getOrders.GetCountOrders().ToString("N0"), ContentAlignment.MiddleLeft), 1, 0);

            //tableLayoutPanelControl.Controls.Add(CreateLabel("labelX", (tableLayoutPanelControl.RowCount).ToString(), ContentAlignment.MiddleLeft), 0, 5);
        }

        private Label CreateLabel(String name, String text, ContentAlignment contentAlignment)
        {
            Label label = new Label();
            label.AutoSize = true;
            label.Dock = System.Windows.Forms.DockStyle.Fill;
            label.Location = new System.Drawing.Point(1, 1);
            label.Name = name;
            label.Size = new System.Drawing.Size(64, 24);
            label.TabIndex = 10;
            label.Text = text;
            label.TextAlign = contentAlignment;
            label.Visible = true;

            return label;
        }

        private Label CreateLabel(String name, String text, ContentAlignment contentAlignment, Font font)
        {
            Label label = new Label();
            label.AutoSize = true;
            label.Dock = System.Windows.Forms.DockStyle.Fill;
            label.Location = new System.Drawing.Point(1, 1);
            label.Name = name;
            label.Size = new System.Drawing.Size(64, 24);
            label.TabIndex = 10;
            label.Text = text;
            label.TextAlign = contentAlignment;
            label.Visible = true;
            label.Font = font;

            return label;
        }

        private void MainLVInsertValue()
        {
            GetValueFromShiftsBase getShifts = new GetValueFromShiftsBase(dataBase);
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetValueFromOrdersBase getOrder = new GetValueFromOrdersBase(dataBase);
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase(dataBase);
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            GetValueFromUserBase userBase = new GetValueFromUserBase(dataBase);

            List<String> users = new List<String>(getShifts.GetActiveUser());

            var name = "listView";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                ListView listView = (ListView)control[0];

                listView.Items.Clear();

                //this.Text += " - " + users.Count;

                for (int i = 0; i < users.Count; i++)
                {
                    List<String> machines = (List<String>)getInfo.GetMachines(users[i]);

                    for (int j = 0; j < machines.Count; j++)
                    {
                        GetLeadTime leadTimeCurr = new GetLeadTime(dataBase, userBase.GetCurrentShiftStart(users[i]), getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j]), machines[j], getOrder.GetCounterRepeat(machines[j], getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j])));

                        List<Order> ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(userBase.GetCurrentShiftStart(users[i]), "");

                        String user = "";
                        String currentTime = "";
                        String order = "";

                        if (getInfo.GetCurrentOrderNumber(machines[j]) != "")
                            order = getInfo.GetCurrentOrderNumber(machines[j]) + ", " + getOrder.GetOrderName(machines[j], getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j]));

                        if (j == 0)
                            user = users[i];
                        else
                            user = "";

                        if (leadTimeCurr.GetCurrentDateTime("timeMakereadyStart") != "")
                            currentTime = leadTimeCurr.GetCurrentDateTime("timeMakereadyStart");
                        else
                            currentTime = leadTimeCurr.GetCurrentDateTime("timeToWorkStart");


                        int idx = ordersCurrentShift.FindLastIndex((v) => v.numberOfOrder == getInfo.GetCurrentOrderNumber(machines[j]) &&
                                                                          v.modificationOfOrder == getInfo.GetCurrentOrderModification(machines[j]) &&
                                                                          v.machineOfOrder == machines[j]);
                        
                        String fullLastTime = "00:00";
                        String fullFactTime = "00:00";
                        String timeDiff = "";

                        if (idx != -1)
                        {
                            fullLastTime = timeOperations.TimeAmount(ordersCurrentShift[idx].plannedTimeMakeready, ordersCurrentShift[idx].plannedTimeWork);
                            fullFactTime = timeOperations.TimeAmount(ordersCurrentShift[idx].facticalTimeMakeready, ordersCurrentShift[idx].facticalTimeWork);

                            if (timeOperations.TimeDifferent(fullLastTime, fullFactTime) == "00:00")
                                timeDiff = "-" + timeOperations.TimeDifferent(fullFactTime, fullLastTime);
                            else if (timeOperations.TimeDifferent(fullFactTime, fullLastTime) == "00:00")
                                timeDiff = timeOperations.TimeDifferent(fullLastTime, fullFactTime);
                            else
                                timeDiff = "00:00";
                        }
                        
                        ListViewItem item = new ListViewItem();

                        item.Name = userBase.GetCurrentShiftStart(users[i]);
                        item.Text = userBase.GetNameUser(user);
                        item.SubItems.Add(getInfo.GetMachineName(machines[j]));
                        item.SubItems.Add(order);
                        item.SubItems.Add(getOrder.GetOrderStatusName(machines[j], getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j])));
                        item.SubItems.Add(currentTime);
                        item.SubItems.Add(timeDiff);

                        listView.Items.Add(item);
                    }


                }
                

            }
        }

        private Object ControlFromKey(String panel, String name)
        {
            if (tableLayoutPanel1.Controls.ContainsKey(panel))
            {
                var controlPanel = tableLayoutPanel1.Controls.Find(panel, true);
                TableLayoutPanel tableLayoutPanelControl = (TableLayoutPanel)controlPanel[0];

                if (tableLayoutPanelControl.Controls.ContainsKey(name))
                {
                    var control = tableLayoutPanelControl.Controls.Find(name, false);
                    return control[0];
                }
                else
                    return null;
            }
            else
                if (panel == "tableLayoutPanel1")
                {
                    if (tableLayoutPanel1.Controls.ContainsKey(name))
                    {
                        var control = tableLayoutPanel1.Controls.Find(name, true);
                        return control[0];
                    }
                    else
                        return null;
                }
                else
                    return null;
        }

        private void StartLoadingStatisticUsers()
        {
            /*
            var name = "comboBoxYear";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                ListView listView = (ListView)control[0];
            }
            */

            ComboBox comboBoxYear = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxYear");
            ComboBox comboBoxMount = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMount");
            ComboBox comboBoxCategory = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxCategory");

            if (comboBoxYear.SelectedIndex != -1 && comboBoxMount.SelectedIndex != -1 && comboBoxCategory.SelectedIndex != -1)
            {
                //ClearAll();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBoxYear.Text) - 1).AddMonths(comboBoxMount.SelectedIndex);

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;

                Task task = new Task(() => LoadUsersFromBase(token, date));

                if (thJob == true)
                {
                    cancelTokenSource.Cancel();
                }
                else
                {
                    //thJob = true;
                    task.Start();
                }
            }
        }

        private void LoadUsersFromBase(CancellationToken token, DateTime date)
        {
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);
            ValueCategory categoryValue = new ValueCategory(dataBase);

            ComboBox comboBoxYear = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxYear");
            ComboBox comboBoxMount = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMount");
            ComboBox comboBoxCategory = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxCategory");

            Label label1 = (Label)ControlFromKey("tableLayoutPanelControl", "label1");
            Label label2 = (Label)ControlFromKey("tableLayoutPanelControl", "label2");
            Label label3 = (Label)ControlFromKey("tableLayoutPanelControl", "label3");

            Label label4 = (Label)ControlFromKey("tableLayoutPanelControl", "label4");
            Label label5 = (Label)ControlFromKey("tableLayoutPanelControl", "label5");
            Label label6 = (Label)ControlFromKey("tableLayoutPanelControl", "label6");

            CheckBox checkBoxAllLoad = (CheckBox)ControlFromKey("tableLayoutPanelControl", "checkBoxAllLoad");

            bool loadAll = false;
            String cLine;

            String category = "Все";
            int selectedCategory = 0;

            Invoke(new Action(() =>
            {
                comboBoxYear.Enabled = false;
                comboBoxMount.Enabled = false;
                comboBoxCategory.Enabled = false;

                category = categoryValue.GetCategoryFromName(comboBoxCategory.Text);
                selectedCategory = comboBoxCategory.SelectedIndex;

                //loadAll = checkBoxAllLoad.Checked; //Сделать чекбокс
            }));

            if (loadAll)
                cLine = "";
            else
                cLine = " WHERE activeUser = 'True'";

            while (!token.IsCancellationRequested)
            {
                thJob = true;

                int fullCountShifts = 0;
                int fullTimeShifts = 0;
                int fullCountOrders = 0;
                int fullCountMakeready = 0;
                int fullAmountOrders = 0;
                int fullTimeWorkingOut = 0;
                float fullPercentWorkingOut = 0;
                int countActiveUser = 0;

                var name = "listView";
                if (tableLayoutPanel1.Controls.ContainsKey(name))
                {
                    var control = tableLayoutPanel1.Controls.Find(name, false);
                    ListView listView = (ListView)control[0];

                    Invoke(new Action(() =>
                    {
                        listView.Items.Clear();
                    }));
                    
                    using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                    {
                        Connect.Open();
                        SQLiteCommand Command = new SQLiteCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM users" + cLine
                        };
                        SQLiteDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                        {
                            if (getUser.CategoryForUser(sqlReader["id"].ToString(), category) || selectedCategory == 0)
                            {
                                Invoke(new Action(() =>
                                {
                                    ListViewItem item = new ListViewItem();

                                    item.Name = sqlReader["id"].ToString();
                                    item.Text = (listView.Items.Count + 1).ToString();
                                    item.SubItems.Add(getUser.GetNameUser(sqlReader["id"].ToString()));
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");

                                    listView.Items.Add(item);
                                }));
                            }

                        }
                        Connect.Close();
                    }

                    using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                    {
                        Connect.Open();
                        SQLiteCommand Command = new SQLiteCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM users" + cLine
                        };
                        SQLiteDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                        {
                            if (getUser.CategoryForUser(sqlReader["id"].ToString(), category) || selectedCategory == 0)
                            {
                                GetShiftsFromBase getShifts = new GetShiftsFromBase(dataBase, sqlReader["id"].ToString());

                                List<ShiftsDetails> shiftsDetails = (List<ShiftsDetails>)getShifts.LoadShiftsFromBase(date, category).Item2;

                                fullCountShifts += shiftsDetails[shiftsDetails.Count - 1].countShifts;
                                fullTimeShifts += shiftsDetails[shiftsDetails.Count - 1].allTimeShift;
                                fullCountOrders += shiftsDetails[shiftsDetails.Count - 1].countOrdersShift;
                                fullCountMakeready += shiftsDetails[shiftsDetails.Count - 1].countMakereadyShift;
                                fullAmountOrders += shiftsDetails[shiftsDetails.Count - 1].amountAllOrdersShift;
                                fullTimeWorkingOut += shiftsDetails[shiftsDetails.Count - 1].allTimeWorkingOutShift;
                                fullPercentWorkingOut += shiftsDetails[shiftsDetails.Count - 1].percentWorkingOutShift;

                                if (calculateNullShiftsFromUser)
                                    countActiveUser++;
                                else
                                    if (shiftsDetails[shiftsDetails.Count - 1].countShifts != 0)
                                    countActiveUser++;

                                Invoke(new Action(() =>
                                {
                                    int index = listView.Items.IndexOfKey(sqlReader["id"].ToString());

                                    ListViewItem item = listView.Items[index];
                                    if (item != null)
                                    {
                                        item.SubItems[2].Text = shiftsDetails[shiftsDetails.Count - 1].countShifts.ToString();
                                        item.SubItems[3].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].allTimeShift);
                                        item.SubItems[4].Text = shiftsDetails[shiftsDetails.Count - 1].countOrdersShift.ToString() + "/" + shiftsDetails[shiftsDetails.Count - 1].countMakereadyShift.ToString();
                                        item.SubItems[5].Text = shiftsDetails[shiftsDetails.Count - 1].amountAllOrdersShift.ToString("N0");
                                        item.SubItems[6].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].allTimeWorkingOutShift);
                                        item.SubItems[7].Text = shiftsDetails[shiftsDetails.Count - 1].percentWorkingOutShift.ToString("N1") + "%";
                                    }
                                }));

                                Invoke(new Action(() =>
                                {
                                    label1.Text = fullCountShifts.ToString("N0");
                                    label2.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(fullTimeShifts);
                                    label3.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(fullTimeWorkingOut);

                                    label4.Text = fullCountOrders.ToString() + "/" + fullCountMakeready.ToString();
                                    label5.Text = fullAmountOrders.ToString("N0");
                                    label6.Text = (fullPercentWorkingOut / countActiveUser).ToString("N1") + "%";
                                }));
                            }

                        }

                        Connect.Close();
                    }

                }
                thJob = false;
                break;
            }

            Invoke(new Action(() =>
            {
                comboBoxYear.Enabled = true;
                comboBoxMount.Enabled = true;
                comboBoxCategory.Enabled = true;
            }));
        }

        private void StartLoadingShifts()
        {
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            ComboBox comboBoxYear = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxYear");
            ComboBox comboBoxMount = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMount");
            ComboBox comboBoxUser = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxUser");

            if ((comboBoxYear.SelectedIndex != -1 && comboBoxMount.SelectedIndex != -1) && comboBoxUser.SelectedIndex != -1)
            {
                //ClearAll();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBoxYear.Text) - 1).AddMonths(comboBoxMount.SelectedIndex);

                String userId;
                userId = getUser.GetIDUserFromName(comboBoxUser.Text);

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;

                Task task = new Task(() => LoadShiftsFromBase(token, date, userId));

                if (thJob == true)
                {
                    cancelTokenSource.Cancel();
                }
                else
                {
                    //thJob = true;
                    task.Start();
                }
            }
        }

        private void LoadShiftsFromBase(CancellationToken token, DateTime date, String nameOfExecutor)
        {
            GetShiftsFromBase getShifts = new GetShiftsFromBase(dataBase, nameOfExecutor);
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();

            ComboBox comboBoxYear = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxYear");
            ComboBox comboBoxMount = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMount");
            ComboBox comboBoxUser = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxUser");

            Label label1 = (Label)ControlFromKey("tableLayoutPanelControl", "label1");
            Label label2 = (Label)ControlFromKey("tableLayoutPanelControl", "label2");
            Label label3 = (Label)ControlFromKey("tableLayoutPanelControl", "label3");

            Label label4 = (Label)ControlFromKey("tableLayoutPanelControl", "label4");
            Label label5 = (Label)ControlFromKey("tableLayoutPanelControl", "label5");
            Label label6 = (Label)ControlFromKey("tableLayoutPanelControl", "label6");

            Invoke(new Action(() =>
            {
                comboBoxYear.Enabled = false;
                comboBoxMount.Enabled = false;
                comboBoxUser.Enabled = false;
            }));

            while (!token.IsCancellationRequested)
            {
                thJob = true;

                var name = "listView";
                if (tableLayoutPanel1.Controls.ContainsKey(name))
                {
                    var control = tableLayoutPanel1.Controls.Find(name, false);
                    ListView listView = (ListView)control[0];

                    Invoke(new Action(() =>
                    {
                        listView.Items.Clear();
                    }));

                    List<Shifts> currentShift = (List<Shifts>)getShifts.LoadShiftsFromBase(date, "").Item1;

                    for (int i = 0; i < currentShift.Count; i++)
                    {
                        ListViewItem item = new ListViewItem();

                        item.Name = currentShift[i].startShift;
                        item.Text = (i + 1).ToString();
                        item.SubItems.Add(currentShift[i].dateShift);
                        item.SubItems.Add(currentShift[i].machinesShift);
                        item.SubItems.Add(currentShift[i].workingTimeShift);
                        item.SubItems.Add(currentShift[i].countOrdersShift.ToString());
                        item.SubItems.Add(currentShift[i].amountOrdersShift.ToString("N0"));
                        item.SubItems.Add(dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift[i].workingOutShift));
                        item.SubItems.Add(getPercent.PercentString(currentShift[i].workingOutShift));

                        Invoke(new Action(() => listView.Items.Add(item)));
                    }

                    List<ShiftsDetails> shiftsDetails = (List<ShiftsDetails>)getShifts.LoadShiftsFromBase(date, "").Item2;

                    Invoke(new Action(() =>
                    {
                        label1.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].countShifts * 680);
                        label2.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].allTimeShift);
                        label3.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails[shiftsDetails.Count - 1].allTimeWorkingOutShift);

                        label4.Text = shiftsDetails[shiftsDetails.Count - 1].countOrdersShift.ToString() + "/" + shiftsDetails[shiftsDetails.Count - 1].countMakereadyShift.ToString();
                        label5.Text = shiftsDetails[shiftsDetails.Count - 1].amountAllOrdersShift.ToString("N0");
                        label6.Text = shiftsDetails[shiftsDetails.Count - 1].percentWorkingOutShift.ToString("N1") + "%";
                    }));

                    Invoke(new Action(() =>
                    {
                        comboBoxYear.Enabled = true;
                        comboBoxMount.Enabled = true;
                        comboBoxUser.Enabled = true;
                    }));
                }
                thJob = false;
                break;
            }
        }

        private void StartLoadingStatisticMachines()
        {
            /*
            var name = "comboBoxYear";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                ListView listView = (ListView)control[0];
            }
            */

            ComboBox comboBoxYear = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxYear");
            ComboBox comboBoxMount = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMount");
            ComboBox comboBoxCategory = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxCategory");

            if (comboBoxYear.SelectedIndex != -1 && comboBoxMount.SelectedIndex != -1 && comboBoxCategory.SelectedIndex != -1)
            {
                //ClearAll();

                DateTime date;
                date = DateTime.MinValue.AddYears(Convert.ToInt32(comboBoxYear.Text) - 1).AddMonths(comboBoxMount.SelectedIndex);

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;

                Task task = new Task(() => LoadMachinesFromBase(token, date));

                if (thJob == true)
                {
                    cancelTokenSource.Cancel();
                }
                else
                {
                    //thJob = true;
                    task.Start();
                }
            }
        }

        private void LoadMachinesFromBase(CancellationToken token, DateTime date)
        {
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);
            ValueCategory categoryValue = new ValueCategory(dataBase);
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            ComboBox comboBoxYear = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxYear");
            ComboBox comboBoxMount = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMount");
            ComboBox comboBoxCategory = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxCategory");

            Label label1 = (Label)ControlFromKey("tableLayoutPanelControl", "label1");
            Label label2 = (Label)ControlFromKey("tableLayoutPanelControl", "label2");

            Label label3 = (Label)ControlFromKey("tableLayoutPanelControl", "label3");
            Label label4 = (Label)ControlFromKey("tableLayoutPanelControl", "label4");

            int selectedCategory = 0;
            String category = "Все";

            Invoke(new Action(() =>
            {
                comboBoxYear.Enabled = false;
                comboBoxMount.Enabled = false;
                comboBoxCategory.Enabled = false;

                selectedCategory = comboBoxCategory.SelectedIndex;
                category = categoryValue.GetCategoryFromName(comboBoxCategory.Text);

                //loadAll = checkBoxAllLoad.Checked; //Сделать чекбокс
            }));

            while (!token.IsCancellationRequested)
            {
                thJob = true;

                int fullCountOrdersMonth = 0;;
                int fullAmountOrdersMonth = 0;
                int fullCountOrdersYear = 0; ;
                int fullAmountOrdersYear = 0;

                var name = "listView";
                if (tableLayoutPanel1.Controls.ContainsKey(name))
                {
                    var control = tableLayoutPanel1.Controls.Find(name, false);
                    ListView listView = (ListView)control[0];

                    Invoke(new Action(() =>
                    {
                        listView.Items.Clear();
                    }));
                    
                    using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                    {
                        Connect.Open();
                        SQLiteCommand Command = new SQLiteCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM machines"
                        };
                        SQLiteDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                        {
                            if (getInfo.GetCategoryMachine(sqlReader["id"].ToString()) == category || selectedCategory == 0)
                            {
                                Invoke(new Action(() =>
                                {
                                    ListViewItem item = new ListViewItem();

                                    item.Name = sqlReader["id"].ToString();
                                    item.Text = (listView.Items.Count + 1).ToString();
                                    item.SubItems.Add(categoryValue.GetCategoryName(getInfo.GetCategoryMachine(sqlReader["id"].ToString())));
                                    item.SubItems.Add(getInfo.GetMachineName(sqlReader["id"].ToString()));
                                    item.SubItems.Add("");
                                    item.SubItems.Add("");

                                    listView.Items.Add(item);
                                }));
                            }
                        }
                        Connect.Close();
                    }

                    GetOrdersFromBase getOrder = new GetOrdersFromBase(dataBase);

                    int countMonth = 0;
                    int amountMonth = 0;
                    int countYear = 0;
                    int amountYear = 0;

                    using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                    {
                        Connect.Open();
                        SQLiteCommand Command = new SQLiteCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM machines"
                        };
                        SQLiteDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                        {
                            if (getInfo.GetCategoryMachine(sqlReader["id"].ToString()) == category || selectedCategory == 0)
                            {
                                List<int> orderCountAmountMonth = new List<int>((List<int>)getOrder.GetOrdersFromMachineForTheMonth(date, sqlReader["id"].ToString()));
                                List<int> orderCountAmountYear = new List<int>((List<int>)getOrder.GetOrdersFromMachineForTheYear(date, sqlReader["id"].ToString()));

                                countMonth = orderCountAmountMonth[0];
                                amountMonth = orderCountAmountMonth[1];

                                countYear = orderCountAmountYear[0];
                                amountYear = orderCountAmountYear[1];

                                fullCountOrdersMonth += countMonth;
                                fullAmountOrdersMonth += amountMonth;

                                fullCountOrdersYear += countYear;
                                fullAmountOrdersYear += amountYear;

                                Invoke(new Action(() =>
                                {
                                    int index = listView.Items.IndexOfKey(sqlReader["id"].ToString());

                                    ListViewItem item = listView.Items[index];
                                    if (item != null)
                                    {
                                        item.SubItems[3].Text = countMonth.ToString("N0") + " (" + countYear.ToString("N0") + ")";
                                        item.SubItems[4].Text = amountMonth.ToString("N0") + " (" + amountYear.ToString("N0") + ")";
                                    }
                                }));

                                Invoke(new Action(() =>
                                {
                                    label1.Text = fullCountOrdersMonth.ToString("N0") + " (" + fullCountOrdersYear.ToString("N0") + ")";
                                    label2.Text = fullAmountOrdersMonth.ToString("N0") + " (" + fullAmountOrdersYear.ToString("N0") + ")";
                                    /*label3.Text = fullCountOrdersYear.ToString("N0");
                                    label4.Text = fullAmountOrdersYear.ToString("N0");*/
                                }));
                            }

                        }

                        Connect.Close();
                    }

                }
                thJob = false;
                break;
            }

            Invoke(new Action(() =>
            {
                comboBoxYear.Enabled = true;
                comboBoxMount.Enabled = true;
                comboBoxCategory.Enabled = true;
            }));
        }

        private void StartLoadingAllOrders()
        {
            if (allOrderTHRead)
            {
                StartLoadingAllOrdersthRead();
            }
            else
            {
                LoadingAllOrders();
            }
        }
        private void LoadingAllOrders()
        {
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            DateTimePicker dateTime = (DateTimePicker)ControlFromKey("tableLayoutPanelControl", "dateTime");
            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");
            TextBox textBoxFilter = (TextBox)ControlFromKey("tableLayoutPanelControl", "textBoxFilter");

            if (comboBoxMachine.SelectedIndex != -1)
            {
                DateTime date;
                date = dateTime.Value;

                String filterKey;
                filterKey = textBoxFilter.Text;

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;

                LoadAllOrdersFromBase(token, date, filterKey);
            }
        }

        private void StartLoadingAllOrdersthRead()
        {
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            DateTimePicker dateTime = (DateTimePicker)ControlFromKey("tableLayoutPanelControl", "dateTime");
            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");
            TextBox textBoxFilter = (TextBox)ControlFromKey("tableLayoutPanelControl", "textBoxFilter");

            if (comboBoxMachine.SelectedIndex != -1)
            {
                //ClearAll();
                
                DateTime date;
                date = dateTime.Value;
                
                String filterKey;
                filterKey = textBoxFilter.Text;

                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                CancellationToken token = cancelTokenSource.Token;
                
                Task task = new Task(() => LoadAllOrdersFromBase(token, date, filterKey));

                if (thJob == true)
                {
                    cancelTokenSource.Cancel();
                    task.Start();
                    Text += thJob.ToString();
                }
                else
                {
                    task.Start();
                }
            }
        }

        private void LoadAllOrdersFromBase(CancellationToken token, DateTime dateTime, String filter)
        {
            GetValueFromOrdersBase ordersBase = new GetValueFromOrdersBase(dataBase);
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            Invoke(new Action(() =>
            {
                comboBoxMachine.Enabled = false;
            }));

            while (!token.IsCancellationRequested)
            {
                thJob = true;

                var name = "listView";
                if (tableLayoutPanel1.Controls.ContainsKey(name))
                {
                    var control = tableLayoutPanel1.Controls.Find(name, false);
                    ListView listView = (ListView)control[0];

                    Invoke(new Action(() =>
                    {
                        listView.Items.Clear();
                    }));
                    

                    ordersNumbers.Clear();
                    //ordersNumbers.Add(new Order("", ""));

                    int index = 0;

                    using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                    {
                        String commandLine;
                        commandLine = "strftime('%Y-%m-%d 00:00:00', date(substr(orderAddedDate, 7, 4) || '-' || substr(orderAddedDate, 4, 2) || '-' || substr(orderAddedDate, 1, 2))) >= '";
                        commandLine += dateTime.ToString("yyyy-MM-dd 00:00:00") + "'";

                        Connect.Open();
                        SQLiteCommand Command = new SQLiteCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM orders WHERE " + commandLine + " AND machine = '" + getInfo.GetMachineFromName(comboBoxMachine.Text) + "'"
                        };
                        SQLiteDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read() && !token.IsCancellationRequested) // считываем и вносим в комбобокс список заголовков
                        {
                            if (token.IsCancellationRequested)
                            {
                                listView.Items.Clear();
                                Invoke(new Action(() =>
                                {
                                    listView.Items.Clear();
                                }));
                                break;
                            }
                                
                            if (sqlReader["numberOfOrder"].ToString().Contains(filter))
                            {
                                GetCountOfDone orderCalc = new GetCountOfDone(dataBase, "", sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), "");
                                GetLeadTime leadTimeFirst = new GetLeadTime(dataBase, "", sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["machine"].ToString(), "0");
                                GetLeadTime leadTimeLast = new GetLeadTime(dataBase, "", sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["machine"].ToString(), sqlReader["counterRepeat"].ToString());

                                ordersNumbers.Add(new OrderNM(sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));

                                String modification = "";
                                if (sqlReader["modification"].ToString() != "")
                                    modification = " (" + sqlReader["modification"].ToString() + ")";

                                String orderStart = "";
                                if (leadTimeFirst.GetFirstValue("timeMakereadyStart").ToString() != "")
                                    orderStart = leadTimeFirst.GetFirstValue("timeMakereadyStart").ToString();
                                else
                                    orderStart = leadTimeFirst.GetFirstValue("timeToWorkStart").ToString();

                                ListViewItem item = new ListViewItem();

                                item.Name = sqlReader["numberOfOrder"].ToString();
                                item.Text = (index + 1).ToString();
                                item.SubItems.Add(sqlReader["numberOfOrder"].ToString() + modification);
                                item.SubItems.Add(sqlReader["nameOfOrder"].ToString());
                                item.SubItems.Add(timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeMakeready"])));
                                item.SubItems.Add(timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeToWork"])));
                                item.SubItems.Add(Convert.ToInt32(sqlReader["amountOfOrder"]).ToString("N0"));
                                item.SubItems.Add(orderStart);
                                item.SubItems.Add(leadTimeLast.GetLastValue("timeToWorkStop").ToString());
                                //item.SubItems.Add(orderCalc.OrderCalculate(true, true).ToString("N0"));
                                item.SubItems.Add(orderCalc.OrderFullCalculate().ToString("N0"));
                                item.SubItems.Add(ordersBase.GetOrderStatusName(getInfo.GetMachineFromName(comboBoxMachine.Text), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));

                                Invoke(new Action(() => listView.Items.Add(item)));

                                index++;
                            }

                        }

                        Connect.Close();
                    }
                    Invoke(new Action(() =>
                    {
                        comboBoxMachine.Enabled = true;
                    }));
                }
                thJob = false;

                break;
            }
            
        }

        private void StartLoadNormFromBase()
        {
            TextBox textBoxFilter = (TextBox)ControlFromKey("tableLayoutPanelControl", "textBoxFilter");

            String filterKey;
            filterKey = textBoxFilter.Text;

            LoadNormFromBase(filterKey);

        }

        private void LoadNormFromBase(String filter)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            var name = "listView";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                ListView listView = (ListView)control[0];

                listView.Items.Clear();

                using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
                {
                    int index = 0;

                    Connect.Open();
                    SQLiteCommand Command = new SQLiteCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT * FROM orders"
                    };
                    SQLiteDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                    {
                        if (sqlReader["orderStamp"].ToString().Contains(filter))
                        {
                            index++;

                            ListViewItem item = new ListViewItem();

                            item.Name = sqlReader["orderStamp"].ToString();
                            item.Text = (index + 1).ToString();
                            item.SubItems.Add(sqlReader["orderStamp"].ToString());
                            item.SubItems.Add(getInfo.GetMachineName(sqlReader["machine"].ToString()));
                            item.SubItems.Add(sqlReader["nameOfOrder"].ToString());
                            item.SubItems.Add(timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeMakeready"])));
                            item.SubItems.Add((60*Convert.ToInt32(sqlReader["amountOfOrder"])/Convert.ToInt32(sqlReader["timeToWork"])).ToString("N0"));

                            listView.Items.Add(item);

                        }

                    }

                    Connect.Close();
                }
            }
        }

        private void LoadUsersFromBase()
        {
            CancellationToken token;

            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            Label label1 = (Label)ControlFromKey("tableLayoutPanelControl", "label1");
            Label label2 = (Label)ControlFromKey("tableLayoutPanelControl", "label2");
            Label label3 = (Label)ControlFromKey("tableLayoutPanelControl", "label3");

            Label label4 = (Label)ControlFromKey("tableLayoutPanelControl", "label4");
            Label label5 = (Label)ControlFromKey("tableLayoutPanelControl", "label5");
            Label label6 = (Label)ControlFromKey("tableLayoutPanelControl", "label6");


            while (!token.IsCancellationRequested)
            {
                thJob = true;

                var name = "listView";
                if (tableLayoutPanel1.Controls.ContainsKey(name))
                {
                    var control = tableLayoutPanel1.Controls.Find(name, false);
                    ListView listView = (ListView)control[0];

                    Invoke(new Action(() =>
                    {
                        listView.Items.Clear();
                    }));

                    List<UserInfo> currentUserInfo = (List<UserInfo>)getUser.GetUserInfo();

                    StringArray categoryArray = new StringArray();
                    ValueCategory valueCategory = new ValueCategory(dataBase);

                    GetDateTimeOperations getTime = new GetDateTimeOperations();

                    for (int i = 0; i < currentUserInfo.Count; i++)
                    {
                        String[] catArr = categoryArray.ArrayFromTheString(currentUserInfo[i].categoryesMachine);
                        String catLine = "";
                        
                        
                        for (int j = 0; j < catArr.Length; j++)
                        {
                            catLine += valueCategory.GetCategoryName(catArr[j]) + "; ";
                        }

                        String wExperience = getTime.WorkExperience(currentUserInfo[i].dateOfDismissal, currentUserInfo[i].dateOfEmployment);
                        String userAge = getTime.AgeUser(DateTime.Now.ToString("dd.MM.yyyy"), currentUserInfo[i].dateOfBirth);

                        ListViewItem item = new ListViewItem();

                        item.Name = currentUserInfo[i].userID.ToString();
                        item.Text = (i + 1).ToString();
                        item.SubItems.Add(currentUserInfo[i].userName);
                        item.SubItems.Add(currentUserInfo[i].surname + " " + currentUserInfo[i].name + " " + currentUserInfo[i].patronymic);
                        item.SubItems.Add(catLine);
                        item.SubItems.Add(currentUserInfo[i].dateOfEmployment);
                        item.SubItems.Add(currentUserInfo[i].dateOfDismissal);
                        item.SubItems.Add(wExperience);
                        item.SubItems.Add(currentUserInfo[i].dateOfBirth);
                        item.SubItems.Add(userAge);
                        item.SubItems.Add(currentUserInfo[i].note);
                        Invoke(new Action(() => listView.Items.Add(item)));
                    }
                }
                thJob = false;
                break;
            }
        }

        private void LoadMachinesAndCategoryesFromBase()
        {
            CancellationToken token;

            ValueCategory getCategoryes = new ValueCategory(dataBase);
            GetValueFromInfoBase getMachines = new GetValueFromInfoBase(dataBase);

            GetDateTimeOperations getTime = new GetDateTimeOperations();

            Label label1 = (Label)ControlFromKey("tableLayoutPanelControl", "label1");
            Label label2 = (Label)ControlFromKey("tableLayoutPanelControl", "label2");
            Label label3 = (Label)ControlFromKey("tableLayoutPanelControl", "label3");

            Label label4 = (Label)ControlFromKey("tableLayoutPanelControl", "label4");
            Label label5 = (Label)ControlFromKey("tableLayoutPanelControl", "label5");
            Label label6 = (Label)ControlFromKey("tableLayoutPanelControl", "label6");

            List<String> categoryes = new List<String>(getCategoryes.GetCategoryesList());

            while (!token.IsCancellationRequested)
            {
                thJob = true;

                var name = "listView";
                if (tableLayoutPanel1.Controls.ContainsKey(name))
                {
                    var control = tableLayoutPanel1.Controls.Find(name, false);
                    ListView listView = (ListView)control[0];

                    Invoke(new Action(() =>
                    {
                        listView.Items.Clear();
                    }));

                    for (int i = 0; i < categoryes.Count; i++)
                    {
                        List<String> machines = new List<String>(getMachines.GetMachinesList(getCategoryes.GetCategoryFromName(categoryes[i])));

                        ListViewGroup listViewGroup = new ListViewGroup(categoryes[i]);
                        listViewGroup.Name = getCategoryes.GetCategoryFromName(categoryes[i]);

                        Invoke(new Action(() => listView.Groups.Add(listViewGroup)));

                        if (machines.Count == 0)
                        {
                            ListViewItem itemMachine = new ListViewItem(listViewGroup);

                            itemMachine.Name = "empty";
                                itemMachine.Text = "";
                                itemMachine.SubItems.Add("<пустой участок>");

                                Invoke(new Action(() => listView.Items.Add(itemMachine)));
                        }
                        else
                        {
                            for (int j = 0; j < machines.Count; j++)
                            {
                                String timeStartWork = getMachines.GetMachineStartWork(machines[j]);
                                String timeWork = getTime.WorkExperience(DateTime.Now.ToString(), timeStartWork);

                                ListViewItem itemMachine = new ListViewItem(listViewGroup);

                                itemMachine.Name = machines[j];
                                itemMachine.Text = (j + 1).ToString();
                                itemMachine.SubItems.Add(getMachines.GetMachineName(machines[j]));
                                itemMachine.SubItems.Add(timeStartWork);
                                itemMachine.SubItems.Add(timeWork);
                                itemMachine.SubItems.Add(getMachines.GetMachineNote(machines[j]));

                                Invoke(new Action(() => listView.Items.Add(itemMachine)));
                            }
                        }
                    }
                }
                thJob = false;
                break;
            }
        }

        private void LoadPage(int page)
        {
            var name = "listView";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, true);
                
                ListView listView = (ListView)control[0];

                SaveHeaders(listView);
            }

            List<ColumnHeader> head;

            currentPage = page;

            switch (page)
            {
                case 1:
                    tableLayoutPanel1.RowStyles[1].Height = 0;
                    CreatActivityControl();
                    head = new List<ColumnHeader>(Headers(page));
                    CreateListView(head);
                    ContextMenuToLV();
                    MainLVInsertValue();
                    break;
                case 2:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateStatisticUsersControls();
                    head = new List<ColumnHeader>(Headers(page));
                    CreateListView(head);
                    StartLoadingStatisticUsers();
                    break;
                case 3:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateShiftsControls();
                    head = new List<ColumnHeader>(Headers(page));
                    CreateListView(head);
                    StartLoadingShifts();
                    break;
                case 4:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateStatisticMachinesControls();
                    head = new List<ColumnHeader>(Headers(page));
                    CreateListView(head);
                    StartLoadingStatisticMachines();
                    break;
                case 5:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateAllOrdersControls();
                    head = new List<ColumnHeader>(Headers(page));
                    CreateListView(head);
                    ContextMenuToLV();
                    StartLoadingAllOrders();
                    break;
                case 6:
                    tableLayoutPanel1.RowStyles[1].Height = 35;
                    CreateNormControls();
                    head = new List<ColumnHeader>(Headers(page));
                    CreateListView(head);
                    StartLoadNormFromBase();
                    break;
                case 7:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateUsersControls();
                    head = new List<ColumnHeader>(Headers(page));
                    CreateListView(head);
                    ContextMenuToLV();
                    LoadUsersFromBase();
                    break;
                case 8:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateMachineControls();
                    head = new List<ColumnHeader>(Headers(page));
                    CreateListView(head);
                    ContextMenuToLV();
                    LoadMachinesAndCategoryesFromBase();
                    break;
                case 9:
                    tableLayoutPanel1.RowStyles[1].Height = 0;
                    CreateSettingsControls();
                    break;
                default:
                    break; 
            }

        }

        private void UpdatePage(int page)
        {
            //MessageBox.Show(selectedYear.ToString() + ", " + selectedMonth.ToString());

            switch (page)
            {
                case 1:
                    MainLVInsertValue();
                    break;
                case 2:
                    StartLoadingStatisticUsers();
                    break;
                case 3:
                    StartLoadingShifts();
                    break;
                case 4:
                    StartLoadingStatisticMachines();
                    break;
                case 5:
                    StartLoadingAllOrders();
                    break;
                case 6:
                    StartLoadNormFromBase();
                    break;
                case 7:
                    LoadUsersFromBase();
                    break;
                case 8:
                    LoadMachinesAndCategoryesFromBase();
                    break;
                default:
                    break;
            }

        }

        private void comboBoxYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedYear = Convert.ToInt32(((ComboBox)sender).Text);

            UpdatePage(currentPage);           
        }

        private void comboBoxMount_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedMonth = ((ComboBox)sender).SelectedIndex + 1;

            UpdatePage(currentPage);
        }

        private void comboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedCategory = ((ComboBox)sender).Text;

            UpdatePage(currentPage);
        }


        private void comboBoxUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedUser = ((ComboBox)sender).Text;

            UpdatePage(currentPage);
        }

        private void comboBoxMachine_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetValueFromInfoBase getMachine = new GetValueFromInfoBase(dataBase);
            selectedMachine = getMachine.GetMachineFromName(((ComboBox)sender).Text);

            UpdatePage(currentPage);
        }

        private void dateTime_ValueChanged(object sender, EventArgs e)
        {
            GetValueFromInfoBase getMachine = new GetValueFromInfoBase(dataBase);
            selectedDateTime = ((DateTimePicker)sender).Value;

            UpdatePage(currentPage);
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            UpdatePage(currentPage);
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            INISettings ini = new INISettings();
            CheckBox checkBox = (CheckBox)sender;

            switch (currentPage)
            {
                case 1:
                    break;
                case 2:

                    break;
                case 3:
                    break;
                case 4:

                    break;
                case 5:
                    break;
                case 6:

                    break;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    ini.SetCheckDBLocalPath(checkBox.Checked);

                    EnabledPathControls(checkBox.Checked);
                    break;

                default:
                    break;
            }
            UpdatePage(currentPage);
        }


        private void ListViewDoubleClick(object sender, EventArgs e)
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            int selectegIndex = ((ListView)sender).SelectedIndices[0];
            String selectedName = ((ListView) sender).SelectedItems[0].Name.ToString();

            switch (currentPage)
            {
                case 1:
                    LoadShiftdetails(selectedName);
                    break;
                case 2:
                    selectedUser = getUser.GetNameUser(selectedName);
                    LoadPage(3);
                    break;
                case 3:
                    LoadShiftdetails(selectedName);
                    break;
                case 4:

                    break;
                case 5:
                    if (((ListView)sender).SelectedItems.Count != 0)
                        LoadSelectedOrder(true, getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[selectegIndex].numberOfOrder, ordersNumbers[selectegIndex].modificationOfOrder);
                    UpdatePage(currentPage);
                    break;
                case 6:
                    
                    break;
                case 7:
                    ShowEditUserForm(((ListView)sender).Items[((ListView)sender).SelectedIndices[0]].Name);
                    UpdatePage(currentPage);
                    break;

                case 8:
                    ShowEditMachineForm(selectedName);
                    UpdatePage(currentPage);
                    break;

                default:
                    break;
            }
            
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            String[] buttons = { "editButton", "editCategoryButton", "editMachineButton", "deleteButton", "deleteCategoryButton", "deleteMachineButton" };

            if (((ListView)sender).SelectedIndices.Count == 0)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    Button button = (Button)ControlFromKey("tableLayoutPanelControl", buttons[i]);
                    if (button != null)
                        button.Enabled = false;
                }
            }
            else
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    Button button = (Button)ControlFromKey("tableLayoutPanelControl", buttons[i]);
                    if (button != null)
                        button.Enabled = true;
                }
            }
        }

        private void myListView_GroupHeaderClick(object sender, int e)
        {
            //Show ContextMenuStrip here. Or just for example:
            //MessageBox.Show(((ListView)sender).Groups[e].Header);
            //MessageBox.Show(e.ToString());
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            Button addButton = (Button)ControlFromKey("tableLayoutPanelControl", "addCategoryButton");

            switch (currentPage)
            {
                case 1:
                    
                    break;
                case 2:

                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:
                    ShowFullOrdersForm(false, 0);
                    //StartLoadingAllOrders();
                    break;
                case 6:

                    break;
                case 7:
                    ShowAddUserForm();
                    //LoadUsersFromBase();
                    break;
                case 8:
                    if (((Button)sender).Name == "addCategoryButton")
                        ShowAddCategoryForm();
                    if (((Button)sender).Name == "addMachineButton")
                        ShowAddMachineForm();
                        //LoadUsersFromBase();
                    break;
                case 9:
                    ShowSelectDataBaseFile();
                    GetDatBasePath();


                    break;

                default:
                    break;
            }
            UpdatePage(currentPage);
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            timer1.Enabled = false;

            ListView listV = (ListView)ControlFromKey("tableLayoutPanel1", "listView");

            e.Cancel = listV.SelectedItems.Count == 0;
        }

        private void contextMenuStrip_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            timer1.Enabled = true;
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            ListView listV = (ListView)ControlFromKey("tableLayoutPanel1", "listView");

            String selectedName = listV.SelectedItems[0].Name.ToString();

            switch (currentPage)
            {
                case 1:
                    LoadShiftdetails(selectedName);
                    break;
                case 2:

                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:
                    LoadSelectedOrder(true, getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[listV.SelectedIndices[0]].numberOfOrder, ordersNumbers[listV.SelectedIndices[0]].modificationOfOrder);
                    break;

                default:
                    break;
            }
        }

        private void closeShift_Click(object sender, EventArgs e)
        {
            //Завершение смены с вводом всех времени завершения операций дляя каждой машины и количеством выполненной продукции
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
            GetValueFromUserBase getUser = new GetValueFromUserBase(dataBase);

            ListView listV = (ListView)ControlFromKey("tableLayoutPanel1", "listView");

            String startOfShift = listV.SelectedItems[0].Name.ToString();
            String userId = getUser.GetCurrentUserIDFromShiftStart(startOfShift);

            List<String> machines = (List<String>)getInfo.GetMachines(userId);

            for (int i = 0; i < machines.Count; i++)
            {
                if (Convert.ToBoolean(getInfo.GetActiveOrder(machines[i])) == true)
                {
                    FormAddCloseOrder form = new FormAddCloseOrder(dataBase, startOfShift, userId, machines[i]);
                    form.ShowDialog();
                }     
            }

            if (getInfo.GetMachinesForUserActive(userId) == true)
            {
                MessageBox.Show("Смена не была завершена т.к. не все операции были завершены.", "Завершение смены", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                SetUpdateUsersBase userBase = new SetUpdateUsersBase(dataBase);
                SetUpdateInfoBase infoBase = new SetUpdateInfoBase(dataBase);
                GetValueFromShiftsBase getShift = new GetValueFromShiftsBase(dataBase);

                getShift.CloseShift(startOfShift, DateTime.Now.ToString());
                infoBase.CompleteTheShift(userId);
                userBase.UpdateCurrentShiftStart(userId, "");

                MessageBox.Show("Смена завершена в " + DateTime.Now.ToString(), "Завершение смены", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UpdatePage(currentPage);
        }



        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView listV = (ListView)ControlFromKey("tableLayoutPanel1", "listView");

            int selectedIndex = listV.SelectedIndices[0];
            String selectedName = listV.Items[selectedIndex].Name;

            String nameItem = ((ToolStripMenuItem)sender).Name;

            switch (currentPage)
            {
                case 1:

                    break;
                case 2:

                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:
                    ShowFullOrdersForm(true, listV.SelectedIndices[0]);
                    break;
                case 6:
                    
                    break;
                case 7:
                    if (listV.SelectedIndices.Count > 0)
                        ShowEditUserForm(selectedName);
                    break;
                case 8:
                    if (nameItem == "editCategoryItem")
                        ShowEditCategoryForm(listV.Items[selectedIndex].Group.Name);
                    if (nameItem == "editMachineItem")
                        ShowEditMachineForm(selectedName);
                    //LoadUsersFromBase();
                    break;
                default:
                    break;
            }
            UpdatePage(currentPage);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView listV = (ListView)ControlFromKey("tableLayoutPanel1", "listView");

            int selectedIndex = listV.SelectedIndices[0];
            String selectedName = listV.Items[selectedIndex].Name;

            String nameItem = ((ToolStripMenuItem)sender).Name;

            switch (currentPage)
            {
                case 1:

                    break;
                case 2:

                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:
                    ShowFullOrdersForm(true, listV.SelectedIndices[0]);
                    break;
                case 6:

                    break;
                case 7:
                    if (listV.SelectedIndices.Count > 0)
                        DeleteUser(selectedName);
                    break;
                case 8:
                    if (nameItem == "editCategoryItem")
                        DeleteCategory(listV.Items[selectedIndex].Group.Name);
                    if (nameItem == "editMachineItem")
                        DeleteMachine(selectedName);
                    //LoadUsersFromBase();
                    break;
                default:
                    break;
            }
            UpdatePage(currentPage);
        }

        private void deactivateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            ListView listV = (ListView)ControlFromKey("tableLayoutPanel1", "listView");

            switch (currentPage)
            {
                case 1:

                    break;
                case 2:

                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:
                    DeactivateOrder(getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[listV.SelectedIndices[0]].numberOfOrder, ordersNumbers[listV.SelectedIndices[0]].modificationOfOrder, "4");
                    UpdatePage(currentPage);
                    break;

                default:
                    break;
            }
        }

        private void LoadShiftdetails(String timeStartShift)
        {
            FormOneShiftDetails form = new FormOneShiftDetails(adminMode, dataBase, timeStartShift);
            form.ShowDialog();
        }

        private void EnabledPathControls(bool eneble)
        {
            TextBox textBoxDBPath = null;
            Button addButtonDBPath = null;

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                TableLayoutPanel panel = (TableLayoutPanel)control[0];

                var subControl = panel.Controls.Find("tableLayoutPanelPath", false);
                var textBox = subControl[0].Controls.Find("textBoxDBPath", false);
                var button = subControl[0].Controls.Find("addButtonDBPath", false);

                textBoxDBPath = (TextBox)textBox[0];
                addButtonDBPath = (Button)button[0];
            }

            if (eneble)
            {
                textBoxDBPath.Enabled = false;
                addButtonDBPath.Enabled = false;
            }
            else
            {
                textBoxDBPath.Enabled = true;
                addButtonDBPath.Enabled = true;
            }

            GetDatBasePath();
        }

        private void GetDatBasePath()
        {
            INISettings ini = new INISettings();

            TextBox textBoxDBPath;

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                TableLayoutPanel panel = (TableLayoutPanel)control[0];

                var subControl = panel.Controls.Find("tableLayoutPanelPath", false);
                var textBox = subControl[0].Controls.Find("textBoxDBPath", false);

                textBoxDBPath = (TextBox)textBox[0];

                textBoxDBPath.Text = ini.DataBasePath();

                dataBase = ini.DataBasePath();
            }
        }

        private void FormAdmin_Load(object sender, EventArgs e)
        {
            selectedYear = DateTime.Now.Year;
            selectedMonth = DateTime.Now.Month;

            LoadPage(currentPage);
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (currentPage == 1)
                UpdatePage(currentPage);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadPage(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadPage(2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadPage(3);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoadPage(4);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadPage(5);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            LoadPage(6);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            LoadPage(7);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            LoadPage(8);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            LoadPage(9);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Application.Exit();
        }
    }
}
