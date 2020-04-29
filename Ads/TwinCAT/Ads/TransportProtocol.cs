namespace TwinCAT.Ads
{
    using System;

    [Flags]
    public enum TransportProtocol
    {
        None,
        Router,
        TcpIp,
        All
    }
}

