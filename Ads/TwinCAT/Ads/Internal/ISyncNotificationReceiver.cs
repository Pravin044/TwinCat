namespace TwinCAT.Ads.Internal
{
    using System;

    internal interface ISyncNotificationReceiver
    {
        void OnNotificationError(Exception e);
        void OnSyncNotification(QueueElement[] elements);
    }
}

