namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Runtime.InteropServices;
    using TwinCAT;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.ValueAccess;

    [DebuggerDisplay("Path = { InstancePath }, Type = {TypeName}, Size = {Size}, Category = {Category}")]
    public class DynamicSymbol : DynamicObject, IDynamicSymbol, ISymbol, IAttributedInstance, IInstance, IBitSize, ISymbolFactoryServicesProvider, IValueSymbol2, IValueSymbol, IValueRawSymbol, IHierarchicalSymbol, IValueAccessorProvider, ISymbolValueChangeNotify, IBinderProvider, IContextMaskProvider, IInstanceInternal, ISymbolInternal
    {
        protected IValueSymbol symbol;
        protected string normalizedName;
        private EventHandler<RawValueChangedArgs> _rawValueChanged;
        private EventHandler<ValueChangedArgs> _valueChanged;

        public event EventHandler<RawValueChangedArgs> RawValueChanged
        {
            add
            {
                DynamicSymbol symbol = this;
                lock (symbol)
                {
                    bool flag2 = false;
                    if ((this._rawValueChanged == null) || (this._rawValueChanged.GetInvocationList().Length == 0))
                    {
                        flag2 = true;
                    }
                    this._rawValueChanged = (EventHandler<RawValueChangedArgs>) Delegate.Combine(this._rawValueChanged, value);
                    if (flag2)
                    {
                        IAccessorNotification valueAccessor = this.ValueAccessor as IAccessorNotification;
                        if (valueAccessor != null)
                        {
                            valueAccessor.OnRegisterNotification(this, SymbolNotificationType.RawValue, this.NotificationSettings);
                        }
                    }
                }
            }
            remove
            {
                DynamicSymbol symbol = this;
                lock (symbol)
                {
                    this._rawValueChanged = (EventHandler<RawValueChangedArgs>) Delegate.Remove(this._rawValueChanged, value);
                    if ((this._rawValueChanged == null) || (this._rawValueChanged.GetInvocationList().Length == 0))
                    {
                        IAccessorNotification valueAccessor = this.ValueAccessor as IAccessorNotification;
                        if (valueAccessor != null)
                        {
                            valueAccessor.OnUnregisterNotification(this, SymbolNotificationType.RawValue);
                        }
                    }
                }
            }
        }

        public event EventHandler<ValueChangedArgs> ValueChanged
        {
            add
            {
                DynamicSymbol symbol = this;
                lock (symbol)
                {
                    bool flag2 = false;
                    if ((this._valueChanged == null) || (this._valueChanged.GetInvocationList().Length == 0))
                    {
                        flag2 = true;
                    }
                    this._valueChanged = (EventHandler<ValueChangedArgs>) Delegate.Combine(this._valueChanged, value);
                    if (flag2)
                    {
                        IAccessorNotification valueAccessor = this.ValueAccessor as IAccessorNotification;
                        if (valueAccessor != null)
                        {
                            valueAccessor.OnRegisterNotification(this, SymbolNotificationType.Value, this.NotificationSettings);
                        }
                    }
                }
            }
            remove
            {
                DynamicSymbol symbol = this;
                lock (symbol)
                {
                    this._valueChanged = (EventHandler<ValueChangedArgs>) Delegate.Remove(this._valueChanged, value);
                    if ((this._valueChanged == null) || (this._valueChanged.GetInvocationList().Length == 0))
                    {
                        IAccessorNotification valueAccessor = this.ValueAccessor as IAccessorNotification;
                        if (valueAccessor != null)
                        {
                            valueAccessor.OnUnregisterNotification(this, SymbolNotificationType.Value);
                        }
                    }
                }
            }
        }

        internal DynamicSymbol(IValueSymbol symbol)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            this.symbol = symbol;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ISymbolCollection CreateSubSymbols(ISymbol parent)
        {
            ISymbolInternal symbol = this.symbol as ISymbolInternal;
            return symbol?.CreateSubSymbols(parent);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (base.GetType() != obj.GetType())
            {
                return false;
            }
            DynamicSymbol symbol = (DynamicSymbol) obj;
            return this.symbol.Equals(symbol.symbol);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            List<string> list = new List<string>(base.GetDynamicMemberNames());
            if (this.symbol is IProcessImageAddress)
            {
                list.Add("IndexGroup");
                list.Add("IndexOffset");
            }
            return list;
        }

        public override int GetHashCode() => 
            ((0x5b * 10) + this.symbol.GetHashCode());

        protected virtual object OnReadAnyValue(Type managedType)
        {
            DateTime time;
            object obj2;
            if (!this.HasValue)
            {
                throw new CannotAccessVirtualSymbolException(this);
            }
            IAccessorValueAny valueAccessor = this.ValueAccessor as IAccessorValueAny;
            if (valueAccessor == null)
            {
                throw new ValueAccessorException($"Accessor '{this.ValueAccessor}' doesn't support IValueAnyAccessor", this.ValueAccessor);
            }
            int num = valueAccessor.TryReadAnyValue(this, managedType, out obj2, out time);
            if (num != 0)
            {
                throw new SymbolException($"Cannot read (any) of Symbol '{this.InstancePath}'! Error: {num}", this.symbol);
            }
            return obj2;
        }

        protected virtual byte[] OnReadRawValue(int timeout) => 
            this.symbol.ReadRawValue(timeout);

        protected virtual object OnReadValue(int timeout)
        {
            DateTime time;
            if (!this.HasValue)
            {
                throw new CannotAccessVirtualSymbolException(this);
            }
            IAccessorValue valueAccessor = (IAccessorValue) this.ValueAccessor;
            IAccessorConnection connection = valueAccessor as IAccessorConnection;
            if ((timeout >= 0) && (connection != null))
            {
                using (new AdsTimeoutSetter(connection.Connection, timeout))
                {
                    return valueAccessor.ReadValue(this, out time);
                }
            }
            return valueAccessor.ReadValue(this, out time);
        }

        protected virtual void OnSetInstanceName(string instanceName)
        {
            ((IInstanceInternal) this.symbol).SetInstanceName(instanceName);
        }

        private object OnUpdateAnyValue(object valueObject)
        {
            DateTime time;
            if (!this.HasValue)
            {
                throw new CannotAccessVirtualSymbolException(this);
            }
            IAccessorValueAny valueAccessor = this.ValueAccessor as IAccessorValueAny;
            if (valueAccessor == null)
            {
                throw new ValueAccessorException($"Accessor '{this.ValueAccessor}' doesn't support IValueAnyAccessor", this.ValueAccessor);
            }
            int num = valueAccessor.TryUpdateAnyValue(this, ref valueObject, out time);
            if (num != 0)
            {
                throw new SymbolException($"Cannot read (any) of Symbol '{this.InstancePath}'! Error: {num}", this);
            }
            return valueObject;
        }

        private void OnWriteAnyValue(object managedValue)
        {
            DateTime time;
            if (!this.HasValue)
            {
                throw new CannotAccessVirtualSymbolException(this);
            }
            IAccessorValueAny valueAccessor = this.ValueAccessor as IAccessorValueAny;
            if (valueAccessor == null)
            {
                throw new ValueAccessorException($"Accessor '{this.ValueAccessor}' doesn't support IValueAnyAccessor", this.ValueAccessor);
            }
            int num = valueAccessor.TryWriteAnyValue(this, managedValue, out time);
            if (num != 0)
            {
                throw new SymbolException($"Cannot write (any) of Symbol '{this.InstancePath}'! Error: {num}", this);
            }
        }

        protected virtual void OnWriteRawValue(byte[] rawValue, int timeout)
        {
            this.symbol.WriteRawValue(rawValue, timeout);
        }

        protected virtual void OnWriteValue(object value, int timeout)
        {
            DateTime time;
            if (!this.HasValue)
            {
                throw new CannotAccessVirtualSymbolException(this);
            }
            IAccessorValue valueAccessor = (IAccessorValue) this.ValueAccessor;
            IAccessorConnection connection = valueAccessor as IAccessorConnection;
            if ((timeout >= 0) && (connection != null))
            {
                using (new AdsTimeoutSetter(connection.Connection, timeout))
                {
                    valueAccessor.WriteValue(this, value, out time);
                    return;
                }
            }
            valueAccessor.WriteValue(this, value, out time);
        }

        public static bool operator ==(DynamicSymbol o1, DynamicSymbol o2) => 
            (!object.Equals(o1, null) ? o1.Equals(o2) : object.Equals(o2, null));

        public static bool operator !=(DynamicSymbol o1, DynamicSymbol o2) => 
            !(o1 == o2);

        public object ReadAnyValue(Type managedType) => 
            this.OnReadAnyValue(managedType);

        public byte[] ReadRawValue() => 
            this.OnReadRawValue(-1);

        public byte[] ReadRawValue(int timeout) => 
            this.OnReadRawValue(timeout);

        public object ReadValue() => 
            this.OnReadValue(-1);

        public object ReadValue(int timeout) => 
            this.OnReadValue(timeout);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void SetParent(ISymbol symbol)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (!(symbol is DynamicSymbol))
            {
                throw new ArgumentException("Symbol is not dynamic!", "symbol");
            }
            this.symbol.SetParent(symbol);
        }

        public override string ToString() => 
            this.symbol.ToString();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            IProcessImageAddress symbol = this.symbol as IProcessImageAddress;
            if (symbol != null)
            {
                if (binder.Name == "IndexGroup")
                {
                    result = symbol.IndexGroup;
                    return true;
                }
                if (binder.Name == "IndexOffset")
                {
                    result = symbol.IndexOffset;
                    return true;
                }
            }
            return base.TryGetMember(binder, ref result);
        }

        void IInstanceInternal.SetInstanceName(string instanceName)
        {
            this.OnSetInstanceName(instanceName);
        }

        void ISymbolValueChangeNotify.OnRawValueChanged(RawValueChangedArgs args)
        {
            if (this._rawValueChanged != null)
            {
                this._rawValueChanged(this, args);
            }
        }

        void ISymbolValueChangeNotify.OnValueChanged(ValueChangedArgs args)
        {
            if (this._valueChanged != null)
            {
                this._valueChanged(this, args);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IValueSymbol Unwrap() => 
            this.symbol;

        public void UpdateAnyValue(ref object valueObject)
        {
            valueObject = this.OnUpdateAnyValue(valueObject);
        }

        public void WriteAnyValue(object managedValue)
        {
            this.OnWriteAnyValue(managedValue);
        }

        public void WriteRawValue(byte[] rawValue)
        {
            this.OnWriteRawValue(rawValue, -1);
        }

        public void WriteRawValue(byte[] rawValue, int timeout)
        {
            this.OnWriteRawValue(rawValue, timeout);
        }

        public void WriteValue(object value)
        {
            this.OnWriteValue(value, -1);
        }

        public void WriteValue(object value, int timeout)
        {
            this.OnWriteValue(value, timeout);
        }

        internal Symbol InnerSymbol =>
            ((Symbol) this.symbol);

        public bool HasValue =>
            this.symbol.HasValue;

        public INotificationSettings NotificationSettings
        {
            get => 
                this.symbol.NotificationSettings;
            set => 
                (this.symbol.NotificationSettings = value);
        }

        public DataTypeCategory Category =>
            this.symbol.Category;

        public ISymbol Parent =>
            this.symbol?.Parent;

        public ReadOnlySymbolCollection SubSymbols
        {
            get
            {
                ISymbolCollection subSymbolsInternal = null;
                ISymbolInternal internal2 = this;
                if (internal2 != null)
                {
                    subSymbolsInternal = internal2.SubSymbolsInternal;
                }
                if (subSymbolsInternal == null)
                {
                    subSymbolsInternal = new SymbolCollection();
                }
                return new ReadOnlySymbolCollection(subSymbolsInternal);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ISymbolCollection SubSymbolsInternal
        {
            get
            {
                ISymbolCollection subSymbolsInternal = null;
                ISymbolInternal symbol = this.symbol as ISymbolInternal;
                if (symbol == null)
                {
                    subSymbolsInternal = new SymbolCollection();
                }
                else
                {
                    if (!symbol.SubSymbolsCreated)
                    {
                        subSymbolsInternal = symbol.CreateSubSymbols(this);
                    }
                    if (subSymbolsInternal == null)
                    {
                        subSymbolsInternal = symbol.SubSymbolsInternal;
                    }
                }
                return subSymbolsInternal;
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool SubSymbolsCreated
        {
            get
            {
                ISymbolInternal symbol = this.symbol as ISymbolInternal;
                return ((symbol != null) && symbol.SubSymbolsCreated);
            }
        }

        public string NormalizedName
        {
            get
            {
                if (this.normalizedName == null)
                {
                    ISymbolFactory symbolFactory = this.FactoryServices.SymbolFactory;
                    if (!symbolFactory.HasInvalidCharacters)
                    {
                        this.normalizedName = this.InstanceName;
                    }
                    else
                    {
                        char[] invalidCharacters = symbolFactory.InvalidCharacters;
                        char[] chArray2 = this.InstanceName.ToCharArray();
                        int index = 0;
                        while (true)
                        {
                            if (index >= chArray2.Length)
                            {
                                this.normalizedName = new string(chArray2);
                                break;
                            }
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 < invalidCharacters.Length)
                                {
                                    if (chArray2[index] != invalidCharacters[num2])
                                    {
                                        num2++;
                                        continue;
                                    }
                                    chArray2[index] = '_';
                                }
                                index++;
                                break;
                            }
                        }
                    }
                }
                return this.normalizedName;
            }
        }

        public SymbolAccessRights AccessRights =>
            this.symbol.AccessRights;

        public IDataType DataType =>
            this.symbol.DataType;

        public string TypeName =>
            this.symbol.TypeName;

        public string InstanceName =>
            this.symbol.InstanceName;

        public string InstancePath =>
            this.symbol.InstancePath;

        public int BitSize =>
            this.symbol.BitSize;

        public bool IsContainerType =>
            ((this.DataType == null) ? PrimitiveTypeConverter.IsContainerType(this.Category) : this.DataType.IsContainer);

        public bool IsPrimitiveType =>
            ((this.DataType == null) ? PrimitiveTypeConverter.IsPrimitiveType(this.Category) : this.DataType.IsPrimitive);

        public bool IsPersistent =>
            this.symbol.IsPersistent;

        public bool IsStatic =>
            this.symbol.IsStatic;

        public bool IsReadOnly =>
            this.symbol.IsReadOnly;

        public int Size =>
            this.symbol.Size;

        public int ByteSize =>
            this.symbol.ByteSize;

        public bool IsByteAligned =>
            this.symbol.IsByteAligned;

        public bool IsBitType =>
            this.symbol.IsBitType;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ISymbolFactoryServices FactoryServices =>
            ((ISymbolFactoryServicesProvider) this.symbol).FactoryServices;

        public ReadOnlyTypeAttributeCollection Attributes =>
            this.symbol.Attributes;

        public bool IsReference
        {
            get
            {
                bool flag = false;
                if (this.DataType != null)
                {
                    return this.DataType.IsReference;
                }
                if (!(this is IVirtualStructInstance))
                {
                    flag = DataTypeStringParser.IsReference(this.TypeName);
                }
                return flag;
            }
        }

        internal bool HasReferenceAncestor
        {
            get
            {
                for (DynamicSymbol symbol = this; symbol.Parent != null; symbol = (DynamicSymbol) symbol.Parent)
                {
                    if (symbol.Parent.IsReference)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsPointer
        {
            get
            {
                bool isPointer = false;
                if (this.DataType != null)
                {
                    isPointer = this.DataType.IsPointer;
                }
                else if (!(this is IVirtualStructInstance))
                {
                    isPointer = DataTypeStringParser.IsPointer(this.TypeName);
                }
                return isPointer;
            }
        }

        public string Comment =>
            this.symbol.Comment;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IBinder Binder
        {
            get
            {
                IBinderProvider symbol = this.symbol as IBinderProvider;
                return symbol?.Binder;
            }
        }

        public byte ContextMask
        {
            get
            {
                IContextMaskProvider symbol = this.symbol as IContextMaskProvider;
                return ((symbol == null) ? 0 : symbol.ContextMask);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public IAccessorRawValue ValueAccessor
        {
            get
            {
                IValueAccessorProvider symbol = this.symbol as IValueAccessorProvider;
                return symbol?.ValueAccessor;
            }
        }

        public IConnection Connection
        {
            get
            {
                IAccessorConnection valueAccessor = this.ValueAccessor as IAccessorConnection;
                return valueAccessor?.Connection;
            }
        }

        public bool IsRecursive =>
            this.symbol.IsRecursive;
    }
}

