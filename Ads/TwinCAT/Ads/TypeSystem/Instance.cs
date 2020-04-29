namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("InstancePath = { instanceName }, Type = {typeName}, Size = {size}, Category = {category}, Static = { staticAddress }")]
    public class Instance : IInstance, IBitSize, ISymbolFlagProvider, IBinderProvider, IInstanceInternal, IResolvableType, IBindable
    {
        internal IBinder resolver;
        protected string ns;
        protected int size;
        protected AdsSymbolFlags flags;
        protected AdsDatatypeId dataTypeId;
        protected DataTypeCategory category;
        protected string typeName;
        protected IDataType resolvedDataType;
        protected string comment;
        protected string instanceName;
        protected bool staticAddress;
        protected TypeAttributeCollection attributes;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal Instance()
        {
            this.flags = AdsSymbolFlags.None;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal Instance(AdsFieldEntry subEntry)
        {
            this.comment = subEntry.comment;
            this.instanceName = subEntry.entryName;
            this.size = (int) subEntry.size;
            this.typeName = AlignTypeName(subEntry.typeName);
            this.dataTypeId = subEntry.baseTypeId;
            this.flags = DataTypeFlagConverter.Convert(subEntry.flags);
            this.staticAddress = (subEntry.flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Static)) != AdsDataTypeFlags.None;
            if (subEntry.HasAttributes && (subEntry.attributes != null))
            {
                this.attributes = new TypeAttributeCollection(subEntry.attributes);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal Instance(AdsSymbolEntry symbol)
        {
            this.comment = symbol.comment;
            this.instanceName = symbol.name;
            this.size = (int) symbol.size;
            this.typeName = AlignTypeName(symbol.type);
            this.dataTypeId = (AdsDatatypeId) symbol.dataType;
            this.flags = symbol.flags;
            if ((symbol.attributeCount > 0) && (symbol.attributes != null))
            {
                this.attributes = new TypeAttributeCollection(symbol.attributes);
            }
        }

        protected static string AlignTypeName(string typeName)
        {
            if ((typeName == null) || (typeName == string.Empty))
            {
                throw new ArgumentException("Type name not valid!");
            }
            string str = typeName;
            if (typeName.EndsWith("(VAR_IN_OUT)"))
            {
                int index = typeName.IndexOf(' ');
                str = typeName.Substring(0, index);
            }
            return str;
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
                throw new ArgumentException($"Instance '{this.InstancePath}' is already bound!");
            }
            this.resolver = binder;
            this.ns = this.resolver.Provider.RootNamespaceName;
            this.OnBound(binder);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        protected virtual void OnBound(IBinder binder)
        {
        }

        protected virtual int OnGetSize() => 
            this.size;

        protected virtual void OnSetInstanceName(string instanceName)
        {
            this.instanceName = instanceName;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IDataType ResolveType(DataTypeResolveStrategy type)
        {
            IResolvableType dataType = this.DataType as IResolvableType;
            return ((dataType == null) ? this.DataType : dataType.ResolveType(type));
        }

        internal void SetComment(string comment)
        {
            this.comment = comment;
        }

        protected void SetContextMask(byte contextMask)
        {
            if (contextMask > 15)
            {
                throw new ArgumentOutOfRangeException("contextMask");
            }
            AdsSymbolFlags flags = ((AdsSymbolFlags) ((ushort) (contextMask << 8))) & AdsSymbolFlags.ContextMask;
            this.flags &= AdsSymbolFlags.Attributes | AdsSymbolFlags.BitValue | AdsSymbolFlags.ExtendedFlags | AdsSymbolFlags.InitOnReset | AdsSymbolFlags.ItfMethodAccess | AdsSymbolFlags.MethodDeref | AdsSymbolFlags.Persistent | AdsSymbolFlags.ReadOnly | AdsSymbolFlags.ReferenceTo | AdsSymbolFlags.Static | AdsSymbolFlags.TComInterfacePtr | AdsSymbolFlags.TypeGuid;
            this.flags |= flags;
        }

        public override string ToString() => 
            this.InstancePath;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        protected virtual bool TryResolveType()
        {
            bool flag = false;
            if (((this.resolver != null) && ((this.resolvedDataType == null) && (this.typeName != null))) && (this.typeName != string.Empty))
            {
                IDataType type = null;
                flag = this.resolver.TryResolveType(this.typeName, out type);
                if (flag)
                {
                    this.resolvedDataType = (TwinCAT.Ads.TypeSystem.DataType) type;
                    DataTypeCategory category = this.category;
                    this.category = this.resolvedDataType.Category;
                }
                if (this.resolvedDataType == null)
                {
                    object[] args = new object[] { this.typeName };
                    Trace.TraceWarning("Cannot resolve DataType '{0}'", args);
                }
            }
            return flag;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        void IInstanceInternal.SetInstanceName(string instanceName)
        {
            this.OnSetInstanceName(instanceName);
        }

        public bool IsBound =>
            (this.resolver != null);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IBinder Binder =>
            this.resolver;

        public string Namespace =>
            this.ns;

        public int Size =>
            this.OnGetSize();

        public AdsSymbolFlags Flags =>
            this.flags;

        public int ByteSize
        {
            get
            {
                int size = 0;
                if (!this.IsBitType)
                {
                    size = this.Size;
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

        public virtual int BitSize =>
            (!this.IsBitType ? (this.Size * 8) : this.Size);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AdsDatatypeId DataTypeId =>
            this.dataTypeId;

        public DataTypeCategory Category
        {
            get
            {
                if (this.category == DataTypeCategory.Unknown)
                {
                    this.TryResolveType();
                }
                return this.category;
            }
        }

        public string TypeName =>
            this.typeName;

        public IDataType DataType
        {
            get
            {
                if (this.resolvedDataType == null)
                {
                    this.TryResolveType();
                    if ((this.resolvedDataType != null) && (this.resolvedDataType.ByteSize != this.ByteSize))
                    {
                        if ((this.resolvedDataType is ReferenceType) || (this.resolvedDataType is PointerType))
                        {
                            TwinCAT.Ads.TypeSystem.DataType resolvedDataType = (TwinCAT.Ads.TypeSystem.DataType) this.resolvedDataType;
                            if ((this.ByteSize == 4) || (this.ByteSize == 8))
                            {
                                ((TwinCAT.TypeSystem.Binder) this.resolver).SetPlatformPointerSize(this.ByteSize);
                            }
                        }
                        else if (this.IsBitType)
                        {
                            if (this.BitSize != this.resolvedDataType.BitSize)
                            {
                                this.size = this.resolvedDataType.Size;
                            }
                        }
                        else if (this.ByteSize != this.resolvedDataType.ByteSize)
                        {
                            object[] args = new object[] { this.InstancePath, this.ByteSize, this.resolvedDataType.Name, this.resolvedDataType.ByteSize };
                            Module.Trace.TraceWarning("Mismatching Byte size Instance: {0} ({1} bytes), Type: {2} ({3} bytes)", args);
                        }
                    }
                }
                return this.resolvedDataType;
            }
        }

        public string Comment =>
            this.comment;

        public string InstanceName =>
            this.instanceName;

        public virtual string InstancePath =>
            this.instanceName;

        public virtual bool HasValue =>
            true;

        public bool IsStatic =>
            this.staticAddress;

        public bool IsBitType =>
            ((this.flags & AdsSymbolFlags.BitValue) == AdsSymbolFlags.BitValue);

        public bool IsReadOnly =>
            ((this.flags & (AdsSymbolFlags.None | AdsSymbolFlags.ReadOnly)) == (AdsSymbolFlags.None | AdsSymbolFlags.ReadOnly));

        public bool IsPersistent =>
            ((this.flags & (AdsSymbolFlags.None | AdsSymbolFlags.Persistent)) == (AdsSymbolFlags.None | AdsSymbolFlags.Persistent));

        public bool IsTcComInterfacePointer =>
            ((this.flags & (AdsSymbolFlags.None | AdsSymbolFlags.TComInterfacePtr)) == (AdsSymbolFlags.None | AdsSymbolFlags.TComInterfacePtr));

        public bool IsTypeGuid =>
            ((this.flags & (AdsSymbolFlags.None | AdsSymbolFlags.TypeGuid)) == (AdsSymbolFlags.None | AdsSymbolFlags.TypeGuid));

        public bool IsReference
        {
            get
            {
                bool isReference = false;
                if (this.resolvedDataType != null)
                {
                    isReference = this.resolvedDataType.IsReference;
                }
                else if (!(this is VirtualStructInstance) && !string.IsNullOrEmpty(this.typeName))
                {
                    isReference = DataTypeStringParser.IsReference(this.typeName);
                }
                return isReference;
            }
        }

        public bool IsPointer
        {
            get
            {
                bool isPointer = false;
                if (this.resolvedDataType != null)
                {
                    isPointer = this.resolvedDataType.IsPointer;
                }
                else if (!(this is VirtualStructInstance) && !string.IsNullOrEmpty(this.typeName))
                {
                    isPointer = DataTypeStringParser.IsPointer(this.typeName);
                }
                return isPointer;
            }
        }

        public byte ContextMask =>
            ((byte) (((ushort) (this.flags & 0xf00)) >> 8));

        public ReadOnlyTypeAttributeCollection Attributes =>
            ((this.attributes == null) ? new TypeAttributeCollection().AsReadOnly() : this.attributes.AsReadOnly());
    }
}

