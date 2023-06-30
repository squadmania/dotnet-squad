using System;

namespace Squadmania.Squad.Rcon
{
    public static class TimeSpanExtensions
    {
        public static string ToBanPeriod(
            this TimeSpan timeSpan
        )
        {
            if (timeSpan <= TimeSpan.Zero)
            {
                return "0";
            }

            return timeSpan < TimeSpan.FromDays(1)
                ? $"{Convert.ToInt64(timeSpan.TotalSeconds):D}s"
                : $"{Convert.ToInt64(timeSpan.TotalDays):D}d";
        }
    }
}