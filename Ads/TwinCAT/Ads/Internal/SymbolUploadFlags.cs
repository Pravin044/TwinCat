namespace TwinCAT.Ads.Internal
{
    using System;

    [Flags]
    public enum SymbolUploadFlags : uint
    {
        None = 0,
        Is64BitPlatform = 1,
        IncludesBaseTypes = 2
    }
}

