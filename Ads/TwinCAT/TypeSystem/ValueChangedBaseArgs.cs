namespace TwinCAT.TypeSystem
{
    using System;

    public class ValueChangedBaseArgs : EventArgs
    {
        public readonly ISymbol Symbol;
        public readonly DateTime UtcRtime = DateTime.MinValue;
        public readonly DateTime UtcLocalSystemTime = DateTime.MinValue;

        protected ValueChangedBaseArgs(ISymbol symbol, DateTime rtUtcTimeStamp, DateTime localUtcTimeStamp)
        {
            this.Symbol = symbol;
            this.UtcRtime = rtUtcTimeStamp;
            this.UtcLocalSystemTime = localUtcTimeStamp;
        }
    }
}

