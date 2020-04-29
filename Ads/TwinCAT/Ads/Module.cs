namespace TwinCAT.Ads
{
    using System;
    using System.IO;
    using System.Reflection;
    using TwinCAT.Ads.Tracing;

    internal static class Module
    {
        internal static TcTraceSource Trace = new TcTraceSource("Ads", TraceSourceIds.ADS, (SourceLevels) SourceLevels.Warning);
        internal static TcTraceSource TraceSession = new TcTraceSource("AdsSession", TraceSourceIds.AdsSession, (SourceLevels) SourceLevels.Warning);

        static Module()
        {
            Trace.TraceApplicationStart();
        }

        internal static string ApplicationPath =>
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        internal static string DllDirectoryPath =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}

