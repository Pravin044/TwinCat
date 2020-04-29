namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public interface ITcAdsDataType : IDataType, IBitSize, IResolvableType
    {
        AdsDatatypeId DataTypeId { get; }

        bool HasArrayInfo { get; }

        ReadOnlyDimensionCollection Dimensions { get; }

        bool HasRpcMethods { get; }

        ReadOnlyRpcMethodCollection RpcMethods { get; }

        ITcAdsDataType BaseType { get; }

        string BaseTypeName { get; }

        bool HasEnumInfo { get; }

        [Obsolete("Use property EnumValues instead!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        ReadOnlyEnumValueCollection EnumInfos { get; }

        ReadOnlyEnumValueCollection EnumValues { get; }

        ReadOnlySubItemCollection SubItems { get; }

        bool HasSubItemInfo { get; }

        [Obsolete("Use IDataType.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        bool IsEnum { get; }

        [Obsolete("Use IDataType.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        bool IsArray { get; }

        [Obsolete("Use IDataType.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        bool IsStruct { get; }

        bool IsSubItem { get; }

        [Obsolete("Use IDataType.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        bool IsAlias { get; }

        [Obsolete("Use IDataType.Category Property instead!", false)]
        bool IsString { get; }

        Type ManagedType { get; }

        bool IsOversamplingArray { get; }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        AdsDataTypeFlags Flags { get; }

        bool IsJaggedArray { get; }
    }
}

