namespace TwinCAT.Ads
{
    using System;

    [Serializable]
    public class AdsInitializeException : AdsException
    {
        public AdsInitializeException(Exception inner) : base($"ADS could not be initialized because the 'TcAdsDll.dll' is not found! Please check DLL search paths!
({inner.Message})", inner)
        {
        }

        public AdsInitializeException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

