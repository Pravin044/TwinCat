namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class SumWrite : TwinCAT.Ads.SumCommand.SumCommand
    {
        protected SumWrite(IAdsConnection connection, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode readWriteMode) : base(connection, TwinCAT.Ads.SumCommand.SumCommand.SumCommandMode.Write, readWriteMode)
        {
        }

        protected internal SumWrite(IAdsConnection connection, IList<SumDataEntity> sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode readWriteMode) : base(connection, sumEntities, TwinCAT.Ads.SumCommand.SumCommand.SumCommandMode.Write, readWriteMode)
        {
        }

        protected override int calcReadLength() => 
            (4 * base.sumEntities.Count);

        protected override int calcWriteLength()
        {
            int num = 0;
            int num2 = 12;
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
            }
        }

        protected override int OnWriteSumEntityData(SumDataEntity entity, BinaryWriter writer)
        {
            int num = 0;
            if (base.mode == TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.IndexGroupIndexOffset)
            {
                IgIoSumEntity entity2 = (IgIoSumEntity) entity;
                num += base.MarshalSumWriteHeader(entity2.IndexGroup, entity2.IndexOffset, entity.WriteLength, writer);
            }
            else if (base.mode == TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ValueByHandle)
            {
                HandleSumEntity entity3 = (HandleSumEntity) entity;
                num += base.MarshalSumWriteHeader(0xf005, entity3.Handle, entity.WriteLength, writer);
            }
            else
            {
                if (base.mode != TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ReleaseHandle)
                {
                    throw new NotSupportedException();
                }
                HandleSumEntity entity4 = (HandleSumEntity) entity;
                num += base.MarshalSumWriteHeader(0xf006, entity4.Handle, entity.WriteLength, writer);
            }
            return num;
        }

        public AdsErrorCode TryWriteRaw(IList<byte[]> writeData, out AdsErrorCode[] returnCodes)
        {
            IList<byte[]> readData = null;
            int[] readDataSizes = null;
            return base.Execute(writeData, out readData, out readDataSizes, out returnCodes);
        }

        public void WriteRaw(IList<byte[]> writeData)
        {
            AdsErrorCode[] returnCodes = null;
            AdsErrorCode code = this.TryWriteRaw(writeData, out returnCodes);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumWriteCommand failed!", this);
            }
        }
    }
}

