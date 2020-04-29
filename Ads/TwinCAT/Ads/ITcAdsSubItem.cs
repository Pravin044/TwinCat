namespace TwinCAT.Ads
{
    using System;
    using TwinCAT.TypeSystem;

    public interface ITcAdsSubItem : ITcAdsDataType, IDataType, IBitSize, IResolvableType
    {
        string SubItemName { get; }

        int Offset { get; }

        bool IsPersistent { get; }
    }
}

