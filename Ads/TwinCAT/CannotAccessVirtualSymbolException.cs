namespace TwinCAT
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    [Serializable]
    public class CannotAccessVirtualSymbolException : SymbolException
    {
        public CannotAccessVirtualSymbolException(ISymbol symbol) : base($"Cannot read/write from/to virtual symbol '{symbol.InstanceName}'!", symbol)
        {
        }
    }
}

