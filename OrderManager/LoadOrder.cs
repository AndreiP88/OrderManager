using System.Collections.Generic;

namespace OrderManager
{
    public class LoadOrder
    {
        private bool isOrderLoad;
        private int idManOrderJobItem;
        private int orderOMIndex;
        private int equipID;
        private string equipName;
        private string orderNumber;
        private string nameCustomer;
        private int makereadyTime;
        private int workTime;
        private int amountOfOrder;
        private int lastAmount;
        private string stamp;
        private string itemOrder;
        private List<string> items;
        private List<LoadOrderOperations> orderOperations = new List<LoadOrderOperations>();

        public LoadOrder()
        {

        }
        public LoadOrder(int idManOrderJobItem, int equipID, string orderNumber, string nameCustomer, int makereadyTime, int workTime, int amountOfOrder, string stamp, string itemOrder, List<LoadOrderOperations> orderOperations)
        {
            IdManOrderJobItem = idManOrderJobItem;
            EquipID = equipID;
            OrderNumber = orderNumber;
            NameCustomer = nameCustomer;
            MakereadyTime = makereadyTime;
            WorkTime = workTime;
            AmountOfOrder = amountOfOrder;
            StampOrder = stamp;
            ItemOrder = itemOrder;
            OrderOperations = orderOperations;
        }

        public bool IsOrderLoad
        {
            get => isOrderLoad;
            set => isOrderLoad = value;
        }
        public int IdManOrderJobItem
        {
            get => idManOrderJobItem;
            set => idManOrderJobItem = value;
        }
        public int OrderOMIndex
        {
            get => orderOMIndex;
            set => orderOMIndex = value;
        }
        public int EquipID
        {
            get => equipID;
            set => equipID = value;
        }
        public string EquipName
        {
            get => equipName;
            set => equipName = value;
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
        public int LastAmount
        {
            get => lastAmount;
            set => lastAmount = value;
        }
        public string StampOrder
        {
            get => stamp;
            set => stamp = value;
        }
        public string ItemOrder
        {
            get => itemOrder;
            set => itemOrder = value;
        }
        public List<string> Items
        {
            get => items;
            set => items = value;
        }
        public List<LoadOrderOperations> OrderOperations
        {
            get => orderOperations;
            set => orderOperations = value;
        }
    }
}
