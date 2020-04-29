namespace TwinCAT
{
    using System;

    public class SessionConnectionStateChangedEventArgs : ConnectionStateChangedEventArgs
    {
        public readonly ISession Session;
        public readonly IConnection Connection;

        public SessionConnectionStateChangedEventArgs(ConnectionStateChangedReason reason, ConnectionState newState, ConnectionState oldState, ISession session, IConnection connection) : this(reason, newState, oldState, session, connection, null)
        {
        }

        public SessionConnectionStateChangedEventArgs(ConnectionStateChangedReason reason, ConnectionState newState, ConnectionState oldState, ISession session, IConnection connection, Exception e) : base(reason, newState, oldState, e)
        {
            this.Session = session;
            this.Connection = connection;
        }
    }
}

