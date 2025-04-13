using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OrderManager
{
    class GetOrderOperations
    {
        public GetOrderOperations()
        {

        }
        private class DoneSumm
        {
            public int id;
            public int summ;

            public DoneSumm()
            {

            }
            public DoneSumm(int id, int done)
            {
                this.id = id;
                this.summ = done;
            }
        }
        public async Task<List<LoadShift>> OperationsForOrder(List<LoadShift> shifts, int loadIdManOrderJobItem = -1)
        {
            List<LoadShift> loadShifts = shifts;

            ValueShiftsBase valueShifts = new ValueShiftsBase();
            ValueInfoBase valueInfo = new ValueInfoBase();

            List<DoneSumm> doneSumms = new List<DoneSumm>();

            try
            {
                for (int i = 0; i < loadShifts.Count; i++)
                {
                    LoadShift shift = loadShifts[i];
                    int idFbcBrigade = shift.IDFbcBrigade;

                    using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                    {
                        await Connect.OpenAsync();
                        SqlCommand Command = new SqlCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT
	                                        fbc_brigade.id_fbc_brigade, 
	                                        man_planjob_list.id_man_order_job_item, 
	                                        norm_operation_table.ord AS operation_type, 
	                                        man_planjob_list.id_norm_operation, 
	                                        man_planjob_list.plan_out_qty, 
	                                        man_planjob_list.normtime, 
	                                        man_factjob.date_begin, 
	                                        man_factjob.date_end, 
	                                        man_factjob.duration, 
	                                        man_factjob.fact_out_qty, 
	                                        man_factjob.id_equip, 
	                                        man_factjob.norm_time, 
	                                        common_equip_directory.equip_name
                                        FROM
	                                        dbo.man_factjob
	                                        INNER JOIN
	                                        dbo.man_planjob_list
	                                        ON 
		                                        man_factjob.id_man_planjob_list = man_planjob_list.id_man_planjob_list
	                                        INNER JOIN
	                                        dbo.norm_operation_table
	                                        ON 
		                                        man_planjob_list.id_norm_operation = norm_operation_table.id_norm_operation
	                                        INNER JOIN
	                                        dbo.fbc_brigade
	                                        ON 
		                                        (
			                                        fbc_brigade.date_end IS NOT NULL AND
			                                        man_factjob.date_begin >= fbc_brigade.date_begin AND
			                                        man_factjob.date_begin <= fbc_brigade.date_end AND
			                                        man_factjob.id_common_employee = fbc_brigade.id_common_employee
		                                        )
	                                        INNER JOIN
	                                        dbo.common_equip_directory
	                                        ON 
		                                        man_factjob.id_equip = common_equip_directory.id_common_equip_directory
                                            WHERE
	                                            fbc_brigade.id_fbc_brigade = @idFbcBrigade
                                            ORDER BY
	                                            fbc_brigade.date_begin ASC, 
	                                            man_factjob.date_begin ASC, 
	                                            man_factjob.date_end ASC"
                        };
                        Command.Parameters.AddWithValue("@idFbcBrigade", idFbcBrigade);

                        DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                        int counter = 0;
                        int counterMK = 0;
                        int counterWK = 0;

                        int tmpIdManOrderJobItem = -1;

                        while (await sqlReader.ReadAsync())
                        {
                            //sqlReader["shift_no"] == DBNull.Value ? 0 : (int)Convert.ToInt32(sqlReader["shift_no"])
                            counter++;

                            int idManOrderJobItem = sqlReader["id_man_order_job_item"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_man_order_job_item"]);
                            int operationType = sqlReader["operation_type"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["operation_type"]);
                            int equipID = sqlReader["id_equip"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_equip"]);
                            string equipName = sqlReader["equip_name"] == DBNull.Value ? "" : sqlReader["equip_name"].ToString();

                            string operationBegin = sqlReader["date_begin"] == DBNull.Value ? "" : sqlReader["date_begin"].ToString();
                            string operationEnd = sqlReader["date_end"] == DBNull.Value ? "" : sqlReader["date_end"].ToString();

                            //string dateEnd = sqlReader["date_end"] == DBNull.Value ? "" : sqlReader["date_end"].ToString();

                            float factQty = sqlReader["fact_out_qty"] == DBNull.Value ? 0 : (float)Convert.ToDouble(sqlReader["fact_out_qty"]);

                            if (loadIdManOrderJobItem != -1)
                            {
                                if (loadIdManOrderJobItem != idManOrderJobItem)
                                {
                                    break;
                                }
                            }

                            if (tmpIdManOrderJobItem != idManOrderJobItem)
                            {
                                counterMK = 0;
                                counterWK = 0;
                            }

                            /*loadOrder.EquipID = equipID;
                            loadOrder.OrderNumber = orderNumber;
                            loadOrder.NameCustomer = nameCustomer;
                            loadOrder.MakereadyTime = makereadyTime;
                            loadOrder.WorkTime = workTime;

                            loadOrder.AmountOfOrder = amountOrder;*/

                            int itemIndex = loadShifts[i].Order.FindIndex((v) => v.IdManOrderJobItem == idManOrderJobItem);

                            if (itemIndex == -1)
                            {
                                loadShifts[i].Order.Add(await GetOrderFromIDAsync(idManOrderJobItem));

                                itemIndex = loadShifts[i].Order.Count - 1;

                                loadShifts[i].Order[itemIndex].EquipID = await valueInfo.GetMachineIndexFromIDEquip(equipID);
                                loadShifts[i].Order[itemIndex].EquipName = equipName;

                                loadShifts[i].Order[itemIndex].OrderOperations.Add(new LoadOrderOperations());
                            }

                            int itemOperationsIndex = loadShifts[i].Order[itemIndex].OrderOperations.Count - 1;
                            
                            if (itemOperationsIndex != -1)
                            {
                                int[] summPreview = await SummPreviewOperationsAll(idManOrderJobItem, operationBegin);
                                
                                if (operationType == 0)
                                {
                                    counterMK++;

                                    if (counterMK == 1)
                                    {
                                        //Console.WriteLine(loadShifts[i].Order[itemIndex].OrderNumber + ": " + loadShifts[i].Order[itemIndex].LastMakeready + " - " + summPreview[0]);
                                        loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStart = Convert.ToDateTime(operationBegin).ToString("HH:mm dd.MM.yyyy");
                                        loadShifts[i].Order[itemIndex].LastMakeready -= summPreview[0];
                                        //Console.WriteLine(loadShifts[i].Order[itemIndex].LastMakeready);
                                    }

                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStop = Convert.ToDateTime(operationEnd).ToString("HH:mm dd.MM.yyyy");
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyComplete += Convert.ToInt32(factQty * 100);
                                }

                                if (operationType == 1)
                                {
                                    counterWK++;

                                    if (counterWK == 1)
                                    {
                                        loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].WorkStart = Convert.ToDateTime(operationBegin).ToString("HH:mm dd.MM.yyyy");
                                        //loadShifts[i].Order[itemIndex].LastAmount = loadShifts[i].Order[itemIndex].AmountOfOrder - await SummPreviewOperations(idManOrderJobItem, operationBegin);
                                        loadShifts[i].Order[itemIndex].LastAmount -= summPreview[1];
                                    }

                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].WorkStop = Convert.ToDateTime(operationEnd).ToString("HH:mm dd.MM.yyyy");
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].Done += (int)factQty;
                                }                                
                            }


                            /*Console.WriteLine(i + ":: " + "<" + itemIndex + ">" + idFbcBrigade + " :: " + idManOrderJobItem + ": " + loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyComplete + "::: " +
                                loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].Done);
                            Console.WriteLine(i + ":: " + loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStart + " - " + loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStop + " ::: " +
                                loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].WorkStart + " - " + loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].WorkStop);*/

                            tmpIdManOrderJobItem = idManOrderJobItem;
                        }

                        Connect.Close();
                    }

                    for (int j = 0; j < loadShifts[i].Order.Count; j++)
                    {
                        //loadShifts[i].Order[j].IsOrderLoad = await IsNewOrderForSelectedShift(loadShifts[i], loadShifts[i].Order[j]);
                        loadShifts[i].Order[j] = await IsNewOrderForSelectedShift(loadShifts[i], loadShifts[i].Order[j]);

                        loadShifts[i].IsLoadShift = loadShifts[i].Order[j].IsOrderLoad;
                    }

                    for (int j = 0; j < loadShifts[i].Order.Count; j++)
                    {
                        if (loadShifts[i].Order[j].IsOrderLoad)
                        {
                            loadShifts[i].IsLoadShift = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                LogException.WriteLine(ex.Message);
            }

            return loadShifts;
        }

        private async Task<LoadOrder> IsNewOrderForSelectedShift(LoadShift shift, LoadOrder order)
        {
            LoadOrder result = order;

            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            ValueOrdersBase valueOrders = new ValueOrdersBase();

            result.IsOrderLoad = false;
            result.OrderOperations[0].OLDValueMakereadyComplete = -1;
            result.OrderOperations[0].OLDValueDone = -1;

            if (!shift.IsNewShift)
            {
                //LoadOrder order = shift.Order[shift.Order.Count - 1];

                int orderOMIndex = -1;

                if (order.IdManOrderJobItem > 0)
                {
                    orderOMIndex = valueOrders.GetOrderID(order.EquipID, order.OrderNumber, order.IdManOrderJobItem);
                }
                else
                {
                    orderOMIndex = valueOrders.GetOrderID(order.EquipID, order.OrderNumber, order.ItemOrder);
                }
                    
                //MessageBox.Show(orderOMIndex + ": " + order.EquipID.ToString() + "; " + order.OrderNumber + " - " + order.ItemOrder);
                if (orderOMIndex == -1)
                {
                    result.IsOrderLoad = true;
                    result.OrderOperations[0].OrderOperationID = -1;
                }
                else
                {
                    int shiftOMIndex = shift.IndexOMShift;

                    LoadOrderOperations orderOperations = await getOrders.LoadOrdersOperation(shiftOMIndex, orderOMIndex);
                    
                    if (orderOperations.OrderOperationID != -1)
                    {
                        if (orderOperations.MakereadyComplete != order.OrderOperations[0].MakereadyComplete)
                        {
                            result.IsOrderLoad = true;
                            result.OrderOperations[0].OLDValueMakereadyComplete = orderOperations.MakereadyComplete;
                        }
                        if (orderOperations.Done != order.OrderOperations[0].Done)
                        {
                            result.IsOrderLoad = true;
                            result.OrderOperations[0].OLDValueDone = orderOperations.Done;
                        }
                    }
                    else
                    {
                        result.IsOrderLoad = true;
                    }

                    result.OrderOperations[0].OrderOperationID = orderOperations.OrderOperationID;
                }

                result.OrderOMIndex = orderOMIndex;
            }
            else
            {
                result.IsOrderLoad = true;
                result.OrderOMIndex = -1;
                result.OrderOperations[0].OrderOperationID = -1;
            }

            return result;
        }

        private async Task<bool> IsNewOrderForSelectedShiftOLD(LoadShift shift, LoadOrder order)
        {
            bool result = false;

            GetOrdersFromBase getOrders = new GetOrdersFromBase();
            ValueOrdersBase valueOrders = new ValueOrdersBase();

            if (!shift.IsNewShift)
            {
                //LoadOrder order = shift.Order[shift.Order.Count - 1];

                int orderOMIndex = -1;

                if (order.IdManOrderJobItem > 0)
                {
                    orderOMIndex = valueOrders.GetOrderID(order.EquipID, order.OrderNumber, order.IdManOrderJobItem);
                }
                else
                {
                    orderOMIndex = valueOrders.GetOrderID(order.EquipID, order.OrderNumber, order.ItemOrder);
                }
                //MessageBox.Show(orderOMIndex + ": " + order.EquipID.ToString() + "; " + order.OrderNumber + " - " + order.ItemOrder);
                if (orderOMIndex == -1)
                {
                    result = true;
                }
                else
                {
                    int shiftOMIndex = shift.IndexOMShift;
                    LoadOrderOperations orderOperations = await getOrders.LoadOrdersOperation(shiftOMIndex, orderOMIndex);
                    //MessageBox.Show(orderOperations.MakereadyComplete + " != " + order.OrderOperations[0].MakereadyComplete + " || " + orderOperations.Done + " != " + order.OrderOperations[0].Done);
                    if (orderOperations.MakereadyComplete != order.OrderOperations[0].MakereadyComplete || orderOperations.Done != order.OrderOperations[0].Done)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            else
            {
                result = true;
            }

            return result;
        }

        /*public List<LoadShift> OperationsForOrder(List<LoadShift> shifts, int loadIdManOrderJobItem = -1)
        {
            List<LoadShift> loadShifts = shifts;

            try
            {
                for (int i = 0; i < loadShifts.Count; i++)
                {
                    LoadShift shift = loadShifts[i];
                    int idFbcBrigade = shift.IDFbcBrigade;

                    using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                    {
                        Connect.OpenAsync();
                        SqlCommand Command = new SqlCommand
                        {
                            Connection = Connect,
                            CommandText = @"SELECT
	                                        fbc_brigade.id_fbc_brigade, 
	                                        man_planjob_list.id_man_order_job_item, 
	                                        order_head.order_num, 
	                                        common_ul_directory.ul_name, 
	                                        norm_operation_table.ord AS operation_type, 
	                                        man_planjob_list.id_norm_operation, 
	                                        man_planjob_list.plan_out_qty, 
	                                        man_planjob_list.normtime, 
	                                        man_factjob.date_begin, 
	                                        man_factjob.date_end, 
	                                        man_factjob.duration, 
	                                        man_factjob.fact_out_qty, 
	                                        man_factjob.id_equip, 
	                                        man_factjob.norm_time
                                        FROM
	                                        dbo.man_factjob
	                                        INNER JOIN
	                                        dbo.man_planjob_list
	                                        ON 
		                                        man_factjob.id_man_planjob_list = man_planjob_list.id_man_planjob_list
	                                        INNER JOIN
	                                        dbo.man_order_job_item
	                                        ON 
		                                        man_planjob_list.id_man_order_job_item = man_order_job_item.id_man_order_job_item
	                                        INNER JOIN
	                                        dbo.man_order_job
	                                        ON 
		                                        man_order_job_item.id_man_order_job = man_order_job.id_man_order_job
	                                        INNER JOIN
	                                        dbo.norm_operation_table
	                                        ON 
		                                        man_planjob_list.id_norm_operation = norm_operation_table.id_norm_operation
	                                        INNER JOIN
	                                        dbo.fbc_brigade
	                                        ON 
		                                        (
			                                        fbc_brigade.date_end IS NOT NULL AND
			                                        man_factjob.date_begin >= fbc_brigade.date_begin AND
			                                        man_factjob.date_begin <= fbc_brigade.date_end AND
			                                        man_factjob.id_common_employee = fbc_brigade.id_common_employee
		                                        )
	                                        INNER JOIN
	                                        dbo.order_head
	                                        ON 
		                                        man_order_job.id_order_head = order_head.id_order_head
	                                        INNER JOIN
	                                        dbo.common_ul_directory
	                                        ON 
		                                        order_head.id_customer = common_ul_directory.id_common_ul_directory
                                        WHERE
	                                        fbc_brigade.id_fbc_brigade = @idFbcBrigade
                                        ORDER BY
	                                        fbc_brigade.date_begin ASC, 
	                                        man_factjob.date_begin ASC, 
	                                        man_factjob.date_end ASC"
                        };
                        Command.Parameters.AddWithValue("@idFbcBrigade", idFbcBrigade);

                        DbDataReader sqlReader = Command.ExecuteReader();

                        int counter = 0;
                        int counterMK = 0;
                        int counterWK = 0;

                        int makereadyTime = 0;
                        int workTime = 0;
                        int amountOrder = 0;

                        int makereadyComplete = 0;
                        int done = 0;

                        string makereadyStart = "";
                        string makereadyStop = "";
                        string workStart = "";
                        string workStop = "";

                        int tmpIdManOrderJobItem = -1;

                        while (sqlReader.Read())
                        {
                            //sqlReader["shift_no"] == DBNull.Value ? 0 : (int)Convert.ToInt32(sqlReader["shift_no"])
                            counter++;

                            int idManOrderJobItem = sqlReader["id_man_order_job_item"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_man_order_job_item"]);
                            int equipID = sqlReader["id_equip"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_equip"]);
                            string orderNumber = sqlReader["order_num"] == DBNull.Value ? string.Empty : sqlReader["order_num"].ToString();
                            string nameCustomer = sqlReader["ul_name"] == DBNull.Value ? string.Empty : sqlReader["ul_name"].ToString();
                            int operationType = sqlReader["operation_type"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["operation_type"]);
                            int normTime = sqlReader["id_norm_operation"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_norm_operation"]);
                            int planQty = sqlReader["plan_out_qty"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["plan_out_qty"]);

                            string operationBegin = sqlReader["date_begin"] == DBNull.Value ? string.Empty : sqlReader["date_begin"].ToString();
                            string operationEnd = sqlReader["date_end"] == DBNull.Value ? string.Empty : sqlReader["date_end"].ToString();

                            float factQty = sqlReader["fact_out_qty"] == DBNull.Value ? 0 : (float)Convert.ToDouble(sqlReader["fact_out_qty"]);

                            if (loadIdManOrderJobItem != -1)
                            {
                                if (loadIdManOrderJobItem != idManOrderJobItem)
                                {
                                    break;
                                }
                            }

                            if (tmpIdManOrderJobItem != idManOrderJobItem)
                            {
                                counterMK = 0;
                                counterWK = 0;

                                makereadyTime = 0;
                                workTime = 0;
                                amountOrder = 0;

                                makereadyComplete = 0;
                                done = 0;

                                makereadyStart = "";
                                makereadyStop = "";
                                workStart = "";
                                workStop = "";
                            }

                            if (operationType == 0)
                            {
                                counterMK++;

                                makereadyTime = normTime / planQty;

                                if (counterMK == 1)
                                {
                                    makereadyStart = operationBegin;
                                }

                                makereadyStop = operationEnd;
                                makereadyComplete = Convert.ToInt32(factQty * 100);
                            }

                            if (operationType == 1)
                            {
                                counterWK++;

                                workTime = normTime;

                                if (counterWK == 1)
                                {
                                    workStart = operationBegin;
                                }

                                workStop = operationEnd;
                                amountOrder = planQty;
                                done = (int)factQty;
                            }

                            *//*loadOrder.EquipID = equipID;
                            loadOrder.OrderNumber = orderNumber;
                            loadOrder.NameCustomer = nameCustomer;
                            loadOrder.MakereadyTime = makereadyTime;
                            loadOrder.WorkTime = workTime;

                            loadOrder.AmountOfOrder = amountOrder;*//*

                            int itemIndex = loadShifts[i].Order.FindIndex((v) => v.IdManOrderJobItem == idManOrderJobItem);
                            int itemOperationsIndex = -1;

                            if (itemIndex == -1)
                            {
                                loadShifts[i].Order.Add(new LoadOrder());

                                itemIndex = loadShifts[i].Order.Count - 1;

                                loadShifts[i].Order[itemIndex].IdManOrderJobItem = idManOrderJobItem;
                                loadShifts[i].Order[itemIndex].EquipID = equipID;
                                loadShifts[i].Order[itemIndex].OrderNumber = orderNumber;
                                loadShifts[i].Order[itemIndex].NameCustomer = nameCustomer;
                                loadShifts[i].Order[itemIndex].MakereadyTime = makereadyTime;
                                loadShifts[i].Order[itemIndex].WorkTime = workTime;
                                loadShifts[i].Order[itemIndex].AmountOfOrder = amountOrder;

                                loadShifts[i].Order[itemIndex].OrderOperations.Add(new LoadOrderOperations(
                                    makereadyStart,
                                    makereadyStop,
                                    workStart,
                                    workStop,
                                    makereadyComplete,
                                    done
                                    ));
                            }
                            else
                            {
                                loadShifts[i].Order[itemIndex].MakereadyTime = makereadyTime;
                                loadShifts[i].Order[itemIndex].WorkTime = workTime;
                                loadShifts[i].Order[itemIndex].AmountOfOrder = amountOrder;

                                itemOperationsIndex = loadShifts[i].Order[itemIndex].OrderOperations.Count - 1;

                                if (itemOperationsIndex != -1)
                                {
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStart = makereadyStart;
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStop = makereadyStop;
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].WorkStart = workStart;
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].WorkStop = workStop;
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyComplete += makereadyComplete;
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].Done += done;
                                }
                            }

                            tmpIdManOrderJobItem = idManOrderJobItem;
                        }

                        Connect.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                LogException.WriteLine(ex.Message);
            }

            return loadShifts;
        }*/

        public async Task<List<LoadShift>> ShiftListForOrderAsync(int idManOrderJobItem)
        {
            List<LoadShift> loadShifts = new List<LoadShift>();

            try
            {
                using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                {
                    await Connect.OpenAsync();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT
	                                        fbc_brigade.id_fbc_brigade, 
	                                        man_factjob.id_common_employee AS user_id, 
	                                        fbc_brigade.date_begin AS shift_begin, 
	                                        fbc_brigade.date_end AS shift_end, 
	                                        fbc_brigade.shift_no
                                        FROM
	                                        dbo.man_factjob
	                                        INNER JOIN
	                                        dbo.man_planjob_list
	                                        ON 
		                                        man_factjob.id_man_planjob_list = man_planjob_list.id_man_planjob_list
	                                        INNER JOIN
	                                        dbo.fbc_brigade
	                                        ON 
		                                        (
			                                        fbc_brigade.date_end IS NOT NULL AND
			                                        man_factjob.date_begin >= fbc_brigade.date_begin AND
			                                        man_factjob.date_begin <= fbc_brigade.date_end AND
			                                        man_factjob.id_common_employee = fbc_brigade.id_common_employee
		                                        )
                                        WHERE
	                                        man_planjob_list.id_man_order_job_item = @idManOrderJobItem
                                        GROUP BY
                                          fbc_brigade.id_fbc_brigade, man_factjob.id_common_employee, fbc_brigade.date_begin, fbc_brigade.date_end, fbc_brigade.shift_no
                                        ORDER BY
	                                        fbc_brigade.date_begin ASC"
                    };
                    Command.Parameters.AddWithValue("@idManOrderJobItem", idManOrderJobItem);

                    DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                    while (await sqlReader.ReadAsync())
                    {
                        //sqlReader["shift_no"] == DBNull.Value ? 0 : (int)Convert.ToInt32(sqlReader["shift_no"])

                        int idFbcBrigade = sqlReader["id_fbc_brigade"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_fbc_brigade"]);
                        int userID = sqlReader["user_id"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["user_id"]);
                        string shiftBegin = sqlReader["shift_begin"] == DBNull.Value ? string.Empty : sqlReader["shift_begin"].ToString();
                        string shiftEnd = sqlReader["shift_end"] == DBNull.Value ? string.Empty : sqlReader["shift_end"].ToString();
                        int shiftNumber = sqlReader["shift_no"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["shift_no"]);

                        int itemIndex = loadShifts.FindIndex((v) => v.IDFbcBrigade == idFbcBrigade);

                        if (itemIndex == -1)
                        {
                            loadShifts.Add(new LoadShift());

                            itemIndex = loadShifts.Count - 1;

                            loadShifts[itemIndex].IDFbcBrigade = idFbcBrigade;
                            loadShifts[itemIndex].UserID = userID;
                            loadShifts[itemIndex].ShiftDate = Convert.ToDateTime(shiftBegin).ToString("dd.MM.yyyy");
                            loadShifts[itemIndex].ShiftNumber = shiftNumber;
                            loadShifts[itemIndex].ShiftStart = Convert.ToDateTime(shiftBegin).ToString("dd.MM.yyyy HH:mm:ss");
                            loadShifts[itemIndex].ShiftEnd = Convert.ToDateTime(shiftEnd).ToString("dd.MM.yyyy HH:mm:ss");
                        }
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                LogException.WriteLine(ex.Message);
            }

            return loadShifts;
        }

        private async Task<LoadOrder> GetOrderFromIDAsync(int idManOrderJobItem)
        {
            LoadOrder loadOrder = new LoadOrder();
            GetValueFromASBase valueFromASBase = new GetValueFromASBase();

            try
            {
                using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                {
                    await Connect.OpenAsync();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT
	                                        man_planjob_list.id_man_order_job_item, 
	                                        order_head.order_num, 
	                                        common_ul_directory.ul_name, 
	                                        norm_operation_table.ord AS operation_type, 
	                                        man_planjob_list.id_norm_operation, 
	                                        man_planjob_list.plan_out_qty, 
	                                        man_planjob_list.normtime, 
	                                        order_head.order_name, 
	                                        order_head.id_order_head, 
	                                        order_detail.detail_name
                                        FROM
	                                        dbo.man_planjob_list
	                                        INNER JOIN
	                                        dbo.man_order_job_item
	                                        ON 
		                                        man_planjob_list.id_man_order_job_item = man_order_job_item.id_man_order_job_item
	                                        INNER JOIN
	                                        dbo.man_order_job
	                                        ON 
		                                        man_order_job_item.id_man_order_job = man_order_job.id_man_order_job
	                                        INNER JOIN
	                                        dbo.norm_operation_table
	                                        ON 
		                                        man_planjob_list.id_norm_operation = norm_operation_table.id_norm_operation
	                                        INNER JOIN
	                                        dbo.order_head
	                                        ON 
		                                        man_order_job.id_order_head = order_head.id_order_head
	                                        INNER JOIN
	                                        dbo.common_ul_directory
	                                        ON 
		                                        order_head.id_customer = common_ul_directory.id_common_ul_directory
	                                        INNER JOIN
	                                        dbo.order_detail
	                                        ON 
		                                        man_order_job_item.itemid = order_detail.id_order_detail
                                        WHERE
	                                        man_planjob_list.id_man_order_job_item = @idManOrderJobItem"
                    };
                    Command.Parameters.AddWithValue("@idManOrderJobItem", idManOrderJobItem);

                    DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                    while (await sqlReader.ReadAsync())
                    {
                        //sqlReader["shift_no"] == DBNull.Value ? 0 : (int)Convert.ToInt32(sqlReader["shift_no"])

                        //int idManOrderJobItem = sqlReader["id_man_order_job_item"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_man_order_job_item"]);
                        //int equipID = sqlReader["id_equip"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_equip"]);
                        string orderNumber = sqlReader["order_num"] == DBNull.Value ? string.Empty : sqlReader["order_num"].ToString();
                        string nameCustomer = sqlReader["ul_name"] == DBNull.Value ? string.Empty : sqlReader["ul_name"].ToString();
                        int operationType = sqlReader["operation_type"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["operation_type"]);
                        int normTime = sqlReader["normtime"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["normtime"]);
                        int planQty = sqlReader["plan_out_qty"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["plan_out_qty"]);
                        string item = sqlReader["order_name"] == DBNull.Value ? string.Empty : sqlReader["order_name"].ToString() + ": ";
                        item += sqlReader["detail_name"] == DBNull.Value ? string.Empty : sqlReader["detail_name"].ToString();

                        int idOrderHead = sqlReader["id_order_head"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_order_head"]);

                        if (operationType == 0)
                        {
                            loadOrder.MakereadyTime = normTime / planQty;
                            loadOrder.LastMakeready = planQty * 100;
                        }

                        if (operationType == 1)
                        {
                            loadOrder.WorkTime = normTime;
                            loadOrder.AmountOfOrder = planQty;
                            loadOrder.LastAmount = planQty;
                        }

                        loadOrder.IdManOrderJobItem = idManOrderJobItem;
                        //loadOrder.EquipID = equipID;
                        loadOrder.OrderNumber = orderNumber;
                        loadOrder.NameCustomer = nameCustomer;
                        loadOrder.StampOrder = valueFromASBase.GetStampFromOrderIDHead(12, idOrderHead);
                        loadOrder.ItemOrder = item;
                        loadOrder.Items = valueFromASBase.LoadItemsFromOrder(idOrderHead);
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetOrderFromID. Ошибка получения заказа: " + ex.Message);
                LogException.WriteLine(ex.Message);
            }

            return loadOrder;
        }

        public async Task<int[]> SummPreviewOperationsAll(int idManOrderJobItem, string currentDateTime)
        {
            int[] summPreviewOperations = { 0, 0 };

            try
            {
                using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                {
                    await Connect.OpenAsync();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT
                                            SUM(CASE WHEN norm_operation_table.ord = 0 THEN fact_out_qty ELSE 0 END) AS summPreviewMakeready,
                                            SUM(CASE WHEN norm_operation_table.ord = 1 THEN fact_out_qty ELSE 0 END) AS summPreviewDone
                                        FROM
	                                        dbo.man_planjob_list
	                                        INNER JOIN
	                                        dbo.man_order_job_item
	                                        ON 
		                                        man_planjob_list.id_man_order_job_item = man_order_job_item.id_man_order_job_item
	                                        INNER JOIN
	                                        dbo.norm_operation_table
	                                        ON 
		                                        man_planjob_list.id_norm_operation = norm_operation_table.id_norm_operation
	                                        INNER JOIN
	                                        dbo.man_factjob
	                                        ON 
		                                        man_planjob_list.id_man_planjob_list = man_factjob.id_man_planjob_list
                                        WHERE
	                                        man_planjob_list.id_man_order_job_item = @idManOrderJobItem
                                            AND man_factjob.date_end < @currentDateTime"
                    };
                    Command.Parameters.AddWithValue("@idManOrderJobItem", idManOrderJobItem);
                    Command.Parameters.AddWithValue("@currentDateTime", currentDateTime);

                    DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                    while (await sqlReader.ReadAsync())
                    {
                        float previewMakeready = sqlReader["summPreviewMakeready"] == DBNull.Value ? 0 : (float)Convert.ToDouble(sqlReader["summPreviewMakeready"]);
                        int previewDone = sqlReader["summPreviewDone"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["summPreviewDone"]);

                        summPreviewOperations[0] += Convert.ToInt32(previewMakeready * 100);
                        summPreviewOperations[1] += previewDone;
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                LogException.WriteLine(ex.Message);
            }

            return summPreviewOperations;
        }
        public async Task<int> SummPreviewOperations(int idManOrderJobItem, string currentDateTime)
        {
            int summPreviewOperations = 0;

            try
            {
                using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                {
                    await Connect.OpenAsync();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT
	                                        SUM(man_factjob.fact_out_qty) AS summPreview
                                        FROM
	                                        dbo.man_planjob_list
	                                        INNER JOIN
	                                        dbo.man_order_job_item
	                                        ON 
		                                        man_planjob_list.id_man_order_job_item = man_order_job_item.id_man_order_job_item
	                                        INNER JOIN
	                                        dbo.norm_operation_table
	                                        ON 
		                                        man_planjob_list.id_norm_operation = norm_operation_table.id_norm_operation
	                                        INNER JOIN
	                                        dbo.man_factjob
	                                        ON 
		                                        man_planjob_list.id_man_planjob_list = man_factjob.id_man_planjob_list
                                        WHERE
	                                        norm_operation_table.ord = 1
                                            AND man_planjob_list.id_man_order_job_item = @idManOrderJobItem
                                            AND man_factjob.date_end < @currentDateTime"
                    };
                    Command.Parameters.AddWithValue("@idManOrderJobItem", idManOrderJobItem);
                    Command.Parameters.AddWithValue("@currentDateTime", currentDateTime);

                    DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                    while (await sqlReader.ReadAsync())
                    {
                        summPreviewOperations = sqlReader["summPreview"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["summPreview"]);
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
                LogException.WriteLine(ex.Message);
            }

            return summPreviewOperations;
        }
    }
}
