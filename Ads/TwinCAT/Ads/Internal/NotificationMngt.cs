namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Threading;
    using TwinCAT.Ads;

    internal abstract class NotificationMngt : ISyncNotificationReceiver, IDisposable
    {
        private Thread _errorThread;
        private AutoResetEvent _errorEvent;
        private Queue<Exception> _exceptionQueue;
        protected TcAdsSyncPort _syncPort;
        protected Dictionary<int, NotificationEntry> _notificationTable;
        protected ISyncWindow _syncWindow;
        protected bool _bSynchronize;
        protected SymbolTable _symbolTable;
        protected bool _bInitialized;
        protected bool _disposed;

        public NotificationMngt(TcAdsSyncPort syncPort, SymbolTable symbolTable, bool synchronize)
        {
            if (synchronize)
            {
                this._syncWindow = new SyncWindow(syncPort, this);
            }
            this._bSynchronize = synchronize;
            this._symbolTable = symbolTable;
            this._syncPort = syncPort;
            this._bInitialized = false;
            this._notificationTable = new Dictionary<int, NotificationEntry>();
            this._exceptionQueue = new Queue<Exception>(10);
            this._errorEvent = new AutoResetEvent(false);
        }

        protected NotificationEntry AddNotification(int notificationHandle, int clientHandle, int variableHandle, AdsStream data, int offset, int length, object userData)
        {
            NotificationEntry entry = new NotificationEntry {
                length = length,
                offset = offset,
                data = data,
                userData = userData,
                clientHandle = clientHandle,
                variableHandle = variableHandle
            };
            Dictionary<int, NotificationEntry> dictionary = this._notificationTable;
            lock (dictionary)
            {
                this._notificationTable.Add(notificationHandle, entry);
            }
            return entry;
        }

        public abstract int AddNotification(string variableName, AdsStream data, int offset, int length, int transMode, int cycleTime, int maxDelay, object userData, out AdsErrorCode result);
        public abstract int AddNotification(uint indexGroup, uint indexOffset, AdsStream data, int offset, int length, int transMode, int cycleTime, int maxDelay, object userData, out AdsErrorCode result);
        public virtual NotificationEntry DeleteNotification(int notificationHandle, out AdsErrorCode result)
        {
            result = AdsErrorCode.NoError;
            NotificationEntry entry = null;
            Dictionary<int, NotificationEntry> dictionary = this._notificationTable;
            lock (dictionary)
            {
                if (this._notificationTable.ContainsKey(notificationHandle))
                {
                    entry = this._notificationTable[notificationHandle];
                }
            }
            if (entry == null)
            {
                result = AdsErrorCode.ClientRemoveHash;
                return null;
            }
            Dictionary<int, NotificationEntry> dictionary2 = this._notificationTable;
            lock (dictionary2)
            {
                this._notificationTable.Remove(notificationHandle);
            }
            return entry;
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                this._bInitialized = false;
                this.Dispose(true);
                this._disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId="_errorEvent")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Queue<Exception> queue = this._exceptionQueue;
                lock (queue)
                {
                    this._errorEvent.Set();
                    this._exceptionQueue.Clear();
                }
                this.RemoveAllNotifications();
                if (this._syncWindow != null)
                {
                    this._syncWindow.Dispose();
                }
            }
        }

        private void ErrorHandling()
        {
            try
            {
                if (this._exceptionQueue.Count > 0)
                {
                    Exception exception;
                    Queue<Exception> queue = this._exceptionQueue;
                    lock (queue)
                    {
                        exception = this._exceptionQueue.Dequeue();
                    }
                    this._syncPort.OnNotificationError(exception);
                }
            }
            catch (Exception exception2)
            {
                Module.Trace.TraceError("Exception in ErrorThread.", exception2);
            }
        }

        private void ErrorThread()
        {
            while (this._bInitialized)
            {
                this._errorEvent.WaitOne();
                this.ErrorHandling();
            }
        }

        ~NotificationMngt()
        {
            this._bInitialized = false;
            this.Dispose(false);
        }

        protected virtual void Init()
        {
            this._bInitialized = true;
            this._errorThread = new Thread(new ThreadStart(this.ErrorThread));
            this._errorThread.IsBackground = true;
            this._errorThread.Start();
        }

        public void OnNotificationError(Exception e)
        {
            Queue<Exception> queue = this._exceptionQueue;
            lock (queue)
            {
                this._exceptionQueue.Enqueue(e);
                this._errorEvent.Set();
            }
        }

        public void OnSyncNotification(QueueElement element)
        {
            NotificationEntry entry = null;
            Dictionary<int, NotificationEntry> dictionary = this._notificationTable;
            lock (dictionary)
            {
                if (this._notificationTable.ContainsKey(element.handle))
                {
                    entry = this._notificationTable[element.handle];
                }
            }
            try
            {
                if (entry != null)
                {
                    if ((entry.length < element.data.Length) || (element.data.Length == 0))
                    {
                        this._syncPort.OnSyncNotification(entry.clientHandle, element.timeStamp, element.data.Length, entry, true);
                    }
                    else
                    {
                        entry.data.Position = entry.offset;
                        if (entry.data.GetBuffer() != element.data)
                        {
                            entry.data.Write(element.data, 0, element.data.Length);
                            entry.data.Position = entry.offset;
                        }
                        this._syncPort.OnSyncNotification(entry.clientHandle, element.timeStamp, element.data.Length, entry, false);
                    }
                }
            }
            catch (Exception exception)
            {
                this.OnNotificationError(exception);
            }
        }

        public void OnSyncNotification(QueueElement[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                this.OnSyncNotification(elements[i]);
            }
        }

        protected abstract void RemoveAllNotifications();

        public virtual bool Synchronize
        {
            get => 
                this._bSynchronize;
            set => 
                (this._bSynchronize = value);
        }

        public bool IsActive =>
            this._bInitialized;
    }
}

