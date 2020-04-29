namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;

    public interface ITcAdsSymbol
    {
        long IndexGroup { get; }

        long IndexOffset { get; }

        int Size { get; }

        [Obsolete("Use ITcAdsSymbol5.DataTypeId instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
        AdsDatatypeId Datatype { get; }

        string Name { get; }

        [Obsolete("Use ITcAdsSymbol5.TypeName instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
        string Type { get; }

        string Comment { get; }
    }
}

