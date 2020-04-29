namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class SumRead : TwinCAT.Ads.SumCommand.SumCommand
    {
        protected SumRead(IAdsConnection connection, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode readWriteMode) : base(connection, TwinCAT.Ads.SumCommand.SumCommand.SumCommandMode.Read, readWriteMode)
        {
        }

        protected internal SumRead(IAdsConnection connection, IList<SumDataEntity> sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode readWriteMode) : base(connection, sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumCommandMode.Read, readWriteMode)
        {
        }

        protected override int calcReadLength()
        {
            int num = 4 * base.sumEntities.Count;
            foreach (SumDataEntity entity in base.sumEntities)
            {
                num += entity.ReadLength;
            }
            return num;
        }

        protected override int calcWriteLength()
        {
            int num2 = 12;
            return (base.sumEntities.Count * num2);
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
                num += base.MarshalSumReadHeader(entity2.IndexGroup, entity2.IndexOffset, entity.ReadLength, writer);
            }
            else
            {
                if (base.mode != TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ValueByHandle)
                {
                    throw new NotSupportedException();
                }
                HandleSumEntity entity3 = (HandleSumEntity) entity;
                num += base.MarshalSumReadHeader(0xf005, entity3.Handle, entity.ReadLength, writer);
            }
            return num;
        }

        public IList<byte[]> ReadRaw()
        {
            IList<byte[]> readData = null;
            AdsErrorCode[] returnCodes = null;
            AdsErrorCode code = this.TryReadRaw(out readData, out returnCodes);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumRead failed!", this);
            }
            return readData;
        }

        public AdsErrorCode TryReadRaw(out IList<byte[]> readData, out AdsErrorCode[] returnCodes)
        {
            int[] readDataSizes = null;
            return base.Execute(null, out readData, out readDataSizes, out returnCodes);
        }
    }
}

