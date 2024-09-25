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
        public float MakereadyWorkTime;
        public int amountAllOrdersShift;
        public float percentWorkingOutShift;
        public float percentBonusShift;


        public ShiftsDetails(int shiftsCount, int workingTime, int shiftAllTime, int ShiftallTimeWorkingOut, int shiftCountOrders, int shiftCountMakeready, float makeReadyTime, int shiftAmountAllOrders, float shiftPercentWorkingOut, float percentBonusShift)
        {
            this.countShifts = shiftsCount;
            this.shiftsWorkingTime = workingTime;
            this.allTimeShift = shiftAllTime;
            this.allTimeWorkingOutShift = ShiftallTimeWorkingOut;
            this.countOrdersShift = shiftCountOrders;
            this.countMakereadyShift = shiftCountMakeready;
            this.MakereadyWorkTime = makeReadyTime;
            this.amountAllOrdersShift = shiftAmountAllOrders;
            this.percentWorkingOutShift = shiftPercentWorkingOut;
            this.percentBonusShift = percentBonusShift;
        }

    }
}
