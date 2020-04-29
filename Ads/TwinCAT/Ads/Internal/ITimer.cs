namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.CompilerServices;

    internal interface ITimer : IDisposable
    {
        event EventHandler Tick;

        int Interval { get; set; }

        bool Enabled { get; set; }
    }
}

