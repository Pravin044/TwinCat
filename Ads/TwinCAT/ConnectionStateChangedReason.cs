namespace TwinCAT
{
    using System;

    public enum ConnectionStateChangedReason
    {
        None,
        Established,
        Closed,
        Lost,
        Error,
        Resurrected
    }
}

