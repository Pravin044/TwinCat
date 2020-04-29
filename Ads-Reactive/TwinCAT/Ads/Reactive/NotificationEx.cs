namespace TwinCAT.Ads.Reactive
{
    using System;
    using TwinCAT.Ads;

    public sealed class NotificationEx : NotificationBase
    {
        internal NotificationEx(AdsNotificationExEventArgs args)
        {
            base.timeStamp = DateTime.FromFileTimeUtc(args.TimeStamp);
            base.userData = args.UserData;
            base.notificationHandle = (uint) args.NotificationHandle;
            base.val = args.Value;
        }
    }
}

