namespace TwinCAT.PlcOpen
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class DATE : DateBase
    {
        public DATE()
        {
        }

        public DATE(DateTime date) : base(date)
        {
        }

        public DATE(long dateValue) : base(dateValue)
        {
        }

        public DATE(uint dateValue) : base(dateValue)
        {
        }

        public DATE(int year, int month, int day) : base(new DateTime(year, month, day))
        {
        }

        public static DATE Parse(string s)
        {
            DATE date = null;
            if (!TryParse(s, out date))
            {
                throw new FormatException("Cannot parse DATE object!");
            }
            return date;
        }

        protected override long ParseToTicks(string s)
        {
            uint ticks = 0;
            if (!PlcOpenDateConverter.TryParseToTicks(s, out ticks))
            {
                throw new FormatException();
            }
            return (long) ticks;
        }

        public override string ToString() => 
            PlcOpenDateConverter.ToString(base.internalDateValue);

        public static bool TryParse(string s, out DATE date)
        {
            uint ticks = 0;
            if (PlcOpenDateConverter.TryParseToTicks(s, out ticks))
            {
                date = new DATE(ticks);
                return true;
            }
            date = null;
            return false;
        }
    }
}

