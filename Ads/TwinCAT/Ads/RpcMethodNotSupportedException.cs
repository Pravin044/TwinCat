namespace TwinCAT.Ads
{
    using System;

    [Serializable]
    public class RpcMethodNotSupportedException : AdsSymbolException
    {
        public RpcMethodNotSupportedException(int vTableIndex, ITcAdsSymbol symbol) : base(string.Format(ResMan.GetString("RpcMethodNotSupported_Message2"), vTableIndex, symbol.Name), symbol)
        {
        }

        public RpcMethodNotSupportedException(string methodName, ITcAdsSymbol symbol) : base(string.Format(ResMan.GetString("RpcMethodNotSupported_Message"), methodName, symbol.Name), symbol)
        {
        }
    }
}

