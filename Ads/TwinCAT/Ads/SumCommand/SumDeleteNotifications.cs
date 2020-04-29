namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    internal class SumDeleteNotifications : TwinCAT.Ads.SumCommand.SumCommand
    {
        private uint[] _notificationHandles;

        public SumDeleteNotifications(IAdsConnection connection, uint[] notificationHandles) : base(connection, TwinCAT.Ads.SumCommand.SumCommand.SumCommandMode.DeleteDeviceNotification, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ReleaseHandle)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (notificationHandles == null)
            {
                throw new ArgumentNullException("notificationHandles");
            }
            this._notificationHandles = notificationHandles;
            base.sumEntities = new List<SumDataEntity>();
            for (int i = 0; i < notificationHandles.Length; i++)
            {
                base.sumEntities.Add(new NotificationHandleReleaseSumEntity(notificationHandles[i]));
            }
        }

        protected override int calcReadLength() => 
            (4 * base.sumEntities.Count);

        protected override int calcWriteLength() => 
            (4 * base.sumEntities.Count);

        protected override void OnReadReturnData(BinaryReader reader, out IList<byte[]> readData, out int[] readDataSizes, out AdsErrorCode[] returnCodes)
        {
            readDataSizes = new int[base.sumEntities.Count];
            returnCodes = new AdsErrorCode[base.sumEntities.Count];
            readData = new List<byte[]>();
            int num = 0;
            for (int i = 0; i < base.sumEntities.Count; i++)
            {
                returnCodes[i] = (AdsErrorCode) reader.ReadUInt32();
                num += 4;
            }
        }

        protected override int OnWriteSumEntityData(SumDataEntity entity, BinaryWriter writer)
        {
            writer.Write(((NotificationHandleReleaseSumEntity) entity).Handle);
            return 4;
        }

        protected override int OnWriteValueData(IList<byte[]> writeData, BinaryWriter writer) => 
            base.OnWriteValueData(writeData, writer);

        public void ReleaseHandles()
        {
            AdsErrorCode[] returnCodes = null;
            AdsErrorCode code = this.TryReleaseHandles(out returnCodes);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumReleaseHandles failed!", this);
            }
        }

        public AdsErrorCode TryReleaseHandles(out AdsErrorCode[] returnCodes)
        {
            IList<byte[]> readData = null;
            int[] readDataSizes = null;
            AdsErrorCode code = base.Execute(null, out readData, out readDataSizes, out returnCodes);
            if (code == AdsErrorCode.NoError)
            {
                for (int i = 0; i < base.sumEntities.Count; i++)
                {
                    uint num2 = 0;
                    if (returnCodes[i] == AdsErrorCode.NoError)
                    {
                        num2 = BitConverter.ToUInt32(readData[i], 0);
                    }
                }
            }
            return code;
        }
    }
}

