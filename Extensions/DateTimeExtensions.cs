using System;
using System.Linq;

namespace Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Calculates number of business minutes, taking into account:
        ///  - weekends (Saturdays and Sundays)
        ///  - holidays in the middle of the week
        /// </summary>
        /// <param name="firstDay">First day in the time interval</param>
        /// <param name="lastDay">Last day in the time interval</param>
        /// <param name="holidays">List of holidays excluding weekends</param>
        /// <returns>Number of business munites during the 'span'</returns>
        public static int BusinessMinutesUntil(this DateTime firstDay, DateTime lastDay, params DateTime[] holidays)
        {
            var original_firstDay = firstDay;
            var original_lastDay = lastDay;
            firstDay = firstDay.Date;
            lastDay = lastDay.Date;

            // lastDay needs to be later than firstDay
            if (firstDay > lastDay)
                throw new ArgumentException("Incorrect last day " + lastDay);

            TimeSpan span = lastDay - firstDay;
            int businessDays = span.Days + 1;
            int fullWeekCount = businessDays / 7;

            bool satsun = false;
            bool satorsunonly = false;


            // find out if there are weekends during the time exceedng the full weeks
            if (businessDays > fullWeekCount * 7)
            {
                // Find out if there is a 1-day or 2-days weekend
                // in the time interval remaining after subtracting the complete weeks
                int firstDayOfWeek = firstDay.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)firstDay.DayOfWeek;
                int lastDayOfWeek = lastDay.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)lastDay.DayOfWeek;

                if (lastDayOfWeek < firstDayOfWeek)
                    lastDayOfWeek += 7;
                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                    {
                        businessDays -= 2;
                        satsun = true;
                    }
                    else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                    {
                        businessDays -= 1;
                        satorsunonly = true;
                    }

                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                {
                    businessDays -= 1;
                    satorsunonly = true;
                }
            }

            // subtract the weekends during the full weeks in the interval
            businessDays -= fullWeekCount + fullWeekCount;

            if (holidays != null && holidays.Any())
            {
                // subtract the number of holidays during the time interval
                foreach (DateTime holiday in holidays)
                {
                    DateTime hd = holiday.Date;
                    if (firstDay <= hd && hd <= lastDay)
                        --businessDays;
                }
            }

            int total_business_minutes = 0;
            if (firstDay.Date == lastDay.Date)
            {
                //If on the same day
                total_business_minutes = (int)(original_lastDay - original_firstDay).TotalMinutes;
            }
            else
            {
                //Convert Business-Days to TotalMinutes
                if (satorsunonly)
                    total_business_minutes = (int)(original_lastDay - original_firstDay).TotalMinutes - 1440;
                else if (satsun)
                    total_business_minutes = (int)(original_lastDay - original_firstDay).TotalMinutes - (2880 * (fullWeekCount + 1));
                else
                    total_business_minutes = (int)(original_lastDay - original_firstDay).TotalMinutes;
            }
            return total_business_minutes;
        }

    }
}
