namespace TwinCAT.Ads.Reactive
{
    using System;
    using TwinCAT.Ads;

    public class Notification : NotificationBase
    {
        protected byte[] _bytes;

        internal Notification(AdsNotificationEventArgs args)
        {
            base.timeStamp = DateTime.FromFileTimeUtc(args.TimeStamp);
            base.userData = args.UserData;
            base.notificationHandle = (uint) args.NotificationHandle;
            this._bytes = new byte[args.Length];
            args.DataStream.Read(this._bytes, args.Offset, args.Length);
        }

        public byte[] RawValue =>
            this._bytes;
    }
}

