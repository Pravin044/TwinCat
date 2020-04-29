namespace TwinCAT.PlcOpen
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public class PlcOpenDTConverter : PlcOpenDateConverterBase
    {
        public static DT Create(DateTime value) => 
            new DT(value);

        public static DT Create(long ticks) => 
            new DT(ticks);

        public static DT Create(uint ticks) => 
            new DT(ticks);

        public static string DateTimeToString(DateTime date) => 
            $"{date.Year}-{date.Month:D2}-{date.Day:D2}-{date.Hour:D2}:{date.Minute:D2}:{date.Second:D2}";

        internal static byte[] GetBytes(DT dt) => 
            BitConverter.GetBytes(dt.Ticks);

        internal static uint ObjectToTicks(object val)
        {
            uint ticks = 0;
            switch (val)
            {
                case (DT _):
                    ticks = ((DT) val).Ticks;
                    break;

                case (DateTime _):
                    ticks = ToTicks((DateTime) val);
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

        internal static string TicksToString(uint ticks) => 
            DateTimeToString(ToDateTime(ticks));

        public static bool TryConvert(object source, out DT timeOfDay)
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
            else if (source.GetType() == typeof(DateTime))
            {
                timeOfDay = Create((DateTime) source);
            }
            else if (source.GetType() == typeof(string))
            {
                TryParse((string) source, out timeOfDay);
            }
            return (timeOfDay != null);
        }

        public static bool TryConvert(DT date, Type targetType, out object targetValue)
        {
            targetValue = null;
            if (targetType == typeof(DateTime))
            {
                targetValue = date.Date;
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

        private static bool TryParse(string str, out int value) => 
            int.TryParse(str, out value);

        public static bool TryParse(string s, out DT dt)
        {
            DateTime time;
            uint ticks = 0;
            if (TryParseToTicks(s, out ticks))
            {
                dt = new DT(ticks);
                return true;
            }
            if (TryParseDateTime(s, out time))
            {
                dt = new DT(time);
                return true;
            }
            dt = null;
            return false;
        }

        internal static bool TryParseToTicks(string s, out uint ticks)
        {
            Match match = new Regex(@"(?<year>(?:19[7-9]\d)|(?:2\d\d\d))-(?<month>(?:0?[1-9])|(?:1[0-2]))-(?<day>[1-2]\d|3[0-1]|0?[1-9])(?:-(?<hour>\d?\d):(?<minute>\d\d)(?::(?<second>\d\d)))?", (RegexOptions) RegexOptions.Compiled).Match(s);
            if (!match.Success || (match.Length != s.Length))
            {
                ticks = 0;
                return false;
            }
            int year = 0;
            int month = 0;
            int day = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            year = int.Parse(match.get_Groups().get_Item("year").Value);
            month = int.Parse(match.get_Groups().get_Item("month").Value);
            day = int.Parse(match.get_Groups().get_Item("day").Value);
            if ((match.get_Groups().get_Item("hour") != null) && TryParse(match.get_Groups().get_Item("hour").Value, out num4))
            {
                TryParse(match.get_Groups().get_Item("minute").Value, out num5);
                TryParse(match.get_Groups().get_Item("second").Value, out num6);
            }
            ticks = ObjectToTicks(new DateTime(year, month, day, num4, num5, num6));
            return true;
        }
    }
}

