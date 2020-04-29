namespace TwinCAT.Ads
{
    using System;

    [Serializable]
    public class AdsException : ApplicationException
    {
        public AdsException()
        {
        }

        public AdsException(string message) : base(message)
        {
        }

        public AdsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

