using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class OrderStatusValue
    {
        public string statusStr;
        public string caption_1;
        public string value_1;
        public string caption_2;
        public string value_2;
        public string caption_3;
        public string value_3;
        public string caption_4;
        public string value_4;
        public int mkTimeDifferent;
        public int wkTimeDifferent;
        public int fullTimeDifferent;
        public string message;
        public Color color;

        public OrderStatusValue(string statusStrVal, string captionVal_1, string valueVal_1, string captionVal_2, string valueVal_2,
            string captionVal_3, string valueVal_3, string captionVal_4, string valueVal_4,
            int mkTimeDifferentVal, int wkTimeDifferentVal, int fullTimeDifferentVal, string messageVal, Color colorVal)
        {
            this.statusStr = statusStrVal;
            this.caption_1 = captionVal_1;
            this.value_1 = valueVal_1;
            this.caption_2 = captionVal_2;
            this.value_2 = valueVal_2;
            this.caption_3 = captionVal_3;
            this.value_3 = valueVal_3;
            this.caption_4 = captionVal_4;
            this.value_4 = valueVal_4;
            this.mkTimeDifferent = mkTimeDifferentVal;
            this.wkTimeDifferent = wkTimeDifferentVal;
            this.fullTimeDifferent = fullTimeDifferentVal;
            this.message = messageVal;
            this.color = colorVal;
        }
    }
}
