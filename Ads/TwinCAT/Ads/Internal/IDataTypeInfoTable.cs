namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    internal interface IDataTypeInfoTable
    {
        AdsErrorCode TryLoadType(string name, bool lookup, out ITcAdsDataType type);

        ReadOnlyTcAdsDataTypeCollection DataTypes { get; }
    }
}

