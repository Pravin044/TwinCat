namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;

    internal class ClientCycleNotificationMngt : NotificationMngt
    {
        private ITimer _timer;
        private Dictionary<int, CycleTableEntry> _cycleTable;
        private Dictionary<int, int> _handleTable;
        private List<CycleListEntry> _initialNotes;
        private bool _bSumupRead;
        private bool _bStopTimer;
        private int _curHandle;

        public ClientCycleNotificationMngt(TcAdsSyncPort syncPort, SymbolTable symbolTable, bool synchronize) : base(syncPort, symbolTable, synchronize)
        {
            this._bSumupRead = false;
            this._bStopTimer = false;
            this._curHandle = 0;
            if (synchronize)
            {
                this._timer = new WindowsFormTimer();
            }
            else
            {
                this._timer = new ThreadTimer();
            }
        }

        public override int AddNotification(string variableName, AdsStream data, int offset, int length, int transMode, int cycleTime, int maxDelay, object userData, out AdsErrorCode result)
        {
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
            result = AdsErrorCode.NoError;
            if (!base._bInitialized)
            {
                this.Init();
            }
            cycleTime /= 0x2710;
            if (cycleTime <= 50)
            {
                cycleTime = 50;
            }
            int nextHandle = this.GetNextHandle();
            NotificationEntry entry = base.AddNotification(nextHandle, nextHandle, clientHandle, data, offset, length, userData);
            ITimer timer = this._timer;
            lock (timer)
            {
                CycleTableEntry entry2;
                if (!this._cycleTable.TryGetValue(cycleTime, out entry2))
                {
                    entry2 = new CycleTableEntry {
                        lastRead = NativeMethods.GetTickCount(),
                        timerCount = 0,
                        cycleList = new List<CycleListEntry>()
                    };
                    this._cycleTable.Add(cycleTime, entry2);
                }
                CycleListEntry item = new CycleListEntry {
                    handle = nextHandle,
                    variable = { 
                        indexGroup = 0xf005,
                        indexOffset = serverHandle,
                        length = entry.length
                    },
                    transMode = transMode,
                    data = new byte[entry.length]
                };
                entry2.cycleList.Add(item);
                this._initialNotes.Add(item);
            }
            return nextHandle;
        }

        public override int AddNotification(uint indexGroup, uint indexOffset, AdsStream data, int offset, int length, int transMode, int cycleTime, int maxDelay, object userData, out AdsErrorCode result)
        {
            result = AdsErrorCode.NoError;
            if (!base._bInitialized)
            {
                this.Init();
            }
            cycleTime /= 0x2710;
            if (cycleTime <= 50)
            {
                cycleTime = 50;
            }
            int nextHandle = this.GetNextHandle();
            NotificationEntry entry = base.AddNotification(nextHandle, nextHandle, 0, data, offset, length, userData);
            this._handleTable[nextHandle] = nextHandle;
            ITimer timer = this._timer;
            lock (timer)
            {
                CycleTableEntry entry2;
                if (!this._cycleTable.TryGetValue(cycleTime, out entry2))
                {
                    entry2 = new CycleTableEntry {
                        lastRead = NativeMethods.GetTickCount(),
                        timerCount = 0,
                        cycleList = new List<CycleListEntry>()
                    };
                    this._cycleTable.Add(cycleTime, entry2);
                }
                CycleListEntry item = new CycleListEntry {
                    handle = nextHandle,
                    variable = { 
                        indexGroup = indexGroup,
                        indexOffset = indexOffset,
                        length = entry.length
                    },
                    transMode = transMode,
                    data = new byte[entry.length]
                };
                entry2.cycleList.Add(item);
                this._initialNotes.Add(item);
            }
            return nextHandle;
        }

        public override NotificationEntry DeleteNotification(int notificationHandle, out AdsErrorCode result)
        {
            result = AdsErrorCode.NoError;
            this._handleTable.Remove(notificationHandle);
            NotificationEntry entry = base.DeleteNotification(notificationHandle, out result);
            if (entry != null)
            {
                ITimer timer = this._timer;
                lock (timer)
                {
                    int index = 0;
                    while (true)
                    {
                        if (index < this._initialNotes.Count)
                        {
                            if (this._initialNotes[index].handle == notificationHandle)
                            {
                                this._initialNotes.RemoveAt(index);
                            }
                            index++;
                            continue;
                        }
                        using (Dictionary<int, CycleTableEntry>.Enumerator enumerator = this._cycleTable.GetEnumerator())
                        {
                            while (true)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    break;
                                }
                                KeyValuePair<int, CycleTableEntry> current = enumerator.Current;
                                CycleTableEntry entry2 = current.Value;
                                int num2 = 0;
                                while (true)
                                {
                                    if (num2 >= entry2.cycleList.Count)
                                    {
                                        break;
                                    }
                                    if (entry2.cycleList[num2].handle != notificationHandle)
                                    {
                                        num2++;
                                        continue;
                                    }
                                    entry2.cycleList.RemoveAt(num2);
                                    if (entry2.cycleList.Count == 0)
                                    {
                                        this._cycleTable.Remove(current.Key);
                                    }
                                    return entry;
                                }
                            }
                        }
                        break;
                    }
                }
                if (entry.variableHandle != 0)
                {
                    AdsErrorCode code = base._symbolTable.TryDeleteVariableHandle(entry.variableHandle);
                }
            }
            return entry;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ITimer timer = this._timer;
                lock (timer)
                {
                    this._bStopTimer = false;
                }
                if (this._timer != null)
                {
                    this._timer.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private int GetNextHandle()
        {
            int num = this._curHandle;
            while ((this._curHandle == 0) || this._handleTable.ContainsKey(this._curHandle))
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
            if (!base._bInitialized)
            {
                this._timer.Interval = 50;
                this._timer.Tick += new EventHandler(this.OnReadCycle);
                this._cycleTable = new Dictionary<int, CycleTableEntry>(100);
                this._handleTable = new Dictionary<int, int>(100);
                this._initialNotes = new List<CycleListEntry>();
                base.Init();
                this._timer.Enabled = true;
                this._bSumupRead = true;
            }
        }

        public void OnReadCycle(object sender, EventArgs e)
        {
            ITimer timer = this._timer;
            lock (timer)
            {
                if (!this._bStopTimer)
                {
                    try
                    {
                        List<CycleListEntry> list = new List<CycleListEntry>();
                        uint tickCount = NativeMethods.GetTickCount();
                        long timeStamp = DateTime.Now.ToFileTime();
                        int num3 = 0;
                        while (true)
                        {
                            if (num3 >= this._initialNotes.Count)
                            {
                                foreach (KeyValuePair<int, CycleTableEntry> pair in this._cycleTable)
                                {
                                    int key = pair.Key;
                                    CycleTableEntry entry = pair.Value;
                                    entry.timerCount++;
                                    uint num5 = (tickCount - entry.lastRead) / entry.timerCount;
                                    if ((tickCount - entry.lastRead) >= key)
                                    {
                                        int num6 = 0;
                                        while (true)
                                        {
                                            if (num6 >= entry.cycleList.Count)
                                            {
                                                entry.lastRead = tickCount;
                                                entry.timerCount = 0;
                                                break;
                                            }
                                            list.Add(entry.cycleList[num6]);
                                            num6++;
                                        }
                                    }
                                }
                                if (list.Count != 0)
                                {
                                    if (this._bSumupRead)
                                    {
                                        int rdLength = list.Count * 4;
                                        int wrLength = list.Count * sizeof(VariableInfo);
                                        byte[] wrData = new byte[wrLength];
                                        int num10 = 0;
                                        while (true)
                                        {
                                            ref byte pinned numRef;
                                            if (num10 >= list.Count)
                                            {
                                                int num9;
                                                byte[] rdData = new byte[rdLength];
                                                if (base._syncPort.ReadWrite(0xf080, (uint) list.Count, 0, rdLength, rdData, 0, wrLength, wrData, false, out num9) != AdsErrorCode.NoError)
                                                {
                                                    CycleListEntry entry3 = list[0];
                                                    byte[] data = new byte[entry3.variable.length];
                                                    AdsErrorCode adsErrorCode = base._syncPort.Read(entry3.variable.indexGroup, entry3.variable.indexOffset, 0, entry3.variable.length, data, false, out num9);
                                                    if (adsErrorCode != AdsErrorCode.NoError)
                                                    {
                                                        TcAdsDllWrapper.ThrowAdsException(adsErrorCode);
                                                    }
                                                    this._bSumupRead = false;
                                                }
                                                if (this._bSumupRead)
                                                {
                                                    int num11 = list.Count * 4;
                                                    int num12 = 0;
                                                    while (num12 < list.Count)
                                                    {
                                                        CycleListEntry entry4 = list[num12];
                                                        bool flag2 = false;
                                                        int index = 0;
                                                        while (true)
                                                        {
                                                            if (index < 4)
                                                            {
                                                                if (rdData[index + (num12 * 4)] == 0)
                                                                {
                                                                    index++;
                                                                    continue;
                                                                }
                                                                flag2 = true;
                                                            }
                                                            if (flag2)
                                                            {
                                                                if ((base._syncWindow == null) || !base._bSynchronize)
                                                                {
                                                                    QueueElement[] elements = new QueueElement[] { new QueueElement(entry4.handle, timeStamp, new byte[0]) };
                                                                    base.OnSyncNotification(elements);
                                                                }
                                                                else
                                                                {
                                                                    QueueElement[] elements = new QueueElement[] { new QueueElement(entry4.handle, timeStamp, new byte[0]) };
                                                                    base._syncWindow.PostNotification(elements);
                                                                }
                                                                num11 += entry4.variable.length;
                                                            }
                                                            else
                                                            {
                                                                index = 0;
                                                                if ((num12 >= this._initialNotes.Count) && (entry4.transMode == 4))
                                                                {
                                                                    index = 0;
                                                                    while ((index < entry4.variable.length) && (rdData[num11 + index] == entry4.data[index]))
                                                                    {
                                                                        index++;
                                                                    }
                                                                }
                                                                if (index != entry4.variable.length)
                                                                {
                                                                    while (true)
                                                                    {
                                                                        if (index >= entry4.variable.length)
                                                                        {
                                                                            if ((base._syncWindow == null) || !base._bSynchronize)
                                                                            {
                                                                                QueueElement[] elements = new QueueElement[] { new QueueElement(entry4.handle, timeStamp, entry4.data) };
                                                                                base.OnSyncNotification(elements);
                                                                            }
                                                                            else
                                                                            {
                                                                                QueueElement[] elements = new QueueElement[] { new QueueElement(entry4.handle, timeStamp, entry4.data) };
                                                                                base._syncWindow.PostNotification(elements);
                                                                            }
                                                                            break;
                                                                        }
                                                                        entry4.data[index] = rdData[num11 + index];
                                                                        index++;
                                                                    }
                                                                }
                                                                num11 += entry4.variable.length;
                                                            }
                                                            num12++;
                                                            break;
                                                        }
                                                    }
                                                }
                                                break;
                                            }
                                            CycleListEntry entry2 = list[num10];
                                            try
                                            {
                                                byte[] buffer3;
                                                if (((buffer3 = wrData) == null) || (buffer3.Length == 0))
                                                {
                                                    numRef = null;
                                                }
                                                else
                                                {
                                                    numRef = buffer3;
                                                }
                                                numRef[num10 * sizeof(VariableInfo)] = (byte) entry2.variable;
                                            }
                                            finally
                                            {
                                                numRef = null;
                                            }
                                            rdLength += entry2.variable.length;
                                            num10++;
                                        }
                                    }
                                    if (!this._bSumupRead)
                                    {
                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            int num15;
                                            CycleListEntry entry5 = list[i];
                                            byte[] data = new byte[entry5.variable.length];
                                            AdsErrorCode adsErrorCode = base._syncPort.Read(entry5.variable.indexGroup, entry5.variable.indexOffset, 0, entry5.variable.length, data, false, out num15);
                                            if (adsErrorCode != AdsErrorCode.NoError)
                                            {
                                                if (adsErrorCode != AdsErrorCode.DeviceInvalidOffset)
                                                {
                                                    TcAdsDllWrapper.ThrowAdsException(adsErrorCode);
                                                }
                                                else if ((base._syncWindow == null) || !base._bSynchronize)
                                                {
                                                    QueueElement[] elements = new QueueElement[] { new QueueElement(entry5.handle, timeStamp, new byte[0]) };
                                                    base.OnSyncNotification(elements);
                                                }
                                                else
                                                {
                                                    QueueElement[] elements = new QueueElement[] { new QueueElement(entry5.handle, timeStamp, new byte[0]) };
                                                    base._syncWindow.PostNotification(elements);
                                                }
                                            }
                                            int index = 0;
                                            if ((i >= this._initialNotes.Count) && (entry5.transMode == 4))
                                            {
                                                index = 0;
                                                while ((index < entry5.variable.length) && (data[index] == entry5.data[index]))
                                                {
                                                    index++;
                                                }
                                            }
                                            if (index != entry5.variable.length)
                                            {
                                                while (true)
                                                {
                                                    if (index >= entry5.variable.length)
                                                    {
                                                        if ((base._syncWindow == null) || !base._bSynchronize)
                                                        {
                                                            QueueElement[] elements = new QueueElement[] { new QueueElement(entry5.handle, timeStamp, entry5.data) };
                                                            base.OnSyncNotification(elements);
                                                        }
                                                        else
                                                        {
                                                            QueueElement[] elements = new QueueElement[] { new QueueElement(entry5.handle, timeStamp, entry5.data) };
                                                            base._syncWindow.PostNotification(elements);
                                                        }
                                                        break;
                                                    }
                                                    entry5.data[index] = data[index];
                                                    index++;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                            list.Add(this._initialNotes[num3]);
                            num3++;
                        }
                    }
                    catch (Exception exception)
                    {
                        base.OnNotificationError(exception);
                    }
                    finally
                    {
                        this._initialNotes.Clear();
                    }
                }
            }
        }

        protected override void RemoveAllNotifications()
        {
            Dictionary<int, NotificationEntry> dictionary = base._notificationTable;
            lock (dictionary)
            {
                this._handleTable.Clear();
                this._initialNotes.Clear();
                this._cycleTable.Clear();
                base._notificationTable.Clear();
            }
        }

        public int TimerInterval
        {
            get => 
                this._timer.Interval;
            set => 
                (this._timer.Interval = value);
        }

        private class NativeMethods
        {
            [DllImport("kernel32")]
            internal static extern uint GetTickCount();
        }
    }
}

