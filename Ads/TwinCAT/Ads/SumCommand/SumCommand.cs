namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public abstract class SumCommand : ISumCommand
    {
        protected IAdsConnection connection;
        protected IList<SumDataEntity> sumEntities;
        protected SumAccessMode mode;
        protected SumCommandMode commandMode;
        protected AdsReservedIndexGroups ig;
        protected AdsErrorCode result;
        protected AdsErrorCode[] subResults;
        protected bool executed;

        protected SumCommand(IAdsConnection connection, SumCommandMode commandMode, SumAccessMode readWriteMode)
        {
            this.ig = AdsReservedIndexGroups.SumCommandRead;
            this.connection = connection;
            this.mode = readWriteMode;
            this.commandMode = commandMode;
            switch (commandMode)
            {
                case SumCommandMode.Read:
                    this.ig = AdsReservedIndexGroups.SumCommandRead;
                    return;

                case SumCommandMode.Write:
                    this.ig = AdsReservedIndexGroups.SumCommandWrite;
                    return;

                case SumCommandMode.ReadWrite:
                    this.ig = AdsReservedIndexGroups.SumCommandReadWrite;
                    return;

                case SumCommandMode.AddDeviceNotification:
                    this.ig = AdsReservedIndexGroups.SumCommandAddDevNote;
                    return;

                case SumCommandMode.DeleteDeviceNotification:
                    this.ig = AdsReservedIndexGroups.SumCommandDelDevNote;
                    return;
            }
            throw new NotImplementedException();
        }

        protected SumCommand(IAdsConnection connection, IList<SumDataEntity> sumEntities, SumCommandMode accessMode, SumAccessMode readWriteMode)
        {
            this.ig = AdsReservedIndexGroups.SumCommandRead;
            this.connection = connection;
            this.sumEntities = sumEntities;
            this.mode = readWriteMode;
            this.commandMode = accessMode;
            switch (accessMode)
            {
                case SumCommandMode.Read:
                    this.ig = AdsReservedIndexGroups.SumCommandRead;
                    return;

                case SumCommandMode.Write:
                    this.ig = AdsReservedIndexGroups.SumCommandWrite;
                    return;

                case SumCommandMode.ReadWrite:
                    this.ig = AdsReservedIndexGroups.SumCommandReadWrite;
                    return;
            }
        }

        protected abstract int calcReadLength();
        protected abstract int calcWriteLength();
        protected AdsErrorCode Execute(IList<byte[]> writeData, out IList<byte[]> readData, out int[] readDataSizes, out AdsErrorCode[] returnCodes)
        {
            this.executed = false;
            this.result = AdsErrorCode.NoError;
            readData = null;
            readDataSizes = null;
            this.subResults = null;
            int length = 0;
            int num2 = 0;
            length = this.calcReadLength();
            num2 = this.calcWriteLength();
            using (AdsBinaryWriter writer = new AdsBinaryWriter(new AdsStream(num2)))
            {
                using (AdsBinaryReader reader = new AdsBinaryReader(new AdsStream(length)))
                {
                    int num3 = 0;
                    foreach (SumDataEntity entity in this.sumEntities)
                    {
                        num3 += this.OnWriteSumEntityData(entity, writer);
                    }
                    if (writeData != null)
                    {
                        num3 += this.OnWriteValueData(writeData, writer);
                    }
                    int readBytes = 0;
                    this.result = this.connection.TryReadWrite((uint) this.ig, (uint) this.sumEntities.Count, (AdsStream) reader.BaseStream, 0, length, (AdsStream) writer.BaseStream, 0, num2, out readBytes);
                    if (this.result == AdsErrorCode.NoError)
                    {
                        reader.BaseStream.Position = 0L;
                        this.OnReadReturnData(reader, out readData, out readDataSizes, out this.subResults);
                    }
                }
            }
            this.executed = true;
            returnCodes = this.subResults;
            return this.result;
        }

        protected int MarshalSumReadHeader(uint indexGroup, uint indexOffset, int bytes, BinaryWriter writer)
        {
            writer.Write(indexGroup);
            writer.Write(indexOffset);
            writer.Write(bytes);
            return 12;
        }

        protected int MarshalSumReadWriteHeader(uint indexGroup, uint indexOffset, int readBytes, int writeBytes, BinaryWriter writer)
        {
            writer.Write(indexGroup);
            writer.Write(indexOffset);
            writer.Write(readBytes);
            writer.Write(writeBytes);
            return 0x10;
        }

        protected int MarshalSumWriteHeader(uint indexGroup, uint indexOffset, int bytes, BinaryWriter writer) => 
            this.MarshalSumReadHeader(indexGroup, indexOffset, bytes, writer);

        protected abstract void OnReadReturnData(BinaryReader reader, out IList<byte[]> readData, out int[] readDataSizes, out AdsErrorCode[] returnCodes);
        protected abstract int OnWriteSumEntityData(SumDataEntity entity, BinaryWriter writer);
        protected virtual int OnWriteValueData(IList<byte[]> writeData, BinaryWriter writer)
        {
            int num = 0;
            for (int i = 0; i < this.sumEntities.Count; i++)
            {
                byte[] buffer = writeData[i];
                SumDataEntity entity = this.sumEntities[i];
                writer.Write(buffer);
                num += buffer.Length;
            }
            return num;
        }

        public AdsErrorCode Result =>
            this.result;

        public AdsErrorCode[] SubResults =>
            this.subResults;

        public bool Executed =>
            this.executed;

        public bool Succeeded
        {
            get
            {
                if (!this.Executed || (this.result != AdsErrorCode.NoError))
                {
                    return false;
                }
                foreach (AdsErrorCode code in this.subResults)
                {
                    if (code != AdsErrorCode.NoError)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool Failed
        {
            get
            {
                if (this.Executed)
                {
                    if (this.result != AdsErrorCode.NoError)
                    {
                        return true;
                    }
                    foreach (AdsErrorCode code in this.subResults)
                    {
                        if (code != AdsErrorCode.NoError)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        internal protected enum SumAccessMode
        {
            IndexGroupIndexOffset = 0,
            ValueByHandle = 1,
            ValueByName = 2,
            [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Obsolete("Use AcquireHandleByName instead", false)]
            AquireHandleByName = 3,
            AcquireHandleByName = 3,
            ReleaseHandle = 4
        }

        internal protected enum SumCommandMode
        {
            Read,
            Write,
            ReadWrite,
            ReadEx,
            ReadEx2,
            AddDeviceNotification,
            DeleteDeviceNotification
        }
    }
}

