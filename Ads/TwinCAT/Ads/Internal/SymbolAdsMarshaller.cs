namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    internal class SymbolAdsMarshaller : RpcMarshaller
    {
        public SymbolAdsMarshaller(IDataTypeResolver resolver) : base(resolver)
        {
        }

        protected override int OnMarshalParameter(int i, RpcMethodParameterCollection inParameters, IList<IDataType> inParameterTypes, object[] parameterValues, byte[] buffer, int offset)
        {
            ITcAdsDataType type = (ITcAdsDataType) inParameterTypes[i];
            object val = parameterValues[i];
            AdsDatatypeId dataTypeId = type.DataTypeId;
            int num = 0;
            if (PrimitiveTypeConverter.CanMarshal(type.Category))
            {
                byte[] data = null;
                PrimitiveTypeConverter.Marshal(dataTypeId, val, out data);
                num = copyHelper(buffer, offset, data);
            }
            else
            {
                if (type.Category != DataTypeCategory.String)
                {
                    throw new MarshalException($"Cannot marshal complex type '{type.Name}'");
                }
                int length = -1;
                bool isUnicode = false;
                DataTypeStringParser.TryParseString(type.Name, out length, out isUnicode);
                string str = (string) val;
                byte[] source = null;
                PlcStringConverter converter = isUnicode ? PlcStringConverter.UnicodeVariableLength : PlcStringConverter.DefaultVariableLength;
                if (converter.MarshalSize(str) > type.ByteSize)
                {
                    throw new MarshalException("String size mismatch");
                }
                source = converter.Marshal(str);
                num = copyHelper(buffer, offset, source);
            }
            return num;
        }

        protected override int OnUnmarshalParameter(int i, RpcMethodParameterCollection outParameters, IList<IDataType> outParameterTypes, byte[] buffer, int offset, ref object[] outValues)
        {
            ITcAdsDataType dataType = (ITcAdsDataType) outParameterTypes[i];
            object obj2 = outValues[i];
            return this.unmarshal(buffer, offset, dataType, out obj2);
        }

        private int unmarshal(byte[] buffer, int offset, ITcAdsDataType dataType, out object value)
        {
            AdsDatatypeId dataTypeId = dataType.DataTypeId;
            value = null;
            int size = dataType.Size;
            if (!PrimitiveTypeConverter.CanMarshal(dataType.Category))
            {
                throw new MarshalException($"Cannot unmarshal complex type '{dataType.Name}'");
            }
            return PrimitiveTypeConverter.Default.Unmarshal(dataType, buffer, offset, out value);
        }

        public override int UnmarshalReturnValue(IRpcMethod method, IDataType returnType, byte[] buffer, int offset, out object returnValue) => 
            this.unmarshal(buffer, offset, (ITcAdsDataType) returnType, out returnValue);
    }
}

