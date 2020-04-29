namespace TwinCAT.Ads.Internal
{
    using System;

    [Flags]
    internal enum DataAreaPidFlags : uint
    {
        PidAddressing = 0x80000000,
        BitTypeFlag = 0x40000000,
        Mask_PidOffset = 0xffffff,
        Mask_PidAreaNo = 0x3f000000
    }
}

