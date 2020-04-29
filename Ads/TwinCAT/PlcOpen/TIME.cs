namespace TwinCAT.PlcOpen
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class TIME : TimeBase
    {
        public TIME()
        {
        }

        public TIME(long timeValue) : base((uint) timeValue)
        {
        }

        public TIME(TimeSpan time)
        {
            base.internalTimeValue = (uint) TimeToValue(time);
        }

        public TIME(uint timeValue) : base(timeValue)
        {
        }

        public TIME(int seconds, int milliseconds)
        {
            base.internalTimeValue = PlcOpenTimeConverter.ToMilliseconds(new TimeSpan(0, 0, seconds, milliseconds));
        }

        public TIME(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            base.internalTimeValue = PlcOpenTimeConverter.ToMilliseconds(new TimeSpan(days, hours, minutes, seconds, milliseconds));
        }

        public static TIME Parse(string str)
        {
            TIME ret = null;
            if (!TryParse(str, out ret))
            {
                throw new FormatException("Cannot create TIME DataType!");
            }
            return ret;
        }

        public override string ToString() => 
            PlcOpenTimeConverter.MillisecondsToString(base.internalTimeValue);

        public static bool TryParse(string str, out TIME ret)
        {
            uint milliseconds = 0;
            if (PlcOpenTimeConverter.TryParseToMilliseconds(str, out milliseconds))
            {
                ret = new TIME(milliseconds);
                return true;
            }
            ret = null;
            return false;
        }
    }
}

