namespace TwinCAT.Ads.Internal
{
    using System;

    internal static class DataTypeFlagConverter
    {
        internal static AdsSymbolFlags Convert(AdsDataTypeFlags dataTypeFlags)
        {
            AdsSymbolFlags none = AdsSymbolFlags.None;
            if ((dataTypeFlags & (AdsDataTypeFlags.None | AdsDataTypeFlags.ReferenceTo)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.ReferenceTo))
            {
                none |= AdsSymbolFlags.None | AdsSymbolFlags.ReferenceTo;
            }
            if ((dataTypeFlags & (AdsDataTypeFlags.None | AdsDataTypeFlags.TComInterfacePtr)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.TComInterfacePtr))
            {
                none |= AdsSymbolFlags.None | AdsSymbolFlags.TComInterfacePtr;
            }
            if ((dataTypeFlags & AdsDataTypeFlags.BitValues) == AdsDataTypeFlags.BitValues)
            {
                none |= AdsSymbolFlags.BitValue;
            }
            if ((dataTypeFlags & (AdsDataTypeFlags.None | AdsDataTypeFlags.TypeGuid)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.TypeGuid))
            {
                none |= AdsSymbolFlags.None | AdsSymbolFlags.TypeGuid;
            }
            if ((dataTypeFlags & (AdsDataTypeFlags.None | AdsDataTypeFlags.Persistent)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.Persistent))
            {
                none |= AdsSymbolFlags.None | AdsSymbolFlags.Persistent;
            }
            return none;
        }
    }
}

