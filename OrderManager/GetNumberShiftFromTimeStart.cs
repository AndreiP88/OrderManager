using System;

namespace OrderManager
{
    internal class GetNumberShiftFromTimeStart
    {
        public string NumberShift(string startTimeShift)
        {
            string result = "0";

            DateTime dateTime = DateTime.Now;

            if (startTimeShift != "")
                dateTime = Convert.ToDateTime(startTimeShift).AddHours(-6);

            DateTime timeOne = Convert.ToDateTime(dateTime.Day + "." + dateTime.Month + "." + dateTime.Year + " " + "08:00");
            DateTime timeTwo = Convert.ToDateTime(dateTime.Day + "." + dateTime.Month + "." + dateTime.Year + " " + "20:00");

            dateTime = dateTime.AddHours(6);

            if (timeOne.AddHours(-3) <= dateTime && dateTime <= timeOne.AddHours(7))
            {
                result = "I";
            }
                
            if (timeTwo.AddHours(-3) <= dateTime && dateTime <= timeTwo.AddHours(7))
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

            if (timeOne.AddHours(-3) <= dateTime && dateTime <= timeOne.AddHours(7))
            {
                result = timeOne.ToString();
            }
                
            if (timeTwo.AddHours(-3) <= dateTime && dateTime <= timeTwo.AddHours(7))
            {
                result = timeTwo.ToString();
            }

            return result;
        }
    }
}
