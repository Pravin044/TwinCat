namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.PlcOpen;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class ValueFactory : IAccessorValueFactory2, IAccessorValueFactory
    {
        protected ValueCreationMode mode;
        protected ISymbolMarshaller valueConverter;
        protected IAccessorRawValue accessor;

        public ValueFactory()
        {
            this.mode = ValueCreationMode.Default;
            this.mode = ValueCreationMode.Default;
            this.valueConverter = new InstanceValueConverter();
        }

        public ValueFactory(ValueCreationMode mode)
        {
            this.mode = ValueCreationMode.Default;
            this.mode = mode;
            this.valueConverter = new InstanceValueConverter();
        }

        public object CreatePrimitiveValue(ISymbol symbol, byte[] rawData, int offset)
        {
            object obj2 = null;
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = symbol.DataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            Encoding encoding = null;
            EncodingAttributeConverter.TryGetEncoding(symbol.Attributes, out encoding);
            this.valueConverter.Unmarshal(symbol, rawData, 0, out obj2);
            return obj2;
        }

        public virtual object CreateValue(ISymbol symbol, byte[] rawData, int offset, DateTime utcTime)
        {
            object untypedValue = null;
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = symbol.DataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            if (!dataType.IsPrimitive)
            {
                untypedValue = rawData;
            }
            else if ((symbol.Category == DataTypeCategory.Enum) && ((this.mode & ValueCreationMode.Enums) > ValueCreationMode.None))
            {
                untypedValue = EnumValueFactory.Create((IEnumType) dataType, rawData, offset);
            }
            else if ((this.mode & ValueCreationMode.Default) <= ValueCreationMode.None)
            {
                Module.Trace.TraceWarning("Value mode not supported!");
                untypedValue = rawData;
            }
            else
            {
                this.valueConverter.Unmarshal(symbol, rawData, 0, out untypedValue);
                if (((this.mode & ValueCreationMode.PlcOpenTypes) == ValueCreationMode.None) && (untypedValue is IPlcOpenType))
                {
                    untypedValue = ((IPlcOpenType) untypedValue).UntypedValue;
                }
            }
            return untypedValue;
        }

        public virtual object CreateValue(ISymbol symbol, byte[] rawData, int offset, IValue parent) => 
            this.CreateValue(symbol, rawData, offset, parent.UtcTimeStamp);

        public void SetValueAccessor(IAccessorRawValue accessor)
        {
            this.accessor = accessor;
        }

        public ValueCreationMode Mode =>
            this.mode;

        public IAccessorRawValue ValueAccessor =>
            this.accessor;
    }
}

