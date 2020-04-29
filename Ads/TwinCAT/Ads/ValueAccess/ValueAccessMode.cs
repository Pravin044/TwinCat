namespace TwinCAT.Ads.ValueAccess
{
    using System;

    public enum ValueAccessMode
    {
        None = 0,
        IndexGroupOffset = 1,
        Symbolic = 2,
        IndexGroupOffsetPreferred = 3,
        Default = 3
    }
}

