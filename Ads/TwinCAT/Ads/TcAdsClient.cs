namespace TwinCAT.Ads
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using TwinCAT;
    using TwinCAT.Ads.Internal;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    [DebuggerDisplay("ID = { _id }, TargetAddress = {_targetAddress}, ClientAddress = { ClientAddress}, ConnectionState = {ConnectionState}, Transport = {Protocol}")]
    public class TcAdsClient : ITcAdsRpcInvoke, IDisposable, IAdsConnection, IConnection, IConnectionStateProvider, IAdsNotifications, IAdsSymbolicAccess, IAdsAnyAccess, IAdsHandleAccess, IAdsReadWriteAccess, IAdsStateControl, IAdsSymbolLoaderFactory
    {
        private bool _bSynchronize;
        private bool _bClientCycle;
        private TcAdsSyncPort _syncPort;
        private NotificationReceiver _notificationReceiver;
        private ISymbolInfoTable _symbolInfoTable;
        private AmsRouterNotificationEventHandler _amsRouterNotificationEventHandlerDelegate;
        private EventHandler _adsSymbolVersionChangedEventHandlerDelegate;
        private AdsStateChangedEventHandler _adsStateChangedEventHandlerDelegate;
        private static int s_id = 0;
        private int _id;
        private bool _disposed;
        [CompilerGenerated]
        private AdsNotificationEventHandler AdsNotification;
        [CompilerGenerated]
        private AdsNotificationExEventHandler AdsNotificationEx;
        [CompilerGenerated]
        private AdsNotificationErrorEventHandler AdsNotificationError;
        private CommunicationInterceptors _interceptors;
        private object _syncConnection;
        private bool _resurrecting;
        [CompilerGenerated]
        private EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged;
        private ITcAdsRaw _rawInterface;
        private static int DEFAULT_TIMEOUT = 0x1388;
        private int _timeout;
        private AmsAddress _targetAddress;
        private TransportProtocol _requestedProtocol;
        private AdsSession _session;
        private Encoding _symbolEncoding;

        public event AdsNotificationEventHandler AdsNotification
        {
            [CompilerGenerated] add
            {
                AdsNotificationEventHandler adsNotification = this.AdsNotification;
                while (true)
                {
                    AdsNotificationEventHandler a = adsNotification;
                    AdsNotificationEventHandler handler3 = (AdsNotificationEventHandler) Delegate.Combine(a, value);
                    adsNotification = Interlocked.CompareExchange<AdsNotificationEventHandler>(ref this.AdsNotification, handler3, a);
                    if (ReferenceEquals(adsNotification, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                AdsNotificationEventHandler adsNotification = this.AdsNotification;
                while (true)
                {
                    AdsNotificationEventHandler source = adsNotification;
                    AdsNotificationEventHandler handler3 = (AdsNotificationEventHandler) Delegate.Remove(source, value);
                    adsNotification = Interlocked.CompareExchange<AdsNotificationEventHandler>(ref this.AdsNotification, handler3, source);
                    if (ReferenceEquals(adsNotification, source))
                    {
                        return;
                    }
                }
            }
        }

        public event AdsNotificationErrorEventHandler AdsNotificationError
        {
            [CompilerGenerated] add
            {
                AdsNotificationErrorEventHandler adsNotificationError = this.AdsNotificationError;
                while (true)
                {
                    AdsNotificationErrorEventHandler a = adsNotificationError;
                    AdsNotificationErrorEventHandler handler3 = (AdsNotificationErrorEventHandler) Delegate.Combine(a, value);
                    adsNotificationError = Interlocked.CompareExchange<AdsNotificationErrorEventHandler>(ref this.AdsNotificationError, handler3, a);
                    if (ReferenceEquals(adsNotificationError, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                AdsNotificationErrorEventHandler adsNotificationError = this.AdsNotificationError;
                while (true)
                {
                    AdsNotificationErrorEventHandler source = adsNotificationError;
                    AdsNotificationErrorEventHandler handler3 = (AdsNotificationErrorEventHandler) Delegate.Remove(source, value);
                    adsNotificationError = Interlocked.CompareExchange<AdsNotificationErrorEventHandler>(ref this.AdsNotificationError, handler3, source);
                    if (ReferenceEquals(adsNotificationError, source))
                    {
                        return;
                    }
                }
            }
        }

        public event AdsNotificationExEventHandler AdsNotificationEx
        {
            [CompilerGenerated] add
            {
                AdsNotificationExEventHandler adsNotificationEx = this.AdsNotificationEx;
                while (true)
                {
                    AdsNotificationExEventHandler a = adsNotificationEx;
                    AdsNotificationExEventHandler handler3 = (AdsNotificationExEventHandler) Delegate.Combine(a, value);
                    adsNotificationEx = Interlocked.CompareExchange<AdsNotificationExEventHandler>(ref this.AdsNotificationEx, handler3, a);
                    if (ReferenceEquals(adsNotificationEx, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                AdsNotificationExEventHandler adsNotificationEx = this.AdsNotificationEx;
                while (true)
                {
                    AdsNotificationExEventHandler source = adsNotificationEx;
                    AdsNotificationExEventHandler handler3 = (AdsNotificationExEventHandler) Delegate.Remove(source, value);
                    adsNotificationEx = Interlocked.CompareExchange<AdsNotificationExEventHandler>(ref this.AdsNotificationEx, handler3, source);
                    if (ReferenceEquals(adsNotificationEx, source))
                    {
                        return;
                    }
                }
            }
        }

        public event AdsStateChangedEventHandler AdsStateChanged
        {
            add
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                if ((this._adsStateChangedEventHandlerDelegate == null) && (this._syncPort != null))
                {
                    this._syncPort.RegisterStateChangedNotification();
                }
                this._adsStateChangedEventHandlerDelegate = (AdsStateChangedEventHandler) Delegate.Combine(this._adsStateChangedEventHandlerDelegate, value);
            }
            remove
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._adsStateChangedEventHandlerDelegate = (AdsStateChangedEventHandler) Delegate.Remove(this._adsStateChangedEventHandlerDelegate, value);
                if ((this._adsStateChangedEventHandlerDelegate == null) && (this._syncPort != null))
                {
                    this._syncPort.UnregisterStateChangedNotification();
                }
            }
        }

        public event EventHandler AdsSymbolVersionChanged
        {
            add
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                if ((this._adsSymbolVersionChangedEventHandlerDelegate == null) && (this._syncPort != null))
                {
                    this._syncPort.RegisterSymbolVersionChangedNotification();
                }
                this._adsSymbolVersionChangedEventHandlerDelegate = (EventHandler) Delegate.Combine(this._adsSymbolVersionChangedEventHandlerDelegate, value);
            }
            remove
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._adsSymbolVersionChangedEventHandlerDelegate = (EventHandler) Delegate.Remove(this._adsSymbolVersionChangedEventHandlerDelegate, value);
                if ((this._adsSymbolVersionChangedEventHandlerDelegate == null) && (this._syncPort != null))
                {
                    this._syncPort.UnregisterSymbolVersionChangedNotification();
                }
            }
        }

        public event AmsRouterNotificationEventHandler AmsRouterNotification
        {
            add
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._amsRouterNotificationEventHandlerDelegate = (AmsRouterNotificationEventHandler) Delegate.Combine(this._amsRouterNotificationEventHandlerDelegate, value);
            }
            remove
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(this.Name);
                }
                this._amsRouterNotificationEventHandlerDelegate = (AmsRouterNotificationEventHandler) Delegate.Remove(this._amsRouterNotificationEventHandlerDelegate, value);
            }
        }

        public event EventHandler<ConnectionStateChangedEventArgs> ConnectionStateChanged
        {
            [CompilerGenerated] add
            {
                EventHandler<ConnectionStateChangedEventArgs> connectionStateChanged = this.ConnectionStateChanged;
                while (true)
                {
                    EventHandler<ConnectionStateChangedEventArgs> a = connectionStateChanged;
                    EventHandler<ConnectionStateChangedEventArgs> handler3 = (EventHandler<ConnectionStateChangedEventArgs>) Delegate.Combine(a, value);
                    connectionStateChanged = Interlocked.CompareExchange<EventHandler<ConnectionStateChangedEventArgs>>(ref this.ConnectionStateChanged, handler3, a);
                    if (ReferenceEquals(connectionStateChanged, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<ConnectionStateChangedEventArgs> connectionStateChanged = this.ConnectionStateChanged;
                while (true)
                {
                    EventHandler<ConnectionStateChangedEventArgs> source = connectionStateChanged;
                    EventHandler<ConnectionStateChangedEventArgs> handler3 = (EventHandler<ConnectionStateChangedEventArgs>) Delegate.Remove(source, value);
                    connectionStateChanged = Interlocked.CompareExchange<EventHandler<ConnectionStateChangedEventArgs>>(ref this.ConnectionStateChanged, handler3, source);
                    if (ReferenceEquals(connectionStateChanged, source))
                    {
                        return;
                    }
                }
            }
        }

        public TcAdsClient() : this(AdsClientSettings.Default)
        {
        }

        [Obsolete("Use TcAdsClient(AdsClientSettings) instead!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
        public TcAdsClient(bool noInterceptors)
        {
            this._id = ++s_id;
            this._syncConnection = new object();
            this._timeout = DEFAULT_TIMEOUT;
            AdsClientSettings settings = noInterceptors ? AdsClientSettings.FastReconnection : AdsClientSettings.Default;
            this._requestedProtocol = settings.Protocol;
            this._notificationReceiver = new NotificationReceiver(this);
            this._bSynchronize = settings.Synchronize;
            this._bClientCycle = settings.ClientCycle;
            this._interceptors = settings.Interceptors;
            this._timeout = settings.Timeout;
        }

        public TcAdsClient(AdsClientSettings settings)
        {
            this._id = ++s_id;
            this._syncConnection = new object();
            this._timeout = DEFAULT_TIMEOUT;
            this._requestedProtocol = settings.Protocol;
            this._notificationReceiver = new NotificationReceiver(this);
            this._bSynchronize = settings.Synchronize;
            this._bClientCycle = settings.ClientCycle;
            this._interceptors = settings.Interceptors;
            this._timeout = settings.Timeout;
        }

        internal TcAdsClient(AdsSession session) : this(AdsClientSettings.Default)
        {
            this._session = session;
        }

        public int AddDeviceNotification(string variableName, AdsStream dataStream, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            if ((transMode != AdsTransMode.CyclicInContext) && (transMode != AdsTransMode.OnChangeInContext))
            {
                cycleTime *= 0x2710;
            }
            return this._syncPort.AddDeviceNotification(variableName, dataStream, 0, (int) dataStream.Length, (int) transMode, cycleTime, maxDelay * 0x2710, userData);
        }

        public int AddDeviceNotification(string variableName, AdsStream dataStream, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if ((transMode == AdsTransMode.CyclicInContext) || (transMode == AdsTransMode.OnChangeInContext))
            {
                throw new ArgumentOutOfRangeException("transMode");
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this._syncPort.AddDeviceNotification(variableName, dataStream, 0, (int) dataStream.Length, (int) transMode, (int) cycleTime.Ticks, (int) maxDelay.Ticks, userData);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotification(int indexGroup, int indexOffset, AdsStream dataStream, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            if ((transMode != AdsTransMode.CyclicInContext) && (transMode != AdsTransMode.OnChangeInContext))
            {
                cycleTime *= 0x2710;
            }
            return this._syncPort.AddDeviceNotification((uint) indexGroup, (uint) indexOffset, dataStream, 0, (int) dataStream.Length, (int) transMode, cycleTime, maxDelay * 0x2710, userData);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotification(int indexGroup, int indexOffset, AdsStream dataStream, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if ((transMode == AdsTransMode.CyclicInContext) || (transMode == AdsTransMode.OnChangeInContext))
            {
                throw new ArgumentOutOfRangeException("transMode");
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this._syncPort.AddDeviceNotification((uint) indexGroup, (uint) indexOffset, dataStream, 0, (int) dataStream.Length, (int) transMode, (int) cycleTime.Ticks, (int) maxDelay.Ticks, userData);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotification(long indexGroup, long indexOffset, AdsStream dataStream, AdsTransMode transMode, int cycleTime, int maxDelay, object userData) => 
            this.AddDeviceNotification((uint) indexGroup, (uint) indexOffset, dataStream, transMode, cycleTime, maxDelay, userData);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotification(long indexGroup, long indexOffset, AdsStream dataStream, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if ((transMode == AdsTransMode.CyclicInContext) || (transMode == AdsTransMode.OnChangeInContext))
            {
                throw new ArgumentOutOfRangeException("transMode");
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this._syncPort.AddDeviceNotification((uint) indexGroup, (uint) indexOffset, dataStream, 0, (int) dataStream.Length, (int) transMode, (int) cycleTime.Ticks, (int) maxDelay.Ticks, userData);
        }

        public int AddDeviceNotification(uint indexGroup, uint indexOffset, AdsStream dataStream, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            if ((transMode != AdsTransMode.CyclicInContext) && (transMode != AdsTransMode.OnChangeInContext))
            {
                cycleTime *= 0x2710;
            }
            return this._syncPort.AddDeviceNotification(indexGroup, indexOffset, dataStream, 0, (int) dataStream.Length, (int) transMode, cycleTime, maxDelay * 0x2710, userData);
        }

        public int AddDeviceNotification(uint indexGroup, uint indexOffset, AdsStream dataStream, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if ((transMode == AdsTransMode.CyclicInContext) || (transMode == AdsTransMode.OnChangeInContext))
            {
                throw new ArgumentOutOfRangeException("transMode");
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this._syncPort.AddDeviceNotification(indexGroup, indexOffset, dataStream, 0, (int) dataStream.Length, (int) transMode, (int) cycleTime.Ticks, (int) maxDelay.Ticks, userData);
        }

        public int AddDeviceNotification(string variableName, AdsStream dataStream, int offset, int length, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            if ((transMode != AdsTransMode.CyclicInContext) && (transMode != AdsTransMode.OnChangeInContext))
            {
                cycleTime *= 0x2710;
            }
            return this._syncPort.AddDeviceNotification(variableName, dataStream, offset, length, (int) transMode, cycleTime, maxDelay * 0x2710, userData);
        }

        public int AddDeviceNotification(string variableName, AdsStream dataStream, int offset, int length, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if ((transMode == AdsTransMode.CyclicInContext) || (transMode == AdsTransMode.OnChangeInContext))
            {
                throw new ArgumentOutOfRangeException("transMode");
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this._syncPort.AddDeviceNotification(variableName, dataStream, offset, length, (int) transMode, (int) cycleTime.Ticks, (int) maxDelay.Ticks, userData);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotification(int indexGroup, int indexOffset, AdsStream dataStream, int offset, int length, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            if ((transMode != AdsTransMode.CyclicInContext) & (transMode != AdsTransMode.OnChangeInContext))
            {
                cycleTime *= 0x2710;
            }
            return this._syncPort.AddDeviceNotification((uint) indexGroup, (uint) indexOffset, dataStream, offset, length, (int) transMode, cycleTime, maxDelay * 0x2710, userData);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotification(int indexGroup, int indexOffset, AdsStream dataStream, int offset, int length, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if ((transMode == AdsTransMode.CyclicInContext) || (transMode == AdsTransMode.OnChangeInContext))
            {
                throw new ArgumentOutOfRangeException("transMode");
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this._syncPort.AddDeviceNotification((uint) indexGroup, (uint) indexOffset, dataStream, offset, length, (int) transMode, (int) cycleTime.Ticks, (int) maxDelay.Ticks, userData);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotification(long indexGroup, long indexOffset, AdsStream dataStream, int offset, int length, AdsTransMode transMode, int cycleTime, int maxDelay, object userData) => 
            this.AddDeviceNotification((uint) indexGroup, (uint) indexOffset, dataStream, offset, length, transMode, cycleTime, maxDelay, userData);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotification(long indexGroup, long indexOffset, AdsStream dataStream, int offset, int length, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if ((transMode == AdsTransMode.CyclicInContext) || (transMode == AdsTransMode.OnChangeInContext))
            {
                throw new ArgumentOutOfRangeException("transMode");
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this._syncPort.AddDeviceNotification((uint) indexGroup, (uint) indexOffset, dataStream, offset, length, (int) transMode, (int) cycleTime.Ticks, (int) maxDelay.Ticks, userData);
        }

        public int AddDeviceNotification(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, AdsTransMode transMode, int cycleTime, int maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            if ((transMode != AdsTransMode.CyclicInContext) && (transMode != AdsTransMode.OnChangeInContext))
            {
                cycleTime *= 0x2710;
            }
            return this._syncPort.AddDeviceNotification(indexGroup, indexOffset, dataStream, offset, length, (int) transMode, cycleTime, maxDelay * 0x2710, userData);
        }

        public int AddDeviceNotification(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if ((transMode == AdsTransMode.CyclicInContext) || (transMode == AdsTransMode.OnChangeInContext))
            {
                throw new ArgumentOutOfRangeException("transMode");
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this._syncPort.AddDeviceNotification(indexGroup, indexOffset, dataStream, offset, length, (int) transMode, (int) cycleTime.Ticks, (int) maxDelay.Ticks, userData);
        }

        public int AddDeviceNotificationEx(string variableName, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type);
            return this.AddDeviceNotification(variableName, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, null, userData));
        }

        public int AddDeviceNotificationEx(string variableName, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData, Type type)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type);
            return this.AddDeviceNotification(variableName, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, null, userData));
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotificationEx(long indexGroup, long indexOffset, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            return this.AddDeviceNotificationEx((uint) indexGroup, (uint) indexOffset, transMode, cycleTime, maxDelay, userData, type);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotificationEx(long indexGroup, long indexOffset, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData, Type type)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type);
            return this.AddDeviceNotification(indexGroup, indexOffset, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, null, userData));
        }

        public int AddDeviceNotificationEx(string variableName, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type, args);
            return this.AddDeviceNotification(variableName, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, args, userData));
        }

        public int AddDeviceNotificationEx(string variableName, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData, Type type, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type, args);
            return this.AddDeviceNotification(variableName, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, args, userData));
        }

        public int AddDeviceNotificationEx(uint indexGroup, uint indexOffset, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type);
            return this.AddDeviceNotification(indexGroup, indexOffset, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, null, userData));
        }

        public int AddDeviceNotificationEx(uint indexGroup, uint indexOffset, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData, Type type)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type);
            return this.AddDeviceNotification(indexGroup, indexOffset, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, null, userData));
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotificationEx(long indexGroup, long indexOffset, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            return this.AddDeviceNotificationEx((uint) indexGroup, (uint) indexOffset, transMode, cycleTime, maxDelay, userData, type, args);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int AddDeviceNotificationEx(long indexGroup, long indexOffset, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData, Type type, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type, args);
            return this.AddDeviceNotification(indexGroup, indexOffset, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, args, userData));
        }

        public int AddDeviceNotificationEx(uint indexGroup, uint indexOffset, AdsTransMode transMode, int cycleTime, int maxDelay, object userData, Type type, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type, args);
            return this.AddDeviceNotification(indexGroup, indexOffset, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, args, userData));
        }

        public int AddDeviceNotificationEx(uint indexGroup, uint indexOffset, AdsTransMode transMode, TimeSpan cycleTime, TimeSpan maxDelay, object userData, Type type, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type, args);
            return this.AddDeviceNotification(indexGroup, indexOffset, new AdsStream(length), 0, length, transMode, cycleTime, maxDelay, new AdsNotificationExUserData(type, args, userData));
        }

        private void AddEventNotifications()
        {
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            if ((this._adsSymbolVersionChangedEventHandlerDelegate != null) && (this._syncPort != null))
            {
                this._syncPort.RegisterSymbolVersionChangedNotification();
            }
            if ((this._adsStateChangedEventHandlerDelegate != null) && (this._syncPort != null))
            {
                this._syncPort.RegisterStateChangedNotification();
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Connect(int srvPort)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            TcLocalSystem instance = TcLocalSystem.GetInstance(this._requestedProtocol);
            try
            {
                this.Connect(instance.NetId, srvPort);
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError(exception);
                throw;
            }
            finally
            {
                instance.Release();
            }
        }

        public void Connect(AmsAddress address)
        {
            this.Connect(address.NetId, address.Port);
        }

        public void Connect(string netID, int srvPort)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if ((netID == null) || (netID.Length == 0))
            {
                this.Connect(srvPort);
            }
            else
            {
                this.Connect(new AmsNetId(netID), srvPort);
            }
        }

        public void Connect(byte[] netID, int srvPort)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.Connect(new AmsNetId(netID), srvPort);
        }

        public void Connect(AmsNetId netID, int srvPort)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            AdsException error = null;
            if (!this.tryConnect(new AmsAddress(netID, srvPort), false, out error))
            {
                throw error;
            }
        }

        public void Connect(AmsNetId netID, AmsPort srvPort)
        {
            this.Connect(netID, (int) srvPort);
        }

        [Obsolete("For new code use the SymbolLoaderFactory.Create method instead!", false)]
        public TcAdsSymbolInfoLoader CreateSymbolInfoLoader()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.PortNotConnected);
            }
            return new TcAdsSymbolInfoLoader(this, SymbolLoaderFactory.readSymbolUploadInfo(this));
        }

        [Obsolete("Use the CreateSymbolLoader(SymbolLoaderSettings) method instead!", true), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IAdsSymbolLoader CreateSymbolLoader()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            SymbolLoaderSettings settings = SymbolLoaderSettings.Default;
            settings.SymbolsLoadMode = SymbolsLoadMode.Flat;
            return this.CreateSymbolLoader(settings);
        }

        [Obsolete("Use the SymbolLoaderFactory instead!", true), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IAdsSymbolLoader CreateSymbolLoader(SymbolLoaderSettings settings)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            return this.CreateSymbolLoader(null, settings);
        }

        [Obsolete("Use the CreateSymbolLoader(SymbolLoaderSettings) method instead!", true), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IAdsSymbolLoader CreateSymbolLoader(SymbolsLoadMode mode)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            SymbolLoaderSettings settings = SymbolLoaderSettings.Default;
            settings.SymbolsLoadMode = mode;
            return this.CreateSymbolLoader(settings);
        }

        public IAdsSymbolLoader CreateSymbolLoader(ISession session, SymbolLoaderSettings settings)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.PortNotConnected);
            }
            SymbolUploadInfo symbolsInfo = SymbolLoaderFactory.readSymbolUploadInfo(this);
            return new AdsSymbolLoader(this, settings, SymbolLoaderFactory.createValueAccessor(this, settings), session, symbolsInfo);
        }

        public int CreateVariableHandle(string variableName)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            int handle = 0;
            this.RawInterface.TryCreateVariableHandle(variableName, true, out handle);
            return handle;
        }

        public void DeleteDeviceNotification(int notificationHandle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            this._syncPort.DelDeviceNotification(notificationHandle);
        }

        public void DeleteVariableHandle(int variableHandle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.RawInterface.TryDeleteVariableHandle(variableHandle, true);
        }

        public bool Disconnect()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            return this.Disconnect(true);
        }

        private bool Disconnect(bool informHandlers)
        {
            if (this._syncPort == null)
            {
                return false;
            }
            try
            {
                TcLocalSystem instance = TcLocalSystem.GetInstance(this._requestedProtocol);
                if (informHandlers)
                {
                    this.OnBeforeDisconnect();
                }
                object obj2 = this._syncConnection;
                lock (obj2)
                {
                    try
                    {
                        if (this._symbolInfoTable != null)
                        {
                            IDisposable disposable = this._symbolInfoTable as IDisposable;
                            if (disposable != null)
                            {
                                disposable.Dispose();
                            }
                        }
                        instance.CloseSyncPort(this._syncPort);
                    }
                    finally
                    {
                        instance.Release();
                        this._syncPort = null;
                        this._symbolInfoTable = null;
                    }
                }
                if (informHandlers)
                {
                    this.OnConnectionStateChanged(TwinCAT.ConnectionState.Disconnected, TwinCAT.ConnectionState.Connected);
                }
                this._rawInterface = null;
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError(exception);
                throw;
            }
            return true;
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this.Dispose(true);
                this._disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Disconnect();
                this._rawInterface = null;
            }
        }

        private void EnsureOffsetLengthZero(int offset, int length)
        {
            if ((length != 0) || (offset != 0))
            {
                throw new OffsetLengthOutOfRangeException();
            }
        }

        private void EnsureValidBufferSize(byte[] buffer, int offset, int length)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("dataStream");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (buffer.Length < (length + offset))
            {
                throw new DataStreamSizeException("buffer");
            }
        }

        private void EnsureValidStreamSize(AdsStream dataStream, int offset, int length)
        {
            if (dataStream == null)
            {
                throw new ArgumentNullException("dataStream");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            if (dataStream.Length < (length + offset))
            {
                throw new DataStreamSizeException("dataStream");
            }
        }

        ~TcAdsClient()
        {
            this.Dispose(false);
        }

        internal ISymbolInfoTable getSymbolTable()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._symbolInfoTable == null)
            {
                SymbolUploadInfo info = SymbolLoaderFactory.readSymbolUploadInfo(this);
                this._symbolEncoding = info.StringEncoding;
                this._symbolInfoTable = new SymbolInfoTable(this, this._symbolEncoding, info.TargetPointerSize);
            }
            return this._symbolInfoTable;
        }

        internal AdsErrorCode InjectError(AdsErrorCode error)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            return ((IAdsErrorInjector) this.RawInterface).InjectError(error, false);
        }

        public object InvokeRpcMethod(string symbolPath, int methodId, object[] parameters)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            TcAdsDllWrapper.ThrowAdsException(this.TryInvokeRpcMethod(symbolPath, methodId, parameters, out obj2));
            return obj2;
        }

        public object InvokeRpcMethod(string symbolPath, string methodName, object[] parameters)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            TcAdsDllWrapper.ThrowAdsException(this.TryInvokeRpcMethod(symbolPath, methodName, parameters, out obj2));
            return obj2;
        }

        public object InvokeRpcMethod(ITcAdsSymbol symbol, int methodId, object[] parameters)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            TcAdsDllWrapper.ThrowAdsException(this.TryInvokeRpcMethod(symbol, methodId, parameters, out obj2));
            return obj2;
        }

        public object InvokeRpcMethod(ITcAdsSymbol symbol, string methodName, object[] parameters)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            TcAdsDllWrapper.ThrowAdsException(this.TryInvokeRpcMethod(symbol, methodName, parameters, out obj2));
            return obj2;
        }

        private object InvokeRpcMethod(ITcAdsSymbol symbol, IRpcMethod rpcMethod, object[] parameters)
        {
            object obj3;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            ISymbolInfoTable table = this.getSymbolTable();
            try
            {
                obj3 = table.InvokeRpcMethod(symbol, rpcMethod, parameters);
            }
            catch (Exception exception)
            {
                Module.Trace.TraceError(exception);
                throw;
            }
            return obj3;
        }

        protected virtual void OnBeforeDisconnect()
        {
            ITcAdsConnectionHandler handler = this._rawInterface as ITcAdsConnectionHandler;
            if (handler != null)
            {
                handler.OnBeforeDisconnected();
            }
        }

        protected virtual void OnConnectionStateChanged(TwinCAT.ConnectionState newState, TwinCAT.ConnectionState oldState)
        {
            ITcAdsConnectionHandler handler = this._rawInterface as ITcAdsConnectionHandler;
            ConnectionStateChangedReason none = ConnectionStateChangedReason.None;
            if (newState == TwinCAT.ConnectionState.Disconnected)
            {
                none = ConnectionStateChangedReason.Closed;
                if (handler != null)
                {
                    handler.OnDisconnected();
                }
            }
            else if (newState == TwinCAT.ConnectionState.Connected)
            {
                none = ConnectionStateChangedReason.Established;
                if (handler != null)
                {
                    handler.OnConnected();
                }
            }
            if (this.ConnectionStateChanged != null)
            {
                this.ConnectionStateChanged(this, new ConnectionStateChangedEventArgs(none, newState, oldState, null));
            }
        }

        private void OnSetTimout(int value)
        {
            this._timeout = value;
            if (this._syncPort != null)
            {
                this._syncPort.Timeout = value;
            }
        }

        public int Read(int variableHandle, AdsStream dataStream) => 
            this.Read(variableHandle, dataStream, 0, (int) dataStream.Length);

        public int Read(int indexGroup, int indexOffset, AdsStream dataStream) => 
            this.Read(indexGroup, indexOffset, dataStream, 0, (int) dataStream.Length);

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int Read(long indexGroup, long indexOffset, AdsStream dataStream) => 
            this.Read((uint) indexGroup, (uint) indexOffset, dataStream, 0, (int) dataStream.Length);

        public int Read(uint indexGroup, uint indexOffset, AdsStream dataStream) => 
            this.Read(indexGroup, indexOffset, dataStream, 0, (int) dataStream.Length);

        public int Read(int variableHandle, AdsStream dataStream, int offset, int length)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.EnsureValidStreamSize(dataStream, offset, length);
            this.RawInterface.Read(variableHandle, offset + dataStream.Origin, length, dataStream.GetBuffer(), true, out num);
            return num;
        }

        public int Read(int indexGroup, int indexOffset, AdsStream dataStream, int offset, int length)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.EnsureValidStreamSize(dataStream, offset, length);
            this.RawInterface.Read((uint) indexGroup, (uint) indexOffset, offset + dataStream.Origin, length, dataStream.GetBuffer(), true, out num);
            return num;
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int Read(long indexGroup, long indexOffset, AdsStream dataStream, int offset, int length) => 
            this.Read((uint) indexGroup, (uint) indexOffset, dataStream, offset, length);

        public int Read(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.EnsureValidStreamSize(dataStream, offset, length);
            this.RawInterface.Read(indexGroup, indexOffset, offset + dataStream.Origin, length, dataStream.GetBuffer(), true, out num);
            return num;
        }

        public int Read(uint indexGroup, uint indexOffset, byte[] dataStream, int offset, int length)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.EnsureValidBufferSize(dataStream, offset, length);
            this.RawInterface.Read(indexGroup, indexOffset, offset, length, dataStream, true, out num);
            return num;
        }

        public object ReadAny(int variableHandle, Type type)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.ReadAny(variableHandle, type, true, out obj2);
            return obj2;
        }

        public object ReadAny(int variableHandle, Type type, int[] args)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.ReadAny(variableHandle, type, args, true, out obj2);
            return obj2;
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public object ReadAny(long indexGroup, long indexOffset, Type type)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.ReadAny((uint) indexGroup, (uint) indexOffset, type, true, out obj2);
            return obj2;
        }

        public object ReadAny(uint indexGroup, uint indexOffset, Type type)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.ReadAny(indexGroup, indexOffset, type, true, out obj2);
            return obj2;
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public object ReadAny(long indexGroup, long indexOffset, Type type, int[] args)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.ReadAny((uint) indexGroup, (uint) indexOffset, type, args, true, out obj2);
            return obj2;
        }

        public object ReadAny(uint indexGroup, uint indexOffset, Type type, int[] args)
        {
            object obj2;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.ReadAny(indexGroup, indexOffset, type, args, true, out obj2);
            return obj2;
        }

        public string ReadAnyString(int variableHandle, int len, Encoding encoding)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            return this.RawInterface.ReadString(variableHandle, len, encoding, true, out code);
        }

        public string ReadAnyString(uint indexGroup, uint indexOffset, int len, Encoding encoding)
        {
            AdsErrorCode code;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            return this.RawInterface.ReadString(indexGroup, indexOffset, len, encoding, true, out code);
        }

        public DeviceInfo ReadDeviceInfo()
        {
            DeviceInfo info;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            this._syncPort.ReadDeviceInfo(true, out info);
            return info;
        }

        public StateInfo ReadState()
        {
            StateInfo info;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.ReadState(true, out info);
            return info;
        }

        public object ReadSymbol(ITcAdsSymbol symbol)
        {
            object obj2;
            Type type;
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            bool flag = (this.useSymbolPathAccess(symbol) & AccessMethods.IndexGroupIndexOffset) != AccessMethods.None;
            bool flag2 = (this.useSymbolPathAccess(symbol) & (AccessMethods.None | AccessMethods.ValueByHandle)) != AccessMethods.None;
            ITcAdsSymbol5 symbol2 = (ITcAdsSymbol5) symbol;
            PrimitiveTypeConverter unicode = PrimitiveTypeConverter.Default;
            if (symbol2.DataTypeId == AdsDatatypeId.ADST_WSTRING)
            {
                unicode = PrimitiveTypeConverter.Unicode;
            }
            if (!PrimitiveTypeConverter.TryGetManagedType(symbol2.DataType, out type))
            {
                throw new AdsDatatypeNotSupportedException($"DataType '{symbol2.TypeName}' symbol '{symbol2.Name}' cannot be read by ReadSymbol!");
            }
            if (!flag)
            {
                obj2 = this.ReadSymbol(symbol.Name, type, false);
            }
            else
            {
                AdsStream stream = new AdsStream(symbol2.ByteSize);
                using (new AdsBinaryReader(stream))
                {
                    int num = this.Read((uint) symbol.IndexGroup, (uint) symbol.IndexOffset, stream);
                    byte[] data = stream.GetBuffer();
                    if (symbol2.Category != DataTypeCategory.Array)
                    {
                        int num3 = unicode.UnmarshalPrimitive(type, data, 0, symbol2.ByteSize, out obj2);
                    }
                    else
                    {
                        bool flag3 = AnyArrayConverter.IsJagged(symbol2.DataType);
                        IList<IDimensionCollection> dimLengths = null;
                        AnyArrayConverter.TryGetJaggedDimensions(symbol2.DataType, out dimLengths);
                        int num2 = unicode.UnmarshalArray(new AnyTypeSpecifier(type, dimLengths), data, 0, symbol2.ByteSize, out obj2);
                    }
                }
            }
            return obj2;
        }

        public object ReadSymbol(string name, Type type, bool reloadSymbolInfo)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this.getSymbolTable().ReadSymbol(name, type, reloadSymbolInfo);
        }

        public ITcAdsSymbol ReadSymbolInfo(string name)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this.getSymbolTable().GetSymbol(name, false);
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int ReadWrite(int indexGroup, int indexOffset, AdsStream rdDataStream, AdsStream wrDataStream)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (wrDataStream == null)
            {
                wrDataStream = new AdsStream(0);
            }
            if (rdDataStream == null)
            {
                rdDataStream = new AdsStream(0);
            }
            this.RawInterface.ReadWrite((uint) indexGroup, (uint) indexOffset, rdDataStream.Origin, (int) rdDataStream.Length, rdDataStream.GetBuffer(), wrDataStream.Origin, (int) wrDataStream.Length, wrDataStream.GetBuffer(), true, out num);
            return num;
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int ReadWrite(long indexGroup, long indexOffset, AdsStream rdDataStream, AdsStream wrDataStream)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            return this.ReadWrite((uint) indexGroup, (uint) indexOffset, rdDataStream, wrDataStream);
        }

        public int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, AdsStream wrDataStream)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (wrDataStream == null)
            {
                wrDataStream = new AdsStream(0);
            }
            if (rdDataStream == null)
            {
                rdDataStream = new AdsStream(0);
            }
            this.RawInterface.ReadWrite(indexGroup, indexOffset, rdDataStream.Origin, (int) rdDataStream.Length, rdDataStream.GetBuffer(), wrDataStream.Origin, (int) wrDataStream.Length, wrDataStream.GetBuffer(), true, out num);
            return num;
        }

        public int ReadWrite(int variableHandle, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (wrDataStream == null)
            {
                this.EnsureOffsetLengthZero(wrOffset, wrLength);
                wrDataStream = new AdsStream(0);
            }
            if (rdDataStream == null)
            {
                this.EnsureOffsetLengthZero(rdOffset, rdLength);
                rdDataStream = new AdsStream(0);
            }
            this.EnsureValidStreamSize(wrDataStream, wrOffset, wrLength);
            this.EnsureValidStreamSize(rdDataStream, rdOffset, rdLength);
            this.RawInterface.ReadWrite(variableHandle, rdOffset + rdDataStream.Origin, rdLength, rdDataStream.GetBuffer(), wrOffset + wrDataStream.Origin, wrLength, wrDataStream.GetBuffer(), true, out num);
            return num;
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int ReadWrite(int indexGroup, int indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (wrDataStream == null)
            {
                this.EnsureOffsetLengthZero(wrOffset, wrLength);
                wrDataStream = new AdsStream(0);
            }
            if (rdDataStream == null)
            {
                this.EnsureOffsetLengthZero(rdOffset, rdLength);
                rdDataStream = new AdsStream(0);
            }
            return this.ReadWrite(indexGroup, indexOffset, rdDataStream.GetBuffer(), rdOffset + rdDataStream.Origin, rdLength, wrDataStream.GetBuffer(), wrOffset + wrDataStream.Origin, wrLength);
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int ReadWrite(int indexGroup, int indexOffset, byte[] readBuffer, int rdOffset, int rdLength, byte[] writeBuffer, int wrOffset, int wrLength)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (readBuffer == null)
            {
                this.EnsureOffsetLengthZero(wrOffset, wrLength);
                readBuffer = new byte[0];
            }
            if (writeBuffer == null)
            {
                this.EnsureOffsetLengthZero(rdOffset, rdLength);
                writeBuffer = new byte[0];
            }
            this.EnsureValidBufferSize(readBuffer, rdOffset, rdLength);
            this.EnsureValidBufferSize(writeBuffer, wrOffset, wrLength);
            this.RawInterface.ReadWrite((uint) indexGroup, (uint) indexOffset, rdOffset, rdLength, readBuffer, wrOffset, wrLength, writeBuffer, true, out num);
            return num;
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int ReadWrite(long indexGroup, long indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength) => 
            this.ReadWrite((uint) indexGroup, (uint) indexOffset, rdDataStream, rdOffset, rdLength, wrDataStream, wrOffset, wrLength);

        public int ReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (wrDataStream == null)
            {
                this.EnsureOffsetLengthZero(wrOffset, wrLength);
                wrDataStream = new AdsStream(0);
            }
            if (rdDataStream == null)
            {
                this.EnsureOffsetLengthZero(rdOffset, rdLength);
                rdDataStream = new AdsStream(0);
            }
            return this.ReadWrite(indexGroup, indexOffset, rdDataStream.GetBuffer(), rdOffset + rdDataStream.Origin, rdLength, wrDataStream.GetBuffer(), wrOffset + wrDataStream.Origin, wrLength);
        }

        public int ReadWrite(uint indexGroup, uint indexOffset, byte[] rdDataStream, int rdOffset, int rdLength, byte[] wrDataStream, int wrOffset, int wrLength)
        {
            int num;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (wrDataStream == null)
            {
                this.EnsureOffsetLengthZero(wrOffset, wrLength);
                wrDataStream = new byte[0];
            }
            if (rdDataStream == null)
            {
                this.EnsureOffsetLengthZero(rdOffset, rdLength);
                rdDataStream = new byte[0];
            }
            this.EnsureValidBufferSize(rdDataStream, rdOffset, rdLength);
            this.EnsureValidBufferSize(wrDataStream, wrOffset, wrLength);
            this.RawInterface.ReadWrite(indexGroup, indexOffset, rdOffset, rdLength, rdDataStream, wrOffset, wrLength, wrDataStream, true, out num);
            return num;
        }

        internal void SetCommunicationInterceptor(CommunicationInterceptors interceptors)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this._interceptors = interceptors;
        }

        public AdsErrorCode TryAddDeviceNotification(string variableName, AdsStream dataStream, int offset, int length, NotificationSettings settings, object userData, out uint handle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                handle = 0;
                return AdsErrorCode.ClientPortNotOpen;
            }
            int cycleTime = settings.CycleTime;
            if ((settings.NotificationMode != AdsTransMode.CyclicInContext) && (settings.NotificationMode != AdsTransMode.OnChangeInContext))
            {
                cycleTime *= 0x2710;
            }
            return this._syncPort.TryAddDeviceNotification(variableName, dataStream, offset, length, (int) settings.NotificationMode, cycleTime, settings.MaxDelay * 0x2710, userData, out handle);
        }

        public AdsErrorCode TryAddDeviceNotificationEx(string variableName, NotificationSettings settings, object userData, Type type, int[] args, out uint handle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            int length = TcAdsDllMarshaller.SizeOf(type, args);
            return this.TryAddDeviceNotification(variableName, new AdsStream(length), 0, length, settings, new AdsNotificationExUserData(type, args, userData), out handle);
        }

        private bool tryConnect(AmsAddress address, bool reconnection, out AdsException error)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            bool flag = false;
            error = null;
            TcLocalSystem instance = TcLocalSystem.GetInstance(this._requestedProtocol);
            try
            {
                object obj2 = this._syncConnection;
                lock (obj2)
                {
                    if (this._syncPort != null)
                    {
                        this.Disconnect(false);
                    }
                    this._syncPort = instance.CreateSyncPort(address.NetId, address.Port, this._notificationReceiver, this._bClientCycle, this._bSynchronize);
                    this._rawInterface = new AdsRawInterceptor(this._syncPort, this._interceptors);
                    this._syncPort.Timeout = this._timeout;
                    this._targetAddress = address;
                    this.AddEventNotifications();
                }
                this.OnConnectionStateChanged(TwinCAT.ConnectionState.Connected, TwinCAT.ConnectionState.Disconnected);
                flag = true;
            }
            catch (Exception exception)
            {
                flag = false;
                if (this._syncPort != null)
                {
                    instance.CloseSyncPort(this._syncPort);
                    this._syncPort = null;
                }
                if (!(exception is AdsException))
                {
                    throw;
                }
                error = (AdsException) exception;
            }
            finally
            {
                instance.Release();
            }
            return flag;
        }

        public AdsErrorCode TryDeleteDeviceNotification(uint notificationHandle)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            return ((this._syncPort != null) ? this._syncPort.TryDeleteDeviceNotification(notificationHandle) : AdsErrorCode.ClientPortNotOpen);
        }

        public AdsErrorCode TryInvokeRpcMethod(string symbolPath, int methodId, object[] parameters, out object retValue)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (string.IsNullOrEmpty(symbolPath))
            {
                throw new ArgumentOutOfRangeException("symbolPath");
            }
            if (methodId < 0)
            {
                throw new ArgumentOutOfRangeException("methodId");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (this._syncPort == null)
            {
                retValue = null;
                return AdsErrorCode.ClientPortNotOpen;
            }
            ITcAdsSymbol symbol = this.ReadSymbolInfo(symbolPath);
            ITcAdsSymbol5 symbol2 = symbol as ITcAdsSymbol5;
            if (symbol2 == null)
            {
                throw new RpcMethodNotSupportedException(methodId, symbol);
            }
            if (!symbol2.HasRpcMethods)
            {
                throw new RpcMethodNotSupportedException(methodId, symbol);
            }
            IRpcMethod method = null;
            if (!symbol2.RpcMethods.TryGetMethod(methodId, out method))
            {
                throw new RpcMethodNotSupportedException(methodId, symbol);
            }
            return this.TryInvokeRpcMethod(symbol2, method, parameters, out retValue);
        }

        public AdsErrorCode TryInvokeRpcMethod(string symbolPath, string methodName, object[] parameters, out object returnValue)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (string.IsNullOrEmpty(symbolPath))
            {
                throw new ArgumentOutOfRangeException("symbolPath");
            }
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentOutOfRangeException("methodName");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            ISymbolInfoTable table = this.getSymbolTable();
            ITcAdsSymbol symbol = this.ReadSymbolInfo(symbolPath);
            return this.TryInvokeRpcMethod(symbol, methodName, parameters, out returnValue);
        }

        public AdsErrorCode TryInvokeRpcMethod(ITcAdsSymbol symbol, int methodId, object[] parameters, out object retValue)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (methodId < 0)
            {
                throw new ArgumentOutOfRangeException("methodId");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            ITcAdsSymbol5 symbol2 = symbol as ITcAdsSymbol5;
            if (symbol2 == null)
            {
                throw new RpcMethodNotSupportedException(methodId, symbol);
            }
            if (!symbol2.HasRpcMethods)
            {
                throw new RpcMethodNotSupportedException(methodId, symbol);
            }
            IRpcMethod method = null;
            if (!symbol2.RpcMethods.TryGetMethod(methodId, out method))
            {
                throw new RpcMethodNotSupportedException(methodId, symbol);
            }
            return this.TryInvokeRpcMethod(symbol2, method, parameters, out retValue);
        }

        public AdsErrorCode TryInvokeRpcMethod(ITcAdsSymbol symbol, string methodName, object[] parameters, out object returnValue)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentOutOfRangeException("methodName");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            ITcAdsSymbol5 symbol2 = symbol as ITcAdsSymbol5;
            if (symbol2 == null)
            {
                throw new RpcMethodNotSupportedException(methodName, symbol);
            }
            if (!symbol2.HasRpcMethods)
            {
                throw new RpcMethodNotSupportedException(methodName, symbol);
            }
            IRpcMethod method = null;
            if (!symbol2.RpcMethods.TryGetMethod(methodName, out method))
            {
                throw new RpcMethodNotSupportedException(methodName, symbol);
            }
            return this.TryInvokeRpcMethod(symbol2, method, parameters, out returnValue);
        }

        private AdsErrorCode TryInvokeRpcMethod(ITcAdsSymbol symbol, IRpcMethod rpcMethod, object[] parameters, out object returnValue)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            try
            {
                returnValue = this.InvokeRpcMethod(symbol, rpcMethod, parameters);
                return AdsErrorCode.NoError;
            }
            catch (AdsErrorException exception)
            {
                Module.Trace.TraceError(exception);
                returnValue = null;
                return exception.ErrorCode;
            }
        }

        public AdsErrorCode TryRead(uint indexGroup, uint indexOffset, AdsStream dataStream, out int readBytes) => 
            this.TryRead(indexGroup, indexOffset, dataStream, 0, (int) dataStream.Length, out readBytes);

        public AdsErrorCode TryRead(int variableHandle, AdsStream dataStream, int offset, int length, out int readBytes)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            readBytes = 0;
            this.EnsureValidStreamSize(dataStream, offset, length);
            return this.RawInterface.Read(variableHandle, offset + dataStream.Origin, length, dataStream.GetBuffer(), false, out readBytes);
        }

        public AdsErrorCode TryRead(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length, out int readBytes)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.EnsureValidStreamSize(dataStream, offset, length);
            return this.RawInterface.Read(indexGroup, indexOffset, offset + dataStream.Origin, length, dataStream.GetBuffer(), false, out readBytes);
        }

        public AdsErrorCode TryRead(uint indexGroup, uint indexOffset, byte[] dataStream, int offset, int length, out int readBytes)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.EnsureValidBufferSize(dataStream, offset, length);
            return this.RawInterface.Read(indexGroup, indexOffset, offset, length, dataStream, false, out readBytes);
        }

        public AdsErrorCode TryReadState(out StateInfo stateInfo)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            return this.RawInterface.ReadState(false, out stateInfo);
        }

        public AdsErrorCode TryReadWrite(int variableHandle, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, out int readBytes)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            readBytes = 0;
            if (wrDataStream == null)
            {
                this.EnsureOffsetLengthZero(wrOffset, wrLength);
                wrDataStream = new AdsStream(0);
            }
            if (rdDataStream == null)
            {
                this.EnsureOffsetLengthZero(rdOffset, rdLength);
                rdDataStream = new AdsStream(0);
            }
            return this.TryReadWrite(variableHandle, rdDataStream.GetBuffer(), rdOffset + rdDataStream.Origin, rdLength, wrDataStream.GetBuffer(), wrOffset + wrDataStream.Origin, wrLength, out readBytes);
        }

        public AdsErrorCode TryReadWrite(int variableHandle, byte[] rdDataStream, int rdOffset, int rdLength, byte[] wrDataStream, int wrOffset, int wrLength, out int readBytes)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            readBytes = 0;
            if (wrDataStream == null)
            {
                this.EnsureOffsetLengthZero(wrOffset, wrLength);
                wrDataStream = new byte[0];
            }
            if (rdDataStream == null)
            {
                this.EnsureOffsetLengthZero(rdOffset, rdLength);
                rdDataStream = new byte[0];
            }
            this.EnsureValidBufferSize(rdDataStream, rdOffset, rdLength);
            this.EnsureValidBufferSize(wrDataStream, wrOffset, wrLength);
            return this.RawInterface.ReadWrite(variableHandle, rdOffset, rdLength, rdDataStream, wrOffset, wrLength, wrDataStream, false, out readBytes);
        }

        public AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, AdsStream rdDataStream, int rdOffset, int rdLength, AdsStream wrDataStream, int wrOffset, int wrLength, out int readBytes)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (rdDataStream == null)
            {
                this.EnsureOffsetLengthZero(rdOffset, rdLength);
                rdDataStream = new AdsStream(0);
            }
            if (wrDataStream == null)
            {
                this.EnsureOffsetLengthZero(wrOffset, rdLength);
                wrDataStream = new AdsStream(0);
            }
            this.EnsureValidStreamSize(rdDataStream, rdOffset, rdLength);
            this.EnsureValidStreamSize(wrDataStream, wrOffset, wrLength);
            return this.RawInterface.ReadWrite(indexGroup, indexOffset, rdDataStream.Origin + rdOffset, rdLength, rdDataStream.GetBuffer(), wrDataStream.Origin + wrOffset, wrLength, wrDataStream.GetBuffer(), false, out readBytes);
        }

        public AdsErrorCode TryReadWrite(uint indexGroup, uint indexOffset, byte[] rdDataStream, int rdOffset, int rdLength, byte[] wrDataStream, int wrOffset, int wrLength, out int readBytes)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (rdDataStream == null)
            {
                this.EnsureOffsetLengthZero(rdOffset, rdLength);
                rdDataStream = new byte[0];
            }
            if (wrDataStream == null)
            {
                this.EnsureOffsetLengthZero(wrOffset, rdLength);
                wrDataStream = new byte[0];
            }
            this.EnsureValidBufferSize(rdDataStream, rdOffset, rdLength);
            this.EnsureValidBufferSize(wrDataStream, wrOffset, wrLength);
            return this.RawInterface.ReadWrite(indexGroup, indexOffset, rdOffset, rdLength, rdDataStream, wrOffset, wrLength, wrDataStream, false, out readBytes);
        }

        internal bool TryResurrect(out AdsException error)
        {
            bool flag = false;
            bool flag2 = true;
            error = null;
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._targetAddress == null)
            {
                throw new AdsException("Target address not set!");
            }
            try
            {
                this._resurrecting = true;
                if (flag2 || (this._syncPort == null))
                {
                    flag = this.tryConnect(this._targetAddress, true, out error);
                }
            }
            finally
            {
                this._resurrecting = false;
            }
            return flag;
        }

        public AdsErrorCode TryWrite(int variableHandle, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (dataStream == null)
            {
                this.EnsureOffsetLengthZero(offset, length);
                dataStream = new AdsStream(0);
            }
            this.EnsureValidStreamSize(dataStream, offset, length);
            return this.RawInterface.Write(variableHandle, offset + dataStream.Origin, length, dataStream.GetBuffer(), false);
        }

        public AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (dataStream == null)
            {
                this.EnsureOffsetLengthZero(offset, length);
                dataStream = new AdsStream(0);
            }
            this.EnsureValidStreamSize(dataStream, offset, length);
            return this.RawInterface.Write(indexGroup, indexOffset, dataStream.Origin + offset, length, dataStream.GetBuffer(), false);
        }

        public AdsErrorCode TryWrite(uint indexGroup, uint indexOffset, byte[] writeBuffer, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (writeBuffer == null)
            {
                this.EnsureOffsetLengthZero(offset, length);
                writeBuffer = new byte[0];
            }
            this.EnsureValidBufferSize(writeBuffer, offset, length);
            return this.RawInterface.Write(indexGroup, indexOffset, offset, length, writeBuffer, false);
        }

        public AdsErrorCode TryWriteControl(StateInfo stateInfo)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            return this.RawInterface.WriteControl(stateInfo, new byte[1], 0, 0, false);
        }

        public AdsErrorCode TryWriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            if (dataStream.Length < (length + offset))
            {
                throw new ArgumentException();
            }
            return this.RawInterface.WriteControl(stateInfo, dataStream.GetBuffer(), offset, length, false);
        }

        bool IConnection.Connect()
        {
            if (this.IsConnected)
            {
                return false;
            }
            if (this._targetAddress == null)
            {
                throw new AdsException(ResMan.GetString("CannotReconnect_Message"));
            }
            AdsException error = null;
            if (!this.TryResurrect(out error))
            {
                throw error;
            }
            return true;
        }

        private AccessMethods useSymbolPathAccess(ITcAdsSymbol symbol)
        {
            AccessMethods methods = AccessMethods.AcquireHandleByName | AccessMethods.IndexGroupIndexOffset | AccessMethods.ReleaseHandle | AccessMethods.ValueByHandle | AccessMethods.ValueByName;
            if ((symbol.IndexGroup == 0xf016L) || (symbol.IndexGroup == 0xf01bL))
            {
                methods = AccessMethods.AcquireHandleByName | AccessMethods.ReleaseHandle | AccessMethods.ValueByHandle | AccessMethods.ValueByName;
            }
            if ((symbol.IndexGroup == 0xf014L) || (symbol.IndexGroup == 0xf01aL))
            {
                methods = AccessMethods.AcquireHandleByName | AccessMethods.ReleaseHandle | AccessMethods.ValueByHandle | AccessMethods.ValueByName;
            }
            if (symbol.IndexGroup == 0xf017L)
            {
                methods = AccessMethods.AcquireHandleByName | AccessMethods.ReleaseHandle | AccessMethods.ValueByHandle | AccessMethods.ValueByName;
            }
            if (symbol.IndexGroup == 0xf019L)
            {
                methods = AccessMethods.AcquireHandleByName | AccessMethods.ReleaseHandle | AccessMethods.ValueByHandle | AccessMethods.ValueByName;
            }
            return methods;
        }

        public void Write(int indexGroup, int indexOffset)
        {
            this.Write(indexGroup, indexOffset, new AdsStream(0));
        }

        public void Write(int variableHandle, AdsStream dataStream)
        {
            this.Write(variableHandle, dataStream, 0, (int) dataStream.Length);
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void Write(long indexGroup, long indexOffset)
        {
            this.Write(indexGroup, indexOffset, new AdsStream(0));
        }

        public void Write(uint indexGroup, uint indexOffset)
        {
            this.Write(indexGroup, indexOffset, new AdsStream(0));
        }

        public void Write(int indexGroup, int indexOffset, AdsStream dataStream)
        {
            if (dataStream == null)
            {
                dataStream = new AdsStream(0);
            }
            this.Write(indexGroup, indexOffset, dataStream, 0, (int) dataStream.Length);
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void Write(long indexGroup, long indexOffset, AdsStream dataStream)
        {
            if (dataStream == null)
            {
                dataStream = new AdsStream(0);
            }
            this.Write(indexGroup, indexOffset, dataStream, 0, (int) dataStream.Length);
        }

        public void Write(uint indexGroup, uint indexOffset, AdsStream dataStream)
        {
            if (dataStream == null)
            {
                dataStream = new AdsStream(0);
            }
            this.Write(indexGroup, indexOffset, dataStream, 0, (int) dataStream.Length);
        }

        public void Write(int variableHandle, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (dataStream == null)
            {
                this.EnsureOffsetLengthZero(offset, length);
                dataStream = new AdsStream(0);
            }
            this.EnsureValidStreamSize(dataStream, offset, length);
            this.RawInterface.Write(variableHandle, offset + dataStream.Origin, length, dataStream.GetBuffer(), true);
        }

        public void Write(int indexGroup, int indexOffset, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (dataStream == null)
            {
                this.EnsureOffsetLengthZero(offset, length);
                dataStream = new AdsStream(0);
            }
            this.Write(indexGroup, indexOffset, dataStream.GetBuffer(), offset + dataStream.Origin, length);
        }

        public void Write(int indexGroup, int indexOffset, byte[] writeBuffer, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (writeBuffer == null)
            {
                this.EnsureOffsetLengthZero(offset, length);
                writeBuffer = new byte[0];
            }
            this.EnsureValidBufferSize(writeBuffer, offset, length);
            this.RawInterface.Write((uint) indexGroup, (uint) indexOffset, offset, length, writeBuffer, true);
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void Write(long indexGroup, long indexOffset, AdsStream dataStream, int offset, int length)
        {
            this.Write((uint) indexGroup, (uint) indexOffset, dataStream, offset, length);
        }

        public void Write(uint indexGroup, uint indexOffset, AdsStream dataStream, int offset, int length)
        {
            if (dataStream == null)
            {
                this.EnsureOffsetLengthZero(offset, length);
                dataStream = new AdsStream(0);
            }
            this.Write(indexGroup, indexOffset, dataStream.GetBuffer(), offset + dataStream.Origin, length);
        }

        public void Write(uint indexGroup, uint indexOffset, byte[] dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (dataStream == null)
            {
                this.EnsureOffsetLengthZero(offset, length);
                dataStream = new byte[0];
            }
            this.EnsureValidBufferSize(dataStream, offset, length);
            this.RawInterface.Write(indexGroup, indexOffset, offset, length, dataStream, true);
        }

        public void WriteAny(int variableHandle, object value)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.WriteAny(variableHandle, value, true);
        }

        public void WriteAny(int variableHandle, object value, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.WriteAny(variableHandle, value, args, true);
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void WriteAny(long indexGroup, long indexOffset, object value)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.WriteAny((uint) indexGroup, (uint) indexOffset, value, true);
        }

        public void WriteAny(uint indexGroup, uint indexOffset, object value)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.WriteAny(indexGroup, indexOffset, value, true);
        }

        [Obsolete("Use the method overload with System.UInt32 as parameters for IndexGroup and IndexOffset!."), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void WriteAny(long indexGroup, long indexOffset, object value, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.WriteAny((uint) indexGroup, (uint) indexOffset, value, args, true);
        }

        public void WriteAny(uint indexGroup, uint indexOffset, object value, int[] args)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.WriteAny(indexGroup, indexOffset, value, args, true);
        }

        public void WriteAnyString(int variableHandle, string value, int length, Encoding encoding)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            this.RawInterface.WriteString(variableHandle, value, length, encoding, true);
        }

        public void WriteAnyString(uint indexGroup, uint indexOffset, string value, int length, Encoding encoding)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.WriteString(indexGroup, indexOffset, value, length, encoding, true);
        }

        public void WriteControl(StateInfo stateInfo)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            this.RawInterface.WriteControl(stateInfo, new byte[1], 0, 0, true);
        }

        public void WriteControl(StateInfo stateInfo, AdsStream dataStream, int offset, int length)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (!this.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            if (dataStream.Length < (length + offset))
            {
                throw new ArgumentException();
            }
            this.RawInterface.WriteControl(stateInfo, dataStream.GetBuffer(), offset, length, true);
        }

        public void WriteSymbol(ITcAdsSymbol symbol, object val)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            bool flag2 = (this.useSymbolPathAccess(symbol) & (AccessMethods.None | AccessMethods.ValueByName)) != AccessMethods.None;
            if ((this.useSymbolPathAccess(symbol) & (AccessMethods.None | AccessMethods.ValueByHandle)) != AccessMethods.None)
            {
                this.WriteSymbol(symbol.Name, val, false);
            }
            else
            {
                Type type2;
                ITcAdsSymbol5 symbol2 = (ITcAdsSymbol5) symbol;
                Type type = val.GetType();
                PrimitiveTypeConverter unicode = PrimitiveTypeConverter.Default;
                if (symbol2.DataTypeId == AdsDatatypeId.ADST_WSTRING)
                {
                    unicode = PrimitiveTypeConverter.Unicode;
                }
                if (!PrimitiveTypeConverter.TryGetManagedType(symbol2.DataType, out type2))
                {
                    throw new AdsDatatypeNotSupportedException($"DataType '{symbol2.TypeName}' of symbol '{symbol2.Name}' is not supported!");
                }
                object obj2 = val;
                if (type2 != val.GetType())
                {
                    obj2 = PrimitiveTypeConverter.Convert(val, type2);
                }
                byte[] buffer = unicode.Marshal(obj2);
                AdsStream stream = new AdsStream(symbol2.ByteSize);
                using (AdsBinaryWriter writer = new AdsBinaryWriter(stream))
                {
                    writer.Write(buffer);
                    this.Write(symbol.IndexGroup, symbol.IndexOffset, stream);
                }
            }
        }

        public void WriteSymbol(string name, object value, bool reloadSymbolInfo)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.Name);
            }
            if (this._syncPort == null)
            {
                TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientPortNotOpen);
            }
            this.getSymbolTable().WriteSymbol(name, value, reloadSymbolInfo);
        }

        public int Id =>
            this._id;

        internal string Name
        {
            get
            {
                string str = $"TcAdsClient_{this._id}";
                if (this._targetAddress != null)
                {
                    str = str + $" ({this._targetAddress})";
                }
                return str;
            }
        }

        public bool Disposed =>
            this._disposed;

        internal CommunicationInterceptors Interceptors =>
            this._interceptors;

        [Browsable(false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), CLSCompliant(false)]
        public ITcAdsRaw RawInterface
        {
            get
            {
                if (this._disposed)
                {
                    return null;
                }
                object obj2 = this._syncConnection;
                lock (obj2)
                {
                    return this._rawInterface;
                }
            }
        }

        public int Timeout
        {
            get => 
                this._timeout;
            set
            {
                if (!this.Disposed && (this._timeout != value))
                {
                    this.OnSetTimout(value);
                }
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool ClientCycle
        {
            get => 
                this._bClientCycle;
            set
            {
                if (this._syncPort == null)
                {
                    this._bClientCycle = value;
                }
                else
                {
                    this._syncPort.ClientCycle = value;
                    this._bClientCycle = this._syncPort.ClientCycle;
                }
            }
        }

        public bool Synchronize
        {
            get => 
                this._bSynchronize;
            set
            {
                if (this._syncPort == null)
                {
                    this._bSynchronize = value;
                }
                else
                {
                    this._syncPort.Synchronize = value;
                    this._bSynchronize = this._syncPort.Synchronize;
                }
            }
        }

        public bool IsLocal =>
            ((this._targetAddress != null) ? this._targetAddress.netId.IsLocal : true);

        [Obsolete("Use TcAdsClient.Address.Port instead", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int ServerPort =>
            this._targetAddress.Port;

        [Obsolete("Use TcAdsClient.Address.NetId instead", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public string ServerNetID =>
            this._targetAddress.NetId.ToString();

        public AmsAddress Address =>
            this._targetAddress;

        [Obsolete("Use TcAdsClient.Address instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public AmsAddress ServerAddress =>
            this.Address;

        [Obsolete("Use TcAdsClient.ClientAddress.Port instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public int ClientPort =>
            ((this._syncPort != null) ? this._syncPort.Port : 0);

        [Obsolete("Use TcAdsClient.ClientAddress.NetId instead!", false), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public string ClientNetID =>
            this._syncPort?.NetId.ToString();

        public AmsAddress ClientAddress =>
            ((this._syncPort != null) ? new AmsAddress(this._syncPort.NetId, this._syncPort.Port) : null);

        public TransportProtocol Protocol
        {
            get
            {
                if (this._disposed)
                {
                    return TransportProtocol.None;
                }
                AmsAddress clientAddress = this.ClientAddress;
                return ((clientAddress != null) ? ((clientAddress.Port != 1) ? ((clientAddress.Port < 0x8000) ? TransportProtocol.None : TransportProtocol.Router) : TransportProtocol.TcpIp) : TransportProtocol.None);
            }
        }

        public bool IsConnected =>
            (this._syncPort != null);

        public AmsRouterState RouterState
        {
            get
            {
                if (this._disposed)
                {
                    return AmsRouterState.Unknown;
                }
                TcLocalSystem instance = TcLocalSystem.GetInstance(this._requestedProtocol);
                AmsRouterState removed = AmsRouterState.Removed;
                try
                {
                    removed = instance.RouterState;
                }
                finally
                {
                    instance.Release();
                }
                return removed;
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public TwinCAT.ConnectionState ConnectionState =>
            (!this.IsConnected ? TwinCAT.ConnectionState.Disconnected : TwinCAT.ConnectionState.Connected);

        public ISession Session =>
            this._session;

        [Flags]
        internal enum AccessMethods : uint
        {
            IndexGroupIndexOffset = 1,
            ValueByHandle = 2,
            ValueByName = 4,
            AcquireHandleByName = 0x10,
            ReleaseHandle = 0x20,
            None = 0,
            Mask_All = 0x37,
            Mask_Symbolic = 0x36
        }

        [Serializable]
        private class DataStreamSizeException : ArgumentException
        {
            public DataStreamSizeException(string parameterName) : base("DataStream/buffer size error!", parameterName)
            {
            }
        }

        private class NotificationReceiver : INotificationReceiver
        {
            private TcAdsClient _client;
            private AmsRouterState _routerState = AmsRouterState.Unknown;

            public NotificationReceiver(TcAdsClient client)
            {
                this._client = client;
            }

            public void OnAdsStateChanged(AdsStateChangedEventArgs eventArgs)
            {
                if (this._client._adsStateChangedEventHandlerDelegate != null)
                {
                    this._client._adsStateChangedEventHandlerDelegate(this._client, eventArgs);
                }
            }

            public unsafe void OnNotification(int handle, long timeStamp, int length, NotificationEntry entry)
            {
                if (((this._client.AdsNotificationEx == null) || (entry.userData == null)) || (entry.userData.GetType() != typeof(AdsNotificationExUserData)))
                {
                    if (this._client.AdsNotification != null)
                    {
                        this._client.AdsNotification(this._client, new AdsNotificationEventArgs(timeStamp, entry.userData, handle, length, entry.offset, entry.data));
                    }
                }
                else
                {
                    byte[] buffer;
                    AdsNotificationExUserData userData = (AdsNotificationExUserData) entry.userData;
                    if (((buffer = entry.data.GetBuffer()) == null) || (buffer.Length == 0))
                    {
                        numRef = null;
                    }
                    else
                    {
                        numRef = buffer;
                    }
                    object obj2 = TcAdsDllMarshaller.PtrToObject((void*) ((numRef + entry.offset) + entry.data.Origin), entry.length, userData.type, userData.args);
                    fixed (byte* numRef = null)
                    {
                        this._client.AdsNotificationEx(this._client, new AdsNotificationExEventArgs(timeStamp, userData.userData, handle, obj2));
                        return;
                    }
                }
            }

            public void OnNotificationError(Exception e)
            {
                Module.Trace.TraceError(e);
                if (this._client.AdsNotificationError != null)
                {
                    this._client.AdsNotificationError(this._client, new AdsNotificationErrorEventArgs(e));
                }
            }

            public void OnNotificationError(int handle, long timeStamp)
            {
                object[] args = new object[] { handle };
                Module.Trace.TraceError("Notification error Handle: {0}", args);
                if (this._client.AdsNotificationError != null)
                {
                    AdsInvalidNotificationException e = null;
                    e = new AdsInvalidNotificationException(handle, timeStamp);
                    this._client.AdsNotificationError(this._client, new AdsNotificationErrorEventArgs(e));
                }
            }

            public void OnRouterNotification(AmsRouterState state)
            {
                if (this._client._amsRouterNotificationEventHandlerDelegate != null)
                {
                    try
                    {
                        this._client._amsRouterNotificationEventHandlerDelegate(this._client, new AmsRouterNotificationEventArgs(state));
                    }
                    catch (Exception exception)
                    {
                        Module.Trace.TraceWarning(exception);
                    }
                }
                AmsRouterState state2 = this._routerState;
                this._routerState = state;
                object[] args = new object[] { state };
                Module.TraceSession.TraceInformation("RouterState changed to '{0}'", args);
                switch (state)
                {
                    case AmsRouterState.Stop:
                    case AmsRouterState.Removed:
                        break;

                    case AmsRouterState.Start:
                        if (state2 == AmsRouterState.Stop)
                        {
                            AdsException error = null;
                            if (!this._client.TryResurrect(out error))
                            {
                                Module.TraceSession.TraceWarning(error);
                            }
                        }
                        break;

                    default:
                        return;
                }
            }

            public void OnSymbolVersionChanged(AdsSymbolVersionChangedEventArgs eventArgs)
            {
                if (this._client._adsSymbolVersionChangedEventHandlerDelegate != null)
                {
                    this._client._adsSymbolVersionChangedEventHandlerDelegate(this._client, eventArgs);
                }
            }
        }

        [Serializable]
        private class OffsetLengthOutOfRangeException : ArgumentOutOfRangeException
        {
            public OffsetLengthOutOfRangeException() : base("Offset and length must be 0 in case of no given AdsStream!")
            {
            }
        }
    }
}

