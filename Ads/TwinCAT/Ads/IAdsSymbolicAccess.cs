namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IAdsSymbolicAccess
    {
        object ReadSymbol(ITcAdsSymbol symbol);
        object ReadSymbol(string name, Type type, bool reloadSymbolInfo);
        ITcAdsSymbol ReadSymbolInfo(string name);
        void WriteSymbol(ITcAdsSymbol symbol, object val);
        void WriteSymbol(string name, object value, bool reloadSymbolInfo);
    }
}

