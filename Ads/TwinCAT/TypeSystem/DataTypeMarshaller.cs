namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;

    internal class DataTypeMarshaller : IDataTypeMarshaller
    {
        private PrimitiveTypeConverter _innerConverter = PrimitiveTypeConverter.Default;

        internal DataTypeMarshaller()
        {
        }

        public object Convert(object sourceValue, Type targetType) => 
            PrimitiveTypeConverter.Convert(sourceValue, targetType);

        public byte[] Marshal(IDataType type, Encoding encoding, object data)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            byte[] bytes = new byte[this.MarshalSize(type, encoding, data)];
            int num = this.Marshal(type, encoding, data, bytes, 0);
            return bytes;
        }

        public int Marshal(IDataType type, Encoding encoding, object value, byte[] bytes, int offset)
        {
            int length = 0;
            if (type.Category == DataTypeCategory.Alias)
            {
                IAliasType type2 = (IAliasType) type;
                return this.Marshal(type2.BaseType, encoding, value, bytes, offset);
            }
            if (type.Category != DataTypeCategory.String)
            {
                if (!type.IsPrimitive)
                {
                    throw new NotSupportedException();
                }
                byte[] buffer2 = this._innerConverter.Marshal(type, value);
                buffer2.CopyTo(bytes, offset);
                length = buffer2.Length;
            }
            else
            {
                IStringType type3 = type as IStringType;
                if (type3 == null)
                {
                    throw new NotSupportedException($"The type '{type.Name}' doesn't implement IStringType!");
                }
                PrimitiveTypeConverter converter = (encoding != null) ? new PrimitiveTypeConverter(encoding) : new PrimitiveTypeConverter(type3.Encoding);
                int length = ((string) value).Length;
                if (type.ByteSize < converter.MarshalSize(value))
                {
                    throw new AdsErrorException($"String is too large for data type '{type.Name}'", AdsErrorCode.DeviceInvalidSize);
                }
                byte[] buffer = converter.Marshal((string) value);
                buffer.CopyTo(bytes, offset);
                length = buffer.Length;
            }
            return length;
        }

        public int MarshalSize(IDataType type, Encoding encoding, object value)
        {
            if (type.Category == DataTypeCategory.Alias)
            {
                IAliasType type2 = (IAliasType) type;
                return this.MarshalSize(type2.BaseType, encoding, value);
            }
            if (type.Category == DataTypeCategory.Enum)
            {
                return ((IEnumType) type).ByteSize;
            }
            if (((type.Category == DataTypeCategory.Primitive) || (type.Category == DataTypeCategory.Pointer)) || (type.Category == DataTypeCategory.SubRange))
            {
                return type.ByteSize;
            }
            return ((encoding != null) ? new PrimitiveTypeConverter(encoding).MarshalSize(value) : this._innerConverter.MarshalSize(value));
        }

        public bool TryGetManagedType(IDataType type, out Type managed) => 
            PrimitiveTypeConverter.TryGetManagedType(type, out managed);

        public int Unmarshal(IDataType type, Encoding encoding, byte[] data, int offset, out object value)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            int num = 0;
            if (type.IsBitType)
            {
                return this.UnmarshalBits(type, offset, data, out value);
            }
            int byteSize = type.ByteSize;
            if (type.Category == DataTypeCategory.Alias)
            {
                IAliasType type2 = (IAliasType) type;
                num = this.Unmarshal(type2.BaseType, encoding, data, offset, out value);
            }
            else if (type.Category == DataTypeCategory.String)
            {
                string str;
                IStringType type3 = type as IStringType;
                if (type3 == null)
                {
                    throw new NotSupportedException($"The type '{type.Name}' doesn't implement IStringType!");
                }
                num = ((encoding != null) ? new PrimitiveTypeConverter(encoding) : new PrimitiveTypeConverter(type3.Encoding)).Unmarshal(data, offset, type3.Length, out str);
                value = str;
            }
            else if (type.Category == DataTypeCategory.Array)
            {
                Type type5;
                IArrayType type4 = (IArrayType) type;
                if (!this.TryGetManagedType(type4, out type5))
                {
                    throw new NotSupportedException($"Could not derive managed type from DataType '{type.Name}'!");
                }
                IList<IDimensionCollection> dimLengths = null;
                AnyArrayConverter.TryGetJaggedDimensions(type4, out dimLengths);
                AnyTypeSpecifier typeSpec = new AnyTypeSpecifier(type5, dimLengths);
                num = this._innerConverter.UnmarshalArray(typeSpec, data, offset, type.ByteSize, out value);
            }
            else
            {
                Type type6;
                if (!type.IsPrimitive)
                {
                    throw new NotSupportedException($"The type '{type.Name}' is not a primitive type!");
                }
                if (!this.TryGetManagedType(type, out type6))
                {
                    throw new NotSupportedException($"Could not derive managed type from DataType '{type.Name}'!");
                }
                num = this._innerConverter.UnmarshalPrimitive(type6, data, offset, type.ByteSize, out value);
            }
            return num;
        }

        internal int UnmarshalBits(IDataType type, int bitOffset, byte[] data, out object result)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (!type.IsBitType)
            {
                throw new ArgumentException($"Type '{type.Name}' is no bit type.", "type");
            }
            if (type.BitSize < 1)
            {
                throw new ArgumentOutOfRangeException("type");
            }
            return BitTypeConverter.Unmarshal(type.BitSize, data, bitOffset, out result);
        }
    }
}

