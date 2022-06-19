using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class Shifts
    {
        public String startShift;
        public String dateShift;
        public String machinesShift;
        public String workingTimeShift;
        public int countOrdersShift;
        public int amountOrdersShift;
        public int workingOutShift;


        public Shifts(String shiftStart, String shiftDate, String shiftMachines, String shiftWokingTime, int shiftCountOrders, int shiftAmountOrders, int shiftWorkingOut)
        {
            this.startShift = shiftStart;
            this.dateShift = shiftDate;
            this.machinesShift = shiftMachines;
            this.workingTimeShift = shiftWokingTime;
            this.countOrdersShift = shiftCountOrders;
            this.amountOrdersShift = shiftAmountOrders;
            this.workingOutShift = shiftWorkingOut;
        }
    }
}
