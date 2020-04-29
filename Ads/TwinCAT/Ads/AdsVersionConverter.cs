namespace TwinCAT.Ads
{
    using System;

    public static class AdsVersionConverter
    {
        public static AdsVersion Convert(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            if (version.Revision != -1)
            {
                throw new ArgumentOutOfRangeException("Revision number not supported by AdsVersion object!");
            }
            return new AdsVersion(version.Major, version.Minor, version.Build);
        }

        public static Version Convert(AdsVersion adsVersion) => 
            new Version(adsVersion.Version, adsVersion.Revision, adsVersion.Build);
    }
}

