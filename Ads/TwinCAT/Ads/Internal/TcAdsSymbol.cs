namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("Path = { _name }, Type = {_typeName}, Size = {_size}, IG = {_indexGroup}, IO = {_indexOffset}")]
    internal class TcAdsSymbol : ITcAdsSymbol5, ITcAdsSymbol4, ITcAdsSymbol3, ITcAdsSymbol2, ITcAdsSymbol
    {
        private uint _indexGroup;
        private uint _indexOffset;
        private uint _size;
        private AdsDatatypeId _typeId;
        private AdsDataTypeFlags _typeEntryFlags;
        private AdsSymbolFlags _flags;
        private string _name;
        private string _typeName;
        private string _comment;
        private TcAdsDataType _dataType;
        private int _attributeCount;
        private AdsAttributeEntry[] _attributes;
        private int _arrayDimensions;
        private AdsDatatypeArrayInfo[] _arrayInfos;

        internal TcAdsSymbol(AdsSymbolEntry symbolEntry, TcAdsDataType typeEntry)
        {
            if (symbolEntry == null)
            {
                throw new ArgumentNullException("symbolEntry");
            }
            if (typeEntry == null)
            {
                object[] args = new object[] { symbolEntry.name };
                Module.Trace.TraceWarning("No data type found for AdsSymbolEntry '{0}'", args);
            }
            this._indexGroup = symbolEntry.indexGroup;
            this._indexOffset = symbolEntry.indexOffset;
            this._size = symbolEntry.size;
            this._typeId = (AdsDatatypeId) symbolEntry.dataType;
            this._typeEntryFlags = typeEntry.Flags;
            this._flags = symbolEntry.flags;
            this._name = symbolEntry.name;
            this._typeName = symbolEntry.type;
            this._comment = symbolEntry.comment;
            this._arrayDimensions = symbolEntry.arrayDim;
            this._arrayInfos = symbolEntry.array;
            this._attributeCount = symbolEntry.attributeCount;
            this._attributes = symbolEntry.attributes;
            this._dataType = typeEntry;
        }

        public System.Type GetManagedType()
        {
            AdsDatatypeId id = this._typeId;
            if (id > AdsDatatypeId.ADST_UINT64)
            {
                if (id == AdsDatatypeId.ADST_STRING)
                {
                    if (this._typeName.StartsWith("STRING(", StringComparison.OrdinalIgnoreCase) && this._typeName.EndsWith(")", StringComparison.OrdinalIgnoreCase))
                    {
                        uint result = 0;
                        if (uint.TryParse(this._typeName.Substring("STRING(".Length), out result))
                        {
                            return typeof(string);
                        }
                    }
                }
                else if (id == AdsDatatypeId.ADST_BIT)
                {
                    if (this._typeName.Equals("BOOL", StringComparison.OrdinalIgnoreCase))
                    {
                        return typeof(bool);
                    }
                }
                else if (id == AdsDatatypeId.ADST_BIGTYPE)
                {
                    if (this._typeName.Equals("TIME", StringComparison.OrdinalIgnoreCase))
                    {
                        goto TR_001D;
                    }
                    else if (this._typeName.Equals("TOD", StringComparison.OrdinalIgnoreCase))
                    {
                        goto TR_001D;
                    }
                    else if (!this._typeName.Equals("TIME_OF_DAY", StringComparison.OrdinalIgnoreCase))
                    {
                        if ((this._typeName.Equals("DATE", StringComparison.OrdinalIgnoreCase) || this._typeName.Equals("DT", StringComparison.OrdinalIgnoreCase)) || this._typeName.Equals("DATE_AND_TIME", StringComparison.OrdinalIgnoreCase))
                        {
                            return typeof(DateTime);
                        }
                    }
                    else
                    {
                        goto TR_001D;
                    }
                }
                goto TR_0000;
            }
            else
            {
                switch (id)
                {
                    case AdsDatatypeId.ADST_INT16:
                        if (!this._typeName.Equals("INT", StringComparison.OrdinalIgnoreCase) && !this._typeName.Equals("INT16", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        return typeof(short);

                    case AdsDatatypeId.ADST_INT32:
                        if (!this._typeName.Equals("DINT", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        return typeof(int);

                    case AdsDatatypeId.ADST_REAL32:
                        if (!this._typeName.Equals("REAL", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        return typeof(float);

                    case AdsDatatypeId.ADST_REAL64:
                        if (!this._typeName.Equals("LREAL", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        return typeof(double);

                    default:
                        switch (id)
                        {
                            case AdsDatatypeId.ADST_INT8:
                                if (!this._typeName.Equals("SINT", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }
                                return typeof(sbyte);

                            case AdsDatatypeId.ADST_UINT8:
                                if (!this._typeName.Equals("USINT", StringComparison.OrdinalIgnoreCase) && !this._typeName.Equals("BYTE", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }
                                return typeof(byte);

                            case AdsDatatypeId.ADST_UINT16:
                                if ((!this._typeName.Equals("UINT", StringComparison.OrdinalIgnoreCase) && !this._typeName.Equals("WORD", StringComparison.OrdinalIgnoreCase)) && !this._typeName.Equals("UINT16", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }
                                return typeof(ushort);

                            case AdsDatatypeId.ADST_UINT32:
                                if (!this._typeName.Equals("UDINT", StringComparison.OrdinalIgnoreCase) && !this._typeName.Equals("DWORD", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }
                                return typeof(uint);

                            case AdsDatatypeId.ADST_INT64:
                                if (!this._typeName.Equals("LINT", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }
                                return typeof(uint);

                            case AdsDatatypeId.ADST_UINT64:
                                if (!this._typeName.Equals("ULINT", StringComparison.OrdinalIgnoreCase) && !this._typeName.Equals("LWORD", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }
                                return typeof(uint);

                            default:
                                break;
                        }
                        break;
                }
                goto TR_0000;
            }
            goto TR_001D;
        TR_0000:
            return null;
        TR_001D:
            return typeof(TimeSpan);
        }

        public bool IsRecursive(IEnumerable<ITcAdsSymbol5> parents) => 
            isSelfReference(parents, this);

        internal static bool isSelfReference(IEnumerable<ITcAdsSymbol5> parents, ITcAdsSymbol5 subSymbol)
        {
            List<ITcAdsSymbol5> list = new List<ITcAdsSymbol5>(parents);
            if (list.Count != 0)
            {
                list.Insert(0, subSymbol);
                int num = 0;
                while (num < (list.Count - 1))
                {
                    ITcAdsSymbol5 symbol = list[num];
                    int num2 = num + 1;
                    while (true)
                    {
                        if (num2 >= list.Count)
                        {
                            num++;
                            break;
                        }
                        ITcAdsSymbol5 symbol2 = list[num2];
                        if (((symbol.DataType != null) && (symbol2.DataType != null)) && ReferenceEquals(symbol.DataType, symbol2.DataType))
                        {
                            return true;
                        }
                        num2++;
                    }
                }
            }
            return false;
        }

        public ITcAdsDataType DataType =>
            this._dataType;

        public long IndexGroup
        {
            get => 
                ((long) this._indexGroup);
            set => 
                (this._indexGroup = (uint) value);
        }

        public long IndexOffset
        {
            get => 
                ((long) this._indexOffset);
            set => 
                (this._indexOffset = (uint) value);
        }

        public int Size =>
            ((int) this._size);

        public AdsDatatypeId DataTypeId =>
            this._typeId;

        [Obsolete("Use ITcAdsSymbol5.DataTypeId instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
        public AdsDatatypeId Datatype =>
            this.DataTypeId;

        public string Name =>
            this._name;

        public string TypeName =>
            this._typeName;

        [Obsolete("Use ITcAdsSymbol5.TypeName instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
        public string Type =>
            this.TypeName;

        public string Comment =>
            this._comment;

        public bool IsPersistent =>
            ((this._flags & (AdsSymbolFlags.None | AdsSymbolFlags.Persistent)) == (AdsSymbolFlags.None | AdsSymbolFlags.Persistent));

        public bool IsStatic =>
            ((this._typeEntryFlags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Static)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.Static));

        public bool IsBitType =>
            ((this._flags & AdsSymbolFlags.BitValue) == AdsSymbolFlags.BitValue);

        public bool IsPointer
        {
            get
            {
                string referencedType = string.Empty;
                return DataTypeStringParser.TryParsePointer(this._typeName, out referencedType);
            }
        }

        public bool IsReference
        {
            get
            {
                string referencedType = string.Empty;
                return DataTypeStringParser.TryParseReference(this._typeName, out referencedType);
            }
        }

        public bool IsTypeGuid =>
            ((this._flags & (AdsSymbolFlags.None | AdsSymbolFlags.TypeGuid)) == (AdsSymbolFlags.None | AdsSymbolFlags.TypeGuid));

        public bool IsReadOnly =>
            ((this._flags & (AdsSymbolFlags.None | AdsSymbolFlags.ReadOnly)) == (AdsSymbolFlags.None | AdsSymbolFlags.ReadOnly));

        public bool IsTcComInterfacePointer =>
            ((this._flags & (AdsSymbolFlags.None | AdsSymbolFlags.TComInterfacePtr)) == (AdsSymbolFlags.None | AdsSymbolFlags.TComInterfacePtr));

        public int ContextMask =>
            (((ushort) (this._flags & 0xf00)) >> 8);

        public int AttributeCount =>
            this._attributeCount;

        public ReadOnlyTypeAttributeCollection Attributes
        {
            get
            {
                TypeAttributeCollection attributes = (this._attributes == null) ? new TypeAttributeCollection() : new TypeAttributeCollection(this._attributes);
                return attributes.AsReadOnly();
            }
        }

        public bool IsArray =>
            (this._arrayDimensions > 0);

        public int ArrayDimensions =>
            this._arrayDimensions;

        public AdsDatatypeArrayInfo[] ArrayInfos =>
            this._arrayInfos;

        public bool IsEnum =>
            ((this._dataType != null) && this._dataType.IsEnum);

        public bool IsStruct =>
            ((this._dataType != null) && this._dataType.IsStruct);

        public bool HasRpcMethods =>
            ((this._dataType != null) && ((this._dataType.RpcMethods != null) && (this._dataType.RpcMethods.Count > 0)));

        public ReadOnlyRpcMethodCollection RpcMethods =>
            ((this._dataType == null) ? ReadOnlyRpcMethodCollection.Empty : this._dataType.RpcMethods);

        public DataTypeCategory Category =>
            ((this._dataType == null) ? DataTypeCategory.Unknown : this._dataType.Category);

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
    }
}

