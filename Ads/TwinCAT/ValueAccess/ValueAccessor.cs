namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public abstract class ValueAccessor : IAccessorRawValue, IAccessorValue, IAccessorConnection
    {
        protected IConnection connection;
        protected ISession session;
        protected IAccessorValueFactory valueFactory;

        protected ValueAccessor(IAccessorValueFactory factory, IConnection connection)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (connection == null)
            {
                throw new ArgumentNullException("adsClient");
            }
            if (!connection.IsConnected)
            {
                throw new ClientNotConnectedException();
            }
            if (connection.Session != null)
            {
                this.session = connection.Session;
            }
            else
            {
                this.connection = connection;
            }
            this.valueFactory = factory;
        }

        protected ValueAccessor(IAccessorValueFactory factory, ISession session)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            this.session = session;
            this.connection = null;
            this.valueFactory = factory;
        }

        protected virtual void OnRawValueChanged(ISymbol symbol, byte[] rawValue, DateTime utcTwinCATTime, DateTime utcLocalSystemTime)
        {
            RawValueChangedArgs args = new RawValueChangedArgs(symbol, rawValue, utcTwinCATTime, utcLocalSystemTime);
            ((ISymbolValueChangeNotify) symbol).OnRawValueChanged(args);
        }

        protected virtual void OnValueChanged(ISymbol symbol, byte[] rawValue, DateTime tcUTCTimeStamp, DateTime utcLocalSystemTime)
        {
            object obj2 = this.valueFactory.CreateValue(symbol, rawValue, 0, utcLocalSystemTime);
            ValueChangedArgs args = new ValueChangedArgs(symbol, obj2, tcUTCTimeStamp, utcLocalSystemTime);
            ((ISymbolValueChangeNotify) symbol).OnValueChanged(args);
        }

        public object ReadValue(ISymbol symbol, out DateTime utcReadTime)
        {
            object obj2 = null;
            AdsErrorCode code = (AdsErrorCode) this.TryReadValue(symbol, out obj2, out utcReadTime);
            if (code == AdsErrorCode.NoError)
            {
                return obj2;
            }
            SymbolException ex = null;
            ex = new SymbolException($"Could not read Symbol '{symbol}'! Error: {code}", symbol, AdsErrorException.Create(code));
            Module.Trace.TraceError(ex);
            throw ex;
        }

        public abstract int TryReadArrayElementValue(ISymbol arrayInstance, int[] indices, out byte[] value, out DateTime utcReadTime);
        public int TryReadValue(ISymbol symbol, out object value, out DateTime utcReadTime)
        {
            value = null;
            byte[] buffer = null;
            ISymbol symbolInstance = symbol;
            IAnySizeArrayInstance instance = symbolInstance as IAnySizeArrayInstance;
            int num = this.TryReadValue(symbolInstance, out buffer, out utcReadTime);
            if (num == 0)
            {
                value = (this.valueFactory == null) ? buffer : this.valueFactory.CreateValue(symbolInstance, buffer, 0, utcReadTime);
            }
            return num;
        }

        public abstract int TryReadValue(ISymbol symbolInstance, out byte[] value, out DateTime utcReadTime);
        public abstract int TryWriteArrayElementValue(ISymbol arrayInstance, int[] indices, byte[] value, int offset, out DateTime utcWriteTime);
        public int TryWriteValue(ISymbol symbol, object value, out DateTime utcWriteTime)
        {
            ISymbol symbolInstance = symbol;
            return this.TryWriteValue(symbolInstance, new InstanceValueConverter().Marshal(symbolInstance, value), 0, out utcWriteTime);
        }

        public abstract int TryWriteValue(ISymbol symbolInstance, byte[] value, int offset, out DateTime utcWriteTime);
        public virtual void WriteValue(ISymbol symbol, object value, out DateTime utcWriteTime)
        {
            AdsErrorCode code = (AdsErrorCode) this.TryWriteValue(symbol, value, out utcWriteTime);
            if (code != AdsErrorCode.NoError)
            {
                SymbolException ex = null;
                ex = new SymbolException($"Could not write Symbol '{symbol} '! Error: {code}", symbol);
                Module.Trace.TraceError(ex);
                throw ex;
            }
        }

        public IConnection Connection =>
            ((this.session == null) ? this.connection : this.session.Connection);

        public ISession Session =>
            this.session;

        public IAccessorValueFactory ValueFactory =>
            this.valueFactory;
    }
}

