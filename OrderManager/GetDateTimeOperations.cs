using System;
using System.Windows.Forms;

namespace OrderManager
{
    internal class GetDateTimeOperations
    {
        public GetDateTimeOperations()
        {

        }

        private DateTime DifferentDate(DateTime firstDate, DateTime secondDate)
        {
            DateTime result = DateTime.MinValue;

            if (firstDate > secondDate)
            {
                result = new DateTime((firstDate - secondDate).Ticks);
            }

            return result;
        }

        public DateTime StringToDateTime(String date)
        {
            DateTime result = DateTime.Now;

            try
            {
                if (date != "")
                {
                    result = Convert.ToDateTime(date);
                }
            }
            catch
            {
                result = DateTime.Now;
            }
            
            return result;
        }

        private String YearsString(int year)
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

        /// <summary>
        /// Вычитает одно значение минут из другого. Можно иключить отрицательные значения.
        /// </summary>
        /// <param name="firstTime">Уменьшаемое</param>
        /// <param name="secondTime">Вычитаемое</param>
        /// <param name="positiveOnly">Использовать только положительный результат</param>
        /// <returns>Возвращает значение в минутах</returns>
        public int MinuteDifference(int firstTime, int secondTime, bool positiveOnly)
        {
            int result = firstTime - secondTime;

            if (positiveOnly && result < 0)
            {
                result = 0;
            }

            return result;
        }

        /// <summary>
        /// Переводит минуты в формат часы:минуты
        /// </summary>
        /// <param name="totalMinutes">Целое число минут</param>
        /// <returns>Строка в формате HH:MM</returns>
        public string MinuteToTimeString(int totalMinutes)
        {
            string result = "00:00";

            int absMinutes = Math.Abs(totalMinutes);

            int hours = 0;
            int minutes = absMinutes % 60;

            if (absMinutes >= 60)
            {
                hours = absMinutes / 60;
            }

            if (totalMinutes < 0)
            {
                hours *= (-1);
            }

            result = hours.ToString("D2") + ":" + minutes.ToString("D2");

            return result;
        }

        /// <summary>
        /// Переводит минуты в вормат TimeSpan
        /// </summary>
        /// <param name="totalMinutes"></param>
        /// <returns>Возвращает значение TimeSpan</returns>
        public TimeSpan MinuteToTimeSpan(int totalMinutes)
        {
            int absMinutes = Math.Abs(totalMinutes);

            int hours = 0;
            int minutes = absMinutes % 60;

            if (absMinutes >= 60)
            {
                hours = absMinutes / 60;
            }

            TimeSpan tTime = TimeSpan.Zero;

            tTime += TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);

            return tTime;
        }

        public TimeSpan DateDifference(string firstDate, string secondDate)
        {
            int coorection = 0;

            TimeSpan totalTime = TimeSpan.Zero;

            DateTime firstD = StringToDateTime(firstDate);
            DateTime secondD = StringToDateTime(secondDate);

            if (firstD > secondD)
            {
                totalTime = firstD.Subtract(secondD.AddMinutes(coorection));
            }

            return totalTime;
        }

        /// <summary>
        /// Получить разницу между датами в минутах, включая отрицательные значения
        /// </summary>
        /// <param name="firstDate">Начальнпая дата</param>
        /// <param name="secondDate">Конечная дата</param>
        /// <returns></returns>
        public int DateDifferenceToMinutesAndNegative(string firstDate, string secondDate)
        {
            int result;

            TimeSpan totalTime;

            DateTime firstD = StringToDateTime(firstDate);
            DateTime secondD = StringToDateTime(secondDate);

            if (firstD > secondD)
            {
                totalTime = firstD.Subtract(secondD);
                result = (int)totalTime.TotalMinutes;
            }
            else
            {
                totalTime = secondD.Subtract(firstD);
                result = (int)totalTime.TotalMinutes * (-1);
            }

            return result;
        }

        /// <summary>
        /// Разница между датами
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondDate"></param>
        /// <returns>Возвращает значение в минутах</returns>
        public int DateDifferenceToMinutes(string firstDate, string secondDate)
        {
            TimeSpan result = DateDifference(firstDate, secondDate);

            return TimeSpanToMinutes(result);
        }

        /// <summary>
        /// Разница между датами
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondDate"></param>
        /// <returns>Возвращает значение в формате HH:MM</returns>
        public string DateDifferenceToString(string firstDate, string secondDate)
        {
            TimeSpan result = DateDifference(firstDate, secondDate);

            return TimeSpanToSting(result);
        }

