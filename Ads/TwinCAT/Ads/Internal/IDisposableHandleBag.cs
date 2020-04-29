namespace TwinCAT.Ads.Internal
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IDisposableHandleBag : IDisposable
    {
        bool Contains(uint handle);
        uint GetHandle(string instancePath);
        bool TryGetHandle(string instancePath, out uint handle);
    }
}

