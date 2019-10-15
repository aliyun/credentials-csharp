using System;

namespace Aliyun.Credentials.Utils
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime DayZero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetTimeMillis(this DateTime d)
        {
            return (long) (d - DayZero).TotalMilliseconds;
        }
    }
}
