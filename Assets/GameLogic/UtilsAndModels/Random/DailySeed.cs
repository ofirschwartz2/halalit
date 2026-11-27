using System;
using System.Globalization;
using UnityEngine;

namespace Assets.Utils
{
    public static class DailySeed
    {
        private const string IANA_ISRAEL_ZONE_ID = "Asia/Jerusalem";
        private const string WINDOWS_ISRAEL_ZONE_ID = "Israel Standard Time";
        private const int FALLBACK_ISRAEL_UTC_OFFSET_HOURS = 2;

        private static TimeZoneInfo _israelZone;
        private static bool _isIsraelZoneResolved;

        // The daily follows Israel time, so the day changes at midnight in Israel, summer and winter
        public static DateTime GetToday()
        {
            return GetIsraelDate(DateTime.UtcNow);
        }

        // An Unspecified Kind counts as UTC
        public static DateTime GetIsraelDate(DateTime utcInstant)
        {
            DateTime utc = utcInstant.Kind == DateTimeKind.Local ? utcInstant.ToUniversalTime() : utcInstant;
            TimeZoneInfo israelZone = GetIsraelZone();

            if (israelZone == null)
            {
                return utc.AddHours(FALLBACK_ISRAEL_UTC_OFFSET_HOURS).Date;
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utc, israelZone).Date;
        }

        public static int GetSeed(DateTime day)
        {
            return GetHash(GetDayNumber(day));
        }

        // Days since 1 Jan of year 1, so never below 0. The time of day is ignored
        public static int GetDayNumber(DateTime day)
        {
            return (int)(new DateTime(day.Year, day.Month, day.Day).Ticks / TimeSpan.TicksPerDay);
        }

        // The daily's hash of a day number. Daily Twist (#273) also hashes its block numbers with it
        public static int GetHash(int number)
        {
            unchecked
            {
                uint h = (uint)number;
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

        private static TimeZoneInfo GetIsraelZone()
        {
            if (!_isIsraelZoneResolved)
            {
                _israelZone = FindTimeZoneOrNull(IANA_ISRAEL_ZONE_ID) ?? FindTimeZoneOrNull(WINDOWS_ISRAEL_ZONE_ID);
                _isIsraelZoneResolved = true;

                if (_israelZone == null)
                {
                    Debug.LogWarning($"DailySeed: Israel time zone not found on this device, using a fixed UTC+{FALLBACK_ISRAEL_UTC_OFFSET_HOURS}. In summer the daily changes at 01:00 Israel time.");
                }
            }

            return _israelZone;
        }

        // Mono and IL2CPP do not all throw TimeZoneNotFoundException, so any exception means not found
        private static TimeZoneInfo FindTimeZoneOrNull(string zoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(zoneId);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
