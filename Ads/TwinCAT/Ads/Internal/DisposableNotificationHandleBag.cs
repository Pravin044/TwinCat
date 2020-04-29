namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.SumCommand;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public sealed class DisposableNotificationHandleBag : DisposableHandleBag, IDisposableSymbolHandleBag, IDisposableHandleBag, IDisposable
    {
        private AdsStream _stream;
        private Dictionary<uint, ISymbol> _handleSymbolDict;

        public DisposableNotificationHandleBag(IAdsConnection client, IEnumerable<ISymbol> symbols, NotificationSettings settings, object userData) : base(client)
        {
            if (symbols == null)
            {
                throw new ArgumentNullException("dict");
            }
            int length = Enumerable.Sum<ISymbol>(symbols, s => s.ByteSize);
            this._stream = new AdsStream(length);
            base.handleDict = new SumHandleList();
            this._handleSymbolDict = new Dictionary<uint, ISymbol>();
            base.validHandleDict = new Dictionary<string, uint>();
            int offset = 0;
            foreach (ISymbol symbol in symbols)
            {
                uint handle = 0;
                int byteSize = symbol.ByteSize;
                AdsErrorCode errorCode = client.TryAddDeviceNotification(symbol.InstancePath, this._stream, offset, byteSize, settings, userData, out handle);
                base.handleDict.Add(new SumHandleInstancePathEntry(symbol.InstancePath, handle, errorCode));
                if (errorCode == AdsErrorCode.NoError)
                {
                    base.validHandleDict.Add(symbol.InstancePath, handle);
                    this._handleSymbolDict.Add(handle, symbol);
                }
                offset += byteSize;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (KeyValuePair<string, uint> pair in base.validHandleDict)
                {
                    AdsErrorCode code = base.connection.TryDeleteDeviceNotification(pair.Value);
                }
                this._stream.Dispose();
                this._stream = null;
                this._handleSymbolDict = null;
            }
        }

        public ISymbol GetSymbol(uint handle)
        {
            ISymbol symbol = null;
            this.TryGetSymbol(handle, out symbol);
            return symbol;
        }

        public bool TryGetSymbol(uint handle, out ISymbol symbol)
        {
            if (base.isDisposed)
            {
                throw new ObjectDisposedException("DisposableNotificationHandleBag");
            }
            if (this._handleSymbolDict != null)
            {
                return this._handleSymbolDict.TryGetValue(handle, out symbol);
            }
            symbol = null;
            return false;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DisposableNotificationHandleBag.<>c <>9 = new DisposableNotificationHandleBag.<>c();
            public static Func<ISymbol, int> <>9__2_0;

            internal int <.ctor>b__2_0(ISymbol s) => 
                s.ByteSize;
        }
    }
}

