using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    class GetOrderOperations
    {
        public GetOrderOperations()
        {

        }

        public LoadOrder OperationsForOrder(int idManOrderJobItem)
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
	                                        fbc_brigade.id_fbc_brigade, 
	                                        man_factjob.id_common_employee AS user_id, 
	                                        fbc_brigade.date_begin AS shift_begin, 
	                                        fbc_brigade.date_end AS shift_end, 
	                                        fbc_brigade.shift_no, 
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
	                                        man_planjob_list.id_man_order_job_item = @idManOrderJobItem
                                        ORDER BY
	                                        fbc_brigade.date_begin ASC, 
	                                        man_factjob.date_begin ASC, 
	                                        man_factjob.date_end ASC"
                    };
                    Command.Parameters.AddWithValue("@idManOrderJobItem", idManOrderJobItem);

                    DbDataReader sqlReader = Command.ExecuteReader();

                    int counter = 0;
                    int counterMK = 0;
                    int counterWK = 0;

                    int makereadyTime = 0;
                    int workTime = 0;
                    int amountOrder = 0;

                    string makereadyStart = "";
                    string makereadyStop = "";
                    string workStart = "";
                    string workStop = "";

                    while (sqlReader.Read())
                    {
                        int makereadyComplete = 0;
                        int done = 0;

                        //sqlReader["shift_no"] == DBNull.Value ? 0 : (int)Convert.ToInt32(sqlReader["shift_no"])
                        counter++;

                        int equipID = sqlReader["id_equip"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_equip"]);
                        string orderNumber = sqlReader["order_num"] == DBNull.Value ? string.Empty : sqlReader["order_num"].ToString();
                        string nameCustomer = sqlReader["ul_name"] == DBNull.Value ? string.Empty : sqlReader["ul_name"].ToString();
                        int operationType = sqlReader["operation_type"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["operation_type"]);
                        int normTime = sqlReader["id_norm_operation"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_norm_operation"]);
                        int planQty = sqlReader["plan_out_qty"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["plan_out_qty"]);

                        int idFbcBrigade = sqlReader["id_fbc_brigade"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_fbc_brigade"]);
                        int userID = sqlReader["user_id"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["user_id"]);
                        string shiftBegin = sqlReader["shift_begin"] == DBNull.Value ? string.Empty : sqlReader["shift_begin"].ToString();
                        string shiftEnd = sqlReader["shift_end"] == DBNull.Value ? string.Empty : sqlReader["shift_end"].ToString();
                        int shiftNumber = sqlReader["shift_no"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["shift_no"]);

                        string operationBegin = sqlReader["date_begin"] == DBNull.Value ? string.Empty : sqlReader["date_begin"].ToString();
                        string operationEnd = sqlReader["date_end"] == DBNull.Value ? string.Empty : sqlReader["date_end"].ToString();

                        float factQty = sqlReader["fact_out_qty"] == DBNull.Value ? 0 : (float)Convert.ToDouble(sqlReader["fact_out_qty"]);


                        if (operationType == 0)
                        {
                            counterMK++;

                            makereadyTime = normTime / planQty;

                            if (counterMK == 1)
                            {
                                makereadyStart = operationBegin;
                            }

                            makereadyStop = operationEnd;
                            makereadyComplete = (int)(factQty * 100);
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

                        loadOrder.EquipID = equipID;
                        loadOrder.OrderNumber = orderNumber;
                        loadOrder.NameCustomer = nameCustomer;
                        loadOrder.MakereadyTime = makereadyTime;
                        loadOrder.WorkTime = workTime;

                        loadOrder.AmountOfOrder = amountOrder;

                        int itemIndex = loadOrder.Shift.FindIndex((v) => v.IDFbcBrigade == idFbcBrigade);
                        int itemOperationsIndex = -1;

                        if (itemIndex == -1)
                        {
                            /*loadOrder.Shift.Add(new LoadShift(
                                idFbcBrigade,
                                userID,
                                Convert.ToDateTime(shiftBegin).ToString("dd.MM.yyyy"),
                                shiftNumber,
                                shiftBegin,
                                shiftEnd,
                                new List<LoadOrderOperations>()
                                ));*/

                            loadOrder.Shift.Add(new LoadShift());

                            itemIndex = loadOrder.Shift.Count - 1;

                            loadOrder.Shift[itemIndex].IDFbcBrigade = idFbcBrigade;
                            loadOrder.Shift[itemIndex].UserID = userID;
                            loadOrder.Shift[itemIndex].ShiftDate = Convert.ToDateTime(shiftBegin).ToString("dd.MM.yyyy");
                            loadOrder.Shift[itemIndex].ShiftNumber = shiftNumber;
                            loadOrder.Shift[itemIndex].ShiftStart = shiftBegin;
                            loadOrder.Shift[itemIndex].ShiftEnd = shiftEnd;
                            loadOrder.Shift[itemIndex].OrderOperations.Add(new LoadOrderOperations(
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
                            itemOperationsIndex = loadOrder.Shift[itemIndex].OrderOperations.Count - 1;

                            if (itemOperationsIndex != -1)
                            {
                                loadOrder.Shift[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStart = makereadyStart;
                                loadOrder.Shift[itemIndex].OrderOperations[itemOperationsIndex].MakereadyStop = makereadyStop;
                                loadOrder.Shift[itemIndex].OrderOperations[itemOperationsIndex].WorkStart = workStart;
                                loadOrder.Shift[itemIndex].OrderOperations[itemOperationsIndex].WorkStop = workStop;
                                loadOrder.Shift[itemIndex].OrderOperations[itemOperationsIndex].MakereadyComplete += makereadyComplete;
                                loadOrder.Shift[itemIndex].OrderOperations[itemOperationsIndex].Done += done;
                            }
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

            return loadOrder;
        }
    }
}
