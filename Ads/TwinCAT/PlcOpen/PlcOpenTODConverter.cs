namespace TwinCAT.PlcOpen
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class PlcOpenTODConverter
    {
        public static TOD Create(long ticks) => 
            new TOD(ticks);

        public static TOD Create(TimeSpan value) => 
            new TOD(value);

        public static TOD Create(uint ticks) => 
            new TOD(ticks);

        public static byte[] GetBytes(TOD tod) => 
            BitConverter.GetBytes(tod.Ticks);

        internal static uint ObjectToTicks(object val)
        {
            uint ticks = 0;
            switch (val)
            {
                case (TOD _):
                    ticks = ((TOD) val).Ticks;
                    break;

                case (TimeSpan _):
                    ticks = ToTicks((TimeSpan) val);
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

        public static string ToString(TimeSpan time) => 
            $"{((time.Days * 0x18) + time.Hours)}:{time.Minutes:D2}:{time.Seconds:D2}.{time.Milliseconds}";

        internal static string ToString(uint ticks) => 
            ToString(PlcOpenTimeConverter.MillisecondsToTimeSpan(ticks));

        internal static uint ToTicks(TimeSpan span) => 
            PlcOpenTimeConverter.ToMilliseconds(span);

        public static TimeSpan ToTimeSpan(uint ticks) => 
            PlcOpenTimeConverter.MillisecondsToTimeSpan(ticks);

        public static bool TryConvert(object source, out TOD timeOfDay)
        {
            timeOfDay = null;
            if (source.GetType() == typeof(long))
            {
                timeOfDay = Create((long) source);
            }
            else if (source.GetType() == typeof(uint))
            {
                timeOfDay = Create((uint) source);
            }
            else if (source.GetType() == typeof(TimeSpan))
            {
                timeOfDay = Create((TimeSpan) source);
            }
            else if (source.GetType() == typeof(string))
            {
                TryParse((string) source, out timeOfDay);
            }
            return (timeOfDay != null);
        }

        public static bool TryConvert(TOD date, Type targetType, out object targetValue)
        {
            targetValue = null;
            if (targetType == typeof(TimeSpan))
            {
                targetValue = date.Time;
            }
            else if (targetType == typeof(uint))
            {
                targetValue = date.Ticks;
            }
            else if (targetType == typeof(long))
            {
                targetValue = date.Ticks;
            }
            else if (targetType == typeof(string))
            {
                targetValue = date.ToString();
            }
            return (targetValue != null);
        }

        public static bool TryParse(string s, out TOD tod)
        {
            uint num;
            TimeSpan span;
            if (TryParseToTicks(s, out num))
            {
                tod = new TOD(num);
                return true;
            }
            if (PlcOpenDateConverterBase.TryParseTimeSpan(s, out span))
            {
                tod = new TOD(span);
                return true;
            }
            tod = null;
            return false;
        }

        internal static bool TryParseToTicks(string s, out uint ticks)
        {
            Match match = new Regex(@"(?<hour>\d+)?(:(?<min>\d+)(:(?<sec>\d+)(\.(?<msec>\d+)?)?)?)?", (RegexOptions) RegexOptions.Compiled).Match(s);
            if (!match.Success || (match.Length != s.Length))
            {
                ticks = 0;
                return false;
            }
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = 0.0;
            if (match.get_Groups().get_Item("hour").Length > 0)
            {
                num = double.Parse(match.get_Groups().get_Item("hour").Value);
            }
            if (match.get_Groups().get_Item("min").Length > 0)
            {
                num2 = double.Parse(match.get_Groups().get_Item("min").Value);
            }
            if (match.get_Groups().get_Item("sec").Length > 0)
            {
                num3 = double.Parse(match.get_Groups().get_Item("sec").Value);
            }
            if (match.get_Groups().get_Item("msec").Length > 0)
            {
                num4 = double.Parse(match.get_Groups().get_Item("msec").Value);
            }
            ticks = (uint) ((((num * 3600000.0) + (num2 * 60000.0)) + (num3 * 1000.0)) + num4);
            return true;
        }
    }
}

