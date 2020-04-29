namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    public interface ITcAdsSymbol3 : ITcAdsSymbol2, ITcAdsSymbol
    {
        [Obsolete("Use ITcAdsSymbol4.Category Property instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        bool IsArray { get; }

        int ArrayDimensions { get; }

        AdsDatatypeArrayInfo[] ArrayInfos { get; }
    }
}

