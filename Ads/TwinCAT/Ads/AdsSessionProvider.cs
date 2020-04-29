namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using TwinCAT;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Export(typeof(ISessionProvider)), ExportMetadata("SessionProvider", "Ads")]
    public class AdsSessionProvider : SessionProvider<AdsSession, AmsAddress, SessionSettings>
    {
        public override ISession Create(object address, ISessionSettings settings)
        {
            AmsAddress address2 = null;
            SessionSettings settings2 = null;
            if (address is AmsAddress)
            {
                address2 = (AmsAddress) address;
            }
            else if (address is string)
            {
                address2 = AmsAddress.Parse((string) address);
            }
            if (settings != null)
            {
                settings2 = (SessionSettings) settings;
            }
            return this.Create(address2, settings2);
        }

        public override AdsSession Create(AmsAddress address, SessionSettings settings)
        {
            if (settings == null)
            {
                settings = SessionSettings.Default;
            }
            return new AdsSession(address, settings);
        }

        public override string Name =>
            "ADS";
    }
}

