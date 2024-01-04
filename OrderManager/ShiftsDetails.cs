namespace OrderManager
{
    internal class ShiftsDetails
    {
        public int countShifts;
        public int shiftsWorkingTime;
        public int allTimeShift;
        public int allTimeWorkingOutShift;
        public int countOrdersShift;
        public int countMakereadyShift;
        public int amountAllOrdersShift;
        public float percentWorkingOutShift;
        public float percentBonusShift;


        public ShiftsDetails(int shiftsCount, int workingTime, int shiftAllTime, int ShiftallTimeWorkingOut, int shiftCountOrders, int shiftCountMakeready, int shiftAmountAllOrders, float shiftPercentWorkingOut, float percentBonusShift)
        {
            this.countShifts = shiftsCount;
            this.shiftsWorkingTime = workingTime;
            this.allTimeShift = shiftAllTime;
            this.allTimeWorkingOutShift = ShiftallTimeWorkingOut;
            this.countOrdersShift = shiftCountOrders;
            this.countMakereadyShift = shiftCountMakeready;
            this.amountAllOrdersShift = shiftAmountAllOrders;
            this.percentWorkingOutShift = shiftPercentWorkingOut;
            this.percentBonusShift = percentBonusShift;
        }

    }
}
