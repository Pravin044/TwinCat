namespace TwinCAT
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    [Serializable]
    public class InsufficientAccessRights : SymbolException
    {
        public InsufficientAccessRights(IValueSymbol symbol, SymbolAccessRights requested) : base($"The requested rights '{requested}' for symbol '{symbol.InstanceName}' are not sufficient (Current rights: {symbol.AccessRights})!", symbol)
        {
        }
    }
}

