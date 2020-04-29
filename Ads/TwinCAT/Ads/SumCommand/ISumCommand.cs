namespace TwinCAT.Ads.SumCommand
{
    using System;
    using TwinCAT.Ads;

    public interface ISumCommand
    {
        AdsErrorCode Result { get; }

        AdsErrorCode[] SubResults { get; }

        bool Executed { get; }

        bool Succeeded { get; }

        bool Failed { get; }
    }
}

