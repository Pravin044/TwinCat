namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;
    using TwinCAT;
    using TwinCAT.ValueAccess;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class SymbolFactoryServices : ISymbolFactoryServices, ISymbolFactoryValueServices
    {
        private IBinder _binder;
        private ISymbolFactory _symbolFactory;
        private IAccessorRawValue _valueAccessor;
        private ISession _session;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public SymbolFactoryServices(IBinder binder, ISymbolFactory factory, IAccessorRawValue accessor, ISession session)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            if (factory == null)
            {
                throw new ArgumentNullException("loader");
            }
            this._binder = binder;
            this._valueAccessor = accessor;
            this._symbolFactory = factory;
            this._session = session;
        }

        public IBinder Binder =>
            this._binder;

        public ISymbolFactory SymbolFactory =>
            this._symbolFactory;

        public IAccessorRawValue ValueAccessor =>
            this._valueAccessor;

        public ISession Session =>
            this._session;
    }
}

