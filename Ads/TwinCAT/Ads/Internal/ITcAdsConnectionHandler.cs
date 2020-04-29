namespace TwinCAT.Ads.Internal
{
    using System;

    internal interface ITcAdsConnectionHandler
    {
        void OnBeforeDisconnected();
        void OnConnected();
        void OnDisconnected();
    }
}

