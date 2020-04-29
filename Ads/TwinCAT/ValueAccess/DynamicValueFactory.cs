namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using TwinCAT.PlcOpen;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class DynamicValueFactory : ValueFactory
    {
        public DynamicValueFactory(ValueCreationMode mode) : base(mode)
        {
        }

        private Array createPrimitiveValueArray(IArrayInstance arraySymbol, byte[] rawData, int offset, DateTime utcReadTime)
        {
            Array array = null;
            IArrayType dataType = (IArrayType) arraySymbol.DataType;
            IDataType elementType = dataType.ElementType;
            IResolvableType type3 = elementType as IResolvableType;
            if (type3 != null)
            {
                elementType = type3.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            int elementCount = dataType.Dimensions.ElementCount;
            Type managed = null;
            int[] dimensionLengths = dataType.Dimensions.GetDimensionLengths();
            Array.Reverse(dimensionLengths);
            array = !base.valueConverter.TryGetManagedType(elementType, out managed) ? Array.CreateInstance(typeof(object), dimensionLengths) : Array.CreateInstance(managed, dimensionLengths);
            foreach (int[] numArray2 in new ArrayIndexIterator(dataType, false))
            {
                int[] numArray3 = ArrayIndexConverter.NormalizeIndices(numArray2, dataType);
                Array.Reverse(numArray3);
                int num2 = ArrayIndexConverter.IndicesToSubIndex(numArray2, dataType);
                object obj2 = this.CreateValue(arraySymbol.SubSymbols[num2], rawData, offset, utcReadTime);
                array.SetValue(obj2, numArray3);
                offset += elementType.ByteSize;
            }
            return array;
        }

        public override object CreateValue(ISymbol symbol, byte[] rawData, int offset, DateTime utcReadTime)
        {
            object untypedValue;
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = symbol.DataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            if (base.mode.HasFlag(ValueCreationMode.FullDynamics))
            {
                if (symbol.IsPointer)
                {
                    base.valueConverter.Unmarshal(symbol, rawData, offset, out untypedValue);
                }
                else
                {
                    untypedValue = !symbol.IsReference ? (!symbol.IsPointer ? new DynamicValue((IDynamicSymbol) symbol, rawData, offset, utcReadTime, this) : new DynamicPointerValue((IDynamicSymbol) symbol, rawData, 0, utcReadTime, this)) : new DynamicReferenceValue((IDynamicSymbol) symbol, rawData, 0, utcReadTime, this);
                }
            }
            else if (dataType.IsPrimitive)
            {
                if ((dataType.Category == DataTypeCategory.Enum) && base.mode.HasFlag(ValueCreationMode.Enums))
                {
                    untypedValue = EnumValueFactory.Create((IEnumType) dataType, rawData, offset);
                }
                else if (!base.mode.HasFlag(ValueCreationMode.Default))
                {
                    untypedValue = new DynamicValue((IDynamicSymbol) symbol, rawData, offset, utcReadTime, this);
                }
                else
                {
                    base.valueConverter.Unmarshal(symbol, rawData, offset, out untypedValue);
                    if (((base.mode & ValueCreationMode.PlcOpenTypes) == ValueCreationMode.None) && (untypedValue is IPlcOpenType))
                    {
                        untypedValue = ((IPlcOpenType) untypedValue).UntypedValue;
                    }
                }
            }
            else if (((dataType.Category != DataTypeCategory.Array) || !base.mode.HasFlag(ValueCreationMode.Default)) || base.mode.HasFlag(ValueCreationMode.Enums))
            {
                untypedValue = !symbol.IsReference ? (!symbol.IsPointer ? new DynamicValue((IDynamicSymbol) symbol, rawData, offset, utcReadTime, this) : new DynamicPointerValue((IDynamicSymbol) symbol, rawData, 0, utcReadTime, this)) : new DynamicReferenceValue((IDynamicSymbol) symbol, rawData, 0, utcReadTime, this);
            }
            else
            {
                IDataType elementType = ((IArrayType) dataType).ElementType;
                IResolvableType type5 = elementType as IResolvableType;
                if (type5 != null)
                {
                    elementType = type5.ResolveType(DataTypeResolveStrategy.AliasReference);
                }
                if (!base.mode.HasFlag(ValueCreationMode.Default) || !elementType.IsPrimitive)
                {
                    untypedValue = new DynamicValue((IDynamicSymbol) symbol, rawData, offset, utcReadTime, this);
                }
                else
                {
                    untypedValue = this.createPrimitiveValueArray((IArrayInstance) symbol, rawData, offset, utcReadTime);
                }
            }
            return untypedValue;
        }

        public override object CreateValue(ISymbol symbol, byte[] rawData, int offset, IValue parent)
        {
            object obj2 = null;
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = symbol.DataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            if (base.mode.HasFlag(ValueCreationMode.FullDynamics) || !dataType.IsPrimitive)
            {
                if (!symbol.IsReference)
                {
                    obj2 = !symbol.IsPointer ? new DynamicValue((IDynamicSymbol) symbol, rawData, offset, (DynamicValue) parent) : new DynamicPointerValue((IDynamicSymbol) symbol, rawData, offset, (DynamicValue) parent);
                }
                else
                {
                    byte[] buffer;
                    DateTime time;
                    IAccessorDynamicValue accessor = base.accessor as IAccessorDynamicValue;
                    if ((accessor != null) && (accessor.TryReadValue(symbol, out buffer, out time) == 0))
                    {
                        obj2 = new DynamicReferenceValue((IDynamicSymbol) symbol, buffer, 0, (DynamicValue) parent);
                    }
                }
            }
            else if ((dataType.Category == DataTypeCategory.Enum) && base.mode.HasFlag(ValueCreationMode.Enums))
            {
                obj2 = EnumValueFactory.Create((IEnumType) dataType, rawData, offset);
            }
            else if (base.mode.HasFlag(ValueCreationMode.Default))
            {
                base.valueConverter.Unmarshal(symbol, rawData, offset, out obj2);
            }
            else
            {
                obj2 = new DynamicValue((IDynamicSymbol) symbol, rawData, offset, (DynamicValue) parent);
            }
            return obj2;
        }
    }
}

