using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormAdmin : Form
    {
        bool adminMode;

        public FormAdmin(bool aMode)
        {
            InitializeComponent();

            this.adminMode = aMode;
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
        int selectedMonthLengthNorm = 0;
        String selectedCategory = "Все";
        String selectedMachine = "";
        String selectedUser = "";
        DateTime selectedDateTime = DateTime.Now.AddMonths(-2);
        DateTime selectedDateTimeNorm = DateTime.Now;

        bool thJob = false;
        bool calculateNullShiftsFromUser = false;

        private void FormAdmin_Load(object sender, EventArgs e)
        {
            selectedYear = DateTime.Now.Year;
            selectedMonth = DateTime.Now.Month;

            LoadBaseInfo();
            LoadVariables();

            LoadPage(currentPage);
            timer1.Enabled = true;
        }

        private void FormAdmin_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Enabled = false;
            SaveVariables();
            Application.Exit();
        }

        private void LoadVariables()
        {
            INISettings ini = new INISettings();

            selectedMonthLengthNorm = Convert.ToInt32(ini.GetSelectedMonthLengthNorm());
            selectedCategory = ini.GetSelectedCategory();
            selectedMachine = ini.GetSelectedMachine();
            selectedDateTime = Convert.ToDateTime(ini.GetSelectedDateTime());
            selectedDateTimeNorm = Convert.ToDateTime(ini.GetSelectedDateTimeNorm());
        }

        private void LoadBaseInfo()
        {
            DBConnection connection = new DBConnection();

            connection.SetDBParameter();

            toolStripStatusLabel2.Text = Form1.BaseConnectionParameters.host;
            toolStripStatusLabel5.Text = Form1.BaseConnectionParameters.database;
        }

        private void SaveVariables()
        {
            INISettings ini = new INISettings();

            ini.SetSelectedMonthLengthNorm(selectedMonthLengthNorm.ToString());
            ini.SetSelectedCategory(selectedCategory);
            ini.SetSelectedMachine(selectedMachine);
            ini.SetSelectedDateTime(selectedDateTime.ToString("d"));
            ini.SetSelectedDateTimeNorm(selectedDateTimeNorm.ToString("d"));
        }

        private void EnabledButtons(bool enabled)
        {
            button1.Enabled = enabled;
            button2.Enabled = enabled;
            button3.Enabled = enabled;
            button4.Enabled = enabled;
            button5.Enabled = enabled;
            button6.Enabled = enabled;
            button7.Enabled = enabled;
            button8.Enabled = enabled;
            button9.Enabled = enabled;
            button10.Enabled = enabled;
        }

        private void SelectedButton(int page)
        {
            Button[] buttons = { button1, button2, button3, button4, button5, button6, button7, button8, button9, button10 };

            buttons[page - 1].BackColor = Color.FromKnownColor(KnownColor.Cornsilk);
            this.Text = "Администрирование - " + buttons[page - 1].Text;

            for (int i = 0; i < buttons.Length; i++)
            {
                if (i != page - 1)
                    buttons[i].BackColor = Color.FromKnownColor(KnownColor.Control);
            }
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

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT startShift FROM shifts"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

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
            ValueInfoBase getInfo = new ValueInfoBase();

            List<String> machine = new List<String>();

            using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT id FROM machines"
                };
                DbDataReader sqlReader = Command.ExecuteReader();

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
            FormFullListOrders form = new FormFullListOrders(detailsLoad, orderMachine, orderNumberm, orderModification);
            form.ShowDialog();
        }

        private void ShowFullOrdersForm(bool editOrder, int selectedIndex)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            FormAddNewOrder form;

            if (editOrder)
                form = new FormAddNewOrder(getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[selectedIndex].numberOfOrder, ordersNumbers[selectedIndex].modificationOfOrder);
            else
                form = new FormAddNewOrder(getInfo.GetMachineFromName(comboBoxMachine.Text));

            form.ShowDialog();
        }

        private void ShowAddUserForm()
        {
            FormAddEditUser form = new FormAddEditUser();

            form.ShowDialog();
        }

        private void ShowEditUserForm(String userID)
        {
            FormAddEditUser form = new FormAddEditUser(userID);

            form.ShowDialog();
        }

        

        private void DeleteUser(String id)
        {
            ValueUserBase userBase = new ValueUserBase();

            DialogResult result;
            result = MessageBox.Show("Вы действительно хотите удалить сотрудника " + userBase.GetNameUser(id) + "?", "Удаление сотрудника", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                userBase.DeleteUser(id);
            }
        }

        private void ShowAddCategoryForm()
        {
            FormAddEditCategory form = new FormAddEditCategory();

            form.ShowDialog();
        }

        private void ShowEditCategoryForm(String categoryID)
        {
            FormAddEditCategory form = new FormAddEditCategory(categoryID);

            form.ShowDialog();
        }

        private void DeleteCategory(String id)
        {
            ValueCategory getCategory = new ValueCategory();

            DialogResult result;
            result = MessageBox.Show("Вы действительно хотите удалить участок '" + getCategory.GetCategoryName(id) + "'?", "Удаление участка", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                getCategory.DeleteCategory(id);
            }
        }

        private void ShowAddMachineForm()
        {
            FormAddEditMachine form = new FormAddEditMachine();

            form.ShowDialog();
        }

        private void ShowEditMachineForm(String machineID)
        {
            FormAddEditMachine form = new FormAddEditMachine(machineID);

            form.ShowDialog();
        }

        private void DeleteMachine(String id)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            DialogResult result;
            result = MessageBox.Show("Вы действительно хотите удалить оборудование '" + infoBase.GetMachineName(id) + "'?", "Удаление оборудования", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                infoBase.DeleteMachine(id);
            }
        }

        private void DeactivateOrder(String machine, String orderNumber, String orderModification)
        {
            ValueOrdersBase orders = new ValueOrdersBase();

            DialogResult result;
            result = MessageBox.Show("Вы действительно хотите завершить заказ?", "Завершение заказа", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                orders.SetNewStatus(machine, orderNumber, orderModification, "4");
        }

        private void AbortOrder(String machine, String orderNumber, String orderModification)
        {
            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            ValueOrdersBase orders = new ValueOrdersBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueInfoBase infoBase = new ValueInfoBase();

            DialogResult result = DialogResult.No;

            String statusOrder = orders.GetOrderStatus(machine, orderNumber, orderModification);

            if (statusOrder != "0" && statusOrder != "4")
            {
                result = MessageBox.Show("Вы действительно хотите прервать заказ?", "Завершение заказа", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (result == DialogResult.Yes)
            {
                orders.SetNewStatus(machine, orderNumber, orderModification, "0");
                orders.IncrementCounterRepeat(machine, orderNumber, orderModification);
                infoBase.UpdateInfo(getInfo.GetMachineFromName(comboBoxMachine.Text), "", "", "", "", "", false);
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

            ToolStripMenuItem abortToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "abortToolStripMenuItem",
                Text = "Прервать "
            };

            ToolStripMenuItem reactivateToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "reactivateToolStripMenuItem",
                Text = "Возобновить"
            };

            ToolStripMenuItem deactivateToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "deactivateToolStripMenuItem",
                Text = "Завершить"
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

            ToolStripItem[] items = new ToolStripItem[] { };

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
                        abortToolStripMenuItem,
                        //reactivateToolStripMenuItem,
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
            abortToolStripMenuItem.Click += new EventHandler(abortToolStripMenuItem_Click);
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
            ValueCategory category = new ValueCategory();

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
        }

        private void CreateShiftsControls()
        {
            ValueUserBase getUser = new ValueUserBase();

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
            ValueCategory category = new ValueCategory();

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
            ValueInfoBase getMachine = new ValueInfoBase();

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
            addButton.ImageList = imageList1;
            addButton.ImageIndex = 2;
            addButton.TextImageRelation = TextImageRelation.ImageAboveText;
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
            ValueInfoBase getMachine = new ValueInfoBase();

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
            addButton.ImageList = imageList1;
            addButton.ImageIndex = 2;
            addButton.TextImageRelation = TextImageRelation.ImageAboveText;
            tableLayoutPanelControl.Controls.Add(addButton, 0, 0);
            addButton.Click += new System.EventHandler(addButton_Click);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "", ContentAlignment.MiddleRight), 1, 0);
        }

        private void CreateMachineControls()
        {
            ValueInfoBase getMachine = new ValueInfoBase();

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
            addCategoryButton.ImageList = imageList1;
            addCategoryButton.ImageIndex = 2;
            addCategoryButton.TextImageRelation = TextImageRelation.ImageAboveText;
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
            addMachineButton.ImageList = imageList1;
            addMachineButton.ImageIndex = 2;
            addMachineButton.TextImageRelation = TextImageRelation.ImageAboveText;
            tableLayoutPanelControl.Controls.Add(addMachineButton, 1, 0);
            addMachineButton.Click += new System.EventHandler(addButton_Click);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "", ContentAlignment.MiddleRight), 2, 0);
        }

        private void CreateNormControls()
        {
            ValueInfoBase getMachine = new ValueInfoBase();

            List<String> machine = new List<String>(LoadMachine());

            String[] m = { "1 месяц", "2 месяца", "3 месяца", "4 месяца", "5 месяцев", "6 месяцев", "7 месяцев", "8 месяцев", "9 месяцев", "10 месяцев", "11 месяцев", "12 месяцев" };

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

            tableLayoutPanelControl.RowCount = 3;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 1);

            DateTimePicker dateTime = new DateTimePicker();
            dateTime.Name = "dateTimeNorm";
            dateTime.Dock = DockStyle.Fill;
            dateTime.Location = new System.Drawing.Point(3, 3);
            dateTime.Size = new System.Drawing.Size(144, 21);
            dateTime.TabIndex = 0;
            dateTime.Value = selectedDateTimeNorm;
            dateTime.Visible = true;
            tableLayoutPanelControl.Controls.Add(dateTime, 0, 0);
            dateTime.ValueChanged += new System.EventHandler(dateTimeNorm_ValueChanged);

            ComboBox comboBoxLength = new ComboBox();
            comboBoxLength.Name = "comboBoxLength";
            comboBoxLength.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxLength.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxLength.FormattingEnabled = true;
            comboBoxLength.Location = new System.Drawing.Point(3, 3);
            comboBoxLength.Size = new System.Drawing.Size(144, 21);
            comboBoxLength.TabIndex = 1;
            comboBoxLength.Items.AddRange(m);
            comboBoxLength.SelectedIndex = selectedMonthLengthNorm;
            comboBoxLength.SelectedIndexChanged += new EventHandler(comboBoxLength_SelectedIndexChanged);

            tableLayoutPanelControl.Controls.Add(comboBoxLength, 0, 1);

            TextBox textBox = new TextBox();
            textBox.Name = "textBoxFilter";
            textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox.Location = new System.Drawing.Point(3, 3);
            textBox.Size = new System.Drawing.Size(144, 21);
            textBox.TabIndex = 2;
            textBox.Text = "";
            textBox.Visible = true;
            tableLayoutPanelControl.Controls.Add(textBox, 0, 2);
            textBox.TextChanged += new System.EventHandler(textBoxFilter_TextChanged);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label001", "- начало периода", ContentAlignment.MiddleLeft), 1, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label002", "- продолжительность", ContentAlignment.MiddleLeft), 1, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label003", "- фильтр", ContentAlignment.MiddleLeft), 1, 2);
        }

        private void CreateSettingsControls()
        {
            ValueOrdersBase getOrders = new ValueOrdersBase();
            ValueUserBase getUser = new ValueUserBase();
            ValueInfoBase getMachine = new ValueInfoBase();
            INISettings ini = new INISettings();

            List<String> machine = new List<String>(LoadMachine());

            /*String path = ini.Path().Replace(@"\\", @"\");*/

            String[] name = { "listView", "tableLayoutPanelControl" };

            for (int i = 0; i < name.Length; i++)
            {
                if (tableLayoutPanel1.Controls.ContainsKey(name[i]))
                {
                    var control = tableLayoutPanel1.Controls.Find(name[i], true);
                    tableLayoutPanel1.Controls.Remove(control[0]);
                }
            }

            IniFile iniBase = new IniFile(Form1.connectionFile);

            List<String> conections = new List<String>();
            String[] sections = iniBase.GetAllSections();

            for (int i = 0; i < sections.Length; i++)
            {
                if (sections[i].StartsWith("mysql"))
                    conections.Add(sections[i].Substring(5));
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

            TableLayoutPanel tableLayoutPanelBase = new TableLayoutPanel();
            tableLayoutPanelBase.Dock = DockStyle.Fill;
            tableLayoutPanelBase.Name = "tableLayoutPanelBase";
            tableLayoutPanelBase.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            tableLayoutPanelBase.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            tableLayoutPanelBase.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tableLayoutPanelBase.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tableLayoutPanelBase.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            tableLayoutPanelBase.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            tableLayoutPanelBase.RowCount = 1;
            tableLayoutPanelBase.Visible = true;

            ComboBox comboBoxBase = new ComboBox();
            comboBoxBase.Name = "comboBoxBase";
            comboBoxBase.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxBase.FormattingEnabled = true;
            comboBoxBase.Location = new System.Drawing.Point(3, 3);
            comboBoxBase.Size = new System.Drawing.Size(144, 21);
            comboBoxBase.TabIndex = 1;
            comboBoxBase.Items.AddRange(conections.ToArray());
            //comboBoxBase.SelectedIndexChanged += new EventHandler(comboBoxBase_SelectedIndexChanged);

            String currentConnection = iniBase.ReadString("selected", "general").Substring(5);

            if (comboBoxBase.Items.Contains(currentConnection))
                comboBoxBase.SelectedItem = currentConnection;
            else
                comboBoxBase.SelectedIndex = 0;

            tableLayoutPanelBase.Controls.Add(comboBoxBase, 0, 0);

            Button acceptButton = new Button();
            acceptButton.Name = "acceptButton";
            acceptButton.Dock = DockStyle.Fill;
            acceptButton.Location = new System.Drawing.Point(0, 0);
            acceptButton.Size = new System.Drawing.Size(144, 21);
            acceptButton.TabIndex = 0;
            acceptButton.Text = "Применить";
            acceptButton.Visible = true;
            tableLayoutPanelBase.Controls.Add(acceptButton, 1, 0);
            acceptButton.Click += new System.EventHandler(acceptBaseButton_Click);

            Button editButton = new Button();
            editButton.Name = "editButton";
            editButton.Dock = DockStyle.Fill;
            editButton.Location = new System.Drawing.Point(0, 0);
            editButton.Size = new System.Drawing.Size(144, 21);
            editButton.TabIndex = 0;
            editButton.Text = "Редактировать";
            editButton.Visible = true;
            tableLayoutPanelBase.Controls.Add(editButton, 2, 0);
            editButton.Click += new System.EventHandler(editBaseButton_Click);

            tableLayoutPanelBase.Controls.Add(CreateLabel("labelBase", "", ContentAlignment.MiddleLeft), 3, 0);

            /*TextBox textBox = new TextBox();
            textBox.Name = "textBoxDBPath";
            textBox.Dock = DockStyle.Fill;
            textBox.Location = new Point(3, 3);
            textBox.Size = new Size(144, 21);
            textBox.TabIndex = 2;
            textBox.Text = "";
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

            

            EnabledPathControls(checkBox.Checked);
            */


            //Первая колонка
            tableLayoutPanelControl.Controls.Add(tableLayoutPanelBase, 0, 0);

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
            ValueShiftsBase getShifts = new ValueShiftsBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueOrdersBase getOrder = new ValueOrdersBase();
            GetOrdersFromBase ordersFromBase = new GetOrdersFromBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            ValueUserBase userBase = new ValueUserBase();

            List<String> users = new List<String>(getShifts.GetActiveUser());

            var name = "listView";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                ListView listView = (ListView)control[0];

                //EnabledButtons(false);

                listView.Items.Clear();

                //this.Text += " - " + users.Count;

                for (int i = 0; i < users.Count; i++)
                {
                    List<String> machines = (List<String>)getInfo.GetMachines(users[i]);

                    for (int j = 0; j < machines.Count; j++)
                    {
                        GetLeadTime leadTimeCurr = new GetLeadTime(userBase.GetCurrentShiftStart(users[i]), getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j]), machines[j], getOrder.GetCounterRepeat(machines[j], getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j])));

                        List<Order> ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(userBase.GetCurrentShiftStart(users[i]), "");

                        String user = "";
                        String currentTime = "";
                        String currentShiftStart = "";
                        String order = "";

                        if (getInfo.GetCurrentOrderNumber(machines[j]) != "")
                            order = getInfo.GetCurrentOrderNumber(machines[j]) + ", " + getOrder.GetOrderName(machines[j], getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j]));

                        if (j == 0)
                        {
                            user = users[i];
                            currentShiftStart = userBase.GetCurrentShiftStart(users[i]);
                        }
                        else
                        {
                            user = "";
                            currentShiftStart = "";
                        }

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
                        item.SubItems.Add(currentShiftStart);
                        item.SubItems.Add(getInfo.GetMachineName(machines[j]));
                        item.SubItems.Add(order);
                        item.SubItems.Add(getOrder.GetOrderStatusName(machines[j], getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j])));
                        item.SubItems.Add(currentTime);
                        item.SubItems.Add(timeDiff);

                        listView.Items.Add(item);
                    }


                }

                //EnabledButtons(true);
            }
        }

        private Object ControlFromKey(String panel, String name)
        {
            /*Control[] controls = Controls.Find(name, true);

            //MessageBox.Show(control.Length.ToString());

            return controls[0];*/



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
            ValueUserBase getUser = new ValueUserBase();
            ValueCategory categoryValue = new ValueCategory();

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
                EnabledButtons(false);

                comboBoxYear.Enabled = false;
                comboBoxMount.Enabled = false;
                comboBoxCategory.Enabled = false;

                category = categoryValue.GetCategoryFromName(comboBoxCategory.Text);
                selectedCategory = comboBoxCategory.SelectedIndex;

                //loadAll = checkBoxAllLoad.Checked; //Сделать чекбокс
            }));

            Invoke(new Action(() =>
            {
                label1.Text = "";
                label2.Text = "";
                label3.Text = "";

                label4.Text = "";
                label5.Text = "";
                label6.Text = "";
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

                    using (MySqlConnection Connect = DBConnection.GetDBConnection())
                    {
                        Connect.Open();
                        MySqlCommand Command = new MySqlCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM users" + cLine
                        };
                        DbDataReader sqlReader = Command.ExecuteReader();

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

                    using (MySqlConnection Connect = DBConnection.GetDBConnection())
                    {
                        Connect.Open();
                        MySqlCommand Command = new MySqlCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM users" + cLine
                        };
                        DbDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                        {
                            if (getUser.CategoryForUser(sqlReader["id"].ToString(), category) || selectedCategory == 0)
                            {
                                GetShiftsFromBase getShifts = new GetShiftsFromBase(sqlReader["id"].ToString());

                                ShiftsDetails shiftsDetails = (ShiftsDetails)getShifts.LoadCurrentDateShiftsDetails(date, category, token);

                                fullCountShifts += shiftsDetails.countShifts;
                                fullTimeShifts += shiftsDetails.allTimeShift;
                                fullCountOrders += shiftsDetails.countOrdersShift;
                                fullCountMakeready += shiftsDetails.countMakereadyShift;
                                fullAmountOrders += shiftsDetails.amountAllOrdersShift;
                                fullTimeWorkingOut += shiftsDetails.allTimeWorkingOutShift;
                                fullPercentWorkingOut += shiftsDetails.percentWorkingOutShift;

                                if (calculateNullShiftsFromUser)
                                    countActiveUser++;
                                else
                                    if (shiftsDetails.countShifts != 0)
                                    countActiveUser++;

                                Invoke(new Action(() =>
                                {
                                    int index = listView.Items.IndexOfKey(sqlReader["id"].ToString());

                                    ListViewItem item = listView.Items[index];
                                    if (item != null)
                                    {
                                        item.SubItems[2].Text = shiftsDetails.countShifts.ToString();
                                        item.SubItems[3].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails.allTimeShift);
                                        item.SubItems[4].Text = shiftsDetails.countOrdersShift.ToString() + "/" + shiftsDetails.countMakereadyShift.ToString();
                                        item.SubItems[5].Text = shiftsDetails.amountAllOrdersShift.ToString("N0");
                                        item.SubItems[6].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetails.allTimeWorkingOutShift);
                                        item.SubItems[7].Text = shiftsDetails.percentWorkingOutShift.ToString("N1") + "%";
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
                EnabledButtons(true);

                comboBoxYear.Enabled = true;
                comboBoxMount.Enabled = true;
                comboBoxCategory.Enabled = true;
            }));
        }

        private void StartLoadingShifts()
        {
            ValueUserBase getUser = new ValueUserBase();

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
            GetShiftsFromBase getShifts = new GetShiftsFromBase(nameOfExecutor);
            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            GetNumberShiftFromTimeStart getNumberShift = new GetNumberShiftFromTimeStart();

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
                EnabledButtons(false);

                comboBoxYear.Enabled = false;
                comboBoxMount.Enabled = false;
                comboBoxUser.Enabled = false;
            }));

            Invoke(new Action(() =>
            {
                label1.Text = "";
                label2.Text = "";
                label3.Text = "";

                label4.Text = "";
                label5.Text = "";
                label6.Text = "";
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

                    //List<Shifts> currentShift = (List<Shifts>)getShifts.LoadShiftsFromBase(date, "").Item1;

                    List<String> shifts = (List<String>)getShifts.LoadShiftsList(date);

                    for (int i = 0; i < shifts.Count; i++)
                    {
                        String dateStr;

                        dateStr = Convert.ToDateTime(shifts[i]).ToString("d");
                        dateStr += ", " + getNumberShift.NumberShift(shifts[i]);

                        ListViewItem item = new ListViewItem();

                        item.Name = shifts[i];
                        item.Text = (i + 1).ToString();
                        item.SubItems.Add(dateStr);
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");

                        Invoke(new Action(() => listView.Items.Add(item)));
                    }

                    for (int i = 0; i < shifts.Count; i++)
                    {
                        Shifts currentShift = getShifts.LoadCurrentShift(shifts[i]);

                        Invoke(new Action(() =>
                        {
                            int index = listView.Items.IndexOfKey(shifts[i]);

                            ListViewItem item = listView.Items[index];
                            if (item != null)
                            {
                                item.SubItems[2].Text = currentShift.machinesShift;
                                item.SubItems[3].Text = currentShift.workingTimeShift;
                                item.SubItems[4].Text = currentShift.countOrdersShift.ToString();
                                item.SubItems[5].Text = currentShift.amountOrdersShift.ToString("N0");
                                item.SubItems[6].Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(currentShift.workingOutShift);
                                item.SubItems[7].Text = getPercent.PercentString(currentShift.workingOutShift);
                            }
                        }));
                    }

                    ShiftsDetails shiftsDetailsSumm = (ShiftsDetails)getShifts.LoadCurrentDateShiftsDetails(date, "", token);

                    Invoke(new Action(() =>
                    {
                        label1.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetailsSumm.shiftsWorkingTime);
                        label2.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetailsSumm.allTimeShift);
                        label3.Text = dateTimeOperations.TotalMinutesToHoursAndMinutesStr(shiftsDetailsSumm.allTimeWorkingOutShift);

                        label4.Text = shiftsDetailsSumm.countOrdersShift.ToString() + "/" + shiftsDetailsSumm.countMakereadyShift.ToString();
                        label5.Text = shiftsDetailsSumm.amountAllOrdersShift.ToString("N0");
                        label6.Text = shiftsDetailsSumm.percentWorkingOutShift.ToString("N1") + "%";
                    }));

                    Invoke(new Action(() =>
                    {
                        EnabledButtons(true);

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
            ValueUserBase getUser = new ValueUserBase();
            ValueCategory categoryValue = new ValueCategory();
            ValueInfoBase getInfo = new ValueInfoBase();

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
                EnabledButtons(false);

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

                int fullCountOrdersMonth = 0; ;
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

                    using (MySqlConnection Connect = DBConnection.GetDBConnection())
                    {
                        Connect.Open();
                        MySqlCommand Command = new MySqlCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM machines"
                        };
                        DbDataReader sqlReader = Command.ExecuteReader();

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

                    GetOrdersFromBase getOrder = new GetOrdersFromBase();

                    int countMonth = 0;
                    int amountMonth = 0;
                    int countYear = 0;
                    int amountYear = 0;

                    using (MySqlConnection Connect = DBConnection.GetDBConnection())
                    {
                        Connect.Open();
                        MySqlCommand Command = new MySqlCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM machines"
                        };
                        DbDataReader sqlReader = Command.ExecuteReader();

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
                EnabledButtons(true);

                comboBoxYear.Enabled = true;
                comboBoxMount.Enabled = true;
                comboBoxCategory.Enabled = true;
            }));
        }

        private void StartLoadingAllOrders()
        {
            DateTimePicker dateTime = (DateTimePicker)ControlFromKey("tableLayoutPanelControl", "dateTime");
            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");
            TextBox textBoxFilter = (TextBox)ControlFromKey("tableLayoutPanelControl", "textBoxFilter");

            if (comboBoxMachine.SelectedIndex != -1)
            {
                DateTime date;
                date = dateTime.Value;

                String filterKey;
                filterKey = textBoxFilter.Text;

                List<string> indexes = new List<string>(LoadIndexesOrdersFromBase(date, filterKey));

                StartLoading(indexes);
            }
        }

        CancellationTokenSource cancelTokenSource;
        private void StartLoading(List<string> indexes)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            string machine = getInfo.GetMachineFromName(comboBoxMachine.Text);

            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
                //Thread.Sleep(100);
            }

            cancelTokenSource = new CancellationTokenSource();

            Task task = new Task(() => LoadOrdersDetailsFromBase(cancelTokenSource.Token, machine, indexes), cancelTokenSource.Token);
            task.Start();
        }

        private List<string> LoadIndexesOrdersFromBase(DateTime dateTime, String filter)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            List<string> result = new List<string>();

            String machine = getInfo.GetMachineFromName(comboBoxMachine.Text);

            EnabledButtons(false);

            comboBoxMachine.Enabled = false;

            var name = "listView";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                ListView listView = (ListView)control[0];

                listView.Items.Clear();

                ordersNumbers.Clear();
                //ordersNumbers.Add(new Order("", ""));

                int index = 0;

                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    String commandLine;
                    //commandLine = "strftime('%Y-%m-%d 00:00:00', date(substr(orderAddedDate, 7, 4) || '-' || substr(orderAddedDate, 4, 2) || '-' || substr(orderAddedDate, 1, 2))) >= '";
                    commandLine = "DATE_FORMAT(STR_TO_DATE(orderAddedDate,'%d.%m.%Y %H:%i:%S'), '%Y-%m-%d 00:00:00') >= '";
                    commandLine += dateTime.ToString("yyyy-MM-dd 00:00:00") + "'";

                    Connect.Open();
                    MySqlCommand Command = new MySqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT * FROM orders WHERE " + commandLine + " AND machine = '" + getInfo.GetMachineFromName(comboBoxMachine.Text) + "'"
                    };
                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                    {
                        if (sqlReader["numberOfOrder"].ToString().Contains(filter))
                        {
                            result.Add(sqlReader["count"].ToString());
                            ordersNumbers.Add(new OrderNM(sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString()));

                            String modification = "";
                            if (sqlReader["modification"].ToString() != "")
                                modification = " (" + sqlReader["modification"].ToString() + ")";

                            ListViewItem item = new ListViewItem();

                            item.Name = sqlReader["count"].ToString();
                            item.Text = (index + 1).ToString();
                            item.SubItems.Add(sqlReader["numberOfOrder"].ToString() + modification);
                            item.SubItems.Add(sqlReader["nameOfOrder"].ToString());
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            item.SubItems.Add("");
                            //item.SubItems.Add(orderCalc.OrderCalculate(true, true).ToString("N0"));
                            item.SubItems.Add("");
                            item.SubItems.Add("");

                            listView.Items.Add(item);

                            index++;
                        }

                    }

                    Connect.Close();
                }

                EnabledButtons(true);

                comboBoxMachine.Enabled = true;
            }

            return result;
        }

        private void LoadOrdersDetailsFromBase(CancellationToken token, string machine, List<string> indexes)
        {
            ValueOrdersBase ordersBase = new ValueOrdersBase();
            ValueInfoBase getInfo = new ValueInfoBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            var name = "listView";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                ListView listView = (ListView)control[0];

                for (int i = 0; i < indexes.Count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    using (MySqlConnection Connect = DBConnection.GetDBConnection())
                    {
                        Connect.Open();
                        MySqlCommand Command = new MySqlCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT * FROM orders WHERE count = '" + indexes[i] + "'"
                        };
                        DbDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                        {
                            GetCountOfDone orderCalc = new GetCountOfDone("", machine, sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), "");
                            GetLeadTime leadTimeFirst = new GetLeadTime("", sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["machine"].ToString(), "0");
                            GetLeadTime leadTimeLast = new GetLeadTime("", sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString(), sqlReader["machine"].ToString(), sqlReader["counterRepeat"].ToString());


                            Invoke(new Action(() =>
                            {
                                int index = listView.Items.IndexOfKey(indexes[i]);

                                if (index >= 0)
                                {
                                    ListViewItem item = listView.Items[index];
                                    if (item != null)
                                    {
                                        item.SubItems[3].Text = timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeMakeready"]));
                                        item.SubItems[4].Text = timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeToWork"]));
                                        item.SubItems[5].Text = Convert.ToInt32(sqlReader["amountOfOrder"]).ToString("N0");
                                        item.SubItems[6].Text = leadTimeFirst.GetFirstValue("timeMakereadyStart").ToString();
                                        item.SubItems[7].Text = leadTimeLast.GetLastValue("timeToWorkStop").ToString();
                                        //item.SubItems.Add(orderCalc.OrderCalculate(true, true).ToString("N0"));
                                        item.SubItems[8].Text = orderCalc.OrderFullCalculate().ToString("N0");
                                        item.SubItems[9].Text = ordersBase.GetOrderStatusName(machine, sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString());
                                    }
                                }
                            }));
                        }

                        Connect.Close();
                    }
                }
            }

            Invoke(new Action(() =>
            {

            }));
        }

        private void StartLoadNormFromBase()
        {
            TextBox textBoxFilter = (TextBox)ControlFromKey("tableLayoutPanelControl", "textBoxFilter");
            DateTimePicker dateTimePicker = (DateTimePicker)ControlFromKey("tableLayoutPanelControl", "dateTimeNorm");
            ComboBox comboBoxLength = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxLength");

            String filterKey;
            filterKey = textBoxFilter.Text;

            DateTime dateTimeStart = dateTimePicker.Value;

            DateTime dateTimeEnd = dateTimePicker.Value.AddMonths(comboBoxLength.SelectedIndex + 1);

            LoadNormFromBase(dateTimeStart, dateTimeEnd, filterKey);

        }

        private void LoadNormFromBase(DateTime dateTimeStart, DateTime dateTimeEnd, String filter)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();
            ValueInfoBase getInfo = new ValueInfoBase();

            var name = "listView";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                ListView listView = (ListView)control[0];

                listView.Items.Clear();

                EnabledButtons(false);

                String commandLine;
                //commandLine = "strftime('%Y-%m-%d 00:00:00', date(substr(orderAddedDate, 7, 4) || '-' || substr(orderAddedDate, 4, 2) || '-' || substr(orderAddedDate, 1, 2))) >= '";
                commandLine = "DATE_FORMAT(STR_TO_DATE(orderAddedDate,'%d.%m.%Y %H:%i:%S'), '%Y-%m-%d 00:00:00') >= '";
                commandLine += dateTimeStart.ToString("yyyy-MM-dd 00:00:00") + "'";
                commandLine += " AND ";
                //commandLine += "strftime('%Y-%m-%d 00:00:00', date(substr(orderAddedDate, 7, 4) || '-' || substr(orderAddedDate, 4, 2) || '-' || substr(orderAddedDate, 1, 2))) <= '";
                commandLine += "DATE_FORMAT(STR_TO_DATE(orderAddedDate,'%d.%m.%Y %H:%i:%S'), '%Y-%m-%d 00:00:00') <= '";
                commandLine += dateTimeEnd.ToString("yyyy-MM-dd 00:00:00") + "'";
                //commandLine += 

                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    int index = 0;

                    Connect.Open();
                    MySqlCommand Command = new MySqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT * FROM orders WHERE (" + commandLine + ") AND orderStamp LIKE '%" + filter + "%'"
                    };
                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                    {
                        index++;

                        ListViewItem item = new ListViewItem();

                        item.Name = sqlReader["orderStamp"].ToString();
                        item.Text = (index).ToString();
                        item.SubItems.Add(sqlReader["orderStamp"].ToString());
                        item.SubItems.Add(sqlReader["nameOfOrder"].ToString());
                        item.SubItems.Add(sqlReader["modification"].ToString());
                        item.SubItems.Add(getInfo.GetMachineName(sqlReader["machine"].ToString()));
                        item.SubItems.Add(Convert.ToDateTime(sqlReader["orderAddedDate"]).ToString("D"));
                        item.SubItems.Add(timeOperations.TotalMinutesToHoursAndMinutesStr(Convert.ToInt32(sqlReader["timeMakeready"])));
                        item.SubItems.Add((60 * Convert.ToInt32(sqlReader["amountOfOrder"]) / Convert.ToInt32(sqlReader["timeToWork"])).ToString("N0"));

                        listView.Items.Add(item);
                    }

                    Connect.Close();
                }

                EnabledButtons(true);
            }
        }

        private void LoadUsersFromBase()
        {
            CancellationToken token;

            GetDateTimeOperations dateTimeOperations = new GetDateTimeOperations();
            GetPercentFromWorkingOut getPercent = new GetPercentFromWorkingOut();
            ValueUserBase getUser = new ValueUserBase();

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
                        EnabledButtons(false);

                        listView.Items.Clear();
                    }));

                    List<UserInfo> currentUserInfo = (List<UserInfo>)getUser.GetUserInfo();

                    StringArray categoryArray = new StringArray();
                    ValueCategory valueCategory = new ValueCategory();

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
                        //item.SubItems.Add(currentUserInfo[i].userName);
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

                Invoke(new Action(() =>
                {
                    EnabledButtons(true);
                }));

                break;
            }
        }

        private void LoadMachinesAndCategoryesFromBase()
        {
            CancellationToken token;

            ValueCategory getCategoryes = new ValueCategory();
            ValueInfoBase getMachines = new ValueInfoBase();

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
                        EnabledButtons(false);

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

                Invoke(new Action(() =>
                {
                    EnabledButtons(true);
                }));

                break;
            }
        }

        private void LoadPage(int page)
        {
            SelectedButton(page);

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
                    tableLayoutPanel1.RowStyles[1].Height = 90;
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

        private void comboBoxLength_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedMonthLengthNorm = ((ComboBox)sender).SelectedIndex;

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
            ValueInfoBase getMachine = new ValueInfoBase();
            selectedMachine = getMachine.GetMachineFromName(((ComboBox)sender).Text);

            UpdatePage(currentPage);
        }

        private void dateTime_ValueChanged(object sender, EventArgs e)
        {
            ValueInfoBase getMachine = new ValueInfoBase();
            selectedDateTime = ((DateTimePicker)sender).Value;

            UpdatePage(currentPage);
        }

        private void dateTimeNorm_ValueChanged(object sender, EventArgs e)
        {
            ValueInfoBase getMachine = new ValueInfoBase();
            selectedDateTimeNorm = ((DateTimePicker)sender).Value;

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
                    //ini.SetCheckDBLocalPath(checkBox.Checked);

                    EnabledPathControls(checkBox.Checked);
                    break;

                default:
                    break;
            }
            UpdatePage(currentPage);
        }


        private void ListViewDoubleClick(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueUserBase getUser = new ValueUserBase();

            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            int selectegIndex = ((ListView)sender).SelectedIndices[0];
            String selectedName = ((ListView)sender).SelectedItems[0].Name.ToString();

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
                    //UpdatePage(currentPage);
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

        private void acceptBaseButton_Click(object sender, EventArgs e)
        {
            ComboBox comboBoxBase = null;

            var name = "tableLayoutPanelControl";
            if (tableLayoutPanel1.Controls.ContainsKey(name))
            {
                var control = tableLayoutPanel1.Controls.Find(name, false);
                TableLayoutPanel panel = (TableLayoutPanel)control[0];

                var subControl = panel.Controls.Find("tableLayoutPanelBase", false);
                var comboBox = subControl[0].Controls.Find("comboBoxBase", false);

                comboBoxBase = (ComboBox)comboBox[0];
            }

            DBConnection connection = new DBConnection();

            IniFile ini = new IniFile(Form1.connectionFile);

            String section = "mysql" + comboBoxBase.Text;

            string host = "";
            int port = 0;
            string database = "";
            string username = "";
            string password = "";

            if (ini.KeyExists("host", section))
                host = ini.ReadString("host", section);

            if (ini.KeyExists("port", section))
                port = ini.ReadInt("port", section);

            if (ini.KeyExists("database", section))
                database = ini.ReadString("database", section);

            if (ini.KeyExists("username", section))
                username = ini.ReadString("username", section);

            if (ini.KeyExists("password", section))
                password = ini.ReadString("password", section);

            if (connection.IsServerConnected(host, port, database, username, password))
            {
                ini.Write("selected", section, "general");

                LoadBaseInfo();

                LoadPage(currentPage);
            }
            else
            {
                MessageBox.Show("Соединение не установлено!", "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void editBaseButton_Click(object sender, EventArgs e)
        {
            FormAddEditTestMySQL form = new FormAddEditTestMySQL(true);
            form.ShowDialog();
            LoadBaseInfo();

            LoadPage(currentPage);
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
                    //ShowSelectFile();

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
            ValueInfoBase getInfo = new ValueInfoBase();

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
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueUserBase getUser = new ValueUserBase();

            ListView listV = (ListView)ControlFromKey("tableLayoutPanel1", "listView");

            String startOfShift = listV.SelectedItems[0].Name.ToString();
            String userId = getUser.GetCurrentUserIDFromShiftStart(startOfShift);

            List<String> machines = (List<String>)getInfo.GetMachines(userId);

            for (int i = 0; i < machines.Count; i++)
            {
                if (Convert.ToBoolean(getInfo.GetActiveOrder(machines[i])) == true)
                {
                    FormAddCloseOrder form = new FormAddCloseOrder(startOfShift, userId, machines[i]);
                    form.ShowDialog();
                }
            }

            if (getInfo.GetMachinesForUserActive(userId) == true)
            {
                MessageBox.Show("Смена не была завершена т.к. не все операции были завершены.", "Завершение смены", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                ValueUserBase userBase = new ValueUserBase();
                ValueInfoBase infoBase = new ValueInfoBase();
                ValueShiftsBase getShift = new ValueShiftsBase();

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

        private void abortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

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
                    AbortOrder(getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[listV.SelectedIndices[0]].numberOfOrder, ordersNumbers[listV.SelectedIndices[0]].modificationOfOrder);
                    UpdatePage(currentPage);
                    break;

                default:
                    break;
            }
        }

        private void deactivateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ValueInfoBase getInfo = new ValueInfoBase();

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
                    DeactivateOrder(getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[listV.SelectedIndices[0]].numberOfOrder, ordersNumbers[listV.SelectedIndices[0]].modificationOfOrder);
                    UpdatePage(currentPage);
                    break;

                default:
                    break;
            }
        }

        private void LoadShiftdetails(String timeStartShift)
        {
            FormOneShiftDetails form = new FormOneShiftDetails(adminMode, timeStartShift);
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
                textBoxDBPath.Enabled = false;
                addButtonDBPath.Enabled = true;
            }
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
