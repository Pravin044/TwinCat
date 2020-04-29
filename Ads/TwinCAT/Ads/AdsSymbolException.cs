namespace TwinCAT.Ads
{
    using System;

    [Serializable]
    public class AdsSymbolException : AdsException
    {
        [NonSerialized]
        public readonly ITcAdsSymbol Symbol;
        [NonSerialized]
        public readonly string SymbolName;

        public AdsSymbolException(string message, string symbolName) : this(message, symbolName, null)
        {
        }

        public AdsSymbolException(string message, ITcAdsSymbol symbol) : this(message, symbol, null)
        {
        }

        public AdsSymbolException(string message, string symbolName, Exception innerException) : base(message, innerException)
        {
            this.Symbol = null;
            this.SymbolName = symbolName;
        }

        public AdsSymbolException(string message, ITcAdsSymbol symbol, Exception innerException) : base(message, innerException)
        {
            this.Symbol = symbol;
            this.SymbolName = symbol.Name;
        }
    }
}

