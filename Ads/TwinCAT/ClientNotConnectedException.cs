namespace TwinCAT
{
    using System;
    using TwinCAT.Ads;

    [Serializable]
    public class ClientNotConnectedException : AdsException
    {
        public ClientNotConnectedException() : base(ResMan.GetString("NotConnected_message"))
        {
        }
    }
}

