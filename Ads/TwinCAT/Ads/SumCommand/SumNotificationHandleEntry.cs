namespace TwinCAT.Ads.SumCommand
{
    using System;
    using TwinCAT.Ads;

    public class SumNotificationHandleEntry : SumHandleEntry
    {
        private uint _notificationHandle;

        internal SumNotificationHandleEntry(uint handle, uint notificationHandle, AdsErrorCode errorCode) : base(handle, errorCode)
        {
            this._notificationHandle = notificationHandle;
        }

        public uint NotificationHandle =>
            this._notificationHandle;
    }
}

