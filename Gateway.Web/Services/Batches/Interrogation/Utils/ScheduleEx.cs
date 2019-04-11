using System;
using System.Collections.Generic;
using System.Linq;
using NCrontab;

namespace Gateway.Web.Services.Batches.Interrogation.Utils
{
    public static class ScheduleEx
    {
        public static bool WillCronTriggerBetween(this string cron, DateTime start, DateTime end)
        {
            var occurrences = GetCronOccurrencesBetween(cron, start, end);
            return occurrences.Any();
        }

        public static IEnumerable<DateTime> GetCronOccurrencesBetween(this string cron, DateTime start, DateTime end)
        {
            var cronSchedule = CrontabSchedule.Parse(cron);
            var occurrences = cronSchedule.GetNextOccurrences(start, end);
            return occurrences;
        }

    }
}
