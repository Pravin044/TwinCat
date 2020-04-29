namespace TwinCAT.Ads.Server
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using TwinCAT.Ads;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class TcAmsServerMarshaller
    {
        private TcAmsServer _server;
        private Thread _receiveThread;
        private Queue<TcAmsCommand> _receiveQueue;
        private AutoResetEvent _queueEvent;
        private bool _bRunThread;
        private Thread _receiveThreadNotify;
        private Queue<TcAmsCommand> _receiveQueueNotify;
        private AutoResetEvent _queueEventNotify;
        private bool _bRunThreadNotify;
        [CompilerGenerated]
        private TcAmsServerExEventDelegate _amsServerExEvent;
        private UnsafeNativeMethods.AmsServerReceiveDelegate _receiveDelegate;
        private const int ReceiveQueueMaximum = 500;
        private byte[] hdrBuffer;
        private MemoryStream headerStream;
        private BinaryReader headerReader;

        private event TcAmsServerExEventDelegate _amsServerExEvent
        {
            [CompilerGenerated] add
            {
                TcAmsServerExEventDelegate objA = this._amsServerExEvent;
                while (true)
                {
                    TcAmsServerExEventDelegate a = objA;
                    TcAmsServerExEventDelegate delegate4 = (TcAmsServerExEventDelegate) Delegate.Combine(a, value);
                    objA = Interlocked.CompareExchange<TcAmsServerExEventDelegate>(ref this._amsServerExEvent, delegate4, a);
                    if (ReferenceEquals(objA, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                TcAmsServerExEventDelegate objA = this._amsServerExEvent;
                while (true)
                {
                    TcAmsServerExEventDelegate source = objA;
                    TcAmsServerExEventDelegate delegate4 = (TcAmsServerExEventDelegate) Delegate.Remove(source, value);
                    objA = Interlocked.CompareExchange<TcAmsServerExEventDelegate>(ref this._amsServerExEvent, delegate4, source);
                    if (ReferenceEquals(objA, source))
                    {
                        return;
                    }
                }
            }
        }

        internal event TcAmsServerExEventDelegate AmsServerException
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

        internal TcAmsServerMarshaller(TcAmsServer server)
        {
            this._server = server;
            this._receiveQueue = new Queue<TcAmsCommand>();
            this._receiveQueueNotify = new Queue<TcAmsCommand>();
            this._receiveDelegate = new UnsafeNativeMethods.AmsServerReceiveDelegate(this.OnAmsReceive);
            this.hdrBuffer = new byte[TcAmsHeaderSize.TCAMSHEADER];
            this.headerStream = new MemoryStream(this.hdrBuffer);
            this.headerReader = new BinaryReader(this.headerStream);
        }

        [SecuritySafeCritical]
        internal ushort Connect()
        {
            ushort num = UnsafeNativeMethods.AmsConnect(this._server.Port, this._server.PortName, this._receiveDelegate);
            if (num != 0)
            {
                this._server.Port = num;
                this.SetServerAddress();
                this._queueEvent = new AutoResetEvent(false);
                this._bRunThread = true;
                this._receiveThread = new Thread(new ThreadStart(this.Receive));
                this._receiveThread.IsBackground = true;
                this._receiveThread.Start();
                this._queueEventNotify = new AutoResetEvent(false);
                this._bRunThreadNotify = true;
                this._receiveThreadNotify = new Thread(new ThreadStart(this.ReceiveNotify));
                this._receiveThreadNotify.IsBackground = true;
                this._receiveThreadNotify.Start();
            }
            return num;
        }

        [SecuritySafeCritical]
        internal uint Disconnect()
        {
            uint num = UnsafeNativeMethods.AmsDisconnect(this._server.Port);
            if (num == 0)
            {
                this._bRunThread = false;
                this._queueEvent.Set();
                try
                {
                    if (this._bRunThread)
                    {
                        this._receiveThread.Join();
                    }
                }
                catch
                {
                    num = 1;
                }
                this._bRunThreadNotify = false;
                this._queueEventNotify.Set();
                try
                {
                    if (this._bRunThreadNotify)
                    {
                        this._receiveThreadNotify.Join();
                    }
                }
                catch
                {
                    num = 1;
                }
            }
            return num;
        }

        internal void OnAmsReceive(IntPtr pAmsCmd)
        {
            if (pAmsCmd != IntPtr.Zero)
            {
                Marshal.Copy(pAmsCmd, this.hdrBuffer, 0, TcAmsHeaderSize.TCAMSHEADER);
                TcAmsHeader amsHeader = new TcAmsHeader(this.hdrBuffer);
                IntPtr source = new IntPtr(pAmsCmd.ToInt64() + TcAmsHeaderSize.TCAMSHEADER);
                byte[] destination = new byte[amsHeader.CbData];
                Marshal.Copy(source, destination, 0, (int) amsHeader.CbData);
                TcAmsCommand item = new TcAmsCommand(amsHeader, destination);
                if (item.AmsHeader.CommandId == 8)
                {
                    object syncRoot = this._receiveQueueNotify.SyncRoot;
                    lock (syncRoot)
                    {
                        this._receiveQueueNotify.Enqueue(item);
                    }
                    this._queueEventNotify.Set();
                }
                else
                {
                    object syncRoot = this._receiveQueue.SyncRoot;
                    lock (syncRoot)
                    {
                        this._receiveQueue.Enqueue(item);
                    }
                    this._queueEvent.Set();
                }
            }
        }

        private void Receive()
        {
            try
            {
                while (this._bRunThread)
                {
                    this._queueEvent.WaitOne();
                    while ((this._receiveQueue.Count > 0) && this._bRunThread)
                    {
                        TcAmsCommand command;
                        object syncRoot = this._receiveQueue.SyncRoot;
                        lock (syncRoot)
                        {
                            if (this._receiveQueue.Count > 500)
                            {
                                throw new TcAmsServerException("To much data in receiveque!", TcAdsAmsServerErrorCode.ReceiveQueueOverflow);
                            }
                            command = this._receiveQueue.Dequeue();
                        }
                        this._server.callReceiveHandler(command);
                    }
                }
            }
            catch (Exception exception)
            {
                this._bRunThread = false;
                if (this._amsServerExEvent != null)
                {
                    this._amsServerExEvent(new TcAmsServerExEventArgs(exception));
                }
            }
        }

        private void ReceiveNotify()
        {
            try
            {
                while (this._bRunThreadNotify)
                {
                    this._queueEventNotify.WaitOne();
                    while ((this._receiveQueueNotify.Count > 0) && this._bRunThreadNotify)
                    {
                        TcAmsCommand command;
                        object syncRoot = this._receiveQueueNotify.SyncRoot;
                        lock (syncRoot)
                        {
                            if (this._receiveQueueNotify.Count > 500)
                            {
                                throw new TcAmsServerException("To much data in notify-receiveque!", TcAdsAmsServerErrorCode.ReceiveNotifcationQueueOverflow);
                            }
                            command = this._receiveQueueNotify.Dequeue();
                        }
                        this._server.callReceiveHandler(command);
                    }
                }
            }
            catch (Exception exception)
            {
                this._bRunThreadNotify = false;
                if (this._amsServerExEvent != null)
                {
                    this._amsServerExEvent(new TcAmsServerExEventArgs(exception));
                }
            }
        }

        [SecuritySafeCritical]
        internal unsafe AdsErrorCode Send(TcAmsCommand amsCmd)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            TcAmsHeader amsHeader = amsCmd.AmsHeader;
            IntPtr ptr = Marshal.AllocHGlobal((int) (Marshal.SizeOf(typeof(TcMarshallableAmsHeader)) + ((int) amsHeader.CbData)));
            Marshal.StructureToPtr(amsCmd.AmsHeader.GetMarshallableHeader(), ptr, false);
            if (amsHeader.CbData > 0)
            {
                IntPtr destination = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(TcMarshallableAmsHeader)));
                Marshal.Copy(amsCmd.Data, 0, destination, (int) amsHeader.CbData);
            }
            void* pAmsCmd = ref ptr.ToPointer();
            noError = (AdsErrorCode) UnsafeNativeMethods.AmsSend(this._server.Port, pAmsCmd);
            Marshal.FreeHGlobal(ptr);
            return noError;
        }

        [SecuritySafeCritical]
        internal unsafe void SetServerAddress()
        {
            IntPtr ptr = Marshal.AllocHGlobal(8);
            UnsafeNativeMethods.GetServerAddress(this._server.Port, ptr.ToPointer());
            TcMarshallableAmsAddress address = (TcMarshallableAmsAddress) Marshal.PtrToStructure(ptr, typeof(TcMarshallableAmsAddress));
            Marshal.FreeHGlobal(ptr);
            this._server.Address = new AmsAddress(address.netId, address.port);
        }

        internal Queue<TcAmsCommand> ReceiveQueue =>
            this._receiveQueue;

        internal delegate void TcAmsServerExEventDelegate(TcAmsServerExEventArgs e);

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        internal class TcMarshallableAmsAddress
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=6)]
            internal byte[] netId = new byte[6];
            internal ushort port;
        }
    }
}

