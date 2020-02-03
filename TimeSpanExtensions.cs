using System;

namespace grasmanek94.Statistics
{
    public static class TimespanExtensions
    {
        public static string ToHumanReadableString(this TimeSpan t)
        {
            if (t.TotalSeconds < 1)
            {
                return $@"{t:s\.ff} seconds";
            }

            if (t.TotalSeconds.E3DP(1))
            {
                return $@"{t:s\.ff} second";
            }

            if (t.TotalMinutes < 1)
            {
                return $@"{t:%s} seconds";
            }

            if (t.TotalMinutes.E3DP(1))
            {
                return $@"{t:%m} minute";
            }

            if (t.TotalHours < 1)
            {
                return $@"{t:%m} minutes";
            }

            if (t.TotalHours.E3DP(1))
            {
                return $@"{t:%h} hour";
            }

            if (t.TotalDays < 1)
            {
                return $@"{t:%h} hours";
            }

            if (t.TotalDays.E3DP(1))
            {
                return $@"{t:%d} day";
            }

            return $@"{t:%d} days";
        }
    }
}
