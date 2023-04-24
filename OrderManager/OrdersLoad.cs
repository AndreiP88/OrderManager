using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class OrdersLoad
    {
        public string numberOfOrder;
        public string nameCustomer;
        public string nameItem;
        public int makereadyTime;
        public int workTime;
        public int amountOfOrder;
        public string stamp;
        public string headOrder;

        public OrdersLoad(string number, string customer, string item, int mkTime, int wkTime, int amount, string orderStamp, string head)
        {
            this.numberOfOrder = number;
            this.nameCustomer = customer;
            this.nameItem = item;
            this.makereadyTime = mkTime;
            this.workTime = wkTime;
            this.amountOfOrder = amount;
            this.stamp = orderStamp;
            this.headOrder = head;   
        }

    }
}
