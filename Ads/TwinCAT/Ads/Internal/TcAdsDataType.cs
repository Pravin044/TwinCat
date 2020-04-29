namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("Name = {Name} : {BaseTypeName}, Size: {Size}, Category: {Category}")]
    internal class TcAdsDataType : ITcAdsDataType, IDataType, IBitSize, IResolvableType, IManagedMappableType
    {
        private static int s_id;
        private int _id;
        protected string _typeName;
        private uint _size;
        protected AdsDataTypeFlags _flags;
        protected uint _offset;
        protected AdsDatatypeId _dataTypeId;
        private string _baseTypeName;
        private IDataTypeResolver _table;
        private TcAdsSubItem[] _subItems;
        protected AdsDatatypeArrayInfo[] _arrayInfo;
        private Type _managedType;
        private AdsEnumInfoEntry[] _enumInfos;
        private AdsMethodEntry[] _rpcMethodInfos;
        private AdsAttributeEntry[] _attributes;
        private uint _typeHashValue;
        private string _comment;
        private DataTypeCategory _category;

        private TcAdsDataType()
        {
            this._flags = AdsDataTypeFlags.DataType;
            this._baseTypeName = string.Empty;
            this._comment = string.Empty;
            this._id = ++s_id;
        }

        internal TcAdsDataType(AdsDataTypeEntry entry, IDataTypeResolver table) : this()
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            this._table = table;
            this._size = entry.size;
            this._typeName = entry.entryName;
            this._typeHashValue = entry.typeHashValue;
            this._comment = entry.Comment;
            this._flags = entry.flags;
            this._offset = entry.offset;
            this._dataTypeId = entry.baseTypeId;
            if (entry.nameLength > 0)
            {
                bool isUnicode = false;
                int length = 0;
                string referencedType = null;
                AdsDatatypeArrayInfo[] dims = null;
                string baseType = null;
                if (entry.IsArray)
                {
                    if (DataTypeStringParser.TryParseArray(this._typeName, out dims, out baseType))
                    {
                        this._category = DataTypeCategory.Array;
                        this._arrayInfo = dims;
                        this._baseTypeName = baseType;
                    }
                    else
                    {
                        this._arrayInfo = entry.arrayInfos;
                        this._category = DataTypeCategory.Array;
                        this._baseTypeName = entry.Name;
                    }
                }
                else if (DataTypeStringParser.TryParseString(this._typeName, out length, out isUnicode))
                {
                    this._category = DataTypeCategory.String;
                }
                else if (DataTypeStringParser.TryParseReference(this._typeName, out referencedType))
                {
                    this._baseTypeName = referencedType;
                    this._category = DataTypeCategory.Reference;
                }
                else if (DataTypeStringParser.TryParsePointer(this._typeName, out referencedType))
                {
                    this._baseTypeName = referencedType;
                    this._category = DataTypeCategory.Pointer;
                }
                else if (DataTypeStringParser.TryParseSubRange(this._typeName, out referencedType))
                {
                    this._baseTypeName = entry.Name;
                    this._category = DataTypeCategory.SubRange;
                }
                else if (!string.IsNullOrEmpty(entry.Name))
                {
                    this._baseTypeName = entry.Name;
                    this._category = DataTypeCategory.Alias;
                }
            }
            if (entry.subItems <= 0)
            {
                if (entry.IsEnum)
                {
                    this._enumInfos = entry.enums;
                    this._category = DataTypeCategory.Enum;
                    bool flag3 = PrimitiveTypeConverter.TryGetManagedType(this._dataTypeId, out this._managedType);
                }
            }
            else
            {
                this._subItems = new TcAdsSubItem[entry.subItems];
                bool flag2 = false;
                int num2 = 0;
                int index = 0;
                while (true)
                {
                    int num1;
                    if (index < entry.subItems)
                    {
                        if (entry.subEntries[index] != null)
                        {
                            this._subItems[index] = new TcAdsSubItem(entry.subEntries[index], table);
                            if (!this._subItems[index].IsStatic && !this._subItems[index].IsProperty)
                            {
                                int num4 = 0;
                                num4 = !this._subItems[index].IsBitType ? (this._subItems[index].Offset * 8) : this._subItems[index].Offset;
                                flag2 |= num4 < num2;
                                num2 += this._subItems[index].BitSize;
                            }
                            index++;
                            continue;
                        }
                        object[] args = new object[] { index, entry.Name };
                        Module.Trace.TraceError("SubEntry '{0}' missing in TcAdsDataType '{1}'", args);
                    }
                    if ((entry.subItems <= 1) || (entry.BitSize >= num2))
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = (int) !entry.IsBitType;
                    }
                    this._category = ((num1 & flag2) == 0) ? DataTypeCategory.Struct : DataTypeCategory.Union;
                    break;
                }
            }
            if (entry.HasAttributes)
            {
                this._attributes = entry.attributes;
            }
            if (entry.HasRpcMethods)
            {
                this._rpcMethodInfos = entry.methods;
            }
            if (this._category == DataTypeCategory.Unknown)
            {
                this._category = (entry.DataTypeId != AdsDatatypeId.ADST_BIGTYPE) ? DataTypeCategory.Primitive : DataTypeCategory.Interface;
            }
            if ((this._category == DataTypeCategory.Array) && string.IsNullOrEmpty(this._baseTypeName))
            {
                string message = $"Base type of ARRAY '{this._typeName}' not defined!";
                Module.Trace.TraceWarning(message);
            }
        }

        internal TcAdsDataType(string name, string elementType, uint elementSize, AdsDatatypeArrayInfo[] dims, IDataTypeResolver resolver) : this(name, AdsDatatypeId.ADST_BIGTYPE, 0, AdsDataTypeFlags.DataType, DataTypeCategory.Array, elementType, null, resolver)
        {
            if (dims == null)
            {
                throw new ArgumentNullException("dims");
            }
            this._arrayInfo = dims;
            this._size = (uint) (AdsArrayDimensionsInfo.GetArrayElementCount(dims) * elementSize);
            this._baseTypeName = elementType;
            if (string.IsNullOrEmpty(this._baseTypeName))
            {
                string message = $"Base type of ARRAY '{name}' not defined!";
                Module.Trace.TraceWarning(message);
            }
        }

        internal TcAdsDataType(string name, AdsDatatypeId dataType, uint size, DataTypeCategory cat, Type managedType) : this(name, dataType, size, AdsDataTypeFlags.DataType, cat, string.Empty, managedType, null)
        {
        }

        internal TcAdsDataType(string name, AdsDatatypeId dataType, uint size, DataTypeCategory cat, string baseType, Type managedType) : this(name, dataType, size, AdsDataTypeFlags.DataType, cat, baseType, managedType, null)
        {
        }

        internal TcAdsDataType(string name, AdsDatatypeId dataType, uint size, AdsDataTypeFlags flags, DataTypeCategory cat, Type managedType, IDataTypeResolver resolver) : this(name, dataType, size, flags, cat, string.Empty, managedType, resolver)
        {
        }

        internal TcAdsDataType(string name, AdsDatatypeId dataType, uint size, AdsDataTypeFlags flags, DataTypeCategory cat, string baseTypeName, Type managedType, IDataTypeResolver resolver) : this()
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentOutOfRangeException("name");
            }
            this._table = resolver;
            this._typeName = name;
            this._size = size;
            this._flags = flags;
            this._dataTypeId = dataType;
            this._category = cat;
            this._managedType = managedType;
            if ((this._category == DataTypeCategory.Array) && string.IsNullOrEmpty(baseTypeName))
            {
                string message = $"Base type of ARRAY '{name}' not defined!";
                Module.Trace.TraceWarning(message);
            }
            this._baseTypeName = baseTypeName;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IDataType ResolveType(DataTypeResolveStrategy type)
        {
            TcAdsDataType baseType = this;
            TcAdsDataType type3 = this;
            if (type == DataTypeResolveStrategy.Alias)
            {
                while (true)
                {
                    if ((baseType == null) || (baseType.Category != DataTypeCategory.Alias))
                    {
                        if ((baseType == null) && (this.Category == DataTypeCategory.Alias))
                        {
                            object[] args = new object[] { type3.Name };
                            Module.Trace.TraceWarning("Cannot resolve type '{0}", args);
                        }
                        break;
                    }
                    type3 = baseType;
                    baseType = (TcAdsDataType) baseType.BaseType;
                }
            }
            else if (type == DataTypeResolveStrategy.AliasReference)
            {
                while (true)
                {
                    if ((baseType == null) || ((baseType.Category != DataTypeCategory.Alias) && (baseType.Category != DataTypeCategory.Reference)))
                    {
                        if ((baseType == null) && ((this.Category == DataTypeCategory.Alias) || (this.Category == DataTypeCategory.Reference)))
                        {
                            object[] args = new object[] { type3.Name };
                            Module.Trace.TraceWarning("Cannot resolve type '{0}", args);
                        }
                        break;
                    }
                    if (baseType.Category == DataTypeCategory.Alias)
                    {
                        type3 = baseType;
                        baseType = (TcAdsDataType) baseType.BaseType;
                    }
                    else
                    {
                        string str;
                        if (!DataTypeStringParser.TryParseReference(baseType.Name, out str))
                        {
                            type3 = baseType;
                            baseType = null;
                        }
                        else
                        {
                            type3 = baseType;
                            IDataType type4 = null;
                            this._table.TryResolveType(str, out type4);
                            baseType = (TcAdsDataType) type4;
                        }
                    }
                }
            }
            return baseType;
        }

        internal void SetPointerSize(int size)
        {
            if (size == 4)
            {
                this._size = (uint) size;
                this._managedType = typeof(uint);
            }
            else if (size == 8)
            {
                this._size = (uint) size;
                this._managedType = typeof(ulong);
            }
        }

        internal void SetResolver(IDataTypeResolver resolver)
        {
            this._table = resolver;
            if ((this.Size != 0) && this.IsPointer)
            {
                this.SetPointerSize(resolver.PlatformPointerSize);
            }
        }

        internal void SetSize(int size, Type managedType)
        {
            this._size = (uint) size;
            this._managedType = managedType;
        }

        public override string ToString() => 
            this._typeName;

        public string Name =>
            this._typeName;

        public int Size =>
            ((int) this._size);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AdsDataTypeFlags Flags =>
            this._flags;

        public bool IsBitType =>
            ((this._flags & AdsDataTypeFlags.BitValues) == AdsDataTypeFlags.BitValues);

        public AdsDatatypeId DataTypeId =>
            this._dataTypeId;

        public ITcAdsDataType BaseType
        {
            get
            {
                ITcAdsDataType type = null;
                if (!string.IsNullOrEmpty(this._baseTypeName))
                {
                    IDataType type2 = null;
                    if (this._table != null)
                    {
                        this._table.TryResolveType(this._baseTypeName, out type2);
                    }
                    type = (ITcAdsDataType) type2;
                }
                return type;
            }
        }

        public string BaseTypeName =>
            this._baseTypeName;

        public bool HasArrayInfo =>
            ((this._arrayInfo != null) && (this._arrayInfo.Length != 0));

        internal AdsDatatypeArrayInfo[] ArrayInfo =>
            this._arrayInfo;

        public ReadOnlyDimensionCollection Dimensions =>
            ((this._arrayInfo == null) ? new DimensionCollection().AsReadOnly() : new DimensionCollection(this._arrayInfo).AsReadOnly());

        public Type ManagedType =>
            this._managedType;

        internal uint TypeHashValue =>
            this._typeHashValue;

        public bool IsStruct =>
            (this.Category == DataTypeCategory.Struct);

        public bool IsArray =>
            (this.Category == DataTypeCategory.Array);

        public bool IsReference =>
            (this.Category == DataTypeCategory.Reference);

        public bool IsPointer =>
            (this.Category == DataTypeCategory.Pointer);

        public bool IsPrimitive =>
            PrimitiveTypeConverter.IsPrimitiveType(this.Category);

        public bool IsEnum =>
            ((this._enumInfos != null) && (!this.IsPointer && !this.IsReference));

        public virtual bool IsSubItem =>
            false;

        public string Comment =>
            this._comment;

        public DataTypeCategory Category
        {
            get
            {
                if (this._category == DataTypeCategory.Unknown)
                {
                    this._category = CategoryConverter.FromType(this);
                }
                return this._category;
            }
        }

        public ReadOnlyTypeAttributeCollection Attributes =>
            ((this._attributes == null) ? new TypeAttributeCollection().AsReadOnly() : new TypeAttributeCollection(this._attributes).AsReadOnly());

        public bool HasRpcMethods =>
            (this._rpcMethodInfos != null);

        public ReadOnlyRpcMethodCollection RpcMethods =>
            new RpcMethodCollection(this._rpcMethodInfos).AsReadOnly();

        public bool HasEnumInfo =>
            ((this._enumInfos != null) && (this._enumInfos.Length != 0));

        public ReadOnlyEnumValueCollection EnumValues
        {
            get
            {
                if (this._enumInfos == null)
                {
                    return null;
                }
                IDataType type = null;
                this._table.TryResolveType(this._baseTypeName, out type);
                return new EnumValueCollection(((TcAdsDataType) type).DataTypeId, this._enumInfos).AsReadOnly();
            }
        }

        [Obsolete("Use EnumValues instead!")]
        ReadOnlyEnumValueCollection ITcAdsDataType.EnumInfos =>
            this.EnumValues;

        public ReadOnlySubItemCollection SubItems
        {
            get
            {
                TcAdsDataType type = (TcAdsDataType) this.ResolveType(DataTypeResolveStrategy.AliasReference);
                if ((type == null) || !type.HasSubItemInfo)
                {
                    return new ReadOnlySubItemCollection();
                }
                ITcAdsSubItem[] destinationArray = new ITcAdsSubItem[type._subItems.Length];
                Array.Copy(type._subItems, destinationArray, type._subItems.Length);
                return new ReadOnlySubItemCollection(destinationArray);
            }
        }

        public bool HasSubItemInfo =>
            ((this._subItems != null) && (this._subItems.Length != 0));

        public int BitSize =>
            (!this.IsBitType ? ((int) (this._size * 8)) : ((int) this._size));

        public int ByteSize
        {
            get
            {
                int num = 0;
                if (!this.IsBitType)
                {
                    num = (int) this._size;
                }
                else
                {
                    num = this.BitSize / 8;
                    if ((this.BitSize % 8) > 0)
                    {
                        num++;
                    }
                }
                return num;
            }
        }

        int IDataType.Id =>
            this._id;

        string IDataType.Namespace =>
            string.Empty;

        string IDataType.FullName =>
            this.Name;

        public bool IsContainer
        {
            get
            {
                TcAdsDataType objA = (TcAdsDataType) this.ResolveType(DataTypeResolveStrategy.AliasReference);
                if ((objA == null) || ReferenceEquals(objA, this))
                {
                    return PrimitiveTypeConverter.IsContainerType(this.Category);
                }
                return objA.IsContainer;
            }
        }

        bool IBitSize.IsByteAligned =>
            ((this.ByteSize % 8) == 0);

        public bool IsAlias =>
            (this.Category == DataTypeCategory.Alias);

        public bool IsString =>
            (this.Category == DataTypeCategory.String);

        public bool IsOversamplingArray =>
            ((this._flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Oversample)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.Oversample));

        public bool IsJaggedArray
        {
            get
            {
                bool flag = false;
                if (((this.Category == DataTypeCategory.Array) && (this.BaseType != null)) && (this.BaseType.Category == DataTypeCategory.Array))
                {
                    flag = true;
                }
                return flag;
            }
        }
    }
}

