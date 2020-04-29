namespace TwinCAT.Ads.Internal
{
    using System;
    using TwinCAT.Ads;

    internal interface INotificationReceiver
    {
        void OnAdsStateChanged(AdsStateChangedEventArgs eventArgs);
        void OnNotification(int handle, long timeStamp, int length, NotificationEntry entry);
        void OnNotificationError(Exception e);
        void OnNotificationError(int handle, long timeStamp);
        void OnRouterNotification(AmsRouterState state);
        void OnSymbolVersionChanged(AdsSymbolVersionChangedEventArgs eventArgs);
    }
}

