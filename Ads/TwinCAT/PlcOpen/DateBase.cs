namespace TwinCAT.PlcOpen
{
    using System;
    using System.ComponentModel;

    public abstract class DateBase : IPlcOpenType<DateTime, uint>, IPlcOpenType
    {
        protected uint internalDateValue;

        protected DateBase()
        {
            this.internalDateValue = 0;
        }

        protected DateBase(DateTime date)
        {
            this.internalDateValue = (uint) DateToValue(date);
        }

        protected DateBase(long dateValue)
        {
            if ((dateValue < 0L) || (dateValue > 0xffffffffUL))
            {
                throw new ArgumentOutOfRangeException();
            }
            this.internalDateValue = (uint) dateValue;
        }

        protected DateBase(uint dateValue)
        {
            this.internalDateValue = dateValue;
        }

        public static long DateToValue(DateTime date) => 
            ((long) PlcOpenDateConverterBase.ToTicks(date));

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
            DateBase base2 = (DateBase) obj;
            return (this.internalDateValue == base2.internalDateValue);
        }

        public override int GetHashCode() => 
            ((0x5b * 10) + ((int) this.internalDateValue));

        protected abstract long ParseToTicks(string s);
        public static DateTime ValueToDate(long dateValue)
        {
            if ((dateValue < 0L) || (dateValue > 0xffffffffUL))
            {
                throw new ArgumentOutOfRangeException();
            }
            return PlcOpenDateConverterBase.ToDateTime((uint) dateValue);
        }

        public static DateTime ValueToDate(uint dateValue) => 
            PlcOpenDateConverterBase.ToDateTime(dateValue);

        public static int MarshalSize =>
            4;

        public DateTime Date =>
            ValueToDate(this.internalDateValue);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public DateTime Value =>
            this.Date;

        public uint Ticks =>
            this.internalDateValue;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public Type TicksValueType =>
            typeof(uint);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public Type ManagedValueType =>
            typeof(TimeSpan);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public object UntypedValue =>
            this.Date;
    }
}

