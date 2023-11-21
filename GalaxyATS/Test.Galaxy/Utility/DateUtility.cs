//using DataGeneration.DbOperations;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Tests.Galaxy.Utility
{

    /// <summary>
    /// Utility methods around links
    /// </summary>
    /// TODO : Could be part of the BaseTest Class
    public class DateUtility
    {
        #region Variable Declaration
        //HolidaysTable holidayTable = new HolidaysTable();
        #endregion

        /// <summary>
        /// GET If a Date is a Holiday
        /// </summary>
        /// <param name="date">Date to be Checked</param>
        /// <returns>Returns boolean value True if Date is a Holiday as per the Holiday Calender else returns false</returns>

        /*public bool IsHoliday(DateTime date)
        {
            return holidayTable.GetHolidays(date.Year).Contains(date);
        }*/

        /// <summary>
        /// GET If a Date is a weekend Date
        /// </summary>
        /// <param name="date">Date to be Checked</param>
        /// <returns>Returns boolean value True if Date is a weekend Date else returns false</returns>
        public bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday
                || date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// GET Next Business Day
        /// </summary>
        /// <param name="date">Date after which next business day is required(Base Date)</param>
        /// <param name="isMonthLastThreeDaysCheckNeeded">false</param>
        /// <returns>Returns next Business Date after the Base Date</returns>
        /*public DateTime GetNextBusinessDay(DateTime date, Boolean isMonthLastThreeDaysCheckNeeded = false)
        {
            do
            {
                date = date.AddDays(1);
            } while (IsHoliday(date) || IsWeekend(date));

            if (isMonthLastThreeDaysCheckNeeded && date.Day > 28)
            {
                date = GetNextBusinessDay(date, isMonthLastThreeDaysCheckNeeded);
            }
            return date;
        }*/

        /// <summary>
        /// Get Next Holiday
        /// </summary>
        /// <returns>Returns Next Holiday after base date</returns>
        /*public DateTime GetNextHoliday()
        {
            DateTime nextHoliday = holidayTable.GetCurrentYearHolidays().FirstOrDefault(x => x > DateTime.Today);
            if (nextHoliday == null)
            {
                nextHoliday = holidayTable.GetNextYearHolidays().FirstOrDefault();
            }
            return nextHoliday;
        }*/

        /// <summary>
        /// Get ordinal conversion of the number
        /// </summary>
        /// <param name="number">Number to convert to ordinal</param>
        /// <returns>Ordinal format of number</returns>
        public string GetOrdinal(int number)
        {
            if (number <= 0) return number.ToString();

            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return number + "th";
            }

            return (number % 10) switch
            {
                1 => number + "st",
                2 => number + "nd",
                3 => number + "rd",
                _ => number + "th",
            };
        }
        /// <summary>
        /// Add Days Within Current Year
        /// </summary>
        /// <param name="days">Number to days to Add</param>
        /// <returns>Date Value after addition of days within the current year only</returns>
        public DateTime GetDateFromTodayInSameYear(int days)
        {
            DateTime thisDay = DateTime.Today;
            DateTime returnDate = thisDay.AddDays(days);
            if (thisDay.Year == returnDate.Year)
            {
                return returnDate;
            }
            else
            {
                return returnDate.Year < thisDay.Year ? new DateTime(thisDay.Year, 1, 1) : new DateTime(thisDay.Year, 12, 31);
            }

        }

        /// <summary>
        /// Get Weekend.
        /// </summary>
        public DateTime GetWeekend(DateTime date)
        {
            DateTime weekend = date;
            for (int temp = 1; temp <= 5; temp++)
            {
                if (date.ToString("dddd").Equals("Saturday") || date.ToString("dddd").Equals("Sunday"))
                {
                    weekend = date;
                    break;
                }
                else
                {
                    date = date.AddDays(1);
                }
            }
            return weekend;
        }

        /// <summary>
        /// Get Today's Eastern Date.
        /// </summary>
        /// <returns>Today's eastern date</returns>
        public DateTime GetTodaysEasternDate()
        {
            DateTime todaysEasternDate = DateTime.Now;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                todaysEasternDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)))
            {
                todaysEasternDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/New_York"));
            }

            return todaysEasternDate;
        }
    }
}
