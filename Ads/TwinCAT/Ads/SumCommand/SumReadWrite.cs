namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class SumReadWrite : TwinCAT.Ads.SumCommand.SumCommand
    {
        internal SumReadWrite(IAdsConnection connection, IList<SumDataEntity> sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode readWriteMode) : base(connection, sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumCommandMode.ReadWrite, readWriteMode)
        {
        }

        protected override int calcReadLength()
        {
            int num = (4 * base.sumEntities.Count) + (4 * base.sumEntities.Count);
            foreach (SumDataEntity entity in base.sumEntities)
            {
                num += entity.ReadLength;
            }
            return num;
        }

        protected override int calcWriteLength()
        {
            int num = 0;
            int num2 = 0x10;
            num = base.sumEntities.Count * num2;
            foreach (SumDataEntity entity in base.sumEntities)
            {
                num += entity.WriteLength;
            }
            return num;
        }

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
                readDataSizes[i] = reader.ReadInt32();
            }
            for (int j = 0; j < base.sumEntities.Count; j++)
            {
                SumDataEntity entity = base.sumEntities[j];
                byte[] item = reader.ReadBytes(entity.ReadLength);
                readData.Add(item);
            }
        }

        protected override int OnWriteSumEntityData(SumDataEntity entity, BinaryWriter writer)
        {
            int num = 0;
            if (base.mode == TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.IndexGroupIndexOffset)
            {
                IgIoSumEntity entity2 = (IgIoSumEntity) entity;
                num += base.MarshalSumReadWriteHeader(entity2.IndexGroup, entity2.IndexOffset, entity.ReadLength, entity.WriteLength, writer);
            }
            else if (base.mode == TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ValueByHandle)
            {
                num += base.MarshalSumReadWriteHeader(0xf005, 0, entity.ReadLength, entity.WriteLength, writer);
            }
            else
            {
                if (base.mode != TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.AquireHandleByName)
                {
                    throw new NotSupportedException();
                }
                num += base.MarshalSumReadWriteHeader(0xf003, 0, entity.ReadLength, entity.WriteLength, writer);
            }
            return num;
        }

        public IList<byte[]> ReadWriteRaw(IList<byte[]> writeData)
        {
            IList<byte[]> readData = null;
            AdsErrorCode[] returnCodes = null;
            AdsErrorCode code = this.TryReadWriteRaw(writeData, out readData, out returnCodes);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumReadWriteCommand failed!", this);
            }
            return readData;
        }

        public AdsErrorCode TryReadWriteRaw(IList<byte[]> writeData, out IList<byte[]> readData, out AdsErrorCode[] returnCodes)
        {
            int[] readDataSizes = null;
            return base.Execute(writeData, out readData, out readDataSizes, out returnCodes);
        }
    }
}

