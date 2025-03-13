using System.Collections.Generic;

namespace OrderManager
{
    public class LoadShift
    {
        private int idFbcBrigade;
        private int userID;
        private string shiftDate;
        private int shiftNumber;
        private string shiftStart;
        private string shiftEnd;
        private List<LoadOrderOperations> orderOperations = new List<LoadOrderOperations>();

        public LoadShift()
        {

        }
        public LoadShift(int idFbcBrigade, int userID, string shiftDate, int shiftNumber, string shiftStart, string shiftEnd, List<LoadOrderOperations> orderOperations)
        {
            IDFbcBrigade = idFbcBrigade;
            UserID = userID;
            ShiftDate = shiftDate;
            ShiftNumber = shiftNumber;
            ShiftStart = shiftStart;
            ShiftEnd = shiftEnd;
            OrderOperations = orderOperations;
        }

        public LoadShift(string shiftDate, int shiftNumber)
        {
            ShiftDate = shiftDate;
            ShiftNumber = shiftNumber;
            ShiftStart = "";
            ShiftEnd = "";
        }
        public int IDFbcBrigade
        {
            get => idFbcBrigade;
            set => idFbcBrigade = value;
        }
        public int UserID
        {
            get => userID;
            set => userID = value;
        }
        public string ShiftDate
        {
            get => shiftDate;
            set => shiftDate = value;
        }
        public int ShiftNumber
        {
            get => shiftNumber;
            set => shiftNumber = value;
        }
        public string ShiftStart
        {
            get => shiftStart;
            set => shiftStart = value;
        }
        public string ShiftEnd
        {
            get => shiftEnd;
            set => shiftEnd = value;
        }
        public List<LoadOrderOperations> OrderOperations
        {
            get => orderOperations;
            set => orderOperations = value;
        }
    }
}
