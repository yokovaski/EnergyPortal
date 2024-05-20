using System;
using System.Collections.Generic;
using System.Linq;
using EnergyPortal.Controllers.WebApi;
using EnergyPortal.Models;

namespace EnergyPortal.Extensions;

public static class OutMetricModelExtensions
{
    public static List<OutMetricModel> FillMissing(this List<OutMetricModel> metrics, TimeGroup timeGroup,
        DateTime from, DateTime to, bool showRealLast)
    {
        if (timeGroup == TimeGroup.None)
        {
            return metrics;
        }
        
        var dateTime = from.GetNormalizedDateTime(timeGroup);
        var lastDateTime = dateTime;
        
        var allTimes = new Dictionary<DateTime, OutMetricModel>();

        if (!metrics.Any())
        {
            for (var time = dateTime; time <= to; time = time.GetNextDateTime(timeGroup))
            {
                allTimes.Add(time, new OutMetricModel { DateTime = time });
            }

            return allTimes.Values.ToList();
        }

        foreach (var metric in metrics)
        {
            metric.DateTime = metric.DateTime.GetNormalizedDateTime(timeGroup);
            
            while (dateTime < metric.DateTime &&
                   GetAbsoluteDifferenceInSeconds(metric.DateTime, dateTime) >
                   GetAbsoluteDifferenceInSeconds(dateTime, dateTime.GetNextDateTime(timeGroup)))
            {
                allTimes.TryAdd(dateTime, new OutMetricModel { DateTime = dateTime });
                dateTime = dateTime.GetNextDateTime(timeGroup);
            }

            _ = allTimes.TryAdd(metric.DateTime, metric);
            lastDateTime = metric.DateTime;

            if (dateTime < lastDateTime)
            {
                dateTime = lastDateTime.GetNextDateTime(timeGroup);
            }
        }

        while (showRealLast && lastDateTime < to)
        {
            lastDateTime = lastDateTime.GetNextDateTime(timeGroup);
            allTimes.Add(lastDateTime, new OutMetricModel { DateTime = lastDateTime });
        }

        return allTimes.Values.ToList();
    }

    private static long GetAbsoluteDifferenceInSeconds(DateTime first, DateTime second)
    {
        return (long) Math.Abs((first - second).TotalSeconds);
    }

    private static DateTime GetNextDateTime(this DateTime dateTime, TimeGroup timeGroup)
    {
        return timeGroup switch
        {
            TimeGroup.TenSeconds => dateTime.AddSeconds(10),
            TimeGroup.Minutes => dateTime.AddMinutes(1),
            TimeGroup.Hours => dateTime.AddHours(1),
            TimeGroup.Days => dateTime.AddDays(1),
            TimeGroup.Weeks => dateTime.AddDays(7),
            TimeGroup.Months => dateTime.AddMonths(1),
            TimeGroup.Years => dateTime.AddYears(1),
            TimeGroup.None => throw new NotSupportedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(timeGroup), timeGroup, null)
        };
    }

    private static DateTime GetNormalizedDateTime(this DateTime dateTime, TimeGroup timeGroup)
    {
        return timeGroup switch
        {
            TimeGroup.TenSeconds => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Kind),
            TimeGroup.Minutes => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, dateTime.Kind),
            TimeGroup.Hours => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, dateTime.Kind),
            TimeGroup.Days => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind),
            TimeGroup.Weeks => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Kind),
            TimeGroup.Months => new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Kind),
            TimeGroup.Years => new DateTime(dateTime.Year, 1, 1, 0, 0, 0, dateTime.Kind),
            TimeGroup.None => dateTime,
            _ => throw new ArgumentOutOfRangeException(nameof(timeGroup), timeGroup, null)
        };
    }
}