using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetUserIDOrMachineFromInfoBase
    {
        String dataBaseDefault = Directory.GetCurrentDirectory() + "\\data.db";
        String dataBase;

        public GetUserIDOrMachineFromInfoBase(String dBase)
        {
            this.dataBase = dBase;

            if (dataBase == "")
                dataBase = dataBaseDefault;
        }
        public String GetIDUser(String machine)
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

        public Object GetMachines(String userID)
        {
            List<String> result = new List<String>();
            result.Clear();

            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + dataBase + "; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand Command = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM Info WHERE nameOfExecutor = '" + userID + "'"
                };
                SQLiteDataReader sqlReader = Command.ExecuteReader();

                while (sqlReader.Read())
                {
                    result.Add(sqlReader["machine"].ToString());
                }

                Connect.Close();
            }
            return result;
        }

        public String GetMachinesStr(String userID)
        {
            GetValueFromInfoBase getInfo = new GetValueFromInfoBase(dataBase); 

            List<String> orderMachines = (List<String>)GetMachines(userID);
            String machines = "";

            for (int i = 0; i < orderMachines.Count; i++)
            {
                machines += getInfo.GetMachineName(orderMachines[i]);

                if (i != orderMachines.Count - 1)
                    machines += ", ";
                else
                    machines += ".";
            }

            return machines;
        }

        public bool GetMachinesForUserActive(String userID)
        {
            GetValueFromInfoBase getValue = new GetValueFromInfoBase(dataBase);

            List<String> orderMachines = (List<String>)GetMachines(userID);
            bool machinesActive = false;

            for (int i = 0; i < orderMachines.Count; i++)
            {
                if (Convert.ToBoolean(getValue.GetActiveOrder(orderMachines[i])) == true)
                    machinesActive = true;
            }

            return machinesActive;
        }
    }
}
