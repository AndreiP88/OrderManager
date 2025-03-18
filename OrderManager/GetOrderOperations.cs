using libData;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace OrderManager
{
    class GetOrderOperations
    {
        public GetOrderOperations()
        {

        }
        public List<LoadShift> OperationsForOrder(List<LoadShift> shifts, int loadIdManOrderJobItem = -1)
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

                        int tmpIdManOrderJobItem = -1;

                        while (sqlReader.Read())
                        {
                            //sqlReader["shift_no"] == DBNull.Value ? 0 : (int)Convert.ToInt32(sqlReader["shift_no"])
                            counter++;

                            int idManOrderJobItem = sqlReader["id_man_order_job_item"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_man_order_job_item"]);
                            int operationType = sqlReader["operation_type"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["operation_type"]);
                            int equipID = sqlReader["id_equip"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_equip"]);

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
                                loadShifts[i].Order.Add(GetOrderFromID(idManOrderJobItem));
                                itemIndex = loadShifts[i].Order.Count - 1;

                                loadShifts[i].Order[itemIndex].EquipID = equipID;

                                loadShifts[i].Order[itemIndex].OrderOperations.Add(new LoadOrderOperations());
                            }

                            int itemOperationsIndex = loadShifts[i].Order[itemIndex].OrderOperations.Count - 1;
                            
                            if (itemOperationsIndex != -1)
                            {
                                if (operationType == 0)
                                {
                                    counterMK++;

                                    if (counterMK == 1)
                                    {
                                        loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStart = operationBegin;
                                    }

                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStop = operationEnd;
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyComplete += Convert.ToInt32(factQty * 100);
                                }

                                if (operationType == 1)
                                {
                                    counterWK++;

                                    if (counterWK == 1)
                                    {
                                        loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].WorkStart = operationBegin;
                                    }

                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].WorkStop = operationEnd;
                                    loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].Done += (int)factQty;
                                }                                
                            }

                            /*Console.WriteLine(i + ":: " + "<" + itemIndex + ">" + idFbcBrigade + " :: " + idManOrderJobItem + ": " + loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].MakereadyComplete + "::: " +
                                loadShifts[i].Order[itemIndex].OrderOperations[itemOperationsIndex].Done);*/
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

        public List<LoadShift> ShiftListForOrder(int idManOrderJobItem)
        {
            List<LoadShift> loadShifts = new List<LoadShift>();

            try
            {
                using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                {
                    Connect.OpenAsync();
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

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
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
                            loadShifts[itemIndex].ShiftStart = shiftBegin;
                            loadShifts[itemIndex].ShiftEnd = shiftEnd;
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

        private LoadOrder GetOrderFromID(int idManOrderJobItem)
        {
            LoadOrder loadOrder = new LoadOrder();

            try
            {
                using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                {
                    Connect.OpenAsync();
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
	                                        man_planjob_list.normtime
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
                                        WHERE
	                                        man_planjob_list.id_man_order_job_item = @idManOrderJobItem"
                    };
                    Command.Parameters.AddWithValue("@idManOrderJobItem", idManOrderJobItem);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    while (sqlReader.Read())
                    {
                        //sqlReader["shift_no"] == DBNull.Value ? 0 : (int)Convert.ToInt32(sqlReader["shift_no"])

                        //int idManOrderJobItem = sqlReader["id_man_order_job_item"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_man_order_job_item"]);
                        //int equipID = sqlReader["id_equip"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_equip"]);
                        string orderNumber = sqlReader["order_num"] == DBNull.Value ? string.Empty : sqlReader["order_num"].ToString();
                        string nameCustomer = sqlReader["ul_name"] == DBNull.Value ? string.Empty : sqlReader["ul_name"].ToString();
                        int operationType = sqlReader["operation_type"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["operation_type"]);
                        int normTime = sqlReader["id_norm_operation"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_norm_operation"]);
                        int planQty = sqlReader["plan_out_qty"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["plan_out_qty"]);

                        if (operationType == 0)
                        {
                            loadOrder.MakereadyTime = normTime / planQty;
                        }

                        if (operationType == 1)
                        {
                            loadOrder.WorkTime = normTime;
                            loadOrder.AmountOfOrder = planQty;
                        }

                        loadOrder.IdManOrderJobItem = idManOrderJobItem;
                        //loadOrder.EquipID = equipID;
                        loadOrder.OrderNumber = orderNumber;
                        loadOrder.NameCustomer = nameCustomer;
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка получения заказа: " + ex.Message);
                LogException.WriteLine(ex.Message);
            }

            return loadOrder;
        }
    }
}
