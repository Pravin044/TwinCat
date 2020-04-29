namespace TwinCAT.PlcOpen
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class TOD : TimeBase
    {
        public TOD()
        {
        }

        public TOD(long time) : base(time)
        {
        }

        public TOD(TimeSpan timeSpan)
        {
            base.internalTimeValue = (uint) TimeToValue(timeSpan);
        }

        public TOD(uint time) : base(time)
        {
        }

        public TOD(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            base.internalTimeValue = PlcOpenTODConverter.ToTicks(new TimeSpan(days, hours, minutes, seconds, milliseconds));
        }

        public static TOD Parse(string str)
        {
            TOD ret = null;
            if (!TryParse(str, out ret))
            {
                throw new FormatException("Cannot parse TOD object!");
            }
            return ret;
        }

        public override string ToString() => 
            PlcOpenTODConverter.ToString(base.internalTimeValue);

        public static bool TryParse(string str, out TOD ret) => 
            PlcOpenTODConverter.TryParse(str, out ret);
    }
}

