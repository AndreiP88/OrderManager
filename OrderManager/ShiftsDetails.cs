namespace OrderManager
{
    internal class ShiftsDetails
    {
        public int countShifts;
        public int allTimeShift;
        public int allTimeWorkingOutShift;
        public int countOrdersShift;
        public int countMakereadyShift;
        public int amountAllOrdersShift;
        public float percentWorkingOutShift;


        public ShiftsDetails(int shiftsCount, int shiftAllTime, int ShiftallTimeWorkingOut, int shiftCountOrders, int shiftCountMakeready, int shiftAmountAllOrders, float shiftPercentWorkingOut)
        {
            this.countShifts = shiftsCount;
            this.allTimeShift = shiftAllTime;
            this.allTimeWorkingOutShift = ShiftallTimeWorkingOut;
            this.countOrdersShift = shiftCountOrders;
            this.countMakereadyShift = shiftCountMakeready;
            this.amountAllOrdersShift = shiftAmountAllOrders;
            this.percentWorkingOutShift = shiftPercentWorkingOut;
        }

    }
}
