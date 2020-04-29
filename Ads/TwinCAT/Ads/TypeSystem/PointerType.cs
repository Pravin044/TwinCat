namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public sealed class PointerType : DataType, IPointerType, IDataType, IBitSize
    {
        private string referencedTypeName;
        private IDataType referencedType;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public PointerType(string referencedTypeName) : base($"POINTER TO {referencedTypeName}", AdsDatatypeId.ADST_VOID, DataTypeCategory.Pointer, 0, null)
        {
            this.referencedTypeName = string.Empty;
            this.referencedTypeName = referencedTypeName;
            base.flags = AdsDataTypeFlags.DataType;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public PointerType(string name, string referencedTypeName) : base(name, AdsDatatypeId.ADST_VOID, DataTypeCategory.Pointer, 0, null)
        {
            this.referencedTypeName = string.Empty;
            this.referencedTypeName = referencedTypeName;
            base.flags = AdsDataTypeFlags.DataType;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        internal PointerType(AdsDataTypeEntry entry, string referencedTypeName) : base(DataTypeCategory.Pointer, entry)
        {
            this.referencedTypeName = string.Empty;
            this.referencedTypeName = referencedTypeName;
            if (entry.size == 4)
            {
                base.dotnetType = typeof(uint);
            }
            else
            {
                base.dotnetType = typeof(ulong);
            }
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
                    else
                    {
                        object[] args = new object[] { base.Name };
                        Module.Trace.TraceWarning("Couldn't resolve Pointer type '{0}' yet!", args);
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
    }
}

