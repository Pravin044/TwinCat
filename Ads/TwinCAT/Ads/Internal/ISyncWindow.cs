namespace TwinCAT.Ads.Internal
{
    using System;
    using TwinCAT.Ads;

    internal interface ISyncWindow : IDisposable
    {
        void PostNotification(QueueElement[] elements);
        void PostNotification(int handle, long timeStamp, int length, NotificationEntry entry, bool bError);
        void PostRouterNotification(AmsRouterState state);
    }
}

