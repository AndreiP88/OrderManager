﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace OrderManager
{
    public partial class FormLoadOrders : Form
    {
        bool _loadForViewOrders;
        string loadMachine;
        string userID;
        int DefaultMachine;

        public FormLoadOrders(string lMachine)
        {
            InitializeComponent();

            this.loadMachine = lMachine;
            this.userID = "0";

            _loadForViewOrders = false;
        }

        public FormLoadOrders(bool loadForView, string lUser, int defaultMachine = -1)
        {
            InitializeComponent();

            this.loadMachine = "0";
            this.userID = lUser;
            DefaultMachine = defaultMachine;

            _loadForViewOrders = loadForView;
        }

        class OrderLoadNumber
        {
            public string numberOfOrder;
            public string nameCustomer;
            public OrderLoadNumber(string number, string customer)
            {
                this.numberOfOrder = number;
                this.nameCustomer = customer;
            }
        }

        List<OrdersLoad> orders = new List<OrdersLoad>();
        List<OrderLoadNumber> orderNumbers = new List<OrderLoadNumber>();

        private async Task LoadMachine()
        {
            ValueInfoBase getInfo = new ValueInfoBase();
            ValueUserBase getUser = new ValueUserBase();

            comboBox1.Items.Clear();

            /*using (MySqlConnection Connect = DBConnection.GetDBConnection())
            {
                Connect.Open();
                MySqlCommand Command = new MySqlCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT DISTINCT * FROM machines WHERE id IN (SELECT machine FROM machinesinfo WHERE nameOfExecutor = @userID)"
                };
                Command.Parameters.AddWithValue("@userID", userID);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                     comboBox1.Items.Add(sqlReader["name"].ToString());
                }

                Connect.Close();
            }*/
            

            string categoryesCurrentUser = getUser.GetCategoryesMachine(userID);

            string[] categoryes = categoryesCurrentUser.Split(';');
            int indexDeafaultMachineItem = -1;

            for (int i = 0; i < categoryes.Length; i++)
            {
                using (MySqlConnection Connect = DBConnection.GetDBConnection())
                {
                    Connect.Open();
                    MySqlCommand Command = new MySqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT DISTINCT * FROM machines WHERE category = @category"
                    };
                    Command.Parameters.AddWithValue("@category", categoryes[i]);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        int idMachine = (int)sqlReader["id"];

                        comboBox1.Items.Add(sqlReader["name"].ToString());

                        if (DefaultMachine == idMachine)
                            indexDeafaultMachineItem = comboBox1.Items.Count - 1;
                    }

                    Connect.Close();
                }
            }

            if (!_loadForViewOrders)
            {
                comboBox1.Enabled = false;
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox1.Items.Add(await getInfo.GetMachineName(loadMachine));
                comboBox1.SelectedIndex = 0;
            }

            if (comboBox1.Items.Count > 0 && _loadForViewOrders)
            {
                //SelectLastMschineToComboBox(nameOfExecutor);
                comboBox1.Enabled = true;
                //comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                if (indexDeafaultMachineItem != -1 && indexDeafaultMachineItem < comboBox1.Items.Count)
                {
                    comboBox1.SelectedIndex = indexDeafaultMachineItem;
                }
                else
                {
                    comboBox1.SelectedIndex = 0;
                }
            }
        }

        private string GetCustomerNameFromID(string id)
        {
            string result = "";

            string connectionString = @"Data Source = SRV-ACS\DSACS; Initial Catalog = asystem; Persist Security Info = True; User ID = ds; Password = 1";

            if (id != "")
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = connection,
                        //CommandText = @"SELECT * FROM dbo.order_head WHERE status = '1' AND order_num LIKE '@order_num'"
                        CommandText = @"SELECT ul_name FROM dbo.common_ul_directory WHERE id_common_ul_directory = @id_customer"
                    };
                    Command.Parameters.AddWithValue("@id_customer", id);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        result = sqlReader["ul_name"].ToString();
                    }

                    connection.Close();
                }
            }

            return result;
        }

        private async Task<string> GetStampFromOrderNumber(string searchNumber)
        {
            ValueCategory valueCategory = new ValueCategory();
            ValueInfoBase getInfo = new ValueInfoBase();

            string category = await getInfo.GetCategoryMachine(loadMachine);

            int normOperation = Convert.ToInt32(valueCategory.GetIDOptionView(category));

            string result = "";

            int nOrderId = 0;
            
            List<string> tools = new List<string>();

            string connectionString = @"Data Source = SRV-ACS\DSACS; Initial Catalog = asystem; Persist Security Info = True; User ID = ds; Password = 1";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand Command = new SqlCommand
                {
                    Connection = connection,
                    CommandText = @"SELECT id_order_head FROM dbo.order_head WHERE order_num = @searchNumber AND status = '1'"
                };
                Command.Parameters.AddWithValue("@searchNumber", searchNumber);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    nOrderId = Convert.ToInt32(sqlReader["id_order_head"]);
                }

                connection.Close();
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand Command = new SqlCommand
                {
                    Connection = connection,
                    /*CommandText = @"exec proc_rpt_order_operation_BY @nOrderId,@nSubdivisionId,@nIsCost,@nReadOnly,@nIsFlex,@nIsPaper,@nIsSubcontract,@nReportId,@nOptionSetId"*/
                    CommandText = @"exec proc_rpt_order_tool @nOrderId, @nSubdivisionId, @nTemplateId, @nReadOnly"
                };
                Command.Parameters.AddWithValue("@nOrderId", nOrderId);
                Command.Parameters.AddWithValue("@nSubdivisionId", 1);
                Command.Parameters.AddWithValue("@nReadOnly", 1);
                Command.Parameters.AddWithValue("@nTemplateId", 16);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (Convert.ToInt32(sqlReader["id_norm_operation"]) == normOperation)
                    {
                        tools.Add(sqlReader["tool_name"].ToString());
                    }
                }

                connection.Close();
            }

            for (int i = 0; i < tools.Count; i++)
            {
                if (i < tools.Count - 1)
                {
                    result += GetNumberStampFromStr(tools[i]) + ", ";
                }
                else
                {
                    result += GetNumberStampFromStr(tools[i]);
                }
            }

            return result;
        }

        private string GetNumberStampFromStr(string str)
        {
            string result = "";

            int startIndex;
            int endIndex;

            if (str.IndexOf("(№", 0) != -1)
            {
                startIndex = str.IndexOf("(№", 0) + 2;
                endIndex = str.IndexOf(")", startIndex);
            }
            else
            {
                startIndex = str.IndexOf("(", 0) + 1;
                endIndex = str.IndexOf(")", startIndex);
            }

            if (startIndex >= 0 && endIndex > 0)
            {
                result = str.Substring(startIndex, endIndex - startIndex);
                result = result.Replace(" ", "");
            }

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //LoadOrdersByNumber(textBox1.Text);
            StartSearch(textBox1.Text);
        }

        private void StartSearch(string searchNumber)
        {
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
            }

            cancelTokenSource = new CancellationTokenSource();

            Task task = new Task(() => LoadOrdersByNumber(cancelTokenSource.Token, searchNumber), cancelTokenSource.Token);
            task.Start();
        }

        private async void LoadOrdersByNumber(CancellationToken token, string searchNumber)
        {
            ValueCategory valueCategory = new ValueCategory();
            ValueInfoBase valueInfo = new ValueInfoBase();
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            Invoke(new Action(() =>
            {
                orders.Clear();
                orderNumbers.Clear();
                listView1.Items.Clear();
            }));

            string category = await valueInfo.GetCategoryMachine(loadMachine);

            string idNormOperation = valueCategory.GetMainIDNormOperation(category);
            string idNormOperationMakeReady = valueCategory.GetMKIDNormOperation(category);
            string idNormOperationMakeWork = valueCategory.GetWKIDNormOperation(category);

            List<string> orderHeadList = new List<string>();
            List<string> orderHeadNameList = new List<string>();

            string connectionString = @"Data Source = SRV-ACS\DSACS; Initial Catalog = asystem; Persist Security Info = True; User ID = ds; Password = 1";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand Command = new SqlCommand
                {
                    Connection = connection,
                    //CommandText = @"SELECT * FROM dbo.order_head WHERE status = '1' AND order_num LIKE '@order_num'"
                    CommandText = @"SELECT * FROM dbo.order_head WHERE (status = '1' AND order_num LIKE '%" + searchNumber + "%')"
                };
                Command.Parameters.AddWithValue("@order_num", "%" + textBox1.Text + "%");

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    orderHeadList.Add(sqlReader["id_order_head"].ToString());
                    orderHeadNameList.Add(sqlReader["order_name"].ToString());

                    orderNumbers.Add(new OrderLoadNumber(
                        sqlReader["order_num"].ToString(),
                        GetCustomerNameFromID(sqlReader["id_customer"].ToString())
                        ));
                }

                connection.Close();
            }

            for (int i = 0; i < orderHeadList.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                List<int> jobItem = new List<int>();
                List<string> itemID = new List<string>();

                string stamp = await GetStampFromOrderNumber(orderNumbers[i].numberOfOrder);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = connection,

                        CommandText = @"SELECT id_man_order_job_item, itemid FROM dbo.man_order_job_item WHERE id_man_order_job IN (
                            SELECT id_man_order_job FROM dbo.man_order_job WHERE id_order_head IN (
                            '" + orderHeadList[i] + "') AND id_norm_operation = '" + idNormOperation + "')"
                    };
                    //Command.Parameters.AddWithValue("@orderItems", orderItemsList[i]); detail_name

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        ///////
                        jobItem.Add(Convert.ToInt32(sqlReader["id_man_order_job_item"]));
                        itemID.Add(sqlReader["itemid"].ToString());
                    }
                    //MessageBox.Show(itemOrder.Count.ToString());
                    connection.Close();
                }

                for (int k = 0; k < jobItem.Count; k++)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    List<string> itemOrder = new List<string>();

                    List<int> mkNormTime = new List<int>();
                    List<int> wkNormTime = new List<int>();
                    List<int> amounts = new List<int>();

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand Command = new SqlCommand
                        {
                            Connection = connection,
                            //CommandText = @"SELECT * FROM dbo.order_head WHERE status = '1' AND order_num LIKE '@order_num'"
                            //CommandText = @"SELECT * FROM dbo.order_head WHERE (status = '1' AND order_num LIKE '%" + searchNumber + "%')"

                            CommandText = @"SELECT detail_name FROM dbo.order_detail WHERE id_order_detail = @itemid"
                        };
                        Command.Parameters.AddWithValue("@itemid", itemID[k]);

                        DbDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            ///////
                            itemOrder.Add(orderHeadNameList[i] + ": " + sqlReader["detail_name"].ToString());
                        }
                        //MessageBox.Show(itemOrder.Count.ToString());
                        connection.Close();
                    }

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand Command = new SqlCommand
                        {
                            Connection = connection,

                            CommandText = @"SELECT id_norm_operation, plan_out_qty, normtime FROM dbo.man_planjob_list WHERE id_man_order_job_item = @itemid"
                        };
                        Command.Parameters.AddWithValue("@itemid", jobItem[k]);

                        DbDataReader sqlReader = Command.ExecuteReader();

                        while (sqlReader.Read())
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            if (sqlReader["id_norm_operation"].ToString() == idNormOperationMakeReady)
                            {
                                if (sqlReader["normtime"].ToString() != "")
                                {
                                    mkNormTime.Add(Convert.ToInt32(sqlReader["normtime"]));
                                }
                                else
                                {
                                    mkNormTime.Add(0);
                                }
                            }

                            if (sqlReader["id_norm_operation"].ToString() == idNormOperationMakeWork)
                            {
                                wkNormTime.Add(sqlReader["normtime"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["normtime"]));
                                amounts.Add(sqlReader["plan_out_qty"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["plan_out_qty"]));
                            }
                        }
                        connection.Close();
                    }

                    if (mkNormTime.Count == wkNormTime.Count)
                    {
                        for (int j = 0; j < mkNormTime.Count; j++)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            string itemOr = "";

                            if (itemOrder.Count > 0)
                            {
                                itemOr = itemOrder[j];
                            }

                            orders.Add(new OrdersLoad(
                                orderNumbers[i].numberOfOrder,
                                orderNumbers[i].nameCustomer,
                                itemOr,
                                mkNormTime[j],
                                wkNormTime[j],
                                amounts[j],
                                stamp,
                                orderHeadList[i],
                                jobItem[k]
                            ));

                            int index = orders.Count - 1;
                            AddOrderToListView(index, orders[index], token);
                        }
                    }

                    if (mkNormTime.Count > wkNormTime.Count)
                    {
                        for (int j = 0; j < mkNormTime.Count; j++)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            string itemOr = "";

                            if (itemOrder.Count > 0)
                            {
                                itemOr = itemOrder[j];
                            }

                            if (j < wkNormTime.Count)
                            {
                                orders.Add(new OrdersLoad(
                                    orderNumbers[i].numberOfOrder,
                                    orderNumbers[i].nameCustomer,
                                    itemOr,
                                    mkNormTime[j],
                                    wkNormTime[j],
                                    amounts[j],
                                    stamp,
                                    orderHeadList[i],
                                    jobItem[k]
                                ));

                                int index = orders.Count - 1;
                                AddOrderToListView(index, orders[index], token);
                            }
                            else
                            {
                                orders.Add(new OrdersLoad(
                                    orderNumbers[i].numberOfOrder,
                                    orderNumbers[i].nameCustomer,
                                    itemOr,
                                    mkNormTime[j],
                                    0,
                                    0,
                                    stamp,
                                    orderHeadList[i],
                                    jobItem[k]
                                ));

                                int index = orders.Count - 1;
                                AddOrderToListView(index, orders[index], token);
                            }


                        }
                    }

                    if (mkNormTime.Count < wkNormTime.Count)
                    {
                        for (int j = 0; j < wkNormTime.Count; j++)
                        {
                            if (token.IsCancellationRequested)
                            {
                                break;
                            }

                            string itemOr = "";

                            if (itemOrder.Count > 0)
                            {
                                itemOr = itemOrder[j];
                            }

                            if (j < mkNormTime.Count)
                            {
                                orders.Add(new OrdersLoad(
                                    orderNumbers[i].numberOfOrder,
                                    orderNumbers[i].nameCustomer,
                                    itemOr,
                                    mkNormTime[j],
                                    wkNormTime[j],
                                    amounts[j],
                                    stamp,
                                    orderHeadList[i],
                                    jobItem[k]
                                ));

                                int index = orders.Count - 1;
                                AddOrderToListView(index, orders[index], token);
                            }
                            else
                            {
                                orders.Add(new OrdersLoad(
                                    orderNumbers[i].numberOfOrder,
                                    orderNumbers[i].nameCustomer,
                                    itemOr,
                                    0,
                                    wkNormTime[j],
                                    amounts[j],
                                    stamp,
                                    orderHeadList[i],
                                    jobItem[k]
                                ));

                                int index = orders.Count - 1;
                                AddOrderToListView(index, orders[index], token);
                            }
                        }
                    }

                    /*ListViewItem item = new ListViewItem();

                    int index = orders.Count - 1;

                    item.Name = index.ToString();
                    item.Text = (index + 1).ToString();
                    item.SubItems.Add(orders[index].numberOfOrder.ToString());
                    item.SubItems.Add(orders[index].nameCustomer.ToString());
                    item.SubItems.Add(orders[index].nameItem.ToString());
                    item.SubItems.Add(timeOperations.MinuteToTimeString(orders[index].makereadyTime));
                    item.SubItems.Add(timeOperations.MinuteToTimeString(orders[index].workTime));
                    item.SubItems.Add(orders[index].amountOfOrder.ToString("N0"));
                    item.SubItems.Add(orders[index].stamp.ToString());

                    Invoke(new Action(() =>
                    {
                        listView1.Items.Add(item);
                    }));*/
                }
            }

            for (int i = 0; i < orders.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                /*ListViewItem item = new ListViewItem();

                item.Name = i.ToString();
                item.Text = (i + 1).ToString();
                item.SubItems.Add(orders[i].numberOfOrder.ToString());
                item.SubItems.Add(orders[i].nameCustomer.ToString());
                item.SubItems.Add(orders[i].nameItem.ToString());
                item.SubItems.Add(timeOperations.MinuteToTimeString(orders[i].makereadyTime));
                item.SubItems.Add(timeOperations.MinuteToTimeString(orders[i].workTime));
                item.SubItems.Add(orders[i].amountOfOrder.ToString("N0"));
                item.SubItems.Add(orders[i].stamp.ToString());

                Invoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                }));*/
            }
        }

        CancellationTokenSource cancelTokenSource;
        private void StartLoading()
        {
            cancelTokenSource?.Cancel();

            cancelTokenSource = new CancellationTokenSource();

            Task task = new Task(() => LoadPlan(cancelTokenSource.Token), cancelTokenSource.Token);
            task.Start();
            //LoadPlan(cancelTokenSource.Token);
        }

        private async void LoadPlan(CancellationToken token)
        {
            ValueCategory valueCategory = new ValueCategory();
            ValueInfoBase valueInfo = new ValueInfoBase();

            Invoke(new Action(() =>
            {
                orders.Clear();
                //orderNumbers.Clear();
                listView1.Items.Clear();
            }));

            string category = await valueInfo.GetCategoryMachine(loadMachine);

            string idNormOperationMakeReady = valueCategory.GetMKIDNormOperation(category);
            string idNormOperationMakeWork = valueCategory.GetWKIDNormOperation(category);
            string idMachine = await valueInfo.GetIDEquipMachine(loadMachine);

            int lastItemIndex = -1;

            try
            {
                using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                {
                    Connect.Open();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT
                                          man_planjob.id_man_planjob,
                                          man_planjob.date_begin,
                                          man_planjob.date_end,
                                          man_planjob.status,
                                          man_planjob.flags,
                                          man_planjob.id_equip,
                                          man_planjob_list.plan_out_qty,
                                          man_planjob_list.normtime,
                                          order_head.order_num,
                                          common_ul_directory.ul_name,
                                          order_head.order_name,
                                          order_detail.detail_name,
                                          common_equip_directory.equip_name,
                                          man_planjob_list.id_norm_operation,
                                          man_idletime.idletime_type,
                                          man_idletime.id_idletime,
                                          idletime_directory.idletime_name,
                                          man_idletime.id_man_idletime,
                                          order_head.id_order_head,
                                          man_planjob.id_man_order_job_item 
                                        FROM
                                          dbo.man_planjob
                                          INNER JOIN dbo.man_planjob_list ON man_planjob.id_man_order_job_item = man_planjob_list.id_man_order_job_item
                                          LEFT JOIN dbo.man_order_job_item ON man_planjob.id_man_order_job_item = man_order_job_item.id_man_order_job_item
                                          LEFT JOIN dbo.man_order_job ON man_order_job_item.id_man_order_job = man_order_job.id_man_order_job
                                          LEFT JOIN dbo.order_head ON man_order_job.id_order_head = order_head.id_order_head
                                          LEFT JOIN dbo.common_ul_directory ON order_head.id_customer = common_ul_directory.id_common_ul_directory
                                          LEFT JOIN dbo.common_equip_directory ON man_order_job.id_equip = common_equip_directory.id_common_equip_directory
                                          LEFT JOIN dbo.man_idletime ON man_order_job.id_man_order_job = man_idletime.id_man_order_job
                                          LEFT JOIN dbo.idletime_directory ON man_idletime.id_idletime = idletime_directory.id_idletime_directory
                                          LEFT JOIN dbo.order_detail ON man_order_job_item.itemid = order_detail.id_order_detail 
                                        WHERE
                                          man_planjob.status <> 2 AND
                                          man_planjob.flags <> 1 AND
                                          plan_out_qty IS NOT NULL AND
	                                      man_planjob.id_equip = @idMachine
                                        ORDER BY
	                                      man_planjob.date_begin ASC"
                    };
                    Command.Parameters.AddWithValue("@idMachine", idMachine);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        int idManPlanJob = Convert.ToInt32(sqlReader["id_man_planjob"]);
                        int idManOrderJobItem = Convert.ToInt32(sqlReader["id_man_order_job_item"]);

                        if (!DBNull.Value.Equals(sqlReader["order_num"]))
                        {
                            //подумать над реализацией
                            int lastIutemIndex = orders.Count - 1;
                            int itemIndex = orders.FindIndex((v) => v.IDManPlanJob == idManPlanJob);

                            if (itemIndex == -1)
                            {
                                orders.Add(new OrdersLoad(
                                        0,
                                        idManPlanJob,
                                        sqlReader["date_begin"].ToString(),
                                        sqlReader["date_end"].ToString(),
                                        sqlReader["order_num"].ToString(),
                                        sqlReader["ul_name"].ToString(),
                                        sqlReader["order_name"].ToString() + ": " + sqlReader["detail_name"].ToString(),
                                        0,
                                        0,
                                        0,
                                        await GetStampFromOrderNumber(sqlReader["order_num"].ToString()),
                                        sqlReader["id_order_head"].ToString(),
                                        idManOrderJobItem
                                    ));

                                itemIndex = orders.Count - 1;
                            }

                            if (sqlReader["id_norm_operation"].ToString() == idNormOperationMakeReady)
                            {
                                orders[itemIndex].makereadyTime = (int)(Convert.ToDouble(sqlReader["normtime"]) / Convert.ToDouble(sqlReader["plan_out_qty"]));
                            }

                            if (sqlReader["id_norm_operation"].ToString() == idNormOperationMakeWork)
                            {
                                orders[itemIndex].workTime = Convert.ToInt32(sqlReader["normtime"]);
                                orders[itemIndex].amountOfOrder = Convert.ToInt32(sqlReader["plan_out_qty"]);
                            }

                            if (orders[orders.Count - 1].IDManPlanJob != idManPlanJob)
                            {
                                //AddOrderToListView(itemIndex, orders[itemIndex], token);
                                lastItemIndex = itemIndex;
                            }
                        }
                        else
                        {
                            int itemIndex = orders.FindIndex((v) => v.IDManPlanJob == idManPlanJob);

                            if (itemIndex == -1)
                            {
                                orders.Add(new OrdersLoad(
                                        1,
                                        idManPlanJob,
                                        sqlReader["date_begin"].ToString(),
                                        sqlReader["date_end"].ToString(),
                                        "",
                                        "",
                                        sqlReader["idletime_name"].ToString(),
                                        0,
                                        Convert.ToInt32(sqlReader["normtime"]),
                                        0,
                                        "",
                                        "",
                                        idManOrderJobItem
                                    ));

                                itemIndex = orders.Count - 1;
                            }

                            //AddOrderToListView(itemIndex, orders[itemIndex], token);
                        }

                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                    }

                    for (int i = 0; i < orders.Count; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        AddOrderToListView(i, orders[i], token);
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                LogException.WriteLine(ex.Message);
            }
        }

        /*private async void LoadPlanOLD(CancellationToken token)
        {
            ValueCategory valueCategory = new ValueCategory();
            ValueInfoBase valueInfo = new ValueInfoBase();

            Invoke(new Action(() =>
            {
                orders.Clear();
                orderNumbers.Clear();
                listView1.Items.Clear();
            }));

            string category = await valueInfo.GetCategoryMachine(loadMachine);

            string idNormOperationMakeReady = valueCategory.GetMKIDNormOperation(category);
            string idNormOperationMakeWork = valueCategory.GetWKIDNormOperation(category);
            string idMachine = await valueInfo.GetIDEquipMachine(loadMachine);

            string endDate = DateTime.Now.AddMonths(-6).ToString();

            List<string> orderItemsList = new List<string>();
            //либо оставить так, либо сделать через класс
            List<string> statusOrderList = new List<string>();

            string connectionString = @"Data Source = SRV-ACS\DSACS; Initial Catalog = asystem; Persist Security Info = True; User ID = ds; Password = 1";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand Command = new SqlCommand
                {
                    Connection = connection,
                    //CommandText = @"SELECT * FROM dbo.order_head WHERE status = '1' AND order_num LIKE '@order_num'"
                    //CommandText = @"SELECT * FROM dbo.man_planjob WHERE ((status <> '2') AND (id_equip = @idMachine AND date_end > @dateEnd))"
                    CommandText = @"SELECT * FROM dbo.man_planjob WHERE ((status <> '2') AND (id_equip = @idMachine AND flags <> '1'))"
                };
                Command.Parameters.AddWithValue("@idMachine", idMachine);
                Command.Parameters.AddWithValue("@dateEnd", endDate);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    orderItemsList.Add(sqlReader["id_man_order_job_item"].ToString());
                    //выше описано
                    statusOrderList.Add(sqlReader["status"].ToString());
                }

                connection.Close();
            }

            for (int i = 0; i < orderItemsList.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                string orderHead = "";
                string orderNumber = "";
                string nameCustomer = "";

                string itemOrder = "";

                List<int> mkNormTime = new List<int>();
                List<int> wkNormTime = new List<int>();
                List<int> amounts = new List<int>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = connection,
                        //CommandText = @"SELECT * FROM dbo.order_head WHERE status = '1' AND order_num LIKE '@order_num'"
                        //CommandText = @"SELECT * FROM dbo.order_head WHERE (status = '1' AND order_num LIKE '%" + searchNumber + "%')"

                        CommandText = @"SELECT id_order_head, order_num, id_customer FROM dbo.order_head WHERE id_order_head IN (
                            SELECT id_order_head FROM dbo.man_order_job WHERE id_man_order_job IN (
                            SELECT id_man_order_job FROM dbo.man_order_job_item WHERE id_man_order_job_item = @orderItems))"
                    };
                    Command.Parameters.AddWithValue("@orderItems", orderItemsList[i]);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        orderHead = sqlReader["id_order_head"].ToString();
                        orderNumber = sqlReader["order_num"].ToString();
                        nameCustomer = GetCustomerNameFromID(sqlReader["id_customer"].ToString());
                    }

                    connection.Close();
                }

                string stamp = await GetStampFromOrderNumber(orderNumber);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = connection,
                        //CommandText = @"SELECT * FROM dbo.order_head WHERE status = '1' AND order_num LIKE '@order_num'"
                        //CommandText = @"SELECT * FROM dbo.order_head WHERE (status = '1' AND order_num LIKE '%" + searchNumber + "%')"

                        CommandText = @"SELECT detail_name FROM dbo.order_detail WHERE id_order_detail IN (
                            SELECT itemid FROM dbo.man_order_job_item WHERE id_man_order_job_item = @orderItems)"
                    };
                    Command.Parameters.AddWithValue("@orderItems", orderItemsList[i]);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        ///////
                        itemOrder = sqlReader["detail_name"].ToString();
                    }

                    connection.Close();
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = connection,

                        CommandText = @"SELECT id_norm_operation, plan_out_qty, normtime FROM dbo.man_planjob_list WHERE id_man_order_job_item = @orderItems"
                    };
                    Command.Parameters.AddWithValue("@orderItems", orderItemsList[i]);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        if (sqlReader["id_norm_operation"].ToString() == idNormOperationMakeReady)
                        {
                            mkNormTime.Add(Convert.ToInt32(sqlReader["normtime"]) / Convert.ToInt32(sqlReader["plan_out_qty"]));
                        }

                        if (sqlReader["id_norm_operation"].ToString() == idNormOperationMakeWork)
                        {
                            wkNormTime.Add(Convert.ToInt32(sqlReader["normtime"]));
                            amounts.Add(Convert.ToInt32(sqlReader["plan_out_qty"]));
                        }
                    }
                    connection.Close();
                }

                if (mkNormTime.Count == wkNormTime.Count)
                {
                    for (int j = 0; j < mkNormTime.Count; j++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        orders.Add(new OrdersLoad(
                            orderNumber,
                            nameCustomer,
                            itemOrder,
                            mkNormTime[j],
                            wkNormTime[j],
                            amounts[j],
                            stamp,
                            orderHead
                        ));

                        int index = orders.Count - 1;
                        AddOrderToListView(index, orders[index], token);
                    }
                }

                if (mkNormTime.Count > wkNormTime.Count)
                {
                    for (int j = 0; j < mkNormTime.Count; j++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        if (j < wkNormTime.Count)
                        {
                            orders.Add(new OrdersLoad(
                                orderNumber,
                                nameCustomer,
                                itemOrder,
                                mkNormTime[j],
                                wkNormTime[j],
                                amounts[j],
                                stamp,
                                orderHead
                            ));

                            int index = orders.Count - 1;
                            AddOrderToListView(index, orders[index], token);
                        }
                        else
                        {
                            orders.Add(new OrdersLoad(
                                orderNumber,
                                nameCustomer,
                                itemOrder,
                                mkNormTime[j],
                                0,
                                0,
                                stamp,
                                orderHead
                            ));

                            int index = orders.Count - 1;
                            AddOrderToListView(index, orders[index], token);
                        }
                    }
                }

                if (mkNormTime.Count < wkNormTime.Count)
                {
                    for (int j = 0; j < wkNormTime.Count; j++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        if (j < mkNormTime.Count)
                        {
                            orders.Add(new OrdersLoad(
                                orderNumber,
                                nameCustomer,
                                itemOrder,
                                mkNormTime[j],
                                wkNormTime[j],
                                amounts[j],
                                stamp,
                                orderHead
                            ));

                            int index = orders.Count - 1;
                            AddOrderToListView(index, orders[index], token);
                        }
                        else
                        {
                            orders.Add(new OrdersLoad(
                                orderNumber,
                                nameCustomer,
                                itemOrder,
                                0,
                                wkNormTime[j],
                                amounts[j],
                                stamp,
                                orderHead
                            ));

                            int index = orders.Count - 1;
                            AddOrderToListView(index, orders[index], token);
                        }
                    }
                }
            }

            for (int i = 0; i < orders.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                *//*ListViewItem item = new ListViewItem();

                item.Name = i.ToString();
                item.Text = (i + 1).ToString();
                item.SubItems.Add(orders[i].numberOfOrder.ToString());
                item.SubItems.Add(orders[i].nameCustomer.ToString());
                item.SubItems.Add(orders[i].nameItem.ToString());
                item.SubItems.Add(timeOperations.MinuteToTimeString(orders[i].makereadyTime));
                item.SubItems.Add(timeOperations.MinuteToTimeString(orders[i].workTime));
                item.SubItems.Add(orders[i].amountOfOrder.ToString("N0"));
                item.SubItems.Add(orders[i].stamp.ToString());
                //item.SubItems.Add(statusOrderList[i].ToString());

                Invoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                }));*//*

            }
        }*/

        private void AddOrderToListView(int index, OrdersLoad order, CancellationToken token)
        {
            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            ListViewItem item = new ListViewItem();

            item.Name = index.ToString();
            item.Text = (index + 1).ToString();
            item.SubItems.Add(order.TimeStartOrder.ToString());
            item.SubItems.Add(order.TimeEndOrder.ToString());
            item.SubItems.Add(order.numberOfOrder.ToString());
            item.SubItems.Add(order.nameCustomer.ToString());
            item.SubItems.Add(order.nameItem.ToString());
            item.SubItems.Add(timeOperations.MinuteToTimeString(order.makereadyTime));
            item.SubItems.Add(timeOperations.MinuteToTimeString(order.workTime));
            item.SubItems.Add(order.amountOfOrder.ToString("N0"));
            item.SubItems.Add(order.stamp.ToString());

            try
            {
                Invoke(new Action(() =>
                {
                    if (!token.IsCancellationRequested)
                    {
                        listView1.Items.Add(item);

                        label1.Text = $"Загружено заказов: {index + 1}";
                    }
                }));
            }
            catch (Exception ex)
            {
                LogException.WriteLine(ex.Message);
            }
        }

        private void calculateCountProductionFromPreviousOperations(int idManPlanjobList)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //LoadPlan();

            StartLoading();
        }

        private List<string> LoadItemsFromOrder(string headID)
        {
            List<string> items = new List<string>();

            string connectionString = @"Data Source = SRV-ACS\DSACS; Initial Catalog = asystem; Persist Security Info = True; User ID = ds; Password = 1";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand Command = new SqlCommand
                {
                    Connection = connection,
                    //CommandText = @"SELECT * FROM dbo.order_head WHERE status = '1' AND order_num LIKE '@order_num'"
                    CommandText = @"SELECT * FROM dbo.order_detail WHERE (id_order_head = @headID)"
                };
                Command.Parameters.AddWithValue("@headID", headID);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    items.Add(sqlReader["detail_name"].ToString());
                    items.Add(sqlReader["tir"].ToString());
                }

                connection.Close();
            }

            return items;
        }

        private bool newValue = false;
        OrdersLoad setNewVal;
        private List<string> typesNewValue;

        public bool NewValue
        {
            get
            {
                return newValue;
            }
            set
            {
                newValue = value;
            }
        }

        public OrdersLoad SetValue
        {
            get
            {
                return setNewVal;
            }
            set
            {
                setNewVal = value;
            }
        }

        public List<string> Types
        {
            get
            {
                return typesNewValue;
            }
            set
            {
                typesNewValue = value;
            }
        }

        private void SendSelectedOrder(int index)
        {
            if (orders[index].TypeJob == 0)
            {
                NewValue = true;

                SetValue = orders[index];
                Types = LoadItemsFromOrder(orders[index].headOrder);
            }
            else
            {
                NewValue = true;

                SetValue = orders[index];
            }

            
            //сделать загрузку видов
        }

        private void Cancel()
        {
            NewValue = false;

            cancelTokenSource?.Cancel();
            //Types.Clear();
        }

        private async void FormLoadOrders_Load(object sender, EventArgs e)
        {
            if (_loadForViewOrders)
            {
                button3.Visible = false;
                button4.Text = "Выход";
            }
            else
            {
                button3.Enabled = false;
                button3.Visible = true;
                button4.Text = "Отмена";
            }

            await LoadMachine();
            StartLoading();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                button3.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (!_loadForViewOrders)
            {
                if (listView1.SelectedIndices.Count > 0)
                {
                    SendSelectedOrder(Convert.ToInt32(listView1.SelectedItems[0].Name));
                    Close();
                }
            }
            else
            {
                
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                SendSelectedOrder(Convert.ToInt32(listView1.SelectedItems[0].Name));
                Close();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Cancel();
            Close();
        }

        private void listView1_Click(object sender, EventArgs e)
        {

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

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                StartSearch(textBox1.Text);
            }
        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ValueInfoBase infoBase = new ValueInfoBase();

            if (_loadForViewOrders)
            {
                cancelTokenSource?.Cancel();

                loadMachine = await infoBase.GetMachineFromName(comboBox1.Text);

                StartLoading();
            }
        }

        private void FormLoadOrders_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancelTokenSource?.Cancel();
            Thread.Sleep(200);
        }
    }
}
