namespace TwinCAT.Ads
{
    using System;

    public sealed class AdsNotificationEventArgs : EventArgs
    {
        private long timeStamp;
        private object userData;
        private int notificationHandle;
        private int length;
        private int offset;
        private AdsStream dataStream;

        public AdsNotificationEventArgs(long timeStamp, object userData, int notificationHandle, int length, int offset, AdsStream dataStream)
        {
            this.timeStamp = timeStamp;
            this.userData = userData;
            this.notificationHandle = notificationHandle;
            this.length = length;
            this.offset = offset;
            this.dataStream = dataStream;
        }

        public long TimeStamp =>
            this.timeStamp;

        public object UserData =>
            this.userData;

        public int NotificationHandle =>
            this.notificationHandle;

        public AdsStream DataStream =>
            this.dataStream;

        public int Length =>
            this.length;

        public int Offset =>
            this.offset;
    }
}

