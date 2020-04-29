namespace TwinCAT
{
    using System;

    public class ConnectionStateChangedEventArgs : EventArgs
    {
        public readonly ConnectionStateChangedReason Reason;
        public readonly System.Exception Exception;
        public readonly ConnectionState OldState;
        public readonly ConnectionState NewState;

        public ConnectionStateChangedEventArgs(ConnectionStateChangedReason reason, ConnectionState newState, ConnectionState oldState) : this(reason, newState, oldState, null)
        {
        }

        public ConnectionStateChangedEventArgs(ConnectionStateChangedReason reason, ConnectionState newState, ConnectionState oldState, System.Exception e)
        {
            this.Reason = reason;
            this.OldState = oldState;
            this.NewState = newState;
            this.Exception = e;
        }
    }
}

