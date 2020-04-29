namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class InstanceValueConverter : ISymbolMarshaller
    {
        private DataTypeMarshaller _typeMarshaller = new DataTypeMarshaller();

        internal InstanceValueConverter()
        {
        }

        public byte[] Marshal(IAttributedInstance symbol, object value)
        {
            Encoding encoding = null;
            this.TryGetEncoding(symbol, out encoding);
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = symbol.DataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            return this._typeMarshaller.Marshal(dataType, encoding, value);
        }

        public int Marshal(IAttributedInstance symbol, object value, byte[] bytes, int offset)
        {
            Encoding encoding = null;
            EncodingAttributeConverter.TryGetEncoding(symbol.Attributes, out encoding);
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = symbol.DataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            return this._typeMarshaller.Marshal(dataType, encoding, value, bytes, offset);
        }

        public int MarshalSize(IAttributedInstance symbol, object value)
        {
            Encoding encoding = null;
            EncodingAttributeConverter.TryGetEncoding(symbol.Attributes, out encoding);
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = symbol.DataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            return this._typeMarshaller.MarshalSize(dataType, encoding, value);
        }

        private bool TryGetEncoding(IAttributedInstance symbol, out Encoding encoding)
        {
            bool flag = EncodingAttributeConverter.TryGetEncoding(symbol.Attributes, out encoding);
            if (!flag && (symbol.DataType.Category == DataTypeCategory.String))
            {
                IStringType dataType = (IStringType) symbol.DataType;
                encoding = dataType.Encoding;
                flag = true;
            }
            return flag;
        }

        public bool TryGetManagedType(IAttributedInstance symbol, out Type managed)
        {
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = symbol.DataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            return this._typeMarshaller.TryGetManagedType(dataType, out managed);
        }

        public bool TryGetManagedType(IDataType type, out Type managed) => 
            this._typeMarshaller.TryGetManagedType(type, out managed);

        public int Unmarshal(IAttributedInstance symbol, byte[] data, int offset, out object value)
        {
            Encoding encoding = null;
            EncodingAttributeConverter.TryGetEncoding(symbol.Attributes, out encoding);
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = symbol.DataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            return this._typeMarshaller.Unmarshal(dataType, encoding, data, offset, out value);
        }

        public DataTypeMarshaller TypeMarshaller =>
            this._typeMarshaller;
    }
}

