using System.Collections.Generic;

namespace OrderManager
{
    public class LoadOrder
    {
        private int equipID;
        private string orderNumber;
        private string nameCustomer;
        private int makereadyTime;
        private int workTime;
        private int amountOfOrder;
        private List<LoadShift> shift = new List<LoadShift>();

        public LoadOrder()
        {

        }
        public LoadOrder(int equipID, string orderNumber, string nameCustomer, int makereadyTime, int workTime, int amountOfOrder, List<LoadShift> shift)
        {
            EquipID = equipID;
            OrderNumber = orderNumber;
            NameCustomer = nameCustomer;
            MakereadyTime = makereadyTime;
            WorkTime = workTime;
            AmountOfOrder = amountOfOrder;
            Shift = shift;
        }

        public int EquipID
        {
            get => equipID;
            set => equipID = value;
        }

        public string OrderNumber
        {
            get => orderNumber;
            set => orderNumber = value;
        }
        public string NameCustomer
        {
            get => nameCustomer;
            set => nameCustomer = value;
        }
        public int MakereadyTime
        {
            get => makereadyTime;
            set => makereadyTime = value;
        }
        public int WorkTime
        {
            get => workTime;
            set => workTime = value;
        }
        public int AmountOfOrder
        {
            get => amountOfOrder;
            set => amountOfOrder = value;
        }
        public List<LoadShift> Shift
        {
            get => shift;
            set => shift = value;
        }
    }
}
