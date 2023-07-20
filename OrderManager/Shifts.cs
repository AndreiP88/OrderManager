using System;

namespace OrderManager
{
    internal class Shifts
    {
        public int startShiftID;
        public String dateShift;
        public String machinesShift;
        public String workingTimeShift;
        public int countOrdersShift;
        public int countMakeReadyShift;
        public int amountOrdersShift;
        public int workingOutShift;


        public Shifts(int shiftStartID, String shiftDate, String shiftMachines, String shiftWokingTime, int shiftCountOrders, int shiftCountMakeReadyShift, int shiftAmountOrders, int shiftWorkingOut)
        {
            this.startShiftID = shiftStartID;
            this.dateShift = shiftDate;
            this.machinesShift = shiftMachines;
            this.workingTimeShift = shiftWokingTime;
            this.countOrdersShift = shiftCountOrders;
            this.countMakeReadyShift = shiftCountMakeReadyShift;
            this.amountOrdersShift = shiftAmountOrders;
            this.workingOutShift = shiftWorkingOut;
        }
    }
}
