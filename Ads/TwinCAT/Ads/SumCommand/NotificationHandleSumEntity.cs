namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using TwinCAT.Ads;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class NotificationHandleSumEntity : IgIoSumEntity
    {
        private NotificationSettings _settings;
        private int _notificationDataLength;

        public NotificationHandleSumEntity(uint indexGroup, uint indexOffset, NotificationSettings settings, int dataLength) : base(indexGroup, indexOffset, 4, 8 + NotificationSettingsMarshaller.MarshalSize(true))
        {
            this._settings = settings;
            this._notificationDataLength = dataLength;
        }

        public byte[] GetWriteBytes()
        {
            byte[] buffer = new byte[MarshalSize];
            using (BinaryWriter writer = new BinaryWriter(new MemoryStream(buffer)))
            {
                writer.Write(base.IndexGroup);
                writer.Write(base.IndexOffset);
                NotificationSettingsMarshaller.Marshal(this._settings, this._notificationDataLength, writer, true);
            }
            int marshalSize = MarshalSize;
            return buffer;
        }

        private static int MarshalSize =>
            (8 + NotificationSettingsMarshaller.MarshalSize(true));
    }
}

