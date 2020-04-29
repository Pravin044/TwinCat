namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("Name = { name }, Size = {size}, Category = {category}")]
    public class DataType : IDataType, IBitSize, IManagedMappableType, IResolvableType, IBindable
    {
        private static int s_idCounter = 1;
        protected int id;
        protected Type dotnetType;
        internal IBinder resolver;
        protected string ns;
        protected DataTypeCategory category;
        protected AdsDatatypeId dataTypeId;
        protected uint typeHashValue;
        protected int size;
        protected string name;
        protected string comment;
        protected AdsDataTypeFlags flags;
        protected TypeAttributeCollection attributes;

        protected DataType(DataType copy) : this(copy.Name, copy.dataTypeId, copy.Category, copy.size, copy.dotnetType)
        {
            this.flags = copy.flags;
            this.ns = copy.ns;
            this.resolver = copy.resolver;
        }

        internal DataType(DataTypeCategory cat, AdsDataTypeEntry entry)
        {
            s_idCounter++;
            this.id = s_idCounter;
            this.comment = string.Empty;
            this.flags = AdsDataTypeFlags.DataType;
            this.attributes = new TypeAttributeCollection();
            this.category = cat;
            this.comment = entry.comment;
            this.name = entry.entryName;
            this.flags = entry.flags;
            this.size = (int) entry.size;
            this.dataTypeId = entry.baseTypeId;
            if ((this.dataTypeId == AdsDatatypeId.ADST_BIGTYPE) && (this.category == DataTypeCategory.Unknown))
            {
                this.category = DataTypeCategory.Interface;
            }
            else if (this.category == DataTypeCategory.Unknown)
            {
                this.category = DataTypeCategory.Primitive;
            }
            this.typeHashValue = entry.typeHashValue;
            if (entry.HasAttributes)
            {
                this.attributes = new TypeAttributeCollection(entry.attributes);
            }
        }

        internal DataType(string name, AdsDatatypeId typeId, DataTypeCategory cat, int size, Type dotnetType) : this(name, typeId, cat, size, dotnetType, AdsDataTypeFlags.DataType)
        {
        }

        internal DataType(string name, AdsDatatypeId typeId, DataTypeCategory cat, int size, Type dotnetType, AdsDataTypeFlags flags)
        {
            s_idCounter++;
            this.id = s_idCounter;
            this.comment = string.Empty;
            this.flags = AdsDataTypeFlags.DataType;
            this.attributes = new TypeAttributeCollection();
            this.name = name;
            this.dataTypeId = typeId;
            this.category = cat;
            this.flags = flags | AdsDataTypeFlags.DataType;
            this.size = size;
            this.dotnetType = dotnetType;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void Bind(IBinder binder)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            if (this.resolver != null)
            {
                throw new ArgumentException($"Type '{this.FullName}' is already bound to namespace!");
            }
            this.resolver = binder;
            this.ns = this.resolver.Provider.RootNamespaceName;
            this.OnBound(binder);
        }

        public static bool IsPointerType(DataTypeCategory cat) => 
            (cat == DataTypeCategory.Pointer);

        public static bool IsReferenceType(DataTypeCategory cat) => 
            (cat == DataTypeCategory.Reference);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        protected virtual void OnBound(IBinder binder)
        {
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IDataType ResolveType(DataTypeResolveStrategy type)
        {
            IDataType type2 = this;
            IDataType baseType = this;
            if (type == DataTypeResolveStrategy.Alias)
            {
                while (true)
                {
                    if ((baseType == null) || (baseType.Category != DataTypeCategory.Alias))
                    {
                        if ((baseType == null) && (this.Category == DataTypeCategory.Alias))
                        {
                            object[] args = new object[] { type2.Name };
                            Module.Trace.TraceWarning("Could not resolve Alias '{0}'", args);
                        }
                        break;
                    }
                    type2 = baseType;
                    baseType = ((IAliasType) baseType).BaseType;
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
                            object[] args = new object[] { type2.Name };
                            Module.Trace.TraceWarning("Could not resolve Alias/Reference '{0}'", args);
                        }
                        break;
                    }
                    if (baseType.Category == DataTypeCategory.Alias)
                    {
                        type2 = baseType;
                        baseType = ((IAliasType) baseType).BaseType;
                    }
                    else if (baseType.Category == DataTypeCategory.Reference)
                    {
                        type2 = baseType;
                        baseType = ((IReferenceType) baseType).ReferencedType;
                    }
                }
            }
            return baseType;
        }

        internal void SetSize(int size, Type managedType)
        {
            this.size = size;
            this.dotnetType = managedType;
        }

        public override string ToString()
        {
            $"{this.Namespace} (Size: {this.Size}, CAT: {this.Category})";
            return this.Name;
        }

        public int Id =>
            this.id;

        public virtual Type ManagedType =>
            this.dotnetType;

        public string Namespace =>
            this.ns;

        public DataTypeCategory Category =>
            this.category;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public uint TypeHashValue =>
            this.typeHashValue;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AdsDatatypeId DataTypeId =>
            this.dataTypeId;

        public int Size
        {
            get
            {
                if (((this.size == 0) && (this.Category == DataTypeCategory.Pointer)) || (this.Category == DataTypeCategory.Reference))
                {
                    this.size = this.resolver.PlatformPointerSize;
                }
                return this.size;
            }
        }

        public int ByteSize
        {
            get
            {
                int size = 0;
                if (!this.IsBitType)
                {
                    size = this.size;
                }
                else
                {
                    size = this.BitSize / 8;
                    if ((this.BitSize % 8) > 0)
                    {
                        size++;
                    }
                }
                return size;
            }
        }

        public bool IsByteAligned =>
            (!this.IsBitType || ((this.size % 8) == 0));

        public int BitSize =>
            (!this.IsBitType ? (this.size * 8) : this.size);

        public string Name =>
            this.name;

        public string FullName =>
            (this.ns + "." + this.name);

        public string Comment =>
            this.comment;

        public virtual bool IsPrimitive =>
            PrimitiveTypeConverter.IsPrimitiveType(this.category);

        public virtual bool IsContainer =>
            PrimitiveTypeConverter.IsContainerType(this.category);

        public virtual bool IsReference =>
            IsReferenceType(this.category);

        public virtual bool IsPointer =>
            IsPointerType(this.category);

        internal AdsDataTypeFlags Flags =>
            this.flags;

        public bool IsBitType =>
            ((this.flags & AdsDataTypeFlags.BitValues) == AdsDataTypeFlags.BitValues);

        public ReadOnlyTypeAttributeCollection Attributes =>
            this.attributes.AsReadOnly();

        public bool IsBound =>
            (this.resolver != null);
    }
}

