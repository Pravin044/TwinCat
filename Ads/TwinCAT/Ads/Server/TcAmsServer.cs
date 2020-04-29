namespace TwinCAT.Ads.Server
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;
    using TwinCAT.Ads;

    internal class TcAmsServer
    {
        private ushort _port;
        private string _portName;
        private AmsAddress _address;
        private TcAmsServerMarshaller _serverMarshaller;
        [CompilerGenerated]
        private TcAmsServerMarshaller.TcAmsServerExEventDelegate _amsServerExEvent;

        private event TcAmsServerMarshaller.TcAmsServerExEventDelegate _amsServerExEvent
        {
            [CompilerGenerated] add
            {
                TcAmsServerMarshaller.TcAmsServerExEventDelegate objA = this._amsServerExEvent;
                while (true)
                {
                    TcAmsServerMarshaller.TcAmsServerExEventDelegate a = objA;
                    TcAmsServerMarshaller.TcAmsServerExEventDelegate delegate4 = (TcAmsServerMarshaller.TcAmsServerExEventDelegate) Delegate.Combine(a, value);
                    objA = Interlocked.CompareExchange<TcAmsServerMarshaller.TcAmsServerExEventDelegate>(ref this._amsServerExEvent, delegate4, a);
                    if (ReferenceEquals(objA, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                TcAmsServerMarshaller.TcAmsServerExEventDelegate objA = this._amsServerExEvent;
                while (true)
                {
                    TcAmsServerMarshaller.TcAmsServerExEventDelegate source = objA;
                    TcAmsServerMarshaller.TcAmsServerExEventDelegate delegate4 = (TcAmsServerMarshaller.TcAmsServerExEventDelegate) Delegate.Remove(source, value);
                    objA = Interlocked.CompareExchange<TcAmsServerMarshaller.TcAmsServerExEventDelegate>(ref this._amsServerExEvent, delegate4, source);
                    if (ReferenceEquals(objA, source))
                    {
                        return;
                    }
                }
            }
        }

        internal event TcAmsServerMarshaller.TcAmsServerExEventDelegate AmsServerException
        {
            add
            {
                this._amsServerExEvent += value;
            }
            remove
            {
                this._amsServerExEvent -= value;
            }
        }

        public event TcAmsServerReceiveEventHandler OnAmsReceive;

        [SecuritySafeCritical]
        private TcAmsServer()
        {
            this._serverMarshaller = new TcAmsServerMarshaller(this);
            UnsafeNativeMethods.AmsServerAPIStartup();
        }

        internal TcAmsServer(string portName) : this()
        {
            this._port = 0xffff;
            this._portName = portName;
        }

        internal TcAmsServer(ushort port, string portName) : this()
        {
            this._port = port;
            this._portName = portName;
        }

        internal void callReceiveHandler(TcAmsCommand amsCmd)
        {
            TcAmsServerReceiveEventHandler handler = this._amsServerReceiveDelegate;
            if (handler != null)
            {
                handler(amsCmd);
            }
        }

        internal void Connect()
        {
            ushort num = this._serverMarshaller.Connect();
            if (num == 0)
            {
                throw new TcAmsServerException("Connect to port " + this._port + "failed", TcAdsAmsServerErrorCode.ConnectPortFailed);
            }
            this._port = num;
            this._serverMarshaller.AmsServerException += new TcAmsServerMarshaller.TcAmsServerExEventDelegate(this.OnAmsServerException);
        }

        internal void Disconnect()
        {
            uint num = this._serverMarshaller.Disconnect();
            this._serverMarshaller.AmsServerException -= new TcAmsServerMarshaller.TcAmsServerExEventDelegate(this.OnAmsServerException);
            if (num != 0)
            {
                object[] objArray1 = new object[] { "Disconnect failed for port ", this._port, ", ADS error code : ", num };
                throw new TcAmsServerException(string.Concat(objArray1), TcAdsAmsServerErrorCode.DisconnectPortFailed);
            }
        }

        private void OnAmsServerException(TcAmsServerExEventArgs e)
        {
            if (e.Exception is TcAmsServerException)
            {
                e = new TcAmsServerExEventArgs(new TcAdsServerException(e.Exception.Message, (e.Exception as TcAmsServerException).ErrorCode));
            }
            if (this._amsServerExEvent != null)
            {
                this._amsServerExEvent(e);
            }
        }

        internal AdsErrorCode Send(TcAmsCommand amsCmd) => 
            this._serverMarshaller.Send(amsCmd);

        internal ushort Port
        {
            get => 
                this._port;
            set => 
                (this._port = value);
        }

        internal string PortName
        {
            get => 
                this._portName;
            set => 
                (this._portName = value);
        }

        internal int QueueSize =>
            this._serverMarshaller.ReceiveQueue.Count;

        internal AmsAddress Address
        {
            get => 
                this._address;
            set => 
                (this._address = value);
        }

        internal delegate void TcAmsServerReceiveEventHandler(TcAmsCommand amsCmd);
    }
}

