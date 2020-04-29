namespace TwinCAT.Ads
{
    using System;

    public sealed class AdsNotificationExEventArgs : EventArgs
    {
        private long timeStamp;
        private object userData;
        private int notificationHandle;
        private object value;

        public AdsNotificationExEventArgs(long timeStamp, object userData, int notificationHandle, object value)
        {
            this.timeStamp = timeStamp;
            this.userData = userData;
            this.notificationHandle = notificationHandle;
            this.value = value;
        }

        public long TimeStamp =>
            this.timeStamp;

        public object UserData =>
            this.userData;

        public object Value =>
            this.value;

        public int NotificationHandle =>
            this.notificationHandle;
    }
}

