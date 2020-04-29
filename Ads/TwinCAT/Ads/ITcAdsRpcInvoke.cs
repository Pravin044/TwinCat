namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.InteropServices;

    public interface ITcAdsRpcInvoke
    {
        object InvokeRpcMethod(string symbolPath, int methodId, object[] parameters);
        object InvokeRpcMethod(string symbolPath, string methodName, object[] parameters);
        object InvokeRpcMethod(ITcAdsSymbol symbol, int methodId, object[] parameters);
        object InvokeRpcMethod(ITcAdsSymbol symbol, string methodName, object[] parameters);
        AdsErrorCode TryInvokeRpcMethod(string symbolPath, int methodId, object[] parameters, out object retValue);
        AdsErrorCode TryInvokeRpcMethod(string symbolPath, string methodName, object[] parameters, out object retValue);
        AdsErrorCode TryInvokeRpcMethod(ITcAdsSymbol symbol, int methodId, object[] parameters, out object retValue);
        AdsErrorCode TryInvokeRpcMethod(ITcAdsSymbol symbol, string methodName, object[] parameters, out object retValue);
    }
}

