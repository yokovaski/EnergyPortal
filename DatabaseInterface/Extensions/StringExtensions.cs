using System;

namespace DatabaseInterface.Extensions
{
    public static class StringExtensions
    {
        public static TimeZoneInfo GetTimeZoneInfo(this string timeZoneId)
        {
            if (timeZoneId == null)
                return TimeZoneInfo.Utc;

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
    }
}