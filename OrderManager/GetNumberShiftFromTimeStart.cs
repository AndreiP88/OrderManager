using System;
using System.Windows.Forms;

namespace OrderManager
{
    internal class GetNumberShiftFromTimeStart
    {
        public String NumberShift(String startTimeShift)
        {
            String result = "0";

            DateTime dateTime = DateTime.Now;

            if (startTimeShift != "")
                dateTime = Convert.ToDateTime(startTimeShift).AddHours(-6);

            DateTime timeOne = Convert.ToDateTime(dateTime.Day + "." + dateTime.Month + "." + dateTime.Year + " " + "08:00");
            DateTime timeTwo = Convert.ToDateTime(dateTime.Day + "." + dateTime.Month + "." + dateTime.Year + " " + "20:00");

            dateTime = dateTime.AddHours(6);

            if (timeOne.AddHours(-4) <= dateTime && dateTime <= timeOne.AddHours(6))
            {
                result = "I";
            }
                
            if (timeTwo.AddHours(-4) <= dateTime && dateTime <= timeTwo.AddHours(6))
            {
                result = "II";
            }
                
            return result;
        }

        public string PlanedStartShift(string startTimeShift)
        {
            string result = "PlanedStartShift";

            DateTime dateTime = DateTime.Now;

            if (startTimeShift != "")
            {
                dateTime = Convert.ToDateTime(startTimeShift).AddHours(-6);
                result = dateTime.ToString();
            }

            DateTime timeOne = Convert.ToDateTime(dateTime.Day + "." + dateTime.Month + "." + dateTime.Year + " " + "08:00:00");
            DateTime timeTwo = Convert.ToDateTime(dateTime.Day + "." + dateTime.Month + "." + dateTime.Year + " " + "20:00:00");

            dateTime = dateTime.AddHours(6);

            if (timeOne.AddHours(-4) <= dateTime && dateTime <= timeOne.AddHours(6))
            {
                result = timeOne.ToString();
            }
                
            if (timeTwo.AddHours(-4) <= dateTime && dateTime <= timeTwo.AddHours(6))
            {
                result = timeTwo.ToString();
            }

            return result;
        }
    }
}
