namespace TwinCAT.Ads.SumCommand
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    public class SumHandleRead : SumRead
    {
        public SumHandleRead(IAdsConnection connection, uint[] serverHandles, Type[] valueTypes) : this(connection, serverHandles, valueTypes, false, 0x100)
        {
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public SumHandleRead(IAdsConnection connection, IDictionary<uint, Type> handleTypeDict, bool unicode = false, int strlen = 0x100) : base(connection, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ValueByHandle)
        {
            PrimitiveTypeConverter converter = PrimitiveTypeConverter.Default;
            if (unicode)
            {
                converter = PrimitiveTypeConverter.Unicode;
            }
            List<SumDataEntity> list = new List<SumDataEntity>();
            foreach (KeyValuePair<uint, Type> pair in handleTypeDict)
            {
                if (pair.Value == typeof(string))
                {
                    list.Add(new HandleSumReadAnyEntity(pair.Key, strlen, converter));
                    continue;
                }
                list.Add(new HandleSumReadAnyEntity(pair.Key, pair.Value, converter));
            }
            base.sumEntities = list;
        }

        public SumHandleRead(IAdsConnection connection, uint[] serverHandles, Type[] valueTypes, bool unicode, int strlen) : base(connection, TwinCAT.Ads.SumCommand.SumCommand.SumAccessMode.ValueByHandle)
        {
            List<SumDataEntity> list = new List<SumDataEntity>();
            PrimitiveTypeConverter converter = PrimitiveTypeConverter.Default;
            if (unicode)
            {
                converter = PrimitiveTypeConverter.Unicode;
            }
            for (int i = 0; i < serverHandles.Length; i++)
            {
                uint handle = serverHandles[i];
                Type tp = valueTypes[i];
                if (tp == typeof(string))
                {
                    list.Add(new HandleSumReadAnyEntity(handle, strlen, converter));
                }
                else
                {
                    list.Add(new HandleSumReadAnyEntity(handle, tp, converter));
                }
            }
            base.sumEntities = list;
        }

        public object[] Read()
        {
            AdsErrorCode[] returnCodes = null;
            object[] values = null;
            AdsErrorCode code = this.TryRead(out values, out returnCodes);
            if (base.Failed)
            {
                throw new AdsSumCommandException("SumHandleRead failed!", this);
            }
            return values;
        }

        public AdsErrorCode TryRead(out object[] values, out AdsErrorCode[] returnCodes)
        {
            IList<byte[]> readData = null;
            values = null;
            AdsErrorCode code = base.TryReadRaw(out readData, out returnCodes);
            if (code == AdsErrorCode.NoError)
            {
                values = new object[base.sumEntities.Count];
                int num = 0;
                for (int i = 0; i < base.sumEntities.Count; i++)
                {
                    AdsErrorCode code2 = returnCodes[i];
                    if (code2 == AdsErrorCode.NoError)
                    {
                        HandleSumReadAnyEntity entity = (HandleSumReadAnyEntity) base.sumEntities[i];
                        byte[] data = readData[i];
                        num = entity.Converter.Unmarshal(entity.TypeSpec, data, 0, data.Length, out values[i]);
                    }
                }
            }
            return code;
        }
    }
}

