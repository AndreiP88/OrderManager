using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
            "Примечание;180",
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
                    CommandText = @"SELECT DISTINCT machine FROM Info"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                {
                    machine.Add(getInfo.GetMachineName(sqlReader["machine"].ToString()));
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
                form = new FormAddNewOrder(true, dataBase, getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[selectedIndex].numberOfOrder, ordersNumbers[selectedIndex].modificationOfOrder);
            else
                form = new FormAddNewOrder(false, dataBase, getInfo.GetMachineFromName(comboBoxMachine.Text), "", "");

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

            ToolStripMenuItem viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ToolStripMenuItem editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ToolStripMenuItem deactivateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ToolStripMenuItem deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();

            menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            viewToolStripMenuItem,
            editToolStripMenuItem,
            deactivateToolStripMenuItem,
            deleteToolStripMenuItem1});
            menu.Name = "contextMenuStrip";
            menu.Size = new System.Drawing.Size(163, 92);
            menu.Opening += new System.ComponentModel.CancelEventHandler(contextMenuStrip_Opening);

            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Text = "Просмотр";
            viewToolStripMenuItem.Click += new System.EventHandler(viewToolStripMenuItem_Click);

            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Text = "Редактировать";
            editToolStripMenuItem.Click += new System.EventHandler(editToolStripMenuItem_Click);

            deactivateToolStripMenuItem.Name = "deactivateToolStripMenuItem";
            deactivateToolStripMenuItem.Text = "Деактивировать";
            deactivateToolStripMenuItem.Click += new System.EventHandler(deactivateToolStripMenuItem_Click);

            deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            deleteToolStripMenuItem1.Size = new System.Drawing.Size(162, 22);
            deleteToolStripMenuItem1.Text = "Удалить";
            deleteToolStripMenuItem1.Visible = false;

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

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30));
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

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100));
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
            tableLayoutPanelControl.Controls.Add(addButton, 0, 0);
            addButton.Click += new System.EventHandler(addButton_Click);

            Button editButton = new Button();
            editButton.Name = "editButton";
            editButton.Dock = DockStyle.Fill;
            editButton.Location = new System.Drawing.Point(3, 3);
            editButton.Size = new System.Drawing.Size(144, 21);
            editButton.TabIndex = 0;
            editButton.Text = "Редактировать";
            editButton.Visible = true;
            tableLayoutPanelControl.Controls.Add(editButton, 0, 1);
            editButton.Click += new System.EventHandler(editButton_Click);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label01", "Всего записей:", ContentAlignment.MiddleRight), 1, 0);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label1", "", ContentAlignment.MiddleLeft), 2, 0);

            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label02", "Активных сотрудников:", ContentAlignment.MiddleRight), 1, 1);
            tableLayoutPanelControl.Controls.Add((Label)CreateLabel("label2", "", ContentAlignment.MiddleLeft), 2, 1);
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

            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150));
            tableLayoutPanelControl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100));

            tableLayoutPanelControl.RowCount = 3;

            tableLayoutPanelControl.Visible = true;
            tableLayoutPanel1.Controls.Add(tableLayoutPanelControl, 0, 1);

            Button categoryButton = new Button();
            categoryButton.Name = "categoryButton";
            categoryButton.Dock = DockStyle.Fill;
            categoryButton.Location = new System.Drawing.Point(3, 3);
            categoryButton.Size = new System.Drawing.Size(144, 21);
            categoryButton.TabIndex = 0;
            categoryButton.Text = "Добавить";
            categoryButton.Visible = true;
            tableLayoutPanelControl.Controls.Add(categoryButton, 0, 0);
            //categoryButton.Click += new System.EventHandler(categoryButton_Click);

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

        private Object CreateLabel(String name, String text, ContentAlignment contentAlignment)
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

        private void MainLVInsertValue()
        {
            GetValueFromShiftsBase getShifts = new GetValueFromShiftsBase(dataBase);
            GetUserIDOrMachineFromInfoBase getMachine = new GetUserIDOrMachineFromInfoBase(dataBase);
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);
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
                    List<String> machines = (List<String>)getMachine.GetMachines(users[i]);

                    for (int j = 0; j < machines.Count; j++)
                    {
                        GetValueFromOrdersBase getOrder = new GetValueFromOrdersBase(dataBase, machines[j], getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j]));
                        GetLeadTime leadTimeCurr = new GetLeadTime(dataBase, userBase.GetCurrentShiftStart(users[i]), getInfo.GetCurrentOrderNumber(machines[j]), getInfo.GetCurrentOrderModification(machines[j]), machines[j], getOrder.GetValue("counterRepeat"));
                        GetOrdersFromBase ordersFromBase = new GetOrdersFromBase(dataBase);

                        List<Order> ordersCurrentShift = (List<Order>)ordersFromBase.LoadAllOrdersFromBase(userBase.GetCurrentShiftStart(users[i]), "");


                        String user = "";
                        String currentTime = "";
                        String order = "";

                        if (getInfo.GetCurrentOrderNumber(machines[j]) != "")
                            order = getInfo.GetCurrentOrderNumber(machines[j]) + ", " + getOrder.GetValue("nameOfOrder");

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
                        item.SubItems.Add(getOrder.GetOrderStatusName());
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
                var controlPanel = tableLayoutPanel1.Controls.Find(panel, false);
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
                            CommandText = @"SELECT * FROM Info"
                        };
                        SQLiteDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                        {
                            if (getInfo.GetCategoryMachine(sqlReader["machine"].ToString()) == category || selectedCategory == 0)
                            {
                                Invoke(new Action(() =>
                                {
                                    ListViewItem item = new ListViewItem();

                                    item.Name = sqlReader["machine"].ToString();
                                    item.Text = (listView.Items.Count + 1).ToString();
                                    item.SubItems.Add(categoryValue.GetCategoryName(getInfo.GetCategoryMachine(sqlReader["machine"].ToString())));
                                    item.SubItems.Add(getInfo.GetMachineName(sqlReader["machine"].ToString()));
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
                            CommandText = @"SELECT * FROM Info"
                        };
                        SQLiteDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read()) // считываем и вносим в комбобокс список заголовков
                        {
                            if (getInfo.GetCategoryMachine(sqlReader["machine"].ToString()) == category || selectedCategory == 0)
                            {
                                List<int> orderCountAmountMonth = new List<int>((List<int>)getOrder.GetOrdersFromMachineForTheMonth(date, sqlReader["machine"].ToString()));
                                List<int> orderCountAmountYear = new List<int>((List<int>)getOrder.GetOrdersFromMachineForTheYear(date, sqlReader["machine"].ToString()));

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
                                    int index = listView.Items.IndexOfKey(sqlReader["machine"].ToString());

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
                                GetValueFromOrdersBase ordersBase = new GetValueFromOrdersBase(dataBase, getInfo.GetMachineFromName(comboBoxMachine.Text), sqlReader["numberOfOrder"].ToString(), sqlReader["modification"].ToString());

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
                                item.SubItems.Add(ordersBase.GetOrderStatusName());

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

        private void LoadPage(int page)
        {
            List<ColumnHeader> head;

            switch (page)
            {
                case 1:
                    tableLayoutPanel1.RowStyles[1].Height = 0;
                    head = new List<ColumnHeader>(ListHeaders(columnHeadersMain));
                    CreateListView(head);
                    MainLVInsertValue();
                    break;
                case 2:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateStatisticUsersControls();
                    head = new List<ColumnHeader>(ListHeaders(columnHeadersStatistic));
                    CreateListView(head);
                    StartLoadingStatisticUsers();
                    break;
                case 3:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateShiftsControls();
                    head = new List<ColumnHeader>(ListHeaders(columnHeadersShifts));
                    CreateListView(head);
                    StartLoadingShifts();
                    break;
                case 4:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateStatisticMachinesControls();
                    head = new List<ColumnHeader>(ListHeaders(columnHeadersStatisticMachines));
                    CreateListView(head);
                    StartLoadingStatisticMachines();
                    break;
                case 5:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateAllOrdersControls();
                    head = new List<ColumnHeader>(ListHeaders(columnHeadersAllOrders));
                    CreateListView(head);
                    ContextMenuToLV();
                    StartLoadingAllOrders();
                    break;
                case 6:
                    tableLayoutPanel1.RowStyles[1].Height = 35;
                    CreateNormControls();
                    head = new List<ColumnHeader>(ListHeaders(columnHeadersNorm));
                    CreateListView(head);
                    StartLoadNormFromBase();
                    break;
                case 7:
                    tableLayoutPanel1.RowStyles[1].Height = 90;
                    CreateUsersControls();
                    head = new List<ColumnHeader>(ListHeaders(columnHeadersUsers));
                    CreateListView(head);
                    LoadUsersFromBase();
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
        
        private void ListViewDoubleClick(object sender, EventArgs e)
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase);

            String shiftStart = ((ListView) sender).SelectedItems[0].Name.ToString();
            ComboBox comboBoxMachine = (ComboBox)ControlFromKey("tableLayoutPanelControl", "comboBoxMachine");

            switch (currentPage)
            {
                case 1:
                    LoadShiftdetails(shiftStart);
                    break;
                case 2:

                    break;
                case 3:
                    LoadShiftdetails(shiftStart);
                    break;
                case 4:

                    break;
                case 5:
                    if (((ListView)sender).SelectedItems.Count != 0)
                        LoadSelectedOrder(true, getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[((ListView)sender).SelectedIndices[0]].numberOfOrder, ordersNumbers[((ListView)sender).SelectedIndices[0]].modificationOfOrder);
                    break;
                case 6:
                    
                    break;
                case 7:
                    ShowEditUserForm(((ListView)sender).Items[((ListView)sender).SelectedIndices[0]].Name);
                    LoadUsersFromBase();
                    break;

                default:
                    break;
            }            
        }

        private void addButton_Click(object sender, EventArgs e)
        {
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
                    StartLoadingAllOrders();
                    break;
                case 6:

                    break;
                case 7:
                    ShowAddUserForm();
                    LoadUsersFromBase();
                    break;

                default:
                    break;
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
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

                    break;
                case 6:

                    break;
                case 7:
                    if (listV.SelectedIndices.Count > 0)
                        ShowEditUserForm(listV.Items[listV.SelectedIndices[0]].Name);
                    LoadUsersFromBase();
                    break;

                default:
                    break;
            }
        }

        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            ListView listV = (ListView)ControlFromKey("tableLayoutPanel1", "listView");

            e.Cancel = listV.SelectedItems.Count == 0;
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
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
                    LoadSelectedOrder(true, getInfo.GetMachineFromName(comboBoxMachine.Text), ordersNumbers[listV.SelectedIndices[0]].numberOfOrder, ordersNumbers[listV.SelectedIndices[0]].modificationOfOrder);
                    break;

                default:
                    break;
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
                    ShowFullOrdersForm(true, listV.SelectedIndices[0]);
                    UpdatePage(currentPage);
                    break;
                case 6:
                    
                    break;

                default:
                    break;
            }
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
            currentPage = 1;
            LoadPage(currentPage);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            currentPage = 2;
            LoadPage(currentPage);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            currentPage = 3;
            LoadPage(currentPage);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            currentPage = 4;
            LoadPage(currentPage);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            currentPage = 5;
            LoadPage(currentPage);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            currentPage = 6;
            LoadPage(currentPage);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            currentPage = 7;
            LoadPage(currentPage);
        }
    }
}
