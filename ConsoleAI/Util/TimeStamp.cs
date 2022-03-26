using System;
using System.Collections.Generic;
using System.Text;

namespace AIProject
{
    public static class TimeStamp
    {
        public static TimeSpan time_span;

        public static void Start()
        {
            time_span = GetTimeSpan();
        }

        public static TimeSpan GetTimeSpan()
        {
            DateTime nowTime = DateTime.Now;
            DateTime severTime = DateTime.UtcNow;
            return nowTime - severTime;
        }

        public static string GetUnixTimeStamp()
        {
            TimeSpan time = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            return Math.Floor(time.TotalSeconds).ToString();
        }
    }
}
