namespace TwinCAT
{
    using System;

    [Serializable]
    public class SessionNotConnectedException : SessionException
    {
        public SessionNotConnectedException(ISession session) : base(string.Format(ResMan.GetString("SessionNotConnected_Message1"), session.Id, session.AddressSpecifier), session)
        {
        }

        public SessionNotConnectedException(string message, ISession session) : base(string.Format(ResMan.GetString("SessionNotConnected_Message2"), session.Id, session.AddressSpecifier, message), session)
        {
        }
    }
}

