using System;
using System.Globalization;

namespace Assets.Utils
{
    public static class DailySeed
    {
        // Clock choice, Ofir's decision pending
        public static DateTime GetToday()
        {
            return DateTime.UtcNow.Date;
        }

        public static int GetSeed(DateTime day)
        {
            int dayNumber = (int)(new DateTime(day.Year, day.Month, day.Day).Ticks / TimeSpan.TicksPerDay);

            unchecked
            {
                uint h = (uint)dayNumber;
                h ^= h >> 16;
                h *= 0x85EBCA6Bu;
                h ^= h >> 13;
                h *= 0xC2B2AE35u;
                h ^= h >> 16;
                return (int)h;
            }
        }

        public static string GetDateKey(DateTime day)
        {
            return day.ToString("dd-MM-yy", CultureInfo.InvariantCulture);
        }
    }
}
