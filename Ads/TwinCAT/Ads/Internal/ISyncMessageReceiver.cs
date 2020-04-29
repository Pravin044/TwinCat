namespace TwinCAT.Ads.Internal
{
    using System;
    using TwinCAT.Ads;

    internal interface ISyncMessageReceiver
    {
        void OnAdsStateChanged(AdsStateChangedEventArgs eventArgs);
        void OnSymbolVersionChanged(AdsSymbolVersionChangedEventArgs eventArgs);
        void OnSyncNotification(int handle, long timeStamp, int length, NotificationEntry entry, bool bError);
        void OnSyncRouterNotification(AmsRouterState state);
    }
}

