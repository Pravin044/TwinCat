namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;

    public class TcAdsNotificationSample
    {
        private uint _notificationHandle;
        private uint _sampleSize;
        private byte[] _sampleData;

        internal TcAdsNotificationSample(BinaryReader reader)
        {
            this._notificationHandle = reader.ReadUInt32();
            this._sampleSize = reader.ReadUInt32();
            if (this._sampleSize <= 0x7fffffff)
            {
                this._sampleData = reader.ReadBytes((int) this._sampleSize);
            }
            else
            {
                byte[] buffer = reader.ReadBytes(0x7fffffff);
                byte[] buffer2 = reader.ReadBytes(((int) this._sampleSize) - 0x7fffffff);
                this._sampleData = new byte[this._sampleSize];
                buffer.CopyTo(this._sampleData, 0);
                buffer2.CopyTo(this._sampleData, 0x7fffffff);
            }
        }

        public TcAdsNotificationSample(uint sampleSize)
        {
            this._sampleSize = sampleSize;
            this._sampleData = new byte[this._sampleSize];
        }

        public TcAdsNotificationSample(uint sampleSize, uint notificationHandle) : this(sampleSize)
        {
            this._notificationHandle = notificationHandle;
        }

        internal void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write(this._notificationHandle);
            writer.Write(this._sampleSize);
            writer.Write(this._sampleData);
        }

        public uint NotificationHandle
        {
            get => 
                this._notificationHandle;
            set => 
                (this._notificationHandle = value);
        }

        public uint SampleSize
        {
            get => 
                this._sampleSize;
            set => 
                (this._sampleSize = value);
        }

        public byte[] SampleData
        {
            get => 
                this._sampleData;
            set => 
                (this._sampleData = value);
        }
    }
}

