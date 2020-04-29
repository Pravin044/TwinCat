namespace TwinCAT.PlcOpen
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class LTIME : LTimeBase
    {
        public LTIME()
        {
        }

        public LTIME(long timeValue) : base((ulong) timeValue)
        {
        }

        public LTIME(TimeSpan time)
        {
            base.internalTimeValue = TimeToValue(time);
        }

        public LTIME(ulong timeValue) : base(timeValue)
        {
        }

        public LTIME(int seconds, int milliseconds, int microseconds)
        {
            base.internalTimeValue = PlcOpenTimeConverter.ToNanoseconds(0, 0, 0, seconds, milliseconds, microseconds, 0);
        }

        public LTIME(int seconds, int milliseconds, int microseconds, int nanoseconds)
        {
            base.internalTimeValue = PlcOpenTimeConverter.ToNanoseconds(0, 0, 0, seconds, milliseconds, microseconds, nanoseconds);
        }

        public LTIME(int days, int hours, int minutes, int seconds, int milliseconds, int microseconds, int nanoseconds)
        {
            base.internalTimeValue = PlcOpenTimeConverter.ToNanoseconds(days, hours, minutes, seconds, milliseconds, microseconds, nanoseconds);
        }

        public static LTIME Parse(string str)
        {
            LTIME ret = null;
            if (!TryParse(str, out ret))
            {
                throw new FormatException("Cannot create LTIME DataType!");
            }
            return ret;
        }

        public override string ToString() => 
            PlcOpenTimeConverter.NanosecondsToString(base.internalTimeValue);

        public static bool TryParse(string str, out LTIME ret)
        {
            ulong nanoseconds = 0UL;
            if (PlcOpenTimeConverter.TryParseToNanoseconds(str, out nanoseconds))
            {
                ret = new LTIME(nanoseconds);
                return true;
            }
            ret = null;
            return false;
        }
    }
}

