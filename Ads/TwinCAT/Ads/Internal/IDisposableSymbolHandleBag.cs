namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IDisposableSymbolHandleBag : IDisposableHandleBag, IDisposable
    {
        ISymbol GetSymbol(uint handle);
        bool TryGetSymbol(uint handle, out ISymbol symbol);
    }
}

