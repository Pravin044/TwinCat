namespace TwinCAT.Ads.Reactive
{
    using System;

    public abstract class NotificationBase
    {
        protected DateTimeOffset timeStamp;
        protected object userData;
        protected uint notificationHandle;
        protected object val;

        protected NotificationBase()
        {
        }

        public DateTimeOffset TimeStamp =>
            this.timeStamp;

        public object UserData =>
            this.userData;

        public uint NotificationHandle =>
            this.notificationHandle;

        public virtual object Value =>
            this.val;
    }
}

