namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class AdsDataTypeEntry : IAdsCustomMarshal<AdsDataTypeEntry>
    {
        internal uint entryLength;
        internal uint version;
        internal uint hashValue;
        internal uint typeHashValue;
        internal uint size;
        internal uint offset;
        internal AdsDatatypeId baseTypeId;
        internal AdsDataTypeFlags flags;
        internal ushort nameLength;
        internal ushort typeLength;
        internal ushort commentLength;
        internal ushort arrayDim;
        internal ushort subItems;
        internal string entryName;
        internal string typeName;
        internal string comment;
        internal AdsDatatypeArrayInfo[] arrayInfos;
        public AdsFieldEntry[] subEntries;
        internal Guid typeGuid;
        internal byte[] copyMask;
        internal ushort methodCount;
        internal AdsMethodEntry[] methods;
        internal ushort attributeCount;
        internal AdsAttributeEntry[] attributes;
        internal ushort enumInfoCount;
        internal AdsEnumInfoEntry[] enums;
        internal byte[] reserved;
        private static int s_idCount;
        internal bool _rootEntry;
        private int _id;
        protected bool isSubItem;

        public AdsDataTypeEntry()
        {
            this.typeGuid = Guid.Empty;
            this._id = ++s_idCount;
            this._rootEntry = false;
        }

        public AdsDataTypeEntry(bool rootEntry, Encoding encoding, AdsBinaryReader reader)
        {
            this.typeGuid = Guid.Empty;
            this._id = ++s_idCount;
            this._rootEntry = rootEntry;
            this.Read(-1L, encoding, reader);
        }

        public void Read(long parentEndPosition, Encoding encoding, AdsBinaryReader reader)
        {
            long position = reader.BaseStream.Position;
            this.entryLength = reader.ReadUInt32();
            long num2 = position + this.entryLength;
            this.version = reader.ReadUInt32();
            this.hashValue = reader.ReadUInt32();
            this.typeHashValue = reader.ReadUInt32();
            this.size = reader.ReadUInt32();
            this.offset = reader.ReadUInt32();
            this.baseTypeId = (AdsDatatypeId) reader.ReadUInt32();
            this.flags = (AdsDataTypeFlags) reader.ReadUInt32();
            this.nameLength = reader.ReadUInt16();
            this.typeLength = reader.ReadUInt16();
            this.commentLength = reader.ReadUInt16();
            this.arrayDim = reader.ReadUInt16();
            this.subItems = reader.ReadUInt16();
            this.entryName = reader.ReadPlcString(this.nameLength + 1, encoding);
            this.typeName = reader.ReadPlcString(this.typeLength + 1, encoding);
            this.comment = reader.ReadPlcString(this.commentLength + 1, encoding);
            this.arrayInfos = SubStructureReader<AdsDatatypeArrayInfo>.Read(this.arrayDim, num2, encoding, reader);
            this.subEntries = SubStructureReader<AdsFieldEntry>.Read(this.subItems, num2, encoding, reader);
            if ((this.subEntries != null) && (this.subEntries.Length != this.subItems))
            {
                object[] args = new object[] { this.entryName, this.typeName, this.subItems, this.subEntries.Length };
                Module.Trace.TraceWarning("Entry name '{0}', type '{1}' indicates {2} subentries but only {3} subentries found!", args);
                this.subItems = (ushort) this.subEntries.Length;
            }
            if (this.flags.HasFlag(AdsDataTypeFlags.None | AdsDataTypeFlags.TypeGuid))
            {
                this.typeGuid = reader.ReadGuid();
            }
            if (this.flags.HasFlag(AdsDataTypeFlags.CopyMask))
            {
                this.copyMask = reader.ReadBytes((int) this.size);
            }
            if (this.flags.HasFlag(AdsDataTypeFlags.MethodInfos))
            {
                this.methodCount = reader.ReadUInt16();
                this.methods = SubStructureReader<AdsMethodEntry>.Read(this.methodCount, num2, encoding, reader);
            }
            if (this.flags.HasFlag(AdsDataTypeFlags.Attributes))
            {
                this.attributeCount = reader.ReadUInt16();
                this.attributes = SubStructureReader<AdsAttributeEntry>.Read(this.attributeCount, num2, encoding, reader);
            }
            if (this.flags.HasFlag(AdsDataTypeFlags.EnumInfos))
            {
                this.enumInfoCount = reader.ReadUInt16();
                this.enums = EnumSubStructureReader<AdsEnumInfoEntry>.Read(this.enumInfoCount, this.size, num2, encoding, reader);
            }
            if (reader.BaseStream.Position <= num2)
            {
                this.reserved = reader.ReadBytes((int) (num2 - reader.BaseStream.Position));
            }
            else if (reader.BaseStream.Position > num2)
            {
                object[] args = new object[] { this.entryName };
                Module.Trace.TraceError("Reading DataType entry for '{0}' failed!", args);
                reader.BaseStream.Position = num2;
            }
        }

        public virtual bool TryGetPointerRef(out string referenceType) => 
            DataTypeStringParser.TryParsePointer(this.entryName, out referenceType);

        public virtual bool TryGetReference(out string referenceType) => 
            DataTypeStringParser.TryParseReference(this.entryName, out referenceType);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AdsDataTypeFlags Flags =>
            this.flags;

        public bool IsPointer
        {
            get
            {
                string referenceType = null;
                return this.TryGetPointerRef(out referenceType);
            }
        }

        public bool IsReference
        {
            get
            {
                string referenceType = null;
                return this.TryGetReference(out referenceType);
            }
        }

        public bool IsSubRange
        {
            get
            {
                string baseType = null;
                return DataTypeStringParser.TryParseSubRange(this.typeName, out baseType);
            }
        }

        public bool IsArray =>
            ((this.arrayDim > 0) && (!this.IsPointer && !this.IsReference));

        public bool IsStruct =>
            ((this.subItems > 0) && (!this.IsPointer && !this.IsReference));

        public bool IsEnum =>
            ((this.enumInfoCount > 0) && (!this.IsPointer && !this.IsReference));

        public bool IsSubItem =>
            this.isSubItem;

        public bool HasAttributes =>
            (this.attributeCount > 0);

        public bool HasRpcMethods =>
            (this.methodCount > 0);

        public int Size =>
            ((int) this.size);

        public bool IsBitType =>
            ((this.flags & AdsDataTypeFlags.BitValues) == AdsDataTypeFlags.BitValues);

        internal bool IsAnySizeArray
        {
            get
            {
                bool flag = (this.flags & AdsDataTypeFlags.AnySizeArray) == AdsDataTypeFlags.BitValues;
                bool flag1 = flag;
                return flag;
            }
        }

        public AdsDatatypeId DataTypeId =>
            this.baseTypeId;

        public string Name =>
            this.typeName;

        public string Comment =>
            this.comment;

        public ReadOnlyTypeAttributeCollection Attributes =>
            ((this.attributes == null) ? new TypeAttributeCollection().AsReadOnly() : new TypeAttributeCollection(this.attributes).AsReadOnly());

        public ReadOnlyRpcMethodCollection RpcMethods =>
            new RpcMethodCollection(this.methods).AsReadOnly();

        public ITcAdsDataType BaseType =>
            null;

        public string BaseTypeName =>
            this.typeName;

        public DataTypeCategory Category
        {
            get
            {
                if (this.DataTypeId == AdsDatatypeId.ADST_BIGTYPE)
                {
                    if (this.IsArray)
                    {
                        return DataTypeCategory.Array;
                    }
                    if (this.IsEnum)
                    {
                        return DataTypeCategory.Enum;
                    }
                    if (this.IsPointer)
                    {
                        return DataTypeCategory.Pointer;
                    }
                    if (this.IsReference)
                    {
                        return DataTypeCategory.Reference;
                    }
                    if (this.IsSubRange)
                    {
                        return DataTypeCategory.SubRange;
                    }
                }
                return CategoryConverter.FromId(this.DataTypeId, this.typeName);
            }
        }

        public ReadOnlyDimensionCollection Dimensions =>
            ((this.arrayInfos == null) ? null : new DimensionCollection(this.arrayInfos).AsReadOnly());

        public ReadOnlyEnumValueCollection EnumInfos =>
            ((this.enums == null) ? null : new EnumValueCollection(this.baseTypeId, this.enums).AsReadOnly());

        public int BitSize =>
            (!this.IsBitType ? ((int) (this.size * 8)) : ((int) this.size));

        public int ByteSize
        {
            get
            {
                int size = 0;
                if (!this.IsBitType)
                {
                    size = (int) this.size;
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
    }
}

