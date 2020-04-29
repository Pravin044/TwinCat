namespace TwinCAT.Ads
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    public sealed class AdsInvalidNotificationException : AdsException
    {
        private int _handle;
        private long _timeStamp;

        internal AdsInvalidNotificationException(int handle, long timeStamp) : base(string.Format(ResMan.GetString("AdsInvalidNotification_Message1"), handle))
        {
            this._handle = handle;
            this._timeStamp = timeStamp;
        }

        internal AdsInvalidNotificationException(string message, int handle, long timeStamp) : base(message)
        {
            this._handle = handle;
            this._timeStamp = timeStamp;
        }

        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this._handle = info.GetInt32("Handle");
            this._timeStamp = info.GetInt64("TimeStamp");
            this.GetObjectData(info, context);
        }

        public int Handle =>
            this._handle;

        public long TimeStamp =>
            this._timeStamp;
    }
}

