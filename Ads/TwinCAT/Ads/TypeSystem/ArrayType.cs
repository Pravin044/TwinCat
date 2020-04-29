namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("Name = { name }, Size = {size}, Category = {category}, Elements = { ElementCount }")]
    public sealed class ArrayType : DataType, IArrayType, IDataType, IBitSize, IOversamplingSupport
    {
        private bool _oversampled;
        private string elementTypeName;
        private DataType elementType;
        private AdsDatatypeId elementTypeId;
        private DimensionCollection _dimensions;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ArrayType(AdsDataTypeEntry entry) : base(DataTypeCategory.Array, entry)
        {
            string str2;
            AdsDatatypeArrayInfo[] arrayInfos = entry.arrayInfos;
            AdsDatatypeArrayInfo[] dims = null;
            AdsDatatypeArrayInfo[] infoArray3 = null;
            string baseType = null;
            if (DataTypeStringParser.TryParseArray(base.name, out dims, out baseType) && DataTypeStringParser.TryParseArray(baseType, out infoArray3, out str2))
            {
                this._dimensions = new DimensionCollection(dims);
                this.elementTypeName = baseType;
                this.elementTypeId = entry.baseTypeId;
            }
            else
            {
                this._dimensions = new DimensionCollection(arrayInfos);
                this.elementTypeName = entry.typeName;
                this.elementTypeId = entry.baseTypeId;
            }
            this._oversampled = (entry.flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Oversample)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.Oversample);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ArrayType(string typeName, DataType elementType, DimensionCollection dims, AdsDataTypeFlags flags) : base(typeName, AdsDatatypeId.ADST_BIGTYPE, DataTypeCategory.Array, dims.ElementCount * elementType.ByteSize, null, flags)
        {
            this.elementType = elementType;
            this.elementTypeName = elementType.Name;
            this._dimensions = dims;
        }

        internal static bool AreIndicesValid(int[] indices, IArrayType type, bool acceptOversampled)
        {
            IOversamplingSupport support = type as IOversamplingSupport;
            bool oversampled = ((support != null) && support.IsOversampled) & acceptOversampled;
            return ArrayIndexConverter.TryCheckIndices(indices, type.Dimensions.LowerBounds, type.Dimensions.UpperBounds, false, oversampled);
        }

        internal void CheckIndices(int[] indices, bool acceptOversampled)
        {
            CheckIndices(indices, this, acceptOversampled);
        }

        internal static void CheckIndices(int[] indices, IArrayType arrayType, bool acceptOversampled)
        {
            IOversamplingSupport support = arrayType as IOversamplingSupport;
            bool flag = ((support != null) && support.IsOversampled) & acceptOversampled;
            ArrayIndexConverter.CheckIndices(indices, arrayType.Dimensions.LowerBounds, arrayType.Dimensions.UpperBounds, false, acceptOversampled);
        }

        internal int GetElementOffset(int[] indices) => 
            GetElementOffset(indices, this);

        internal static int GetElementOffset(int[] indices, IArrayType type)
        {
            int elementPosition = GetElementPosition(indices, type);
            int num3 = 0;
            num3 = (type.ElementType == null) ? (type.Size / type.Dimensions.ElementCount) : type.ElementType.Size;
            return (num3 * elementPosition);
        }

        internal int GetElementPosition(int[] indices) => 
            GetElementPosition(indices, this);

        internal static int GetElementPosition(int[] indices, IArrayType type)
        {
            CheckIndices(indices, type, false);
            return ArrayIndexConverter.IndicesToSubIndex(indices, type);
        }

        internal int[] GetIndicesOfPosition(int position) => 
            ArrayIndexConverter.SubIndexToIndices(position, this);

        internal static bool IsOversamplingIndex(int[] indices, IArrayType type) => 
            ArrayIndexConverter.IsOversamplingIndex(indices, type);

        public override Type ManagedType
        {
            get
            {
                if (base.dotnetType == null)
                {
                    IDataType elementType = this.ElementType;
                    if (elementType == null)
                    {
                        return base.ManagedType;
                    }
                    Type managedType = ((DataType) elementType).ManagedType;
                    if (managedType != null)
                    {
                        base.dotnetType = (this.DimensionCount <= 1) ? managedType.MakeArrayType() : managedType.MakeArrayType(this.DimensionCount);
                    }
                }
                return base.dotnetType;
            }
        }

        public bool IsOversampled =>
            this._oversampled;

        public string ElementTypeName =>
            this.elementTypeName;

        public IDataType ElementType
        {
            get
            {
                if (this.elementType == null)
                {
                    IDataType type = null;
                    if (base.resolver.TryResolveType(this.elementTypeName, out type))
                    {
                        this.elementType = (DataType) type;
                    }
                }
                return this.elementType;
            }
        }

        public bool IsJagged =>
            (this.JaggedLevel > 1);

        public int JaggedLevel
        {
            get
            {
                int num = 1;
                for (IArrayType type = this; (type.ElementType != null) && (type.ElementType.Category == DataTypeCategory.Array); type = (IArrayType) type.ElementType)
                {
                    num++;
                }
                return num;
            }
        }

        [Obsolete]
        internal AdsDatatypeId ElementTypeId =>
            this.elementTypeId;

        public ReadOnlyDimensionCollection Dimensions =>
            this._dimensions.AsReadOnly();

        public int DimensionCount =>
            this._dimensions.Count;

        public int ElementCount =>
            this._dimensions.ElementCount;

        public int ElementSize
        {
            get
            {
                IDataType elementType = this.ElementType;
                return ((elementType == null) ? (base.Size / this.Dimensions.ElementCount) : elementType.Size);
            }
        }

        public override bool IsPrimitive
        {
            get
            {
                IDataType elementType = this.ElementType;
                return ((elementType == null) ? base.IsPrimitive : elementType.IsPrimitive);
            }
        }
    }
}

