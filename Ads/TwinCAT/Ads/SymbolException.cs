namespace TwinCAT.Ads
{
    using System;
    using TwinCAT.TypeSystem;

    [Serializable]
    public class SymbolException : AdsException
    {
        [NonSerialized]
        public readonly ISymbol Symbol;

        public SymbolException(ISymbol symbol) : this($"Cannot access symbol '{symbol.InstancePath}'!", symbol, null)
        {
        }

        public SymbolException(string message, ISymbol symbol) : base(message)
        {
            this.Symbol = symbol;
        }

        public SymbolException(ISymbol symbol, Exception innerException) : this($"Cannot access symbol '{symbol.InstancePath}'!", symbol, innerException)
        {
        }

        public SymbolException(ISymbol symbol, int errorCode) : this($"Cannot access symbol '{symbol.InstancePath}' (AdsErrorCode: {errorCode:x})!", symbol, null)
        {
        }

        public SymbolException(ISymbol symbol, AdsErrorCode errorCode) : this($"Cannot access symbol '{symbol.InstancePath}' (AdsErrorCode: {errorCode:x})!", symbol, null)
        {
        }

        public SymbolException(string message, ISymbol symbol, Exception innerException) : base(message, innerException)
        {
            this.Symbol = symbol;
        }

        public SymbolException(ISymbol symbol, AdsErrorCode errorCode, Exception innerException) : this($"Cannot access symbol '{symbol.InstancePath}' (AdsErrorCode: {errorCode:x})!", symbol, innerException)
        {
        }
    }
}

