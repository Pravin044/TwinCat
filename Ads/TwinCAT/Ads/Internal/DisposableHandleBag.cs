namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.SumCommand;

    public class DisposableHandleBag : IDisposableHandleBag, IDisposable
    {
        protected IAdsConnection connection;
        protected ISumHandleCollection handleDict;
        protected IDictionary<string, uint> validHandleDict;
        protected bool isDisposed;

        protected DisposableHandleBag(IAdsConnection client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            this.connection = client;
        }

        public DisposableHandleBag(IAdsConnection client, IList<string> symbolPaths) : this(client)
        {
            if (symbolPaths == null)
            {
                throw new ArgumentNullException("symbolPaths");
            }
            if (symbolPaths.Count == 0)
            {
                throw new ArgumentOutOfRangeException("symbolPaths");
            }
            if (new SumCreateHandles(this.connection, symbolPaths).TryCreateHandles(out this.handleDict) == AdsErrorCode.NoError)
            {
                this.validHandleDict = new Dictionary<string, uint>();
                foreach (SumHandleInstancePathEntry entry in this.handleDict)
                {
                    if (entry.ErrorCode == AdsErrorCode.NoError)
                    {
                        this.validHandleDict.Add(entry.InstancePath, entry.Handle);
                    }
                }
            }
        }

        public void Close()
        {
            this.Dispose(true);
        }

        public bool Contains(uint handle) => 
            Enumerable.Contains<uint>(this.handleDict.ValidHandles, handle);

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.Dispose(true);
                this.isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                AdsErrorCode[] returnCodes = null;
                uint[] validHandles = this.handleDict.ValidHandles;
                AdsErrorCode code = new SumReleaseHandles(this.connection, validHandles).TryReleaseHandles(out returnCodes);
                this.handleDict = null;
            }
        }

        ~DisposableHandleBag()
        {
            this.Dispose(false);
        }

        public uint GetHandle(string instancePath)
        {
            uint handle = 0;
            this.TryGetHandle(instancePath, out handle);
            return handle;
        }

        public bool TryGetHandle(string instancePath, out uint handle)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("DisposableHandleBag");
            }
            return this.validHandleDict.TryGetValue(instancePath, out handle);
        }
    }
}

