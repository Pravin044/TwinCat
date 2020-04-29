namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    internal class AnySizeArrayInstance : ArrayInstance, IAnySizeArrayInstance
    {
        private ISymbol _arraySizeSymbol;

        internal AnySizeArrayInstance(ISymbol parent, IArrayType type, string instanceName, int fieldOffset) : base(parent, type, instanceName, fieldOffset)
        {
            base.category = DataTypeCategory.Array;
        }

        internal override ISymbolCollection OnCreateSubSymbols(ISymbol parentInstance)
        {
            ISymbolCollection symbols = null;
            ISymbolFactory symbolFactory = this.FactoryServices.SymbolFactory;
            IArrayType arrayType = this.UpdateDynamicType();
            base.size = arrayType.Size;
            try
            {
                symbols = symbolFactory.CreateArrayElementInstances(parentInstance, arrayType);
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError(exception);
                throw;
            }
            return symbols;
        }

        protected override int OnGetSize()
        {
            if (base.size == 0)
            {
                IArrayType type = this.UpdateDynamicType();
                base.size = type.Size;
            }
            return base.size;
        }

        internal override int OnGetSubSymbolCount(ISymbol parentSymbol) => 
            this.readArraySize();

        protected override byte[] OnReadRawValue(int timeout)
        {
            this.UpdateDynamicType();
            return base.OnReadRawValue(timeout);
        }

        protected override object OnReadValue(int timeout)
        {
            this.UpdateDynamicType();
            return base.OnReadValue(timeout);
        }

        protected override void OnWriteRawValue(byte[] value, int timeout)
        {
            this.UpdateDynamicType();
            base.OnWriteRawValue(value, timeout);
        }

        protected override void OnWriteValue(object value, int timeout)
        {
            this.UpdateDynamicType();
            base.OnWriteValue(value, timeout);
        }

        private int readArraySize()
        {
            int num = 0;
            IValueSymbol arraySizeSymbol = (IValueSymbol) this.ArraySizeSymbol;
            if (arraySizeSymbol != null)
            {
                try
                {
                    num = PrimitiveTypeConverter.Convert<int>(arraySizeSymbol.ReadValue());
                }
                catch (Exception)
                {
                }
            }
            return num;
        }

        protected override bool TryResolveType()
        {
            if (base.resolvedDataType == null)
            {
                base.resolvedDataType = this.UpdateDynamicType();
            }
            return base.TryResolveType();
        }

        public IArrayType UpdateDynamicType()
        {
            int elementCount = this.readArraySize();
            ArrayType dataType = (ArrayType) base.DataType;
            ReadOnlyDimensionCollection dimensions = dataType.Dimensions;
            DimensionCollection dims = new DimensionCollection();
            for (int i = 0; i < dimensions.Count; i++)
            {
                Dimension item = null;
                item = (i != 0) ? new Dimension(dimensions[i].LowerBound, dimensions[i].ElementCount) : new Dimension(dimensions[i].LowerBound, elementCount);
                dims.Add(item);
            }
            ArrayType type2 = new ArrayType(dataType.Name, (DataType) dataType.ElementType, dims, AdsDataTypeFlags.AnySizeArray | AdsDataTypeFlags.DataType);
            base.resolvedDataType = type2;
            return type2;
        }

        private PointerInstance ParentPointer =>
            ((PointerInstance) base.parent);

        private string TcArraySizeFieldName
        {
            get
            {
                PointerInstance parentPointer = this.ParentPointer;
                string str = string.Empty;
                if (parentPointer != null)
                {
                    parentPointer.Attributes.TryGetValue("TcArrayLengthIs", out str);
                }
                return str;
            }
        }

        private IStructInstance ParentStruct =>
            ((this.ParentPointer == null) ? null : ((IStructInstance) this.ParentPointer.Parent));

        private ISymbol ArraySizeSymbol
        {
            get
            {
                if (this._arraySizeSymbol == null)
                {
                    IStructInstance parentStruct = this.ParentStruct;
                    string tcArraySizeFieldName = this.TcArraySizeFieldName;
                    if ((parentStruct != null) && !string.IsNullOrEmpty(tcArraySizeFieldName))
                    {
                        IList<ISymbol> symbols = null;
                        if (parentStruct.MemberInstances.TryGetInstanceByName(tcArraySizeFieldName, out symbols) && (symbols.Count == 1))
                        {
                            this._arraySizeSymbol = symbols[0];
                        }
                    }
                }
                return this._arraySizeSymbol;
            }
        }
    }
}

