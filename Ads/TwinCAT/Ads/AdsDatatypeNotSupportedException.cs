namespace TwinCAT.Ads
{
    using System;

    [Serializable]
    public class AdsDatatypeNotSupportedException : AdsException
    {
        public AdsDatatypeNotSupportedException() : base(ResMan.GetString("AdsDataTypeNotSupported_Message"))
        {
        }

        public AdsDatatypeNotSupportedException(string message) : base(message)
        {
        }

        public AdsDatatypeNotSupportedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

