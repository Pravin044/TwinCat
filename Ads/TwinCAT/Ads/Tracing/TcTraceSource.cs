namespace TwinCAT.Ads.Tracing
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;

    internal class TcTraceSource : TraceSource
    {
        private int _id;
        protected bool _dumpCaller;
        private const string sep = "==========================================================================================================";

        public TcTraceSource(string name, TraceSourceIds id, SourceLevels level) : this(name, (int) id, level, true, true)
        {
        }

        public TcTraceSource(string name, int id, SourceLevels level, bool dumpCaller, bool traceApplicationStart) : base(name, level)
        {
            this._dumpCaller = dumpCaller;
            this._id = id;
            if (traceApplicationStart)
            {
                this.TraceApplicationStart();
            }
        }

        public TcTraceSource(string name, TraceSourceIds id, SourceLevels level, bool dumpCaller, bool traceApplicationStart) : this(name, (int) id, level, dumpCaller, traceApplicationStart)
        {
        }

        [Conditional("TRACE")]
        public void TraceApplicationEnd()
        {
            if (base.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Information))
            {
                string message = $"'{base.Name}' (ThreadID: {Thread.CurrentThread.ManagedThreadId}, Time: {DateTime.Now.ToString()} has ended!";
                string str2 = "==========================================================================================================";
                this.TraceImpl((TraceEventType) TraceEventType.Information, -1, message);
                this.TraceImpl((TraceEventType) TraceEventType.Information, -1, str2);
            }
        }

        [Conditional("TRACE")]
        public void TraceApplicationStart()
        {
            if (base.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Information) && (Thread.CurrentThread != null))
            {
                string message = $"'{base.Name}' (ThreadID: {Thread.CurrentThread.ManagedThreadId}, Time: {DateTime.Now.ToString()} is started!";
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                if ((entryAssembly != null) && (executingAssembly != null))
                {
                    string str2 = $"Entry Assembly: {entryAssembly.ToString()}";
                    string str3 = $"Executing Assembly: {executingAssembly.ToString()}";
                    string str4 = "==========================================================================================================";
                    this.TraceImpl((TraceEventType) TraceEventType.Information, -1, str4);
                    this.TraceImpl((TraceEventType) TraceEventType.Information, -1, message);
                    this.TraceImpl((TraceEventType) TraceEventType.Information, -1, str2);
                    this.TraceImpl((TraceEventType) TraceEventType.Information, -1, str3);
                }
            }
        }

        [Conditional("TRACE")]
        public void TraceError(Exception ex)
        {
            if (base.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Error))
            {
                this.TraceImpl((TraceEventType) TraceEventType.Error, 2, "Exception: " + ex.Message);
                if (ex.InnerException != null)
                {
                    this.TraceImpl((TraceEventType) TraceEventType.Error, -1, "InnerException: " + ex.InnerException);
                }
            }
        }

        [Conditional("TRACE")]
        public void TraceError(string message)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Error, 2, message);
        }

        [Conditional("TRACE")]
        public void TraceError(string message, Exception ex)
        {
            if (base.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Error))
            {
                this.TraceImpl((TraceEventType) TraceEventType.Error, 2, message);
                this.TraceImpl((TraceEventType) TraceEventType.Error, 2, "Exception: " + ex.Message);
                if (ex.InnerException != null)
                {
                    this.TraceImpl((TraceEventType) TraceEventType.Error, 2, "InnerException: " + ex.InnerException.Message);
                }
            }
        }

        [Conditional("TRACE")]
        public void TraceError(string format, params object[] args)
        {
            int skipLevels = this._dumpCaller ? 2 : -1;
            this.TraceImpl((TraceEventType) TraceEventType.Error, skipLevels, format, args);
        }

        [Conditional("TRACE")]
        public void TraceErrorMethod(string format, params object[] args)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Error, 2, format, args);
        }

        [Conditional("TRACE")]
        public void TraceErrorMethod(Exception ex, string format, params object[] args)
        {
            if (base.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Error))
            {
                this.TraceImpl((TraceEventType) TraceEventType.Error, 2, format, args);
                if (ex != null)
                {
                    this.TraceImpl((TraceEventType) TraceEventType.Error, 2, "Exception: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        this.TraceImpl((TraceEventType) TraceEventType.Error, 2, "InnerException: " + ex.InnerException.Message);
                    }
                }
            }
        }

        private void TraceEventWithStack(TraceEventType type, int skipFrames, string message, params object[] args)
        {
            if (base.get_Switch().ShouldTrace(type))
            {
                MethodBase method = new StackFrame(skipFrames).GetMethod();
                string str = string.Format(message, args);
                string str2 = string.Empty;
                str2 = (method == null) ? $"[{"<Unknown>"}:{"<Unknown>"}()] {str}" : $"[{method.DeclaringType.Name}:{method.Name}()] {str}";
                base.TraceEvent(type, this._id, str2, args);
            }
        }

        internal void TraceImpl(TraceEventType type, int skipLevels, string message)
        {
            if ((skipLevels <= 0) || !this._dumpCaller)
            {
                base.TraceEvent(type, this._id, message);
            }
            else
            {
                this.TraceEventWithStack(type, ++skipLevels, message, new object[0]);
            }
        }

        internal void TraceImpl(TraceEventType type, int skipLevels, string format, params object[] args)
        {
            if ((skipLevels <= 0) || !this._dumpCaller)
            {
                base.TraceEvent(type, this._id, format, args);
            }
            else
            {
                this.TraceEventWithStack(type, ++skipLevels, format, args);
            }
        }

        [Conditional("TRACE")]
        public void TraceInformation(string message)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Information, 2, message);
        }

        [Conditional("TRACE")]
        public void TraceInformation(string format, params object[] args)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Information, 2, format, args);
        }

        [Conditional("TRACE")]
        public void TraceMethodInfo(TraceEventType type, string format, params object[] args)
        {
            this.TraceEventWithStack(type, 2, format, args);
        }

        [Conditional("TRACE")]
        public void TraceStart()
        {
            this.TraceImpl((TraceEventType) TraceEventType.Start, 2, string.Empty);
        }

        [Conditional("TRACE")]
        public void TraceStart(string format, params object[] args)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Start, 2, format, args);
        }

        [Conditional("TRACE")]
        public void TraceStop()
        {
            this.TraceImpl((TraceEventType) TraceEventType.Stop, 2, string.Empty);
        }

        [Conditional("TRACE")]
        public void TraceStop(string format, params object[] args)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Stop, 2, format, args);
        }

        [Conditional("TRACE")]
        public void TraceVerbose(string message)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Verbose, 2, message);
        }

        [Conditional("TRACE")]
        public void TraceVerbose(string format, params object[] args)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Verbose, 2, format, args);
        }

        [Conditional("TRACE")]
        public void TraceWarning(Exception ex)
        {
            if (base.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Warning))
            {
                this.TraceImpl((TraceEventType) TraceEventType.Warning, 2, ex.Message);
                if (ex.InnerException != null)
                {
                    this.TraceImpl((TraceEventType) TraceEventType.Warning, 2, "InnerException: " + ex.InnerException.Message);
                }
            }
        }

        [Conditional("TRACE")]
        public void TraceWarning(string message)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Warning, 2, message);
        }

        [Conditional("TRACE")]
        public void TraceWarning(string format, params object[] args)
        {
            this.TraceImpl((TraceEventType) TraceEventType.Warning, 2, format, args);
        }

        [Conditional("TRACE")]
        public void TraceWarning(string message, Exception ex)
        {
            if (base.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Warning))
            {
                this.TraceImpl((TraceEventType) TraceEventType.Warning, 2, message);
                this.TraceImpl((TraceEventType) TraceEventType.Warning, 2, "Exception: " + ex.Message);
                if (ex.InnerException != null)
                {
                    this.TraceImpl((TraceEventType) TraceEventType.Warning, 2, "InnerException: " + ex.InnerException.Message);
                }
            }
        }

        [Conditional("TRACE")]
        public void TraceWarning(Exception ex, string message, params object[] args)
        {
            if (base.get_Switch().ShouldTrace((TraceEventType) TraceEventType.Warning))
            {
                this.TraceImpl((TraceEventType) TraceEventType.Warning, 2, message);
                this.TraceImpl((TraceEventType) TraceEventType.Warning, 2, "Exception: " + ex.Message);
            }
        }

        public bool DumpCaller
        {
            get => 
                this._dumpCaller;
            set => 
                (this._dumpCaller = value);
        }
    }
}

