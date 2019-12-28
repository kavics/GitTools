using System;

namespace GitT
{
    public static class DateTools
    {
        public static string FormatDate(DateTime date)
        {
            return FormatDate(date, DateTime.Now);
        }
        public static string FormatDate(DateTime date, DateTime now)
        {
            var diff = now - date;
            if (diff.TotalSeconds < 60)
                return "just now";
            if (diff.TotalSeconds < 120)
                return "just a minute ago";
            if (diff.TotalMinutes < 60)
                return $"{Math.Round(diff.TotalMinutes)} minutes ago";
            if (diff.TotalHours < 2)
                return "an hour ago";
            if (diff.TotalHours < 5)
                return $"{Math.Round(diff.TotalHours)} hours ago";
            if (now.Year == date.Year && now.Month == date.Month && now.Day == date.Day)
                return $"today at {date:HH:mm}";
            if (now.Year == date.Year && now.Month == date.Month && now.Day == date.Day + 1)
                return $"yesterday at {date:HH:mm}";
            return date.ToString("yyyy-MM-dd HH:mm");
        }

    }
}
