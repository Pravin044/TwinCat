namespace TwinCAT
{
    using System;

    public interface ISession : IConnectionStateProvider
    {
        void Close();
        IConnection Connect();
        bool Disconnect();

        ISessionProvider Provider { get; }

        string AddressSpecifier { get; }

        int Id { get; }

        bool IsConnected { get; }

        IConnection Connection { get; }

        DateTime EstablishedAt { get; }
    }
}

