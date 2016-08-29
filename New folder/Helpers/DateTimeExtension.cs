using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMSERoute.Helpers
{
    public static class DateTimeExtension
    {
        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime dateTime)
        {
            DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
        }

        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            //--------Calculation Description
            //dt = Today = WesnesDay, 2011/01/19
            //StartOfWeek = DayOfWeek.Sunday
            //diff = WednesDay - Sunday = 4
            //-> StartOfWeek = WesnesDay.AddDays(-diff) = Sunday, 2011/01/16

            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-diff);
        }

        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek endOfWeek)
        {
            //--------Calculation Description
            //dt = Today = WesnesDay, 2011/01/19
            //EndOfWeek = DayOfWeek.Saturday
            //diff = Saturday - WesnesDay = 3
            //-> EndOfWeek = WesnesDay.AddDays(diff) = Saturday, 2011/01/22

            int diff = endOfWeek - dt.DayOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(diff);
        }

        public static string ToShortPattern(this DateTime? dateTime)
        {
            if (dateTime == null) { return String.Empty; }
            return dateTime.Value.ToString(Utility.info.DateTimeFormat.ShortDatePattern);
        }

        //public static string ToFullDateTimePattern(this DateTime? dateTime)
        //{
        //    if (dateTime == null) { return String.Empty; }
        //    return dateTime.Value.ToString(Utility.info.DateTimeFormat.FullDateTimePattern);
        //}

        public static string ToTimePattern(this DateTime? dateTime)
        {
            if (dateTime == null) { return String.Empty; }
            return dateTime.Value.ToString(Utility.info.DateTimeFormat.ShortTimePattern);
        }

        public static string ToShortPattern(this DateTime dateTime)
        {
            if (dateTime == null || dateTime.Year == 1) { return String.Empty; }
            return dateTime.ToString(Utility.info.DateTimeFormat.ShortDatePattern);
        }

        public static string ToFullDateTimePattern(this DateTime dateTime)
        {
            if (dateTime == null) { return String.Empty; }
            return dateTime.ToString(Utility.info.DateTimeFormat.FullDateTimePattern);
        }

        public static string ToTimePattern(this DateTime dateTime)
        {
            if (dateTime == null) { return String.Empty; }
            return dateTime.ToString(Utility.info.DateTimeFormat.ShortTimePattern);
        }

        public static string ToSQLPattern(this DateTime? dateTime)
        {
            if (dateTime == null) { return String.Empty; }
            return dateTime.Value.ToString(Utility.DateSQLPattern);
        }

        public static string ToSQLPattern(this DateTime dateTime)
        {
            if (dateTime == null || dateTime.Year == 1) { return String.Empty; }
            return dateTime.ToString(Utility.DateSQLPattern);
        }

        public static string ToReportPattern(this DateTime? dateTime)
        {
            if (dateTime == null) { return String.Empty; }
            return dateTime.Value.ToString(Utility.DateReportPattern);
        }

        public static string ToReportPattern(this DateTime dateTime)
        {
            if (dateTime == null || dateTime.Year == 1) { return String.Empty; }
            return dateTime.ToString(Utility.DateReportPattern);
        }

        public static string ToOSBPattern(this DateTime dateTime)
        {
            if (dateTime == null || dateTime.Year == 1) { return String.Empty; }
            return dateTime.ToString("YYYYMMDD");
        }

        /// <summary>
        /// Adds the time to the DateTime object to move it to END of day.
        /// </summary>
        /// <param name="dateTime">The DateTime object to add time </param>
        /// <returns></returns>
        public static DateTime AddTimeToTheEndOfDay(this DateTime dateTime)
        {
            var result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            return result.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        /// <summary>
        /// Adds/Subtract the time to the DateTime object to move it to START of day.
        /// </summary>
        /// <param name="dateTime">The DateTime object to add/subtract time</param>
        /// <returns></returns>
        public static DateTime AddTimeToTheStartOfDay(this DateTime dateTime)
        {
            var result = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            return result;
        }
    }
}