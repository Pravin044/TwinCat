namespace TwinCAT.PlcOpen
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    public class PlcOpenDateConverter : PlcOpenDateConverterBase
    {
        public static DATE Create(DateTime value) => 
            new DATE(value);

        public static DATE Create(long ticks) => 
            new DATE(ticks);

        public static DATE Create(uint ticks) => 
            new DATE(ticks);

        internal static byte[] GetBytes(DATE dt) => 
            BitConverter.GetBytes(dt.Ticks);

        internal static uint ObjectToTicks(object val)
        {
            uint ticks = 0;
            switch (val)
            {
                case (DATE _):
                    ticks = ((DATE) val).Ticks;
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

        public static string ToString(DateTime date) => 
            $"{date.Year}-{date.Month:D2}-{date.Day:D2}";

        internal static string ToString(uint ticks) => 
            ToString(ToDateTime(ticks));

        public static bool TryConvert(object source, out DATE timeOfDay)
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

        public static bool TryConvert(DateBase date, Type targetType, out object targetValue)
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

        public static bool TryParse(string str, out DATE date)
        {
            DateTime time;
            uint ticks = 0;
            if (TryParseToTicks(str, out ticks))
            {
                date = new DATE(ticks);
                return true;
            }
            if (TryParseDateTime(str, out time))
            {
                date = new DATE(time);
                return true;
            }
            date = null;
            return false;
        }

        internal static bool TryParseToTicks(string s, out uint ticks)
        {
            Match match = new Regex(@"(?<year>(19[7-9]\d)|(2\d\d\d))-(?<month>(0?[1-9])|(1[0-2]))-(?<day>([1-2]\d)|(3[0-1])|(0?[1-9]))", (RegexOptions) RegexOptions.Compiled).Match(s);
            if (!match.Success || (match.Length != s.Length))
            {
                ticks = 0;
                return false;
            }
            int year = 0;
            year = int.Parse(match.get_Groups().get_Item("year").Value);
            ticks = ObjectToTicks(new DateTime(year, int.Parse(match.get_Groups().get_Item("month").Value), int.Parse(match.get_Groups().get_Item("day").Value)));
            return true;
        }
    }
}

