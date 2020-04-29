namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.TypeSystem;

    public sealed class SubRangeType<T> : DataType, ISubRangeType<T>, ISubRangeType, IDataType, IBitSize where T: struct
    {
        private string baseTypeName;
        private IDataType baseType;
        private T _lowerBound;
        private T _upperBound;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal SubRangeType(string name, string baseType, int size, T lowerBound, T upperBound) : base(name, AdsDatatypeId.ADST_VOID, DataTypeCategory.SubRange, size, typeof(T))
        {
            this.baseTypeName = string.Empty;
            bool flag = PrimitiveTypeConverter.TryGetDataTypeId(typeof(T), out base.dataTypeId);
            this.baseTypeName = baseType;
            this._lowerBound = lowerBound;
            this._upperBound = upperBound;
        }

        public override Type ManagedType =>
            typeof(T);

        public IDataType BaseType
        {
            get
            {
                if (this.baseType == null)
                {
                    IDataType type = null;
                    if (base.resolver.TryResolveType(this.baseTypeName, out type))
                    {
                        this.baseType = type;
                        DataType baseType = (DataType) this.baseType;
                        base.size = !baseType.IsBitType ? baseType.Size : baseType.BitSize;
                        base.flags = baseType.Flags;
                    }
                }
                return this.baseType;
            }
        }

        public T LowerBound =>
            this._lowerBound;

        public T UpperBound =>
            this._upperBound;
    }
}

