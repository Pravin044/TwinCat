namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads.Internal;

    public interface ISymbolFlagProvider
    {
        AdsSymbolFlags Flags { get; }

        byte ContextMask { get; }

        bool IsReadOnly { get; }

        bool IsPersistent { get; }

        bool IsTcComInterfacePointer { get; }

        bool IsTypeGuid { get; }
    }
}

