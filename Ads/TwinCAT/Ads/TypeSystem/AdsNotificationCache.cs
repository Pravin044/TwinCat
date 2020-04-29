namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;
    using TwinCAT.ValueAccess;

    internal class AdsNotificationCache
    {
        private object _sync = new object();
        private Dictionary<int, NotificationSymbolInfo> _notificationHandleDict = new Dictionary<int, NotificationSymbolInfo>();
        private Dictionary<ISymbol, NotificationSymbolInfo> _notificationSymbolDict = new Dictionary<ISymbol, NotificationSymbolInfo>();

        internal AdsNotificationCache()
        {
        }

        internal void Add(ISymbol symbol, int handle, SymbolNotificationType notificationType, NotificationSettings settings)
        {
            NotificationSymbolInfo info = null;
            object obj2 = this._sync;
            lock (obj2)
            {
                if (this._notificationHandleDict.TryGetValue(handle, out info))
                {
                    throw new ArgumentException("Symbol already registered!");
                }
                NotificationSymbolInfo info2 = new NotificationSymbolInfo(symbol, handle, notificationType, settings);
                this._notificationHandleDict.Add(handle, info2);
                this._notificationSymbolDict.Add(symbol, info2);
            }
        }

        internal bool Contains(ISymbol symbol)
        {
            object obj2 = this._sync;
            lock (obj2)
            {
                return this._notificationSymbolDict.ContainsKey(symbol);
            }
        }

        internal int GetLargestSymbolSize()
        {
            int num = 0;
            object obj2 = this._sync;
            lock (obj2)
            {
                if (this._notificationSymbolDict.Count > 0)
                {
                    num = Enumerable.Max<ISymbol>(this._notificationSymbolDict.Keys, symbol => symbol.ByteSize);
                }
            }
            return num;
        }

        internal SymbolNotificationType GetNotificationType(ISymbol symbol)
        {
            SymbolNotificationType none = SymbolNotificationType.None;
            NotificationSymbolInfo info = null;
            object obj2 = this._sync;
            lock (obj2)
            {
                if (this._notificationSymbolDict.TryGetValue(symbol, out info))
                {
                    none = info.NotificationType;
                }
            }
            return none;
        }

        internal bool Remove(ISymbol symbol)
        {
            bool flag = false;
            NotificationSymbolInfo info = null;
            object obj2 = this._sync;
            lock (obj2)
            {
                if (this._notificationSymbolDict.TryGetValue(symbol, out info))
                {
                    this._notificationHandleDict.Remove(info.Handle);
                    flag = this._notificationSymbolDict.Remove(info.Symbol);
                }
            }
            return flag;
        }

        internal bool Remove(ISymbol symbol, SymbolNotificationType notificationType)
        {
            bool flag = false;
            NotificationSymbolInfo info = null;
            object obj2 = this._sync;
            lock (obj2)
            {
                if (this._notificationSymbolDict.TryGetValue(symbol, out info))
                {
                    SymbolNotificationType type = SymbolNotificationType.Both & ~notificationType;
                    info.NotificationType &= type;
                    if (info.NotificationType == SymbolNotificationType.None)
                    {
                        this._notificationHandleDict.Remove(info.Handle);
                        flag = this._notificationSymbolDict.Remove(symbol);
                    }
                }
            }
            return flag;
        }

        internal bool TryGetNotificationHandle(ISymbol symbol, out int handle)
        {
            bool flag = false;
            NotificationSymbolInfo info = null;
            handle = 0;
            object obj2 = this._sync;
            lock (obj2)
            {
                if (this._notificationSymbolDict.TryGetValue(symbol, out info))
                {
                    handle = info.Handle;
                    flag = true;
                }
            }
            return flag;
        }

        internal bool TryGetRegisteredNotificationSettings(ISymbol symbol, out NotificationSettings settings)
        {
            bool flag = false;
            settings = null;
            object obj2 = this._sync;
            lock (obj2)
            {
                NotificationSymbolInfo info = null;
                if (this._notificationSymbolDict.TryGetValue(symbol, out info))
                {
                    settings = info.Settings;
                    flag = true;
                }
            }
            return flag;
        }

        internal void Update(ISymbol symbol, SymbolNotificationType type, NotificationSettings settings)
        {
            NotificationSymbolInfo info = null;
            object obj2 = this._sync;
            lock (obj2)
            {
                if (!this._notificationSymbolDict.TryGetValue(symbol, out info))
                {
                    throw new ArgumentException("Symbol is not registered for Notifications!");
                }
                info.Settings = settings;
                info.NotificationType |= type;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AdsNotificationCache.<>c <>9 = new AdsNotificationCache.<>c();
            public static Func<ISymbol, int> <>9__5_0;

            internal int <GetLargestSymbolSize>b__5_0(ISymbol symbol) => 
                symbol.ByteSize;
        }

        private class NotificationSymbolInfo
        {
            internal ISymbol Symbol;
            internal SymbolNotificationType NotificationType;
            internal int Handle;
            internal NotificationSettings Settings;

            internal NotificationSymbolInfo(ISymbol symbol, int handle, SymbolNotificationType notificationType, NotificationSettings settings)
            {
                this.Symbol = symbol;
                this.NotificationType = notificationType;
                this.Handle = handle;
                this.Settings = settings;
            }
        }
    }
}

