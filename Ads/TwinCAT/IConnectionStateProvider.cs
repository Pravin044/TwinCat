namespace TwinCAT
{
    using System;
    using System.Runtime.CompilerServices;

    public interface IConnectionStateProvider
    {
        event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;

        TwinCAT.ConnectionState ConnectionState { get; }
    }
}

