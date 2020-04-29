namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    internal class CategoryConverter
    {
        internal static AdsDatatypeId FromCategory(DataTypeCategory cat)
        {
            switch (cat)
            {
                case DataTypeCategory.Unknown:
                case DataTypeCategory.Primitive:
                case DataTypeCategory.Alias:
                    return AdsDatatypeId.ADST_VOID;

                case DataTypeCategory.Enum:
                case DataTypeCategory.Array:
                case DataTypeCategory.Struct:
                case DataTypeCategory.FunctionBlock:
                case DataTypeCategory.Program:
                case DataTypeCategory.Function:
                case DataTypeCategory.SubRange:
                    return AdsDatatypeId.ADST_BIGTYPE;

                case DataTypeCategory.Bitset:
                    return AdsDatatypeId.ADST_BIT;
            }
            return AdsDatatypeId.ADST_VOID;
        }

        internal static DataTypeCategory FromId(AdsDatatypeId id)
        {
            if (id > AdsDatatypeId.ADST_UINT64)
            {
                switch (id)
                {
                    case AdsDatatypeId.ADST_STRING:
                    case AdsDatatypeId.ADST_WSTRING:
                        return DataTypeCategory.String;

                    case AdsDatatypeId.ADST_REAL80:
                        goto TR_0000;

                    case AdsDatatypeId.ADST_BIT:
                        break;

                    default:
                        if (id == AdsDatatypeId.ADST_BIGTYPE)
                        {
                            return DataTypeCategory.Unknown;
                        }
                        goto TR_0000;
                }
                goto TR_0001;
            }
            else
            {
                switch (id)
                {
                    case AdsDatatypeId.ADST_VOID:
                    case AdsDatatypeId.ADST_INT16:
                    case AdsDatatypeId.ADST_INT32:
                    case AdsDatatypeId.ADST_REAL32:
                    case AdsDatatypeId.ADST_REAL64:
                        goto TR_0001;

                    case ((AdsDatatypeId) 1):
                        break;

                    default:
                        switch (id)
                        {
                            case AdsDatatypeId.ADST_INT8:
                            case AdsDatatypeId.ADST_UINT8:
                            case AdsDatatypeId.ADST_UINT16:
                            case AdsDatatypeId.ADST_UINT32:
                            case AdsDatatypeId.ADST_INT64:
                            case AdsDatatypeId.ADST_UINT64:
                                break;

                            default:
                                goto TR_0000;
                        }
                        goto TR_0001;
                }
            }
        TR_0000:
            return DataTypeCategory.Unknown;
        TR_0001:
            return DataTypeCategory.Primitive;
        }

        internal static DataTypeCategory FromId(AdsDatatypeId id, string typeName)
        {
            DataTypeCategory pointer = FromId(id);
            if (!string.IsNullOrEmpty(typeName) && (pointer == DataTypeCategory.Unknown))
            {
                if (DataTypeStringParser.IsPointer(typeName))
                {
                    pointer = DataTypeCategory.Pointer;
                }
                else if (DataTypeStringParser.IsReference(typeName))
                {
                    pointer = DataTypeCategory.Reference;
                }
                else if (DataTypeStringParser.IsArray(typeName))
                {
                    pointer = DataTypeCategory.Array;
                }
                else if (DataTypeStringParser.IsSubRange(typeName))
                {
                    pointer = DataTypeCategory.SubRange;
                }
                else if (DataTypeStringParser.IsIntrinsicType(typeName))
                {
                    pointer = DataTypeCategory.Primitive;
                }
                else if (DataTypeStringParser.IsString(typeName))
                {
                    pointer = DataTypeCategory.String;
                }
            }
            return pointer;
        }

        internal static DataTypeCategory FromType(ITcAdsDataType type) => 
            (!DataTypeStringParser.IsReference(type.Name) ? (!DataTypeStringParser.IsPointer(type.Name) ? (!DataTypeStringParser.IsSubRange(type.Name) ? (!type.HasSubItemInfo ? (!type.HasArrayInfo ? (!type.HasEnumInfo ? (string.IsNullOrEmpty(type.BaseTypeName) ? (!DataTypeStringParser.IsString(type.Name) ? FromId(type.DataTypeId, type.Name) : DataTypeCategory.String) : DataTypeCategory.Alias) : DataTypeCategory.Enum) : DataTypeCategory.Array) : DataTypeCategory.Struct) : DataTypeCategory.SubRange) : DataTypeCategory.Pointer) : DataTypeCategory.Reference);
    }
}

