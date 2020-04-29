namespace TwinCAT.Ads.Tracing
{
    using System;

    internal class SessionMethodTrace : MethodTraceBase
    {
        internal SessionMethodTrace() : base(Module.TraceSession)
        {
        }

        internal SessionMethodTrace(string message, params object[] args) : base(Module.TraceSession, message, args)
        {
        }
    }
}

