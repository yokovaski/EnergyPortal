using System;

namespace DatabaseInterface.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ConvertToTimeZone(this DateTime dateTime, string timeZoneId)
        {
            var timeZoneInfo = timeZoneId.GetTimeZoneInfo();
            
            return timeZoneInfo.BaseUtcOffset != TimeZoneInfo.Utc.BaseUtcOffset 
                ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo) 
                : dateTime;
        }
        
        public static DateTime ConvertToUtc(this DateTime dateTime, string timeZoneId)
        {
            var timeZoneInfo = timeZoneId.GetTimeZoneInfo();
            
            return timeZoneInfo.BaseUtcOffset != TimeZoneInfo.Utc.BaseUtcOffset 
                ? TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZoneInfo) 
                : dateTime;
        }

        public static bool IsLastDayOfMonth(this DateTime dateTime)
        {
            var firstOfThisMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstOfThisMonth.AddMonths(1).AddDays(-1).Day == dateTime.Day;
        }

        public static bool IsLastDayOfYear(this DateTime dateTime)
        {
            return dateTime.Month == 12 && dateTime.Day == 31;
        }

        public static bool IsThisMonth(this DateTime dateTime, string timeZoneId = null)
        {
            var nowDate = DateTime.UtcNow.ConvertToTimeZone(timeZoneId).Date;
            return dateTime.Year == nowDate.Year && dateTime.Month == nowDate.Month;
        }

        public static bool IsThisYear(this DateTime dateTime, string timeZoneId = null)
        {
            var nowDate = DateTime.UtcNow.ConvertToTimeZone(timeZoneId).Date;
            return dateTime.Year == nowDate.Year;
        }
    }
}