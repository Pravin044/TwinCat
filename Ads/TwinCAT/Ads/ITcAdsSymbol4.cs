namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using TwinCAT.TypeSystem;

    public interface ITcAdsSymbol4 : ITcAdsSymbol3, ITcAdsSymbol2, ITcAdsSymbol
    {
        ReadOnlyTypeAttributeCollection Attributes { get; }

        [Obsolete("Use ITcAdsSymbol4.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        bool IsEnum { get; }

        [Obsolete("Use ITcAdsSymbol4.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        bool IsStruct { get; }

        bool HasRpcMethods { get; }

        ReadOnlyRpcMethodCollection RpcMethods { get; }

        DataTypeCategory Category { get; }

        int BitSize { get; }

        int ByteSize { get; }
    }
}

