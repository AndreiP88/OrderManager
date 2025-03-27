using System.Collections.Generic;

namespace OrderManager
{
    public class LoadShift
    {
        private bool isNewShift;
        private bool isLoadShift;
        private int idFbcBrigade;
        private int indexOMShift;
        private int userID;
        private int userIDBaseOM;
        private string shiftDate;
        private int shiftNumber;
        private string shiftStart;
        private string shiftEnd;
        private List<LoadOrder> order = new List<LoadOrder>();

        public LoadShift()
        {

        }
        public LoadShift(bool isNewShift, int idFbcBrigade, int indexOMShift, int userID, int userIDBaseOM, string shiftDate, int shiftNumber, string shiftStart, string shiftEnd, List<LoadOrder> order)
        {
            IsNewShift = isNewShift;
            IDFbcBrigade = idFbcBrigade;
            IndexOMShift = indexOMShift;
            UserID = userID;
            UserIDBaseOM = userIDBaseOM;
            ShiftDate = shiftDate;
            ShiftNumber = shiftNumber;
            ShiftStart = shiftStart;
            ShiftEnd = shiftEnd;
            Order = order;
        }

        public LoadShift(string shiftDate, int shiftNumber)
        {
            ShiftDate = shiftDate;
            ShiftNumber = shiftNumber;
            ShiftStart = "";
            ShiftEnd = "";
        }

        public bool IsNewShift
        {
            get => isNewShift;
            set => isNewShift = value;
        }
        public bool IsLoadShift
        {
            get => isLoadShift;
            set => isLoadShift = value;
        }
        public int IDFbcBrigade
        {
            get => idFbcBrigade;
            set => idFbcBrigade = value;
        }
        public int IndexOMShift
        {
            get => indexOMShift;
            set => indexOMShift = value;
        }
        public int UserID
        {
            get => userID;
            set => userID = value;
        }
        public int UserIDBaseOM
        {
            get => userIDBaseOM;
            set => userIDBaseOM = value;
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

        public List<LoadOrder> Order
        {
            get => order;
            set => order = value;
        }
    }
}
