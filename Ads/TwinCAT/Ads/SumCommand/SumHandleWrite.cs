namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    public class SumHandleWrite : SumWrite
    {
        private PrimitiveTypeConverter _converter;

        public SumHandleWrite(IAdsConnection connection, IDictionary<uint, Type> handleTypeDict) : base(connection, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ValueByHandle)
        {
            this._converter = PrimitiveTypeConverter.Default;
            base.sumEntities = new List<SumDataEntity>();
            foreach (KeyValuePair<uint, Type> pair in handleTypeDict)
            {
                base.sumEntities.Add(new HandleSumWriteAnyEntity(pair.Key, pair.Value, this._converter));
            }
        }

        public SumHandleWrite(IAdsConnection connection, uint[] serverHandles, Type[] valueTypes) : base(connection, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ValueByHandle)
        {
            this._converter = PrimitiveTypeConverter.Default;
            base.sumEntities = new List<SumDataEntity>();
            for (int i = 0; i < serverHandles.Length; i++)
            {
                base.sumEntities.Add(new HandleSumWriteAnyEntity(serverHandles[i], valueTypes[i], this._converter));
            }
        }

        public AdsErrorCode TryWrite(object[] values, out AdsErrorCode[] returnCodes)
        {
            List<byte[]> writeData = new List<byte[]>();
            for (int i = 0; i < base.sumEntities.Count; i++)
            {
                HandleSumWriteAnyEntity entity = (HandleSumWriteAnyEntity) base.sumEntities[i];
                byte[] sourceArray = null;
                if (entity.Type != typeof(string))
                {
                    sourceArray = entity.Converter.Marshal(values[i]);
                }
                else
                {
                    int writeLength = entity.WriteLength;
                    sourceArray = entity.Converter.Marshal(values[i]);
                    entity.SetWriteLength(sourceArray.Length);
                    if ((writeLength > 0) && (writeLength < entity.WriteLength))
                    {
                        byte[] destinationArray = new byte[writeLength];
                        Array.Copy(sourceArray, destinationArray, writeLength);
                        sourceArray = destinationArray;
                    }
                }
                writeData.Add(sourceArray);
            }
            return base.TryWriteRaw(writeData, out returnCodes);
        }

        public void Write(object[] values)
        {
            AdsErrorCode[] returnCodes = null;
            AdsErrorCode code = this.TryWrite(values, out returnCodes);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumHandleWrite failed!", this);
            }
        }
    }
}

