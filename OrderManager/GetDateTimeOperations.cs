using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManager
{
    internal class GetDateTimeOperations
    {
        public GetDateTimeOperations()
        {

        }

        private DateTime DifferentDate (DateTime firstDate, DateTime secondDate)
        {
            DateTime result = DateTime.MinValue;

            if (firstDate > secondDate)
                result = new DateTime((firstDate - secondDate).Ticks);

            return result;
        }

        private DateTime StringToDateTime (String date)
        {
            DateTime result = DateTime.Now;

            if (date != "")
                result = Convert.ToDateTime(date);

            return result;
        }

        private String YearsString (int year)
        {
            String result;

            if (year > 4 && year < 21)
                result = "лет";
            else
            {
                switch (year % 10)
                {
                    case 1:
                        result = "год";
                        break;
                    case 2:
                        result = "года";
                        break;
                    case 3:
                        result = "года";
                        break;
                    case 4:
                        result = "года";
                        break;
                    default:
                        result = "лет";
                        break;
                }
            }

            return result;
        }

        private String MonthString(int month)
        {
            String result;

            if (month > 4 && month < 21)
                result = "месяцев";
            else
            {
                switch (month % 10)
                {
                    case 1:
                        result = "месяц";
                        break;
                    case 2:
                        result = "месяца";
                        break;
                    case 3:
                        result = "месяца";
                        break;
                    case 4:
                        result = "месяца";
                        break;
                    default:
                        result = "месяцев";
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Общий стаж работы
        /// </summary>
        /// <param name="firstDate">Конечная дата</param>
        /// <param name="secondDate">Начальная дата</param>
        /// <returns>Стаж работы в формате XX лет, ХХ месяцев</returns>
        public String WorkExperience(String firstDate, String secondDate)
        {
            String result = "";

            DateTime firstD = StringToDateTime(firstDate);
            DateTime secondD = StringToDateTime(secondDate);

            DateTime different = DifferentDate(firstD, secondD);

            int year = different.Year - 1;
            int month = different.Month - 1;

            if (different != DateTime.MinValue)
                result = year.ToString() + " " + YearsString(year) + ", " + month.ToString() + " " + MonthString(month);

            return result;
        }
        /// <summary>
        /// Возраст пользователя
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondDate"></param>
        /// <returns>Возраст в формате ХХ лет</returns>
        public String AgeUser(String firstDate, String secondDate)
        {
            String result = "";

            DateTime firstD = StringToDateTime(firstDate);
            DateTime secondD = StringToDateTime(secondDate);

            DateTime different = DifferentDate(firstD, secondD);

            int year = different.Year - 1;

            if (different != DateTime.MinValue)
                result = year.ToString() + " " + YearsString(year);

            return result;
        }

        public String DateDifferent(String firstDate, String secondDate)
        {
            TimeSpan totalTime = TimeSpan.Zero;

            DateTime firstD = StringToDateTime(firstDate);
            DateTime secondD = StringToDateTime(secondDate);

            if (firstD > secondD)
                totalTime = firstD.Subtract(secondD.AddMinutes(-1));

            return TimeToSting(totalTime);
        }

        public int DateDifferentToMinutes(String firstDate, String secondDate)
        {
            TimeSpan totalTime = TimeSpan.Zero;

            DateTime firstD = StringToDateTime(firstDate);
            DateTime secondD = StringToDateTime(secondDate);

            if (firstD > secondD)
                totalTime = firstD.Subtract(secondD.AddMinutes(-1));

            return TotalHoursToMinutesTS(totalTime);
        }

        public String DateAmount(String firstDate, String secondTime)//не правильно считает
        {
            DateTime totalTime;
            DateTime firstD;
            TimeSpan secondD;

            //DateTime.MinValue.AddYears(Convert.ToInt32(comboBox1.Text) - 1).AddMonths(comboBox2.SelectedIndex).AddDays(DateTime.Now.Day);

            if (firstDate != "")
                firstD = Convert.ToDateTime(firstDate);
            else
                firstD = DateTime.Now;

            if (secondTime != "")
            {
                string[] time = secondTime.Split(':');

                int days = Convert.ToInt32(time[0]) / 24;
                int hours = Convert.ToInt32(time[0]) % 24;

                secondD = new TimeSpan(days, hours, Convert.ToInt32(time[1]), 0);
            }
            else
                secondD = TimeSpan.Zero;

            //totalTime = firstD.AddDays(secondD.TotalDays).AddHours(secondD.TotalHours).AddMinutes(secondD.TotalMinutes);
            totalTime = firstD.Add(secondD);

            return DateToSting(totalTime);
        }

        public String TimeAmount(String firstTime, String secondTime)
        {
            int totalTime;
            int firstT;
            int secondT;

            firstT = totallTimeHHMMToMinutes(firstTime);
            secondT = totallTimeHHMMToMinutes(secondTime);

            totalTime = firstT + secondT;

            return TotalMinutesToHoursAndMinutesStr(totalTime);
        }

        public String TimeDifferent(String firstTime, String secondTime)
        {
            int totalTime = 0;
            int firstD;
            int secondD;

            firstD = totallTimeHHMMToMinutes(firstTime);
            secondD = totallTimeHHMMToMinutes(secondTime);

            if (firstD > secondD)
            {
                totalTime = firstD - secondD;
            }

            return TotalMinutesToHoursAndMinutesStr(totalTime);
        }

        public int TimeDifferentToMinutes(String firstTime, String secondTime)
        {
            TimeSpan totalTime;
            DateTime firstD;
            DateTime secondD;

            firstD = Convert.ToDateTime(firstTime);
            secondD = Convert.ToDateTime(secondTime);

            if (firstD > secondD)
            {
                totalTime = firstD - secondD;
            }
            else
            {
                totalTime = TimeSpan.Zero;
            }

            return TotalHoursToMinutesTS(totalTime);
        }

        public int TotalHoursToMinutes(int hours, int minutes)
        {
            int hoursToMinutes = minutes;

            if (hours > 0)
                hoursToMinutes += hours * 60;

            return hoursToMinutes;
        }

        public int TotalHoursToMinutesTS(TimeSpan totalHours)
        {
            int result = 60 * 24 * totalHours.Days + 60 * totalHours.Hours + totalHours.Minutes;

            return result;
        }

        public (int, int) TotalMinutesToHoursAndMinutes(int totalMinutes)
        {
            int hours = 0;
            int minutes = totalMinutes % 60;

            if (totalMinutes >= 60)
                hours = totalMinutes / 60;

            return (hours, minutes);
        }

        public String TotalMinutesToHoursAndMinutesStr(int totalMinutes)
        {
            int hours = TotalMinutesToHoursAndMinutes(totalMinutes).Item1;
            int minutes = TotalMinutesToHoursAndMinutes(totalMinutes).Item2;

            TimeSpan tTime = TimeSpan.Zero;

            tTime += TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);

            return TimeToSting(tTime);
        }

        private int totallTimeHHMMToMinutes(String tTimeHHMM)
        {
            int result = 0;

            if (tTimeHHMM != "")
            {
                string[] time = tTimeHHMM.Split(':');

                result = 60 * Convert.ToInt32(time[0]) + Convert.ToInt32(time[1]);
            }

            return result;
        }

        private String TimeToSting(TimeSpan tTime)
        {
            String result = "";
            int H = 24 * tTime.Days + tTime.Hours;
            int M = tTime.Minutes;

            if (H <= 9)
                result += "0" + H.ToString();
            else
                result += H.ToString();

            result += ":";

            if (M <= 9)
                result += "0" + M.ToString();
            else
                result += M.ToString();

            return result;
        }

        private int TimeToTotallMinutes(TimeSpan tTime)
        {
            int result = 24 * tTime.Days + 60 * tTime.Hours + tTime.Minutes;

            return result;
        }

        private String DateToSting(DateTime tDate)
        {
            String result = tDate.ToString("HH:mm dd.MM.yyyy");

            return result;
        }
    }
}
