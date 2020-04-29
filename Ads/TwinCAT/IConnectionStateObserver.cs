namespace TwinCAT
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public interface IConnectionStateObserver : IConnectionStateProvider
    {
        DateTime LastAccess { get; }

        DateTime LastSucceededAccess { get; }

        int TotalCycles { get; }

        int TotalErrors { get; }

        int ErrorsSinceLastSucceeded { get; }

        TimeSpan Quality { get; }
    }
}

