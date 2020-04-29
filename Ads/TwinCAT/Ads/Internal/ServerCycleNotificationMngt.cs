namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Threading;
    using TwinCAT.Ads;

    internal class ServerCycleNotificationMngt : NotificationMngt
    {
        private const int DEFAULT_QUEUESIZE = 500;
        private const int QUEUEPEAK = 0x2710;
        private Queue<QueueElement> _queue;
        private ManualResetEvent _enqueueEvent;
        private Thread _notificationThread;
        private Dictionary<int, int> _clientHandleTable;
        private bool _bPeak;
        private bool _bPeakError;
        private long _bPeakStart;
        private int _curHandle;
        private TcAdsDllWrapper.AdsNotificationDelegate _adsNotificationDelegate;

        public ServerCycleNotificationMngt(TcAdsSyncPort syncPort, SymbolTable symbolTable, bool synchronize) : base(syncPort, symbolTable, synchronize)
        {
            this._curHandle = 0;
            this._bPeak = false;
            this._bPeakError = false;
            this._clientHandleTable = new Dictionary<int, int>();
            this._enqueueEvent = new ManualResetEvent(false);
            this._queue = new Queue<QueueElement>(500);
            this._adsNotificationDelegate = new TcAdsDllWrapper.AdsNotificationDelegate(this.OnNotification);
        }

        public override unsafe int AddNotification(string variableName, AdsStream data, int offset, int length, int transMode, int cycleTime, int maxDelay, object userData, out AdsErrorCode result)
        {
            result = AdsErrorCode.NoError;
            int clientHandle = 0;
            result = base._symbolTable.TryCreateVariableHandle(variableName, out clientHandle);
            if (result != AdsErrorCode.NoError)
            {
                return 0;
            }
            uint serverHandle = base._symbolTable.GetServerHandle(clientHandle, out result);
            if (result != AdsErrorCode.NoError)
            {
                return 0;
            }
            AdsNotificationAttrib noteAttrib = new AdsNotificationAttrib {
                cbLength = length,
                nCycleTime = cycleTime,
                nMaxDelay = maxDelay,
                nTransMode = transMode
            };
            int hNotification = 0;
            int nextClientHandle = 0;
            if (!base._bInitialized)
            {
                this.Init();
            }
            result = base._syncPort.AddDeviceNotification(0xf005, serverHandle, &noteAttrib, this._adsNotificationDelegate, 0, false, out hNotification);
            if (result == AdsErrorCode.NoError)
            {
                nextClientHandle = this.GetNextClientHandle();
                base.AddNotification(hNotification, nextClientHandle, clientHandle, data, offset, length, userData);
                this._clientHandleTable.Add(nextClientHandle, hNotification);
            }
            return nextClientHandle;
        }

        public override unsafe int AddNotification(uint indexGroup, uint indexOffset, AdsStream data, int offset, int length, int transMode, int cycleTime, int maxDelay, object userData, out AdsErrorCode result)
        {
            AdsNotificationAttrib noteAttrib = new AdsNotificationAttrib {
                cbLength = length,
                nCycleTime = cycleTime,
                nMaxDelay = maxDelay,
                nTransMode = transMode
            };
            int hNotification = 0;
            int clientHandle = 0;
            if (!base._bInitialized)
            {
                this.Init();
            }
            result = base._syncPort.AddDeviceNotification(indexGroup, indexOffset, &noteAttrib, this._adsNotificationDelegate, 0, false, out hNotification);
            if (result == AdsErrorCode.NoError)
            {
                clientHandle = this.GetNextClientHandle();
                base.AddNotification(hNotification, clientHandle, 0, data, offset, length, userData);
                this._clientHandleTable.Add(clientHandle, hNotification);
            }
            return clientHandle;
        }

        public override NotificationEntry DeleteNotification(int notificationHandle, out AdsErrorCode result)
        {
            if (!this._clientHandleTable.ContainsKey(notificationHandle))
            {
                result = AdsErrorCode.ClientRemoveHash;
                return null;
            }
            int num = this._clientHandleTable[notificationHandle];
            NotificationEntry entry = base.DeleteNotification(num, out result);
            if (entry != null)
            {
                this._clientHandleTable.Remove(entry.clientHandle);
                result = base._syncPort.DelDeviceNotification(num, false);
                if (entry.variableHandle != 0)
                {
                    AdsErrorCode code = base._symbolTable.TryDeleteVariableHandle(entry.variableHandle);
                }
            }
            return entry;
        }

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId="_enqueueEvent")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Queue<QueueElement> queue = this._queue;
                lock (queue)
                {
                    this._enqueueEvent.Set();
                    this._queue.Clear();
                    this._queue = null;
                }
                if ((this._notificationThread != null) && !this._notificationThread.Join(200))
                {
                    this._notificationThread.Abort();
                }
                this._notificationThread = null;
            }
            base.Dispose(disposing);
        }

        private int GetNextClientHandle()
        {
            int num = this._curHandle;
            while ((this._curHandle == 0) || this._clientHandleTable.ContainsKey(this._curHandle))
            {
                this._curHandle++;
                if (this._curHandle == num)
                {
                    return 0;
                }
            }
            int num2 = this._curHandle;
            this._curHandle = num2 + 1;
            return num2;
        }

        protected override void Init()
        {
            ServerCycleNotificationMngt mngt = this;
            lock (mngt)
            {
                if (!base._bInitialized)
                {
                    this._notificationThread = new Thread(new ThreadStart(this.NotificationThread));
                    this._notificationThread.IsBackground = true;
                    this._notificationThread.Name = $"NotificationThread({base._syncPort.TargetAddress.ToString()})";
                    this._notificationThread.Start();
                    base.Init();
                }
            }
        }

        public void NotificationThread()
        {
            bool flag = false;
            Type type = null;
            this._enqueueEvent.WaitOne();
            while (base._bInitialized)
            {
                try
                {
                    QueueElement[] array = null;
                    Queue<QueueElement> queue = this._queue;
                    lock (queue)
                    {
                        this._enqueueEvent.Reset();
                        int count = this._queue.Count;
                        if (count != 0)
                        {
                            array = new QueueElement[count];
                            this._queue.CopyTo(array, 0);
                            this._queue.Clear();
                            if (count >= 0x2328)
                            {
                                flag = true;
                            }
                        }
                    }
                    if ((array != null) && (array.Length != 0))
                    {
                        if ((base._syncWindow == null) || !base._bSynchronize)
                        {
                            base.OnSyncNotification(array);
                        }
                        else
                        {
                            base._syncWindow.PostNotification(array);
                        }
                    }
                    type = null;
                }
                catch (Exception exception)
                {
                    Type type2 = exception.GetType();
                    if (type2 != type)
                    {
                        base.OnNotificationError(exception);
                        type = type2;
                    }
                }
                if (base._bInitialized)
                {
                    this._enqueueEvent.WaitOne();
                }
            }
        }

        protected void OnNotification(int handle, long timeStamp, byte[] data)
        {
            if (!base._disposed)
            {
                QueueElement item = new QueueElement(handle, timeStamp, data);
                try
                {
                    int count;
                    Queue<QueueElement> queue = this._queue;
                    lock (queue)
                    {
                        count = this._queue.Count;
                    }
                    if (this._bPeak)
                    {
                        if (count >= 0x2328)
                        {
                            if ((DateTime.Now.Ticks - this._bPeakStart) > 0x2faf080L)
                            {
                                if (!this._bPeakError)
                                {
                                    this._bPeakError = true;
                                    TcAdsDllWrapper.ThrowAdsException(AdsErrorCode.ClientQueueFull);
                                }
                                return;
                            }
                        }
                        else
                        {
                            this._bPeak = false;
                            this._bPeakError = false;
                        }
                    }
                    else if (count >= 0x2710)
                    {
                        this._bPeak = true;
                        this._bPeakStart = DateTime.Now.Ticks;
                    }
                    Queue<QueueElement> queue2 = this._queue;
                    lock (queue2)
                    {
                        this._queue.Enqueue(item);
                        this._enqueueEvent.Set();
                    }
                }
                catch (Exception exception)
                {
                    base.OnNotificationError(exception);
                }
            }
        }

        internal unsafe void OnNotification(IntPtr pAddr, IntPtr pNotification, int hUser)
        {
            byte* numPtr = (byte*) pNotification.ToPointer();
            int handle = *((int*) numPtr);
            numPtr += 4;
            long timeStamp = *((long*) numPtr);
            numPtr += 8;
            int num3 = *((int*) numPtr);
            numPtr += 4;
            byte[] data = new byte[num3];
            for (int i = 0; i < num3; i++)
            {
                data[i] = numPtr[i];
            }
            this.OnNotification(handle, timeStamp, data);
        }

        protected override void RemoveAllNotifications()
        {
            this._clientHandleTable.Clear();
            if (base._notificationTable != null)
            {
                Dictionary<int, NotificationEntry> dictionary = base._notificationTable;
                lock (dictionary)
                {
                    foreach (KeyValuePair<int, NotificationEntry> pair in base._notificationTable)
                    {
                        base._syncPort.DelDeviceNotification(pair.Key, false);
                    }
                    base._notificationTable.Clear();
                }
            }
        }
    }
}

