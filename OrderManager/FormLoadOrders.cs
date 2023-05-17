using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OrderManager
{
    public partial class FormLoadOrders : Form
    {
        string loadMachine;

        public FormLoadOrders(string lMachine)
        {
            InitializeComponent();

            this.loadMachine = lMachine;
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

        private string GetCustomerNameFromID(string id)
        {
            string result = "";

            string connectionString = @"Data Source = SRV-ACS\DSACS; Initial Catalog = asystem; Persist Security Info = True; User ID = ds; Password = 1";

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

            return result;
        }

        private string GetStampFromOrderNumber(string searchNumber)
        {
            string result = "";

            int nOrderId = 0;
            int normOperation = 12;
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

            result = str.Substring(startIndex, endIndex - startIndex);
            result = result.Replace(" ", "");

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadOrdersByNumber(textBox1.Text);
        }

        private void LoadOrdersByNumber(string searchNumber)
        {
            ValueCategory valueCategory = new ValueCategory();
            ValueInfoBase valueInfo = new ValueInfoBase();

            orders.Clear();
            orderNumbers.Clear();
            listView1.Items.Clear();

            string category = valueInfo.GetCategoryMachine(loadMachine);

            string idNormOperation = valueCategory.GetMainIDNormOperation(category);
            string idNormOperationMakeReady = valueCategory.GetMKIDNormOperation(category);
            string idNormOperationMakeWork = valueCategory.GetWKIDNormOperation(category);

            List<string> orderHeadList = new List<string>();

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
                    orderHeadList.Add(sqlReader["id_order_head"].ToString());

                    orderNumbers.Add(new OrderLoadNumber(
                        sqlReader["order_num"].ToString(),
                        GetCustomerNameFromID(sqlReader["id_customer"].ToString())
                        ));
                }

                connection.Close();
            }

            for (int i = 0; i < orderHeadList.Count; i++)
            {
                List<string> jobItem = new List<string>();
                List<string> itemID = new List<string>();

                string stamp = GetStampFromOrderNumber(orderNumbers[i].numberOfOrder);

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
                        ///////
                        jobItem.Add(sqlReader["id_man_order_job_item"].ToString());
                        itemID.Add(sqlReader["itemid"].ToString());
                    }
                    //MessageBox.Show(itemOrder.Count.ToString());
                    connection.Close();
                }

                for(int k = 0; k < jobItem.Count; k++)
                {
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
                            ///////
                            itemOrder.Add(sqlReader["detail_name"].ToString());
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
                            if (sqlReader["id_norm_operation"].ToString() == idNormOperationMakeReady)
                            {
                                if (sqlReader["normtime"] != null)
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
                                orderHeadList[i]
                            ));
                        }
                    }

                    if (mkNormTime.Count > wkNormTime.Count)
                    {
                        for (int j = 0; j < mkNormTime.Count; j++)
                        {
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
                                    orderHeadList[i]
                                ));
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
                                    orderHeadList[i]
                                ));
                            }


                        }
                    }

                    if (mkNormTime.Count < wkNormTime.Count)
                    {
                        for (int j = 0; j < wkNormTime.Count; j++)
                        {
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
                                    orderHeadList[i]
                                ));
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
                                    orderHeadList[i]
                                ));
                            }
                        }
                    }
                }
            }

            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            for (int i = 0; i < orders.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = i.ToString();
                item.Text = (i + 1).ToString();
                item.SubItems.Add(orders[i].numberOfOrder.ToString());
                item.SubItems.Add(orders[i].nameCustomer.ToString());
                item.SubItems.Add(orders[i].nameItem.ToString());
                item.SubItems.Add(timeOperations.MinuteToTimeString(orders[i].makereadyTime));
                item.SubItems.Add(timeOperations.MinuteToTimeString(orders[i].workTime));
                item.SubItems.Add(orders[i].amountOfOrder.ToString("N0"));
                item.SubItems.Add(orders[i].stamp.ToString());

                listView1.Items.Add(item);
            }
        }

        private void LoadPlan()
        {
            ValueCategory valueCategory = new ValueCategory();
            ValueInfoBase valueInfo = new ValueInfoBase();

            orders.Clear();
            orderNumbers.Clear();
            listView1.Items.Clear();

            string category = valueInfo.GetCategoryMachine(loadMachine);

            string idNormOperationMakeReady = valueCategory.GetMKIDNormOperation(category);
            string idNormOperationMakeWork = valueCategory.GetWKIDNormOperation(category);
            string idMachine = valueInfo.GetIDEquipMachine(loadMachine);

            string endDate = DateTime.Now.AddYears(-1).ToString();

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
                    CommandText = @"SELECT * FROM dbo.man_planjob WHERE ((status <> '2') AND (id_equip = @idMachine AND date_end > @dateEnd))"
                };
                Command.Parameters.AddWithValue("@idMachine", idMachine);
                Command.Parameters.AddWithValue("@dateEnd", endDate);

                DbDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    orderItemsList.Add(sqlReader["id_man_order_job_item"].ToString());
                    //выше описано
                    statusOrderList.Add(sqlReader["status"].ToString());
                }

                connection.Close();
            }

            for (int i = 0; i < orderItemsList.Count; i++)
            {
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
                        orderHead = sqlReader["id_order_head"].ToString();
                        orderNumber = sqlReader["order_num"].ToString();
                        nameCustomer = GetCustomerNameFromID(sqlReader["id_customer"].ToString());
                    }

                    connection.Close();
                }

                string stamp = GetStampFromOrderNumber(orderNumber);

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
                        if (sqlReader["id_norm_operation"].ToString() == idNormOperationMakeReady)
                        {
                            mkNormTime.Add(Convert.ToInt32(sqlReader["normtime"]));
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
                    }
                }

                if (mkNormTime.Count > wkNormTime.Count)
                {
                    for (int j = 0; j < mkNormTime.Count; j++)
                    {
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
                        }
                    }
                }

                if (mkNormTime.Count < wkNormTime.Count)
                {
                    for (int j = 0; j < wkNormTime.Count; j++)
                    {
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
                        }
                    }
                }
            }

            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            for (int i = 0; i < orders.Count; i++)
            {
                ListViewItem item = new ListViewItem();

                item.Name = i.ToString();
                item.Text = (i + 1).ToString();
                item.SubItems.Add(orders[i].numberOfOrder.ToString());
                item.SubItems.Add(orders[i].nameCustomer.ToString());
                item.SubItems.Add(orders[i].nameItem.ToString());
                item.SubItems.Add(timeOperations.MinuteToTimeString(orders[i].makereadyTime));
                item.SubItems.Add(timeOperations.MinuteToTimeString(orders[i].workTime));
                item.SubItems.Add(orders[i].amountOfOrder.ToString("N0"));
                item.SubItems.Add(orders[i].stamp.ToString());

                listView1.Items.Add(item);
            }
        }





        CancellationTokenSource cancelTokenSource;
        private void StartLoading()
        {
            if (cancelTokenSource != null)
            {
                cancelTokenSource.Cancel();
            }

            cancelTokenSource = new CancellationTokenSource();

            Task task = new Task(() => LoadPlan2(cancelTokenSource.Token), cancelTokenSource.Token);
            task.Start();
        }

        private void LoadPlan2(CancellationToken token)
        {
            ValueCategory valueCategory = new ValueCategory();
            ValueInfoBase valueInfo = new ValueInfoBase();

            Invoke(new Action(() =>
            {
                orders.Clear();
                orderNumbers.Clear();
                listView1.Items.Clear();
            }));

            string category = valueInfo.GetCategoryMachine(loadMachine);

            string idNormOperationMakeReady = valueCategory.GetMKIDNormOperation(category);
            string idNormOperationMakeWork = valueCategory.GetWKIDNormOperation(category);
            string idMachine = valueInfo.GetIDEquipMachine(loadMachine);

            string endDate = DateTime.Now.AddYears(-1).ToString();

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
                    CommandText = @"SELECT * FROM dbo.man_planjob WHERE ((status <> '2') AND (id_equip = @idMachine AND date_end > @dateEnd))"
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

                string stamp = GetStampFromOrderNumber(orderNumber);

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
                            mkNormTime.Add(Convert.ToInt32(sqlReader["normtime"]));
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
                    }
                }

                if (mkNormTime.Count > wkNormTime.Count)
                {
                    for (int j = 0; j < mkNormTime.Count; j++)
                    {
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
                        }
                    }
                }

                if (mkNormTime.Count < wkNormTime.Count)
                {
                    for (int j = 0; j < wkNormTime.Count; j++)
                    {
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
                        }
                    }
                }
            }

            GetDateTimeOperations timeOperations = new GetDateTimeOperations();

            for (int i = 0; i < orders.Count; i++)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                ListViewItem item = new ListViewItem();

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
                }));

                
            }

            Invoke(new Action(() =>
            {

            }));
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
            NewValue = true;

            SetValue = orders[index];
            Types = LoadItemsFromOrder(orders[index].headOrder);
            //сделать загрузку видов
        }

        private void Cancel()
        {
            NewValue = false;
            //Types.Clear();
        }

        private void FormLoadOrders_Load(object sender, EventArgs e)
        {
            button3.Enabled = false;
            //LoadPlan();
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
            if (listView1.SelectedIndices.Count > 0)
            {
                SendSelectedOrder(Convert.ToInt32(listView1.SelectedItems[0].Name));
                Close();
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
                LoadOrdersByNumber(textBox1.Text);
            }
        }
    }
}
