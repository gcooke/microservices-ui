﻿using System;

namespace Gateway.Web.Utils
{
    public class CronTabExpression
    {
        private readonly string _minute;
        private readonly string _hours;
        private readonly string _dayOfMonth;
        private readonly string _month;
        private readonly string _dayOfWeek;

        public CronTabExpression(string minute, string hours, string dayOfMonth, string month, string dayOfWeek)
        {
            _minute = minute;
            _hours = hours;
            _dayOfMonth = dayOfMonth;
            _month = month;
            _dayOfWeek = dayOfWeek;
        }

        public override string ToString()
        {
            return $"{_minute} {_hours} {_dayOfMonth} {_month} {_dayOfWeek}";
        }

        public static CronTabExpression Parse(string cronExpression)
        {
            var parts = cronExpression.Split(' ');

            if (parts.Length != 5)
                throw new Exception("Invalid cron expression. Expression must contain 5 parts -  {minute} {hour} {day of month} {month} {day of week}");

            return new CronTabExpression(parts[0], parts[1], parts[2], parts[3], parts[4]);
        }
    }
}