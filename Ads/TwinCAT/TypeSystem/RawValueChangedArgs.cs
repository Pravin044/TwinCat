namespace TwinCAT.TypeSystem
{
    using System;

    public class RawValueChangedArgs : ValueChangedBaseArgs
    {
        public readonly byte[] Value;

        internal RawValueChangedArgs(ISymbol symbol, byte[] value, DateTime rtTimeStamp, DateTime localTimeStamp) : base(symbol, rtTimeStamp, localTimeStamp)
        {
            this.Value = value;
        }
    }
}

