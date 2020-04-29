namespace TwinCAT.Ads.Tracing
{
    using System;
    using System.Diagnostics;

    internal class MethodTraceBase : IDisposable
    {
        protected TcTraceSource traceSource;
        private string _message;
        private object[] _args;
        private int _stackLevel;
        private bool _disposed;

        protected MethodTraceBase(TcTraceSource source) : this(source, string.Empty, null)
        {
        }

        protected MethodTraceBase(TcTraceSource source, string message, params object[] args)
        {
            this._stackLevel = 3;
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            this.traceSource = source;
            if (this.traceSource.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Start))
            {
                this._stackLevel++;
                this._message = message;
                this._args = args;
                if ((this._message == null) || (this._message == string.Empty))
                {
                    this.traceSource.TraceImpl((TraceEventType) TraceEventType.Start, this._stackLevel, string.Empty);
                }
                else
                {
                    this.traceSource.TraceImpl((TraceEventType) TraceEventType.Start, this._stackLevel, this._message, this._args);
                }
            }
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
                this._disposed = true;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if ((this.traceSource != null) && this.traceSource.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Stop))
                {
                    if ((this._message == null) || (this._message == string.Empty))
                    {
                        this.traceSource.TraceImpl((TraceEventType) TraceEventType.Stop, this._stackLevel, string.Empty);
                    }
                    else
                    {
                        this.traceSource.TraceImpl((TraceEventType) TraceEventType.Stop, this._stackLevel, this._message, this._args);
                    }
                }
                this.traceSource = null;
                this._message = null;
                this._args = null;
            }
        }

        ~MethodTraceBase()
        {
            this.Dispose(false);
        }
    }
}

