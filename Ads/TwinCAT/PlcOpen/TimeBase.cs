namespace TwinCAT.PlcOpen
{
    using System;
    using System.ComponentModel;

    public abstract class TimeBase : IPlcOpenType<TimeSpan, uint>, IPlcOpenType
    {
        protected uint internalTimeValue;

        protected TimeBase()
        {
            this.internalTimeValue = 0;
        }

        protected TimeBase(long timeValue)
        {
            if ((timeValue < 0L) || (timeValue > 0xffffffffUL))
            {
                throw new ArgumentOutOfRangeException();
            }
            this.internalTimeValue = (uint) timeValue;
        }

        protected TimeBase(uint timeValue)
        {
            this.internalTimeValue = timeValue;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            TimeBase base2 = (TimeBase) obj;
            return (this.internalTimeValue == base2.internalTimeValue);
        }

        public override int GetHashCode() => 
            ((0x5b * 10) + ((int) this.internalTimeValue));

        public static long TimeToValue(TimeSpan time) => 
            ((long) PlcOpenTimeConverter.ToMilliseconds(time));

        public static TimeSpan ValueToTime(long timeValue)
        {
            if ((timeValue < 0L) || (timeValue > 0xffffffffUL))
            {
                throw new ArgumentOutOfRangeException("timeValue");
            }
            return ValueToTime((uint) timeValue);
        }

        public static TimeSpan ValueToTime(uint timeValue) => 
            PlcOpenTimeConverter.MillisecondsToTimeSpan(timeValue);

        public static int MarshalSize =>
            4;

        public virtual TimeSpan Time =>
            ValueToTime(this.internalTimeValue);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public TimeSpan Value =>
            this.Time;

        public uint Ticks =>
            this.internalTimeValue;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public Type TicksValueType =>
            typeof(uint);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public Type ManagedValueType =>
            typeof(TimeSpan);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public object UntypedValue =>
            this.Time;
    }
}

