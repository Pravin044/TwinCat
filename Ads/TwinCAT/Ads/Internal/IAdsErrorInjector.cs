namespace TwinCAT.Ads.Internal
{
    using System;
    using TwinCAT.Ads;

    internal interface IAdsErrorInjector
    {
        AdsErrorCode InjectError(AdsErrorCode error, bool throwAdsException);
    }
}

