namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    internal interface ISymbolInfoTable
    {
        ITcAdsSymbol GetSymbol(string symbolPath, bool bLookup);
        object InvokeRpcMethod(ITcAdsSymbol symbol, IRpcMethod rpcMethod, object[] parameterValues);
        object ReadSymbol(string symbolPath, Type managedType, bool bReloadInfo);
        AdsErrorCode TryGetDataType(ITcAdsSymbol symbol, bool bLookup, out ITcAdsDataType dataType);
        AdsErrorCode TryGetSymbol(string symbolPath, bool bLookup, out ITcAdsSymbol symbol);
        void WriteSymbol(string name, object value, bool bReloadInfo);
    }
}

