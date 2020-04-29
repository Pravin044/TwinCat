namespace TwinCAT.TypeSystem
{
    using System;
    using TwinCAT.Ads;

    [Serializable]
    public class MarshalException : AdsException
    {
        public MarshalException() : base(ResMan.GetString("MarshalException_Message"))
        {
        }

        public MarshalException(string message) : base(message)
        {
        }

        public MarshalException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

