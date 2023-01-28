﻿using System;

namespace OrderManager
{
    internal class GetNumberShiftFromTimeStart
    {
        public String NumberShift(String startTimeShift)
        {
            String result = "sm";

            DateTime dateTime = DateTime.Now;
            DateTime dateTimePreview;
            DateTime dateTimeNext;

            if (startTimeShift != "")
                dateTime = Convert.ToDateTime(startTimeShift);

            DateTime timeOne = Convert.ToDateTime(dateTime.Day + "." + dateTime.Month + "." + dateTime.Year + " " + "08:00");
            DateTime timeTwo = Convert.ToDateTime(dateTime.Day + "." + dateTime.Month + "." + dateTime.Year + " " + "20:00");

            dateTimePreview = dateTime.AddHours(-5);
            dateTimeNext = dateTime.AddHours(5);

            if (dateTimePreview <= timeOne && timeOne <= dateTimeNext)
                result = "I";
            if (dateTimePreview <= timeTwo && timeTwo <= dateTimeNext)
                result = "II";

            return result;
        }
    }
}
