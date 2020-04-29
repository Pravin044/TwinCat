namespace TwinCAT.Ads
{
    using System;
    using TwinCAT;

    public interface IAdsSession : ISession, IConnectionStateProvider, ISymbolServerProvider
    {
        AmsNetId NetId { get; }

        int Port { get; }

        object Owner { get; }

        AmsAddress Address { get; }
    }
}

