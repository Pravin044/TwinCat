namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    internal class SumAddNotifications : TwinCAT.Ads.SumCommand.SumCommand
    {
        private IAdsConnection _connection;
        private NotificationSettings _settings;
        private uint[] _variableHandles;
        private int[] _variableLengths;

        public SumAddNotifications(IAdsConnection connection, uint[] variableHandles, int[] lengths, NotificationSettings settings, AdsStream stream) : base(connection, TwinCAT.Ads.SumCommand.SumCommand.SumCommandMode.AddDeviceNotification, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ValueByHandle)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (variableHandles == null)
            {
                throw new ArgumentNullException("variableHandles");
            }
            if (lengths == null)
            {
                throw new ArgumentNullException("lengths");
            }
            if (variableHandles.Length != lengths.Length)
            {
                throw new ArgumentException("Handles/lenghts mismatch!");
            }
            int num = Enumerable.Max(lengths);
            if (stream.Length < num)
            {
                throw new ArgumentException("Notification Buffer/Stream is to small");
            }
            this._connection = connection;
            this._variableHandles = variableHandles;
            this._variableLengths = lengths;
            this._settings = settings;
            base.sumEntities = this.CreateSumEntityInfos();
        }

        protected override int calcReadLength() => 
            (8 * base.sumEntities.Count);

        protected override int calcWriteLength() => 
            ((8 + NotificationSettingsMarshaller.MarshalSize(true)) * base.sumEntities.Count);

        private IList<SumDataEntity> CreateSumEntityInfos()
        {
            List<SumDataEntity> list = new List<SumDataEntity>();
            for (int i = 0; i < this._variableHandles.Length; i++)
            {
                uint indexOffset = this._variableHandles[i];
                int dataLength = this._variableLengths[i];
                NotificationHandleSumEntity item = new NotificationHandleSumEntity(0xf005, indexOffset, this._settings, dataLength);
                list.Add(item);
            }
            return list;
        }

        protected override void OnReadReturnData(BinaryReader reader, out IList<byte[]> readData, out int[] readDataSizes, out AdsErrorCode[] returnCodes)
        {
            readDataSizes = new int[base.sumEntities.Count];
            returnCodes = new AdsErrorCode[base.sumEntities.Count];
            readData = new List<byte[]>(base.sumEntities.Count);
            int num = 0;
            for (int i = 0; i < base.sumEntities.Count; i++)
            {
                returnCodes[i] = (AdsErrorCode) reader.ReadUInt32();
                readDataSizes[i] = 4;
                readData.Add(reader.ReadBytes(4));
                num = (num + 4) + 4;
            }
        }

        protected override int OnWriteSumEntityData(SumDataEntity entity, BinaryWriter writer)
        {
            NotificationHandleSumEntity entity2 = (NotificationHandleSumEntity) entity;
            writer.Write(entity2.GetWriteBytes());
            return entity2.WriteLength;
        }

        protected override int OnWriteValueData(IList<byte[]> writeData, BinaryWriter writer) => 
            base.OnWriteValueData(writeData, writer);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AdsErrorCode TryCreateHandles(out ISumHandleCollection handles)
        {
            IList<SumDataEntity> list = this.CreateSumEntityInfos();
            handles = null;
            AdsErrorCode[] returnCodes = null;
            IList<byte[]> readData = null;
            int[] readDataSizes = null;
            AdsErrorCode code = base.Execute(null, out readData, out readDataSizes, out returnCodes);
            if (code == AdsErrorCode.NoError)
            {
                handles = new SumHandleList();
                for (int i = 0; i < list.Count; i++)
                {
                    uint notificationHandle = 0;
                    if (returnCodes[i] == AdsErrorCode.NoError)
                    {
                        notificationHandle = BitConverter.ToUInt32(readData[i], 0);
                    }
                    handles.Add(new SumNotificationHandleEntry(this._variableHandles[i], notificationHandle, returnCodes[i]));
                }
            }
            return code;
        }

        public AdsErrorCode TryCreateHandles(out uint[] handles, out AdsErrorCode[] returnCodes)
        {
            ISumHandleCollection handles2 = null;
            AdsErrorCode code = this.TryCreateHandles(out handles2);
            returnCodes = new AdsErrorCode[handles2.Count];
            handles = new uint[handles2.Count];
            for (int i = 0; i < handles2.Count; i++)
            {
                SumNotificationHandleEntry entry = (SumNotificationHandleEntry) handles2[i];
                handles[i] = entry.NotificationHandle;
                returnCodes[i] = handles2[i].ErrorCode;
            }
            return code;
        }
    }
}

