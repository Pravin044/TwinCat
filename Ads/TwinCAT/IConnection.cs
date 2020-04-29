namespace TwinCAT
{
    using System;

    public interface IConnection : IConnectionStateProvider
    {
        void Close();
        bool Connect();
        bool Disconnect();

        int Id { get; }

        bool IsConnected { get; }

        int Timeout { get; set; }

        ISession Session { get; }
    }
}

