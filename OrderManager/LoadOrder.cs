using System.Collections.Generic;

namespace OrderManager
{
    public class LoadOrder
    {
        private int idManOrderJobItem;
        private int equipID;
        private string orderNumber;
        private string nameCustomer;
        private int makereadyTime;
        private int workTime;
        private int amountOfOrder;
        private List<LoadOrderOperations> orderOperations = new List<LoadOrderOperations>();

        public LoadOrder()
        {

        }
        public LoadOrder(int idManOrderJobItem, int equipID, string orderNumber, string nameCustomer, int makereadyTime, int workTime, int amountOfOrder, List<LoadOrderOperations> orderOperations)
        {
            IdManOrderJobItem = idManOrderJobItem;
            EquipID = equipID;
            OrderNumber = orderNumber;
            NameCustomer = nameCustomer;
            MakereadyTime = makereadyTime;
            WorkTime = workTime;
            AmountOfOrder = amountOfOrder;
            OrderOperations = orderOperations;
        }

        public int IdManOrderJobItem
        {
            get => idManOrderJobItem;
            set => idManOrderJobItem = value;
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
        public List<LoadOrderOperations> OrderOperations
        {
            get => orderOperations;
            set => orderOperations = value;
        }
    }
}
