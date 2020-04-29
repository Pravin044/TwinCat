namespace TwinCAT.Ads
{
    using System;
    using TwinCAT;

    public interface IAdsSessionSettings : ISessionSettings
    {
        int Timeout { get; }

        TimeSpan ResurrectionTime { get; set; }

        SymbolLoaderSettings SymbolLoader { get; set; }
    }
}

