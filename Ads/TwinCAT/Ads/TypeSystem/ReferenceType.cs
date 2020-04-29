namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public sealed class ReferenceType : DataType, IReferenceType, IDataType, IBitSize
    {
        private string referencedTypeName;
        private IDataType referencedType;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ReferenceType(string referencedTypeName, int byteSize) : this(null, referencedTypeName, byteSize)
        {
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ReferenceType(AdsDataTypeEntry entry, string referencedTypeName) : base(DataTypeCategory.Reference, entry)
        {
            this.referencedTypeName = string.Empty;
            this.referencedTypeName = referencedTypeName;
            if (base.size == 4)
            {
                base.dotnetType = typeof(uint);
            }
            else if (base.size == 8)
            {
                base.dotnetType = typeof(ulong);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ReferenceType(string name, string referencedTypeName, int size) : base(name, AdsDatatypeId.ADST_VOID, DataTypeCategory.Reference, size, null)
        {
            this.referencedTypeName = string.Empty;
            if ((size != 4) && (size != 8))
            {
                throw new ArgumentOutOfRangeException("size", $"Reference size is {size}!");
            }
            if (string.IsNullOrEmpty(referencedTypeName))
            {
                throw new ArgumentOutOfRangeException("referencedTypeName");
            }
            if (string.IsNullOrEmpty(name))
            {
                base.name = $"REFERENCE TO {referencedTypeName}";
            }
            this.referencedTypeName = referencedTypeName;
            if (size == 4)
            {
                base.dotnetType = typeof(uint);
            }
            else if (size == 8)
            {
                base.dotnetType = typeof(ulong);
            }
            base.flags = AdsDataTypeFlags.DataType | AdsDataTypeFlags.ReferenceTo;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        protected override void OnBound(IBinder binder)
        {
            if ((base.size == 4) || (base.size == 8))
            {
                if (binder.PlatformPointerSize <= 0)
                {
                    ((Binder) binder).SetPlatformPointerSize(base.Size);
                }
            }
            else
            {
                base.size = binder.PlatformPointerSize;
                if (base.size == 4)
                {
                    base.dotnetType = typeof(uint);
                }
                if (base.size == 8)
                {
                    base.dotnetType = typeof(ulong);
                }
            }
        }

        public override bool IsContainer
        {
            get
            {
                bool isContainer = false;
                IDataType referencedType = this.ReferencedType;
                if (referencedType != null)
                {
                    isContainer = referencedType.IsContainer;
                }
                return isContainer;
            }
        }

        public override Type ManagedType
        {
            get
            {
                if (base.dotnetType == null)
                {
                    int platformPointerSize = base.resolver.PlatformPointerSize;
                    if (platformPointerSize == 4)
                    {
                        base.dotnetType = typeof(uint);
                    }
                    else if (platformPointerSize == 8)
                    {
                        base.dotnetType = typeof(ulong);
                    }
                }
                return base.dotnetType;
            }
        }

        public IDataType ReferencedType
        {
            get
            {
                if (this.referencedType == null)
                {
                    IDataType type = null;
                    base.resolver.TryResolveType(this.referencedTypeName, out type);
                    this.referencedType = (DataType) type;
                }
                return this.referencedType;
            }
        }

        public DataTypeCategory ResolvedCategory
        {
            get
            {
                IDataType resolvedType = this.ResolvedType;
                return ((resolvedType == null) ? DataTypeCategory.Unknown : resolvedType.Category);
            }
        }

        public int ResolvedByteSize
        {
            get
            {
                IDataType resolvedType = this.ResolvedType;
                return ((resolvedType == null) ? 0 : resolvedType.ByteSize);
            }
        }

        public IDataType ResolvedType =>
            base.ResolveType(DataTypeResolveStrategy.AliasReference);
    }
}

