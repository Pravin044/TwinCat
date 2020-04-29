namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    public interface IAdsSymbolLoader : ISymbolLoader, ISymbolProvider, ITypeBinderEvents
    {
        INotificationSettings DefaultNotificationSettings { get; set; }

        AmsAddress ImageBaseAddress { get; }
    }
}

