namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using TwinCAT.Ads;

    internal class SymbolTable : IDisposable
    {
        private Dictionary<string, int> _symbolPathTable = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<int, SymbolEntry> _symbolTable = new Dictionary<int, SymbolEntry>();
        private ITcAdsRaw _syncPort;
        private bool _disposed = true;

        public SymbolTable(ITcAdsRaw syncPort)
        {
            this._syncPort = syncPort;
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.RemoveAll();
            }
        }

        ~SymbolTable()
        {
            this.Dispose(false);
        }

        public uint GetServerHandle(int clientHandle, out AdsErrorCode result)
        {
            result = AdsErrorCode.NoError;
            uint serverHandle = 0;
            Dictionary<int, SymbolEntry> dictionary = this._symbolTable;
            lock (dictionary)
            {
                if (this._symbolTable.ContainsKey(clientHandle))
                {
                    serverHandle = this._symbolTable[clientHandle].serverHandle;
                }
                else
                {
                    result = AdsErrorCode.DeviceInvalidOffset;
                }
            }
            return serverHandle;
        }

        public void RemoveAll()
        {
            Dictionary<int, SymbolEntry> dictionary = this._symbolTable;
            lock (dictionary)
            {
                foreach (KeyValuePair<int, SymbolEntry> pair in this._symbolTable)
                {
                    uint serverHandle = pair.Value.serverHandle;
                    this._syncPort.Write(0xf006, 0, serverHandle, false);
                }
                this._symbolTable.Clear();
                this._symbolPathTable.Clear();
            }
        }

        public AdsErrorCode TryCreateVariableHandle(string variableName, out int clientHandle)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            clientHandle = 0;
            bool flag = false;
            noError = AdsErrorCode.NoError;
            Dictionary<int, SymbolEntry> dictionary = this._symbolTable;
            lock (dictionary)
            {
                flag = this._symbolPathTable.TryGetValue(variableName, out clientHandle);
            }
            uint rdValue = (uint) clientHandle;
            if (!flag)
            {
                noError = this._syncPort.ReadWrite(0xf003, 0, variableName, false, out rdValue);
            }
            if (noError == AdsErrorCode.NoError)
            {
                clientHandle = (int) rdValue;
                SymbolEntry entry = null;
                Dictionary<int, SymbolEntry> dictionary2 = this._symbolTable;
                lock (dictionary2)
                {
                    if (!flag)
                    {
                        this._symbolPathTable.Add(variableName, clientHandle);
                    }
                    if (!this._symbolTable.TryGetValue(clientHandle, out entry))
                    {
                        entry = new SymbolEntry(rdValue, variableName);
                        this._symbolTable.Add(clientHandle, entry);
                    }
                }
                Interlocked.Increment(ref entry.referenceCount);
            }
            return noError;
        }

        public AdsErrorCode TryDeleteVariableHandle(int variableHandle)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            Dictionary<int, SymbolEntry> dictionary = this._symbolTable;
            lock (dictionary)
            {
                if (!this._symbolTable.ContainsKey(variableHandle))
                {
                    noError = AdsErrorCode.DeviceSymbolNotFound;
                }
                else
                {
                    SymbolEntry entry = this._symbolTable[variableHandle];
                    int num = entry.referenceCount - 1;
                    entry.referenceCount = num;
                    if (num != 0)
                    {
                        noError = AdsErrorCode.NoError;
                    }
                    else
                    {
                        noError = this._syncPort.Write(0xf006, 0, entry.serverHandle, false);
                        bool flag2 = this._symbolTable.Remove(variableHandle);
                        bool flag3 = this._symbolPathTable.Remove(entry.symbolPath);
                    }
                }
            }
            return noError;
        }

        private class SymbolEntry
        {
            public uint serverHandle;
            public int referenceCount;
            public string symbolPath;

            public SymbolEntry(uint serverHandle, string symbolPath)
            {
                this.serverHandle = serverHandle;
                this.symbolPath = symbolPath;
                this.referenceCount = 0;
            }
        }
    }
}

