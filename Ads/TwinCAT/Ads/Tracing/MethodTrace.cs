namespace TwinCAT.Ads.Tracing
{
    using System;

    internal class MethodTrace : MethodTraceBase
    {
        internal MethodTrace() : base(Module.Trace)
        {
        }

        internal MethodTrace(string message, params object[] args) : base(Module.Trace, message, args)
        {
        }
    }
}

