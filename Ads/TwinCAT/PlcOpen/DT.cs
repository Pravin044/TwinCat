namespace TwinCAT.PlcOpen
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public sealed class DT : DateBase
    {
        public DT()
        {
        }

        public DT(DateTime date) : base(date)
        {
        }

        public DT(long dateValue) : base(dateValue)
        {
        }

        public DT(uint dateValue) : base(dateValue)
        {
        }

        public DT(int year, int month, int day, int hour, int minute, int second) : base(new DateTime(year, month, day, hour, minute, second))
        {
        }

        public static DT Parse(string s)
        {
            DT dt = null;
            if (!TryParse(s, out dt))
            {
                throw new FormatException("Cannot parse DT object!");
            }
            return dt;
        }

        protected override long ParseToTicks(string s)
        {
            uint ticks = 0;
            if (!PlcOpenDTConverter.TryParseToTicks(s, out ticks))
            {
                throw new FormatException();
            }
            return (long) ticks;
        }

        public override string ToString() => 
            PlcOpenDTConverter.TicksToString(base.internalDateValue);

        public static bool TryParse(string s, out DT dt) => 
            PlcOpenDTConverter.TryParse(s, out dt);
    }
}

