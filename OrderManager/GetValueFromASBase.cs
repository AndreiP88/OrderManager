using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetValueFromASBase
    {
        public GetValueFromASBase()
        {

        }

        private async Task<string> GetStampFromOrderNumber(string searchNumber, int normOperation)
        {
            ValueCategory valueCategory = new ValueCategory();
            ValueInfoBase getInfo = new ValueInfoBase();

            //string category = await getInfo.GetCategoryMachine(loadMachine);

            //int normOperation = Convert.ToInt32(valueCategory.GetIDOptionView(category));

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

        public string GetStampFromOrderIDHead(int normOperation, int nOrderId)
        {
            ValueCategory valueCategory = new ValueCategory();
            ValueInfoBase getInfo = new ValueInfoBase();

            //string category = await getInfo.GetCategoryMachine(loadMachine);

            //int normOperation = Convert.ToInt32(valueCategory.GetIDOptionView(category));

            string result = "";

            List<string> tools = new List<string>();

            string connectionString = @"Data Source = SRV-ACS\DSACS; Initial Catalog = asystem; Persist Security Info = True; User ID = ds; Password = 1";

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

        public List<string> LoadItemsFromOrder(int headID)
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
        public async Task<int> GetIdManOrderJobItem(int equipID, string orderNumber)
        {
            int idManOrderJobItem = -1;

            try
            {
                using (SqlConnection Connect = DBConnection.GetSQLServerConnection())
                {
                    await Connect.OpenAsync();
                    SqlCommand Command = new SqlCommand
                    {
                        Connection = Connect,
                        CommandText = @"SELECT
	                                        id_man_order_job_item
                                        FROM
	                                        dbo.order_head
	                                        INNER JOIN
	                                        dbo.man_order_job
	                                        ON 
		                                        order_head.id_order_head = man_order_job.id_order_head
	                                        INNER JOIN
	                                        dbo.man_order_job_item
	                                        ON 
		                                        man_order_job.id_man_order_job = man_order_job_item.id_man_order_job
                                        WHERE
	                                        order_num = @orderNumber AND id_equip = @equipID AND id_norm_operation is not null"
                    };
                    Command.Parameters.AddWithValue("@equipID", equipID);
                    Command.Parameters.AddWithValue("@orderNumber", orderNumber);

                    DbDataReader sqlReader = await Command.ExecuteReaderAsync();

                    while (await sqlReader.ReadAsync())
                    {
                        idManOrderJobItem = sqlReader["id_man_order_job_item"] == DBNull.Value ? 0 : Convert.ToInt32(sqlReader["id_man_order_job_item"]);
                    }

                    Connect.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка получения idManOrderJobItem: " + ex.Message);
                LogException.WriteLine(ex.Message);
            }

            return idManOrderJobItem;
        }
    }
}
