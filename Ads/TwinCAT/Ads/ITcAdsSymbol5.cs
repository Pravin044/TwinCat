namespace TwinCAT.Ads
{
    using System;
    using System.Collections.Generic;

    public interface ITcAdsSymbol5 : ITcAdsSymbol4, ITcAdsSymbol3, ITcAdsSymbol2, ITcAdsSymbol
    {
        bool IsRecursive(IEnumerable<ITcAdsSymbol5> parents);

        ITcAdsDataType DataType { get; }

        AdsDatatypeId DataTypeId { get; }

        string TypeName { get; }

        bool IsStatic { get; }
    }
}