        /// <summary>
        /// Переводит из формата TimeSpan в минуты
        /// </summary>
        /// <param name="totalHours"></param>
        /// <returns>Целое значение минут</returns>
        public int TimeSpanToMinutes(TimeSpan totalHours)
        {
            int result = 60 * 24 * totalHours.Days + 60 * totalHours.Hours + totalHours.Minutes;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tTime"></param>
        /// <returns></returns>
        private String TimeSpanToSting(TimeSpan tTime)
        {
            String result = "";

            //MessageBox.Show(tTime.ToString());

            string k = "";

            if (tTime.ToString().Substring(0, 1) == "-")
            {
                k = "-";
            }

            int H = 24 * tTime.Days + tTime.Hours;
            int M = Math.Abs(tTime.Minutes);

            if (H <= 9)
                result += "0" + H.ToString();
            else
                result += H.ToString();

            result += ":";

            if (M <= 9)
                result += "0" + M.ToString();
            else
                result += M.ToString();

            return k + result.Replace("-", "");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondTime"></param>
        /// <returns></returns>
        public string DateTimeAmountMunutes(string firstDate, int secondTime)//не правильно считает
        {
            DateTime totalTime;
            DateTime firstD;
            TimeSpan secondD = MinuteToTimeSpan(secondTime);

            firstD = StringToDateTime(firstDate);


            /*if (secondTime != "")
            {
                string[] time = secondTime.Split(':');

                int k = 1;

                if (secondTime.Substring(0, 1) == "-")
                {
                    k = -1;
                }

                int days = Convert.ToInt32(time[0]) / 24;
                int hours = Convert.ToInt32(time[0]) % 24;

                secondD = new TimeSpan(days, hours, Convert.ToInt32(time[1]) * k, 0);
            }
            else
                secondD = TimeSpan.Zero;*/

            //totalTime = firstD.AddDays(secondD.TotalDays).AddHours(secondD.TotalHours).AddMinutes(secondD.TotalMinutes);


            //totalTime = firstD.Add(secondD);
            totalTime = firstD.AddMinutes(secondTime);

            return DateToSting(totalTime);
        }

        /// <summary>
        /// Вичитает из даты указанное количество минут
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondTime"></param>
        /// <returns></returns>
        public string DateTimeDifferenceMunutes(string firstDate, int secondTime)//не правильно считает
        {
            DateTime totalTime;
            DateTime firstD;
            TimeSpan secondD = MinuteToTimeSpan(secondTime);

            firstD = StringToDateTime(firstDate);


            /*if (secondTime != "")
            {
                string[] time = secondTime.Split(':');

                int k = 1;

                if (secondTime.Substring(0, 1) == "-")
                {
                    k = -1;
                }

                int days = Convert.ToInt32(time[0]) / 24;
                int hours = Convert.ToInt32(time[0]) % 24;

                secondD = new TimeSpan(days, hours, Convert.ToInt32(time[1]) * k, 0);
            }
            else
                secondD = TimeSpan.Zero;*/

            //totalTime = firstD.AddDays(secondD.TotalDays).AddHours(secondD.TotalHours).AddMinutes(secondD.TotalMinutes);


            //totalTime = firstD.Add(secondD);
            totalTime = firstD.AddMinutes(secondTime * (-1));

            return DateToSting(totalTime);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondTime"></param>
        /// <returns></returns>
        public string DateTimeAmount(string firstDate, string secondTime)//не правильно считает// перевести время в минуты и посчитать
        {
            DateTime totalTime;
            DateTime firstD;
            TimeSpan secondD;

            firstD = StringToDateTime(firstDate);

            if (secondTime != "")
            {
                string[] time = secondTime.Split(':');

                int k = 1;

                if (secondTime.Substring(0, 1) == "-")
                {
                    k = -1;
                }

                int days = Convert.ToInt32(time[0]) / 24;
                int hours = Convert.ToInt32(time[0]) % 24;

                secondD = new TimeSpan(days, hours, Convert.ToInt32(time[1]) * k, 0);
            }
            else
                secondD = TimeSpan.Zero;

            //totalTime = firstD.AddDays(secondD.TotalDays).AddHours(secondD.TotalHours).AddMinutes(secondD.TotalMinutes);


            totalTime = firstD.Add(secondD);

            return DateToSting(totalTime);
        }






















        public String DateDifferent(String firstDate, String secondDate)
        {
            int coorection = 0;

            TimeSpan totalTime = TimeSpan.Zero;

            DateTime firstD = StringToDateTime(firstDate);
            DateTime secondD = StringToDateTime(secondDate);

            if (firstD > secondD)
                totalTime = firstD.Subtract(secondD.AddMinutes(coorection));

            return TimeToSting(totalTime);
        }

        public int DateDifferentToMinutes(String firstDate, String secondDate)
        {
            int coorection = 0;

            TimeSpan totalTime = TimeSpan.Zero;

            DateTime firstD = StringToDateTime(firstDate);
            DateTime secondD = StringToDateTime(secondDate);

            if (firstD > secondD)
                totalTime = firstD.Subtract(secondD.AddMinutes(coorection));

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

                int k = 1;

                if (secondTime.Substring(0, 1) == "-")
                {
                    k = -1;
                }

                int days = Convert.ToInt32(time[0]) / 24;
                int hours = Convert.ToInt32(time[0]) % 24;

                secondD = new TimeSpan(days, hours, Convert.ToInt32(time[1]) * k, 0);
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

            //MessageBox.Show(firstT.ToString() + " + " + secondT.ToString() + " = " + totalTime.ToString());

            return TotalMinutesToHoursAndMinutesStr(totalTime);
        }

        public String TimeAmountN(String firstTime, String secondTime)
        {
            int totalTime = 0;
            int firstT;
            int secondT;

            TimeSpan tDate;
            string[] fDate = firstTime.Split(':');
            string[] sDate = secondTime.Split(':');

            TimeSpan f = new TimeSpan(Convert.ToInt32(fDate[0]), Convert.ToInt32(fDate[1]), 0);
            TimeSpan s = new TimeSpan(Convert.ToInt32(sDate[0]), Convert.ToInt32(sDate[1]), 0);

            firstT = totallTimeHHMMToMinutes(firstTime);
            secondT = totallTimeHHMMToMinutes(secondTime);

            tDate = f.Add(s);

            return tDate.ToString();

            //MessageBox.Show(firstT.ToString() + " + " + secondT.ToString() + " = " + totalTime.ToString());

            //return TotalMinutesToHoursAndMinutesStr(totalTime);
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

        public String TimeDifferentAndNegative(String firstTime, String secondTime)
        {
            int totalTime;
            int firstD;
            int secondD;

            string sign = "";

            firstD = totallTimeHHMMToMinutes(firstTime);
            secondD = totallTimeHHMMToMinutes(secondTime);

            if (firstD >= secondD)
            {
                totalTime = firstD - secondD;
                sign = "";
            }
            else
            {
                totalTime = secondD - firstD;
                sign = "-";
            }

            return sign + TotalMinutesToHoursAndMinutesStr(totalTime);
        }

        public int TimeDifferentToMinutes(String firstTime, String secondTime)
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

            return (totalTime);
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
            int minutes = Math.Abs(totalMinutes % 60);

            if (Math.Abs(totalMinutes) >= 60)
                hours = totalMinutes / 60;

            //MessageBox.Show(hours.ToString() + ":" + minutes.ToString());
            //Console.WriteLine(hours + ":" + minutes);

            return (hours, minutes);
        }

        public String TotalMinutesToHoursAndMinutesStr(int totalMinutes)
        {
            int hours = TotalMinutesToHoursAndMinutes(totalMinutes).Item1;
            int minutes = Math.Abs(TotalMinutesToHoursAndMinutes(totalMinutes).Item2);

            TimeSpan tTime = TimeSpan.Zero;

            tTime += TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes);

            //MessageBox.Show(hours.ToString() + ":" + minutes.ToString() + " --- " + tTime.ToString());

            return TimeToSting(tTime);
            //return tTime.ToString(@"hh\:mm");
        }

        public int totallTimeHHMMToMinutes(String tTimeHHMM)
        {
            int result = 0;

            if (tTimeHHMM != "")
            {
                string[] time = tTimeHHMM.Split(':');

                int k = 1;

                if (time[0].Substring(0, 1) == "-")
                {
                    k = -1;
                }

                result = (60 * Convert.ToInt32(time[0].Replace("-", "")) + Convert.ToInt32(time[1])) * k;

                //MessageBox.Show(time[0] + ":" + time[1] + ", (" + Convert.ToInt32(time[0]).ToString() + ":" + Convert.ToInt32(time[1]).ToString() + ") : " + result);
            }

            return result;
        }

        private String TimeToSting(TimeSpan tTime)
        {
            String result = "";

            //MessageBox.Show(tTime.ToString());

            string k = "";

            if (tTime.ToString().Substring(0, 1) == "-")
            {
                k = "-";
            }

            int H = 24 * tTime.Days + tTime.Hours;
            int M = Math.Abs(tTime.Minutes);

            if (H <= 9)
                result += "0" + H.ToString();
            else
                result += H.ToString();

            result += ":";

            if (M <= 9)
                result += "0" + M.ToString();
            else
                result += M.ToString();

            return k + result.Replace("-", "");
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
