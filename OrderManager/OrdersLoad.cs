namespace OrderManager
{
    public class OrdersLoad
    {
        public int TypeJob;
        public int IDManPlanJob;
        public string TimeStartOrder;
        public string TimeEndOrder;
        public string numberOfOrder;
        public string nameCustomer;
        public string nameItem;
        public int makereadyTime;
        public int workTime;
        public int amountOfOrder;
        public string stamp;
        public string headOrder;
        public int idManOrderJobItem;

        public OrdersLoad(int typeJob, int idManPlanJob, string timeStartOrder, string timeEndOrder, string number, string customer, string item, int mkTime, int wkTime, int amount, string orderStamp, string head, int idManOrderJobItem)
        {
            this.TypeJob = typeJob;
            this.IDManPlanJob = idManPlanJob;
            this.TimeStartOrder = timeStartOrder;
            this.TimeEndOrder = timeEndOrder;
            this.numberOfOrder = number;
            this.nameCustomer = customer;
            this.nameItem = item;
            this.makereadyTime = mkTime;
            this.workTime = wkTime;
            this.amountOfOrder = amount;
            this.stamp = orderStamp;
            this.headOrder = head;
            this.idManOrderJobItem = idManOrderJobItem;
        }

        public OrdersLoad(string number, string customer, string item, int mkTime, int wkTime, int amount, string orderStamp, string head, int idManOrderJobItem)
        {
            this.TypeJob = 0;
            this.IDManPlanJob = -1;
            this.TimeStartOrder = "";
            this.TimeEndOrder = "";
            this.numberOfOrder = number;
            this.nameCustomer = customer;
            this.nameItem = item;
            this.makereadyTime = mkTime;
            this.workTime = wkTime;
            this.amountOfOrder = amount;
            this.stamp = orderStamp;
            this.headOrder = head;
            this.idManOrderJobItem = idManOrderJobItem;
        }
    }
}
