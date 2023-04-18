﻿using System;
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
        public int makereadyTime;
        public int workTime;
        public int amountOfOrder;
        public string stamp;

        public OrdersLoad(string number, string customer, int mkTime, int wkTime, int amount, string orderStamp)
        {
            this.numberOfOrder = number;
            this.nameCustomer = customer;
            this.makereadyTime = mkTime;
            this.workTime = wkTime;
            this.amountOfOrder = amount;
            this.stamp = orderStamp;
        }

    }
}