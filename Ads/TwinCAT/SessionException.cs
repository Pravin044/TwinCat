namespace TwinCAT
{
    using System;
    using System.Runtime.Serialization;
    using TwinCAT.Ads;

    [Serializable]
    public class SessionException : AdsException
    {
        [NonSerialized]
        public readonly ISession Session;

        public SessionException(string message, ISession session) : this(message, session, null)
        {
        }

        public SessionException(string message, ISession session, Exception innerException) : base(message, innerException)
        {
            this.Session = session;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("Session", this.Session);
        }
    }
}

