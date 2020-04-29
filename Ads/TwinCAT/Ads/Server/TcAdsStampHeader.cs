namespace TwinCAT.Ads.Server
{
    using System;
    using System.IO;

    public class TcAdsStampHeader
    {
        private long _timeStamp;
        private uint _numSamples;
        private TcAdsNotificationSample[] _notificationSamples;

        internal TcAdsStampHeader(BinaryReader reader)
        {
            this._timeStamp = reader.ReadInt64();
            this._numSamples = reader.ReadUInt32();
            this._notificationSamples = new TcAdsNotificationSample[this._numSamples];
            for (int i = 0; i < this._numSamples; i++)
            {
                this._notificationSamples[i] = new TcAdsNotificationSample(reader);
            }
        }

        internal TcAdsStampHeader(TcMarshallableStampHeader mStampHeader) : this(mStampHeader.timeStamp, mStampHeader.numSamples)
        {
        }

        public TcAdsStampHeader(long timeStamp, uint numSamples)
        {
            this._timeStamp = timeStamp;
            this._numSamples = numSamples;
            this._notificationSamples = new TcAdsNotificationSample[this._numSamples];
        }

        internal void WriteToBuffer(BinaryWriter writer)
        {
            writer.Write(this._timeStamp);
            writer.Write(this._numSamples);
            foreach (TcAdsNotificationSample sample in this._notificationSamples)
            {
                sample.WriteToBuffer(writer);
            }
        }

        public long TimeStamp
        {
            get => 
                this._timeStamp;
            set => 
                (this._timeStamp = value);
        }

        public uint NumSamples
        {
            get => 
                this._numSamples;
            set => 
                (this._numSamples = value);
        }

        public TcAdsNotificationSample[] NotificationSamples
        {
            get => 
                this._notificationSamples;
            set => 
                (this._notificationSamples = value);
        }
    }
}

