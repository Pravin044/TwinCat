namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Tracing;

    internal sealed class TcLocalSystem : TcAdsDllWrapper
    {
        private static TcLocalSystem s_instance = null;
        private static readonly object s_instLock = new object();
        private const int _routerNotificationInterval = 50;
        private int _refCount;
        private int _port;
        private bool _routerPort;
        private bool _routerNotificationsRegistered;
        private AmsRouterState _routerState;
        private ITimer _routerNotificationTimer;
        private Dictionary<int, WeakReference> _syncPortDict = new Dictionary<int, WeakReference>();
        private TcAdsDllWrapper.AmsRouterNotificationDelegate _amsRouterNotificationDelegate;

        [SecuritySafeCritical]
        private TcLocalSystem(TransportProtocol protocol)
        {
            object[] args = new object[] { this.GetHashCode() };
            using (new MethodTrace("ID: {0:d})", args))
            {
                if (protocol == TransportProtocol.All)
                {
                    this._port = this.AdsPortOpen();
                }
                else
                {
                    if (protocol != TransportProtocol.Router)
                    {
                        throw new NotSupportedException("Enforced Tcp/IP Transport not available yet!");
                    }
                    this._port = this.AdsPortOpenEx();
                }
                this._routerPort = this._port >= 0x8000;
                if (this._port == 0)
                {
                    ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
                }
                this._refCount = 0;
                this._amsRouterNotificationDelegate = new TcAdsDllWrapper.AmsRouterNotificationDelegate(this.OnRouterNotification);
                base.address = new AmsAddress(base.GetLocalNetId(), AmsPort.SystemService);
                this.UpdateRouterState();
                AdsErrorCode code = this.AmsRegisterRouterNotification(this._amsRouterNotificationDelegate, false);
                this._routerNotificationsRegistered = code == AdsErrorCode.NoError;
            }
        }

        private void _routerNotificationTimer_Tick(object sender, EventArgs e)
        {
            if (!this._routerNotificationsRegistered)
            {
                AmsRouterState state = this._routerState;
                this.UpdateRouterState();
                if (state != this._routerState)
                {
                    this.OnRouterNotification(this._routerState);
                }
            }
        }

        public int AddRef()
        {
            int num = this._refCount + 1;
            this._refCount = num;
            return num;
        }

        public void CloseSyncPort(TcAdsSyncPort syncPort)
        {
            object obj2 = s_instLock;
            lock (obj2)
            {
                this._syncPortDict.Remove(syncPort.Id);
                object[] args = new object[] { syncPort.Id };
                Module.Trace.TraceVerbose("SyncPort {0} removed!", args);
                syncPort.Dispose();
                int num = this._refCount - 1;
                this._refCount = num;
                if (num == 0)
                {
                    base.Dispose();
                }
            }
        }

        public TcAdsSyncPort CreateSyncPort(int port, INotificationReceiver iNoteReceiver, bool clientCycle, bool synchronize) => 
            this.CreateSyncPort(new AmsAddress(base.address.NetId, port), iNoteReceiver, clientCycle, synchronize);

        public TcAdsSyncPort CreateSyncPort(AmsAddress addr, INotificationReceiver iNoteReceiver, bool clientCycle, bool synchronize)
        {
            object obj2 = s_instLock;
            lock (obj2)
            {
                TcAdsSyncPort target = !this._routerPort ? new TcAdsSyncPort(addr, this, iNoteReceiver, clientCycle, synchronize) : new TcAdsSyncPortRouter(addr, this, iNoteReceiver, clientCycle, synchronize);
                target.Connect();
                WeakReference reference = new WeakReference(target, false);
                this._syncPortDict.Add(target.Id, reference);
                object[] args = new object[] { target.Id };
                Module.Trace.TraceVerbose("SyncPort {0} added!", args);
                this.AddRef();
                if (!this._routerNotificationsRegistered && (this._routerNotificationTimer == null))
                {
                    this._routerNotificationTimer = new ThreadTimer();
                    this._routerNotificationTimer.Interval = 50;
                    this._routerNotificationTimer.Tick += new EventHandler(this._routerNotificationTimer_Tick);
                    this._routerNotificationTimer.Enabled = true;
                }
                return target;
            }
        }

        public TcAdsSyncPort CreateSyncPort(AmsNetId netId, int port, INotificationReceiver iNoteReceiver, bool clientCycle, bool synchronize) => 
            this.CreateSyncPort(new AmsAddress(netId, port), iNoteReceiver, clientCycle, synchronize);

        protected override void Dispose(bool disposing)
        {
            if (!base._disposed)
            {
                object[] args = new object[] { this.GetHashCode() };
                Module.Trace.TraceInformation("ID: {0::d}", args);
                if (disposing && (this._routerNotificationTimer != null))
                {
                    this._routerNotificationTimer.Dispose();
                }
                this.AmsUnRegisterRouterNotification(false);
                this.AdsPortClose(false);
                s_instance = null;
                base.Dispose(disposing);
            }
            base._disposed = true;
        }

        ~TcLocalSystem()
        {
            this.Dispose(false);
        }

        public static TcLocalSystem GetInstance(TransportProtocol protocol)
        {
            object obj2 = s_instLock;
            lock (obj2)
            {
                if (s_instance == null)
                {
                    s_instance = new TcLocalSystem(protocol);
                }
                s_instance.AddRef();
                return s_instance;
            }
        }

        public void OnRouterNotification(AmsRouterState state)
        {
            IList<WeakReference> list = null;
            object obj2 = s_instLock;
            lock (obj2)
            {
                this._routerState = state;
                list = new List<WeakReference>(this._syncPortDict.Values);
            }
            try
            {
                foreach (WeakReference reference in list)
                {
                    if (!reference.IsAlive)
                    {
                        continue;
                    }
                    TcAdsSyncPort target = (TcAdsSyncPort) reference.Target;
                    if (target != null)
                    {
                        if (!target.IsDisposed)
                        {
                            target.OnRouterNotification(this._routerState);
                            continue;
                        }
                        string message = $"SyncPort '{target.Id}' is already disposed!";
                        Module.Trace.TraceWarning(message);
                    }
                }
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError(exception);
            }
        }

        public int Release()
        {
            object obj2 = s_instLock;
            lock (obj2)
            {
                int num = this._refCount - 1;
                this._refCount = num;
                if (num == 0)
                {
                    base.Dispose();
                }
                return this._refCount;
            }
        }

        private void UpdateRouterState()
        {
            bool enabled = false;
            if (!base._disposed)
            {
                try
                {
                    if (this.AmsPortEnabled(false, out enabled) == AdsErrorCode.NoError)
                    {
                        if (enabled)
                        {
                            this._routerState = AmsRouterState.Start;
                        }
                        else
                        {
                            StateInfo info;
                            AdsErrorCode code = this.ReadState(false, out info);
                            this._routerState = AmsRouterState.Stop;
                            if (code == AdsErrorCode.PortNotConnected)
                            {
                                this._routerState = AmsRouterState.Removed;
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Module.Trace.TraceError(exception);
                }
            }
        }

        public AmsNetId NetId =>
            base.address?.NetId;

        public int Port =>
            this._port;

        public bool RouterNotificationsRegistered =>
            this._routerNotificationsRegistered;

        public AmsRouterState RouterState =>
            this._routerState;
    }
}

