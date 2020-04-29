namespace TwinCAT.PlcOpen
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class PlcOpenTimeConverter
    {
        public static LTIME CreateLTime(TimeSpan timeSpan) => 
            new LTIME(timeSpan);

        public static LTIME CreateLTime(ulong ticks) => 
            new LTIME(ticks);

        public static TIME CreateTime(TimeSpan value) => 
            new TIME(value);

        public static TIME CreateTime(uint ticks) => 
            new TIME(ticks);

        public static void FromNanoseconds(ulong totalNanoseconds, out int days, out int hours, out int minutes, out int seconds, out int msec, out int usec, out int nsec)
        {
            ulong num = totalNanoseconds / ((ulong) 0x4e94914f0000L);
            ulong num7 = totalNanoseconds % ((ulong) 0x4e94914f0000L);
            ulong num2 = num7 / ((ulong) 0x34630b8a000L);
            num7 = num7 % ((ulong) 0x34630b8a000L);
            ulong num3 = num7 / ((ulong) 0xdf8475800L);
            num7 = num7 % ((ulong) 0xdf8475800L);
            ulong num4 = num7 / ((ulong) 0x3b9aca00L);
            num7 = num7 % ((ulong) 0x3b9aca00L);
            ulong num5 = num7 / ((ulong) 0xf4240L);
            num7 = num7 % ((ulong) 0xf4240L);
            ulong num6 = num7 / ((ulong) 0x3e8L);
            num7 = num7 % ((ulong) 0x3e8L);
            days = (int) num;
            hours = (int) num2;
            minutes = (int) num3;
            seconds = (int) num4;
            msec = (int) num5;
            usec = (int) num6;
            nsec = (int) num7;
        }

        public static byte[] GetBytes(LTIME time) => 
            BitConverter.GetBytes(time.Ticks);

        public static byte[] GetBytes(TIME time) => 
            BitConverter.GetBytes(time.Ticks);

        internal static string MillisecondsToString(uint milliseconds)
        {
            TimeSpan span = MillisecondsToTimeSpan(milliseconds);
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            if (span.Days > 0)
            {
                flag = true;
                object[] args = new object[] { span.Days };
                builder.AppendFormat(null, "{0}d", args);
            }
            if (flag || (span.Hours > 0))
            {
                flag = true;
                object[] args = new object[] { span.Hours };
                builder.AppendFormat(null, "{0}h", args);
            }
            if (flag || (span.Minutes > 0))
            {
                flag = true;
                object[] args = new object[] { span.Minutes };
                builder.AppendFormat(null, "{0}m", args);
            }
            if (flag || (span.Seconds > 0))
            {
                flag = true;
                object[] args = new object[] { span.Seconds };
                builder.AppendFormat(null, "{0}s", args);
            }
            if (flag || (span.Milliseconds > 0))
            {
                flag = true;
                object[] args = new object[] { span.Milliseconds };
                builder.AppendFormat(null, "{0}ms", args);
            }
            return builder.ToString();
        }

        public static TimeSpan MillisecondsToTimeSpan(uint milliseconds)
        {
            if ((milliseconds < 0) || (milliseconds > uint.MaxValue))
            {
                throw new ArgumentOutOfRangeException();
            }
            uint num5 = milliseconds % 0x5265c00;
            num5 = num5 % 0x36ee80;
            num5 = num5 % 0xea60;
            return new TimeSpan((int) (milliseconds / 0x5265c00), (int) (num5 / 0x36ee80), (int) (num5 / 0xea60), (int) (num5 / 0x3e8), (int) (num5 % 0x3e8));
        }

        internal static string NanosecondsToString(ulong nanoseconds)
        {
            int num;
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            int num7;
            FromNanoseconds(nanoseconds, out num, out num2, out num3, out num4, out num5, out num6, out num7);
            StringBuilder builder = new StringBuilder();
            bool flag = false;
            if (num > 0)
            {
                flag = true;
                object[] args = new object[] { num };
                builder.AppendFormat(null, "{0}d", args);
            }
            if (flag || (num2 > 0))
            {
                flag = true;
                object[] args = new object[] { num2 };
                builder.AppendFormat(null, "{0}h", args);
            }
            if (flag || (num3 > 0))
            {
                flag = true;
                object[] args = new object[] { num3 };
                builder.AppendFormat(null, "{0}m", args);
            }
            if (flag || (num4 > 0))
            {
                flag = true;
                object[] args = new object[] { num4 };
                builder.AppendFormat(null, "{0}s", args);
            }
            if (flag || (num5 > 0))
            {
                flag = true;
                object[] args = new object[] { num5 };
                builder.AppendFormat(null, "{0}ms", args);
            }
            if (flag || (num6 > 0))
            {
                flag = true;
                object[] args = new object[] { num6 };
                builder.AppendFormat(null, "{0}us", args);
            }
            if (flag || (num7 > 0))
            {
                flag = true;
                object[] args = new object[] { num7 };
                builder.AppendFormat(null, "{0}ns", args);
            }
            return builder.ToString();
        }

        public static TimeSpan NanosecondsToTimeSpan(ulong nanoseconds) => 
            new TimeSpan((long) (nanoseconds / ((long) 100)));

        internal static uint ObjectToMilliseconds(object val)
        {
            uint ticks = 0;
            switch (val)
            {
                case (TIME _):
                    ticks = ((TIME) val).Ticks;
                    break;

                case (TimeSpan _):
                    ticks = ToMilliseconds((TimeSpan) val);
                    break;

                case (uint _):
                    ticks = (uint) val;
                    break;

                case (string _):
                    break;

                default:
                    throw new NotSupportedException();
                    break;
            }
            return ticks;
        }

        internal static uint ToMilliseconds(string s)
        {
            uint milliseconds = 0;
            if (!TryParseToMilliseconds(s, out milliseconds))
            {
                throw new FormatException();
            }
            return milliseconds;
        }

        public static uint ToMilliseconds(TimeSpan timeSpan)
        {
            if ((timeSpan < new TimeSpan(0L)) || (timeSpan > new TimeSpan(0, 0, 0x1179e, 0x2f, 0x127)))
            {
                throw new ArgumentOutOfRangeException("TimeSpan is out of range for PLCOpen TIME data type!");
            }
            return (uint) (((((timeSpan.Days * 0x5265c00) + (timeSpan.Hours * 0x36ee80)) + (timeSpan.Minutes * 0xea60)) + (timeSpan.Seconds * 0x3e8)) + timeSpan.Milliseconds);
        }

        internal static ulong ToNanoseconds(string s)
        {
            ulong nanoseconds = 0UL;
            if (!TryParseToNanoseconds(s, out nanoseconds))
            {
                throw new FormatException();
            }
            return nanoseconds;
        }

        public static ulong ToNanoseconds(TimeSpan timeSpan)
        {
            if ((timeSpan < new TimeSpan(0L)) || (timeSpan > new TimeSpan(0x341ff, 0x17, 0x22, 0x21, 0x2c5)))
            {
                throw new ArgumentOutOfRangeException("TimeSpan is out of range for PLCOpen LTIME data type!");
            }
            return (ulong) (timeSpan.Ticks * 100);
        }

        internal static ulong ToNanoseconds(int days, int hours, int minutes, int seconds, int milliseconds, int microseconds, int nanoseconds) => 
            ((ulong) (((((((days * 0x4e94914f0000L) + (hours * 0x34630b8a000L)) + (minutes * 0xdf8475800L)) + (seconds * 0x3b9aca00L)) + (milliseconds * 0xf4240L)) + (microseconds * 0x3e8L)) + nanoseconds));

        public static TimeSpan ToTimeSpan(string str)
        {
            uint milliseconds = 0;
            if (!TryParseToMilliseconds(str, out milliseconds))
            {
                throw new FormatException();
            }
            return MillisecondsToTimeSpan(milliseconds);
        }

        public static bool TryConvert(object source, out LTIME time)
        {
            time = null;
            if (source.GetType() == typeof(long))
            {
                time = CreateLTime((ulong) source);
            }
            else if (source.GetType() == typeof(ulong))
            {
                time = CreateLTime((ulong) source);
            }
            else if (source.GetType() == typeof(uint))
            {
                time = CreateLTime((ulong) source);
            }
            else if (source.GetType() == typeof(int))
            {
                time = CreateLTime((ulong) source);
            }
            else if (source.GetType() == typeof(TimeSpan))
            {
                time = new LTIME((TimeSpan) source);
            }
            else if (source.GetType() == typeof(string))
            {
                TryParse((string) source, out time);
            }
            return (time != null);
        }

        public static bool TryConvert(object source, out TIME time)
        {
            time = null;
            if (source.GetType() == typeof(long))
            {
                time = CreateTime((uint) source);
            }
            else if (source.GetType() == typeof(uint))
            {
                time = CreateTime((uint) source);
            }
            else if (source.GetType() == typeof(int))
            {
                time = CreateTime((uint) source);
            }
            else if (source.GetType() == typeof(TimeSpan))
            {
                time = CreateTime((TimeSpan) source);
            }
            else if (source.GetType() == typeof(string))
            {
                TryParse((string) source, out time);
            }
            return (time != null);
        }

        public static bool TryConvert(LTimeBase time, Type targetType, out object targetValue)
        {
            targetValue = null;
            if (targetType == typeof(TimeSpan))
            {
                targetValue = time.Time;
            }
            else if (targetType == typeof(long))
            {
                targetValue = (long) time.Ticks;
            }
            else if (targetType == typeof(ulong))
            {
                targetValue = time.Ticks;
            }
            else if (targetType == typeof(string))
            {
                targetValue = time.ToString();
            }
            return (targetValue != null);
        }

        public static bool TryConvert(TimeBase time, Type targetType, out object targetValue)
        {
            targetValue = null;
            if (targetType == typeof(TimeSpan))
            {
                targetValue = time.Time;
            }
            else if (targetType == typeof(uint))
            {
                targetValue = time.Ticks;
            }
            else if (targetType == typeof(int))
            {
                targetValue = (int) time.Ticks;
            }
            else if (targetType == typeof(long))
            {
                targetValue = time.Ticks;
            }
            else if (targetType == typeof(ulong))
            {
                targetValue = time.Ticks;
            }
            else if (targetType == typeof(string))
            {
                targetValue = time.ToString();
            }
            return (targetValue != null);
        }

        public static bool TryParse(string s, out LTIME time)
        {
            TimeSpan span;
            ulong nanoseconds = 0UL;
            if (TryParseToNanoseconds(s, out nanoseconds))
            {
                time = new LTIME(nanoseconds);
                return true;
            }
            if (PlcOpenDateConverterBase.TryParseTimeSpan(s, out span))
            {
                time = new LTIME(span);
                return true;
            }
            time = null;
            return false;
        }

        public static bool TryParse(string s, out TIME time)
        {
            TimeSpan span;
            uint milliseconds = 0;
            if (TryParseToMilliseconds(s, out milliseconds))
            {
                time = new TIME(milliseconds);
                return true;
            }
            if (PlcOpenDateConverterBase.TryParseTimeSpan(s, out span))
            {
                time = new TIME(span);
                return true;
            }
            time = null;
            return false;
        }

        internal static bool TryParseToMilliseconds(string s, out uint milliseconds)
        {
            Match match = new Regex(@"^(?:(?<day>\d+(?:\.d*)?)d_?)?(?:(?<hour>\d+(?:\.d*)?)h_?)?(?:(?<min>\d+(?:\.d*)?)m_?)?(?:(?<sec>\d+(?:\.d*)?)s_?)?(?:(?<msec>\d+(?:\.d*)?)ms_?)?$", (RegexOptions) RegexOptions.Compiled).Match(s);
            if (!match.Success || (match.Length != s.Length))
            {
                milliseconds = 0;
                return false;
            }
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = 0.0;
            double num5 = 0.0;
            if (match.get_Groups().get_Item("day").Length > 0)
            {
                num = double.Parse(match.get_Groups().get_Item("day").Value);
            }
            if (match.get_Groups().get_Item("hour").Length > 0)
            {
                num2 = double.Parse(match.get_Groups().get_Item("hour").Value);
            }
            if (match.get_Groups().get_Item("min").Length > 0)
            {
                num3 = double.Parse(match.get_Groups().get_Item("min").Value);
            }
            if (match.get_Groups().get_Item("sec").Length > 0)
            {
                num4 = double.Parse(match.get_Groups().get_Item("sec").Value);
            }
            if (match.get_Groups().get_Item("msec").Length > 0)
            {
                num5 = double.Parse(match.get_Groups().get_Item("msec").Value);
            }
            milliseconds = (uint) (((((num * 86400000.0) + (num2 * 3600000.0)) + (num3 * 60000.0)) + (num4 * 1000.0)) + num5);
            return true;
        }

        internal static bool TryParseToNanoseconds(string s, out ulong nanoseconds)
        {
            Match match = new Regex(@"^(?:(?<day>\d+(?:\.d*)?)d_?)?(?:(?<hour>\d+(?:\.d*)?)h_?)?(?:(?<min>\d+(?:\.d*)?)m_?)?(?:(?<sec>\d+(?:\.d*)?)s_?)?(?:(?<msec>\d+(?:\.d*)?)ms_?)?(?:(?<usec>\d+(?:\.d*)?)us_?)?(?:(?<nsec>\d+(?:\.d*)?)ns_?)?$", (RegexOptions) RegexOptions.Compiled).Match(s);
            if (!match.Success || (match.Length != s.Length))
            {
                nanoseconds = 0L;
                return false;
            }
            ulong num = 0UL;
            ulong num2 = 0UL;
            ulong num3 = 0UL;
            ulong num4 = 0UL;
            ulong num5 = 0UL;
            ulong num6 = 0UL;
            ulong num7 = 0UL;
            if (match.get_Groups().get_Item("day").Length > 0)
            {
                num = ulong.Parse(match.get_Groups().get_Item("day").Value);
            }
            if (match.get_Groups().get_Item("hour").Length > 0)
            {
                num2 = ulong.Parse(match.get_Groups().get_Item("hour").Value);
            }
            if (match.get_Groups().get_Item("min").Length > 0)
            {
                num3 = ulong.Parse(match.get_Groups().get_Item("min").Value);
            }
            if (match.get_Groups().get_Item("sec").Length > 0)
            {
                num4 = ulong.Parse(match.get_Groups().get_Item("sec").Value);
            }
            if (match.get_Groups().get_Item("msec").Length > 0)
            {
                num5 = ulong.Parse(match.get_Groups().get_Item("msec").Value);
            }
            if (match.get_Groups().get_Item("usec").Length > 0)
            {
                num6 = ulong.Parse(match.get_Groups().get_Item("usec").Value);
            }
            if (match.get_Groups().get_Item("nsec").Length > 0)
            {
                num7 = ulong.Parse(match.get_Groups().get_Item("nsec").Value);
            }
            nanoseconds = ((((((num * ((ulong) 0x4e94914f0000L)) + (num2 * ((ulong) 0x34630b8a000L))) + (num3 * ((ulong) 0xdf8475800L))) + (num4 * ((ulong) 0x3b9aca00L))) + (num5 * ((ulong) 0xf4240L))) + (num6 * ((ulong) 0x3e8L))) + num7;
            return true;
        }
    }
}

