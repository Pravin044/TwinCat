namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;

    public class ValueChangedArgs : ValueChangedBaseArgs
    {
        public readonly object Value;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ValueChangedArgs(ISymbol symbol, object value, DateTime utcTime) : this(symbol, value, utcTime, utcTime)
        {
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ValueChangedArgs(ISymbol symbol, object value, DateTime rtUtcTimeStamp, DateTime localUtcTimeStamp) : base(symbol, rtUtcTimeStamp, localUtcTimeStamp)
        {
            this.Value = value;
        }
    }
}

