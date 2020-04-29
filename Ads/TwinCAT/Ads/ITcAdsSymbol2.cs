namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    public interface ITcAdsSymbol2 : ITcAdsSymbol
    {
        bool IsPersistent { get; }

        bool IsBitType { get; }

        [Obsolete("Use ITcAdsSymbol4.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        bool IsReference { get; }

        [Obsolete("Use ITcAdsSymbol4.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        bool IsPointer { get; }

        bool IsTypeGuid { get; }

        bool IsReadOnly { get; }

        bool IsTcComInterfacePointer { get; }

        int ContextMask { get; }
    }
}

