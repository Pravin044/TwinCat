namespace TwinCAT.Ads
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads.Internal;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("Path = { instancePath }, Type = {typeName}, Size = {size}, IG = {indexGroup}, IO = {indexOffset}, IsStatic = {IsStatic}")]
    public class TcAdsSymbolInfo : ITcAdsSymbol5, ITcAdsSymbol4, ITcAdsSymbol3, ITcAdsSymbol2, ITcAdsSymbol, ITcAdsSymbolBrowser
    {
        private static int id_counter = 1;
        internal uint indexGroup;
        internal uint indexOffset;
        internal uint size;
        internal AdsDatatypeId dataTypeId;
        internal AdsDataTypeFlags typeEntryFlags;
        internal AdsSymbolFlags flags;
        internal string instancePath;
        internal string shortName;
        internal string typeName;
        internal string comment;
        internal ITcAdsDataType dataType;
        internal AdsParseSymbols symbolParser;
        internal TcAdsSymbolInfo parent;
        internal int subIndex;
        internal TcAdsSymbolInfoCollection subSymbols;
        internal AdsDatatypeArrayInfo[] arrayInfo;
        internal ReadOnlyTypeAttributeCollection attributes;
        private int _id;

        internal TcAdsSymbolInfo(AdsParseSymbols symbolParser, TcAdsSymbolInfo parent, int subIndex)
        {
            if (symbolParser == null)
            {
                throw new ArgumentNullException("symbolParser");
            }
            id_counter++;
            this._id = id_counter;
            this.symbolParser = symbolParser;
            this.parent = parent;
            this.subIndex = subIndex;
            this.subSymbols = null;
        }

        internal TcAdsSymbolInfo(AdsParseSymbols symbolParser, TcAdsSymbolInfo parent, int subIndex, TcAdsDataType typeEntry) : this(symbolParser, parent, subIndex)
        {
            if (typeEntry == null)
            {
                throw new ArgumentNullException("typeEntry");
            }
            this.indexGroup = (uint) parent.IndexGroup;
            this.indexOffset = (uint) parent.IndexOffset;
            this.size = (uint) typeEntry.Size;
            this.dataTypeId = typeEntry.DataTypeId;
            this.typeEntryFlags = typeEntry.Flags;
            this.flags = DataTypeFlagConverter.Convert(typeEntry.Flags);
            this.instancePath = parent.Name;
            this.shortName = parent.ShortName;
            this.typeName = typeEntry.Name;
            this.comment = typeEntry.Comment;
            this.dataType = typeEntry;
            this.arrayInfo = typeEntry.ArrayInfo;
            this.attributes = typeEntry.Attributes;
        }

        internal TcAdsSymbolInfo(AdsParseSymbols symbolParser, TcAdsSymbolInfo parent, int subIndex, AdsSymbolEntry symbolEntry, TcAdsDataType typeEntry) : this(symbolParser, parent, subIndex)
        {
            if (symbolEntry == null)
            {
                throw new ArgumentNullException("symbolEntry");
            }
            this.indexGroup = symbolEntry.indexGroup;
            this.indexOffset = symbolEntry.indexOffset;
            this.size = symbolEntry.size;
            this.dataTypeId = (AdsDatatypeId) symbolEntry.dataType;
            if (typeEntry != null)
            {
                this.typeEntryFlags = typeEntry.Flags;
            }
            if (this.typeEntryFlags.HasFlag(AdsDataTypeFlags.None | AdsDataTypeFlags.Static))
            {
                this.indexGroup = 0xf019;
                this.indexOffset = 0;
            }
            this.flags = symbolEntry.flags;
            this.instancePath = symbolEntry.name;
            this.shortName = symbolEntry.name;
            this.typeName = symbolEntry.type;
            this.comment = symbolEntry.comment;
            this.dataType = typeEntry;
            if (((symbolEntry.array != null) || (typeEntry == null)) || !typeEntry.IsArray)
            {
                this.arrayInfo = symbolEntry.array;
            }
            else
            {
                this.arrayInfo = typeEntry.Dimensions.ToArray();
                if (typeEntry.Size != this.size)
                {
                    this.size = (uint) typeEntry.Size;
                }
            }
            if (symbolEntry.attributes != null)
            {
                this.attributes = new TypeAttributeCollection(symbolEntry.attributes).AsReadOnly();
            }
            else
            {
                this.attributes = null;
            }
        }

        private void AlignBitType()
        {
            if (((this.dataType != null) && this.dataType.IsBitType) && !this.IsBitType)
            {
                string message = $"Correction Symbol '{this.Name}:{this.dataType.Name}' --> IsBitType";
                Module.Trace.TraceWarning(message);
                this.SetBitType(true);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (base.GetType() != obj.GetType())
            {
                return false;
            }
            TcAdsSymbolInfo info = (TcAdsSymbolInfo) obj;
            return (StringComparer.OrdinalIgnoreCase.Compare(this.instancePath, info.instancePath) == 0);
        }

        internal TcAdsSymbolInfo GetFirstSubSymbol(bool dereference) => 
            this.symbolParser?.GetSubSymbol(this, 0, dereference);

        public override int GetHashCode() => 
            ((0x5b * 10) + StringComparer.OrdinalIgnoreCase.GetHashCode(this.instancePath));

        public bool IsRecursive(IEnumerable<ITcAdsSymbol5> parents) => 
            TcAdsSymbol.isSelfReference(parents, this);

        public static bool operator ==(TcAdsSymbolInfo s1, TcAdsSymbolInfo s2) => 
            (!Equals(s1, null) ? s1.Equals(s2) : Equals(s2, null));

        public static bool operator !=(TcAdsSymbolInfo s1, TcAdsSymbolInfo s2) => 
            !(s1 == s2);

        internal ITcAdsDataType ResolveType(DataTypeResolveStrategy type)
        {
            ITcAdsDataType dataType = this.DataType;
            return ((dataType == null) ? null : ((ITcAdsDataType) dataType.ResolveType(type)));
        }

        private void SetBitType(bool bitType)
        {
            if (bitType)
            {
                this.flags |= AdsSymbolFlags.BitValue;
            }
            else
            {
                this.flags &= AdsSymbolFlags.Attributes | AdsSymbolFlags.ContextMask | AdsSymbolFlags.ExtendedFlags | AdsSymbolFlags.InitOnReset | AdsSymbolFlags.ItfMethodAccess | AdsSymbolFlags.MethodDeref | AdsSymbolFlags.Persistent | AdsSymbolFlags.ReadOnly | AdsSymbolFlags.ReferenceTo | AdsSymbolFlags.Static | AdsSymbolFlags.TComInterfacePtr | AdsSymbolFlags.TypeGuid;
            }
        }

        public override string ToString() => 
            this.Name;

        public bool TryGetPointerRef(out string referencedType)
        {
            ITcAdsDataType type = this.ResolveType(DataTypeResolveStrategy.AliasReference);
            string typeName = this.typeName;
            if (type != null)
            {
                typeName = type.Name;
            }
            return DataTypeStringParser.TryParsePointer(typeName, out referencedType);
        }

        public bool TryGetReference(out string referencedType)
        {
            ITcAdsDataType type = this.ResolveType(DataTypeResolveStrategy.AliasReference);
            string typeName = this.typeName;
            if (type != null)
            {
                typeName = type.Name;
            }
            return DataTypeStringParser.TryParseReference(typeName, out referencedType);
        }

        public ITcAdsDataType DataType
        {
            get
            {
                if (this.dataType == null)
                {
                    this.dataType = (ITcAdsDataType) this.symbolParser.ResolveDataType(this.typeName);
                }
                return this.dataType;
            }
        }

        public TcAdsSymbolInfo Parent =>
            this.parent;

        [Obsolete("Use the SubSymbols Collection instead", false)]
        public TcAdsSymbolInfo NextSymbol =>
            ((this.symbolParser != null) ? ((this.parent == null) ? this.symbolParser.GetSymbol((int) (this.subIndex + 1)) : this.symbolParser.GetSubSymbol(this.parent, this.subIndex + 1, true)) : null);

        [Obsolete("Use the SubSymbols Collection instead", false)]
        public TcAdsSymbolInfo FirstSubSymbol =>
            this.GetFirstSubSymbol(true);

        public TcAdsSymbolInfoCollection SubSymbols
        {
            get
            {
                if (this.symbolParser == null)
                {
                    return null;
                }
                if (this.subSymbols == null)
                {
                    this.subSymbols = new TcAdsSymbolInfoCollection(this);
                }
                return this.subSymbols;
            }
        }

        [Obsolete("Use the SubSymbols Collection instead", false)]
        public int SubSymbolCount =>
            ((this.symbolParser != null) ? this.symbolParser.GetSubSymbolCount(this) : 0);

        public long IndexGroup =>
            ((long) this.indexGroup);

        public long IndexOffset =>
            ((long) this.indexOffset);

        public int Size =>
            ((int) this.size);

        [Obsolete("Use ITcAdsSymbol5.DataTypeId instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
        public AdsDatatypeId Datatype =>
            this.DataTypeId;

        public AdsDatatypeId DataTypeId =>
            this.dataTypeId;

        public string Name =>
            this.instancePath;

        public string ShortName =>
            this.shortName;

        [Obsolete("Use ITcAdsSymbol5.TypeName instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
        public string Type =>
            this.TypeName;

        public string TypeName =>
            this.typeName;

        public string Comment =>
            this.comment;

        [Obsolete("Use TcAdsSymbolInfo.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool IsPointer =>
            (this.Category == DataTypeCategory.Pointer);

        [Obsolete("Use TcAdsSymbolInfo.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool IsReference =>
            (this.Category == DataTypeCategory.Reference);

        internal bool IsDereferencedReference
        {
            get
            {
                for (TcAdsSymbolInfo info = this; info.Parent != null; info = info.Parent)
                {
                    if (info.Parent.IsReference)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        internal bool IsDereferencedPointer
        {
            get
            {
                for (TcAdsSymbolInfo info = this; info.Parent != null; info = info.Parent)
                {
                    if (info.Parent.IsPointer)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsPersistent =>
            ((this.flags & (AdsSymbolFlags.None | AdsSymbolFlags.Persistent)) == (AdsSymbolFlags.None | AdsSymbolFlags.Persistent));

        public bool IsBitType =>
            ((this.flags & AdsSymbolFlags.BitValue) == AdsSymbolFlags.BitValue);

        public bool IsStatic =>
            ((this.typeEntryFlags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Static)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.Static));

        public bool IsTypeGuid =>
            ((this.flags & (AdsSymbolFlags.None | AdsSymbolFlags.TypeGuid)) == (AdsSymbolFlags.None | AdsSymbolFlags.TypeGuid));

        public bool IsReadOnly =>
            ((this.flags & (AdsSymbolFlags.None | AdsSymbolFlags.ReadOnly)) == (AdsSymbolFlags.None | AdsSymbolFlags.ReadOnly));

        public bool IsTcComInterfacePointer =>
            ((this.flags & (AdsSymbolFlags.None | AdsSymbolFlags.TComInterfacePtr)) == (AdsSymbolFlags.None | AdsSymbolFlags.TComInterfacePtr));

        public int ContextMask =>
            (((ushort) (this.flags & 0xf00)) >> 8);

        [Obsolete("Use TcAdsSymbolInfo.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool IsArray =>
            ((this.arrayInfo != null) && (this.arrayInfo.Length != 0));

        public int ArrayDimensions =>
            (!this.IsArray ? 0 : this.arrayInfo.Length);

        public AdsDatatypeArrayInfo[] ArrayInfos =>
            this.arrayInfo;

        public bool IsOversamplingArray
        {
            get
            {
                if (!this.IsArray)
                {
                    return false;
                }
                ITcAdsDataType dataType = this.DataType;
                return ((dataType != null) && dataType.IsOversamplingArray);
            }
        }

        public ReadOnlyTypeAttributeCollection Attributes =>
            ((this.attributes == null) ? new TypeAttributeCollection().AsReadOnly() : this.attributes);

        [Obsolete("Use ITcAdsSymbol4.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool IsEnum =>
            ((this.DataType != null) && (this.DataType.Category == DataTypeCategory.Enum));

        public bool HasRpcMethods =>
            ((this.DataType != null) && this.DataType.HasRpcMethods);

        public ReadOnlyRpcMethodCollection RpcMethods =>
            ((this.DataType == null) ? ReadOnlyRpcMethodCollection.Empty : this.DataType.RpcMethods);

        public DataTypeCategory Category =>
            ((this.DataType == null) ? CategoryConverter.FromId(this.DataTypeId, this.typeName) : this.DataType.Category);

        [Obsolete("Use ITcAdsSymbol4.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool IsStruct =>
            ((this.DataType != null) && (this.DataType.Category == DataTypeCategory.Struct));

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

