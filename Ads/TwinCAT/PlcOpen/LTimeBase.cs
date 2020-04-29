namespace TwinCAT.PlcOpen
{
    using System;
    using System.ComponentModel;

    public abstract class LTimeBase : IPlcOpenType<TimeSpan, ulong>, IPlcOpenType
    {
        protected ulong internalTimeValue;

        protected LTimeBase()
        {
            this.internalTimeValue = 0UL;
        }

        protected LTimeBase(ulong timeValue)
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
            LTimeBase base2 = (LTimeBase) obj;
            return (this.internalTimeValue == base2.internalTimeValue);
        }

        public override int GetHashCode() => 
            ((0x5b * 10) + ((int) (this.internalTimeValue ^ (this.internalTimeValue >> 0x20))));

        public static ulong TimeToValue(TimeSpan time) => 
            PlcOpenTimeConverter.ToNanoseconds(time);

        public static TimeSpan ValueToTime(long nanoseconds) => 
            ValueToTime((long) ((uint) nanoseconds));

        public static TimeSpan ValueToTime(ulong nanoseconds) => 
            PlcOpenTimeConverter.NanosecondsToTimeSpan(nanoseconds);

        public static int MarshalSize =>
            8;

        public TimeSpan Time =>
            ValueToTime(this.internalTimeValue);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public TimeSpan Value =>
            this.Time;

        public ulong Ticks =>
            this.internalTimeValue;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public Type TicksValueType =>
            typeof(ulong);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public Type ManagedValueType =>
            typeof(TimeSpan);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public object UntypedValue =>
            this.Time;
    }
}

