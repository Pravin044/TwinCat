namespace TwinCAT.Ads.SumCommand
{
    using System;

    internal class NotificationHandleReleaseSumEntity : SumDataEntity
    {
        private uint _notificationHandle;

        public NotificationHandleReleaseSumEntity(uint notificationHandle) : base(0, 0)
        {
            this._notificationHandle = notificationHandle;
        }

        public uint Handle =>
            this._notificationHandle;
    }
}

