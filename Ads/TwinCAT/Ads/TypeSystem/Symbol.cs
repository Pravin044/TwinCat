namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TwinCAT;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;
    using TwinCAT.TypeSystem.Generic;
    using TwinCAT.ValueAccess;

    [DebuggerDisplay("Path = { instancePath }, Type = {typeName}, Size = {size}, IG = {indexGroup}, IO = {indexOffset}, Category = {category}, Static = {staticAddress}")]
    public class Symbol : Instance, IValueSymbol2, IValueSymbol, IValueRawSymbol, IHierarchicalSymbol, ISymbol, IAttributedInstance, IInstance, IBitSize, IValueAnySymbol, IValueAccessorProvider, ISymbolFactoryServicesProvider, ISymbolValueChangeNotify, IContextMaskProvider, IInstanceInternal, ISymbolInternal, IAdsSymbol, IProcessImageAddress
    {
        private AdsDataTypeFlags _memberFlags;
        private ISymbolFactoryServices factoryServices;
        private const uint TCOMOBJ_MIN_OID = 0x100000;
        private INotificationSettings _notificationSettings;
        protected ISymbol parent;
        protected uint indexGroup;
        protected uint indexOffset;
        protected string instancePath;
        protected AmsAddress imageBaseAddress;
        internal WeakReference subSymbols;
        private EventHandler<RawValueChangedArgs> _rawValueChanged;
        private EventHandler<ValueChangedArgs> _valueChanged;
        protected SymbolAccessRights accessRights;

        public event EventHandler<RawValueChangedArgs> RawValueChanged
        {
            add
            {
                Symbol symbol = this;
                lock (symbol)
                {
                    bool flag2 = false;
                    if ((this._rawValueChanged != null) || (this._rawValueChanged.GetInvocationList().Length == 0))
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
                Symbol symbol = this;
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
                Symbol symbol = this;
                lock (symbol)
                {
                    bool flag2 = false;
                    if ((this._valueChanged == null) || (this._valueChanged.GetInvocationList().Length == 0))
                    {
                        flag2 = true;
                    }
                    this._valueChanged = (EventHandler<ValueChangedArgs>) Delegate.Combine(this._valueChanged, value);
                    if (flag2 && ((this.accessRights & SymbolAccessRights.Read) > SymbolAccessRights.None))
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
                Symbol symbol = this;
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

        internal Symbol(Member member, ISymbol parent)
        {
            this.accessRights = SymbolAccessRights.All;
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            this.factoryServices = ((ISymbolFactoryServicesProvider) parent).FactoryServices;
            this.parent = parent;
            base.Bind(((IBinderProvider) parent).Binder);
            string instanceName = member.InstanceName;
            base.instanceName = instanceName;
            this.instancePath = parent.InstancePath + "." + instanceName;
            base.resolvedDataType = (DataType) member.DataType;
            base.flags = this.getSymbolFlags(parent, member);
            this._memberFlags = member.MemberFlags;
            this.staticAddress = parent.IsStatic || this._memberFlags.HasFlag(AdsDataTypeFlags.None | AdsDataTypeFlags.Static);
            base.attributes = (member.Attributes.Count <= 0) ? null : new TypeAttributeCollection(member.Attributes);
            base.comment = member.Comment;
            if (base.resolvedDataType != null)
            {
                base.size = base.resolvedDataType.Size;
                base.dataTypeId = ((DataType) base.resolvedDataType).DataTypeId;
                base.category = base.resolvedDataType.Category;
                base.typeName = base.resolvedDataType.Name;
            }
            else
            {
                base.size = member.Size;
                base.dataTypeId = member.DataTypeId;
                base.category = member.Category;
                base.typeName = member.TypeName;
            }
            this.calcAccess(parent, member, out this.indexGroup, out this.indexOffset);
            base.SetContextMask(((IContextMaskProvider) parent).ContextMask);
            DataTypeCategory category = base.category;
        }

        internal Symbol(int[] indices, bool oversampleElement, ISymbol parent)
        {
            this.accessRights = SymbolAccessRights.All;
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            this.factoryServices = ((ISymbolFactoryServicesProvider) parent).FactoryServices;
            this.parent = parent;
            base.Bind(((IBinderProvider) parent).Binder);
            string str = string.Empty;
            ArrayType type = (ArrayType) ((DataType) parent.DataType).ResolveType(DataTypeResolveStrategy.AliasReference);
            type.CheckIndices(indices, oversampleElement);
            str = !oversampleElement ? ArrayIndexConverter.IndicesToString(indices) : ArrayIndexConverter.OversamplingSubElementToString(type.Dimensions.ElementCount);
            base.instanceName = parent.InstanceName + str;
            this.instancePath = parent.InstancePath + str;
            DataType elementType = (DataType) type.ElementType;
            base.resolvedDataType = elementType;
            base.flags = this.getSymbolFlags(parent, elementType);
            if (base.resolvedDataType != null)
            {
                base.size = base.resolvedDataType.Size;
                base.dataTypeId = ((DataType) base.resolvedDataType).DataTypeId;
                base.category = base.resolvedDataType.Category;
                base.typeName = base.resolvedDataType.Name;
            }
            else
            {
                base.size = !base.IsBitType ? type.ElementSize : 1;
                base.dataTypeId = type.ElementTypeId;
                base.category = DataTypeCategory.Unknown;
                base.typeName = type.ElementTypeName;
            }
            if (!oversampleElement)
            {
                this.calcAccess(parent, elementType, ArrayType.GetElementOffset(indices, (IArrayType) ((DataType) parent.DataType).ResolveType(DataTypeResolveStrategy.AliasReference)), base.flags, elementType.Flags, out this.indexGroup, out this.indexOffset);
            }
            else
            {
                this.indexGroup = 0xf019;
                this.indexOffset = 0;
            }
            base.SetContextMask(((IContextMaskProvider) parent).ContextMask);
            DataTypeCategory category = base.category;
        }

        internal Symbol(string instanceName, string instancePath, ISymbolFactoryServices factoryServices) : this(0, 0, null, null, instanceName, instancePath, factoryServices)
        {
            DataTypeCategory category = base.category;
        }

        internal Symbol(string instanceName, ISymbol parent, ISymbolFactoryServices factoryServices) : this(0, 0, parent, null, instanceName, null, factoryServices)
        {
            DataTypeCategory category = base.category;
        }

        internal Symbol(AdsSymbolEntry entry, ISymbol parent, ISymbolFactoryServices factoryServices)
        {
            this.accessRights = SymbolAccessRights.All;
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            if (factoryServices == null)
            {
                throw new ArgumentNullException("factoryServices");
            }
            this.factoryServices = factoryServices;
            this.indexGroup = entry.indexGroup;
            this.indexOffset = entry.indexOffset;
            base.comment = entry.comment;
            base.dataTypeId = (AdsDatatypeId) entry.dataType;
            base.category = DataTypeCategory.Unknown;
            this.parent = parent;
            base.instanceName = this.getInstanceName(entry.name);
            this.instancePath = this.getInstancePath(entry.name);
            base.size = (int) entry.size;
            base.typeName = entry.type;
            base.resolvedDataType = null;
            base.flags = entry.flags;
            if (entry.attributeCount > 0)
            {
                base.attributes = new TypeAttributeCollection(entry.attributes);
            }
        }

        internal Symbol(AdsSymbolEntry entry, IDataType type, ISymbol parent, ISymbolFactoryServices factoryServices) : this(entry, parent, factoryServices)
        {
            base.category = type.Category;
            base.resolvedDataType = type;
            if ((base.attributes != null) && (base.attributes.Count >= 2))
            {
                ISubRangeType subRange = null;
                if (TryParseSubRange(type, base.attributes, factoryServices.Binder, out subRange))
                {
                    base.category = subRange.Category;
                    base.resolvedDataType = subRange;
                }
            }
        }

        internal Symbol(ISymbol parent, IDataType type, string instanceName, int fieldOffset, ISymbolFactoryServices factoryServices) : this(0, 0, parent, type, instanceName, null, factoryServices)
        {
            DataType type2 = (DataType) type;
            this.calcAccess(parent, (DataType) type, fieldOffset, DataTypeFlagConverter.Convert(type2.Flags), type2.Flags, out this.indexGroup, out this.indexOffset);
            DataTypeCategory category = base.category;
        }

        private Symbol(uint indexGroup, uint indexOffset, ISymbol parent, IDataType type, string instanceName, string instancePath, ISymbolFactoryServices factoryServices)
        {
            this.accessRights = SymbolAccessRights.All;
            if (factoryServices == null)
            {
                throw new ArgumentNullException("factoryServices");
            }
            if (string.IsNullOrEmpty(instanceName))
            {
                throw new ArgumentOutOfRangeException("instanceName");
            }
            this.factoryServices = factoryServices;
            this.parent = parent;
            if (parent != null)
            {
                base.Bind(((IBinderProvider) parent).Binder);
            }
            this.indexGroup = indexGroup;
            this.indexOffset = indexOffset;
            if (type != null)
            {
                base.dataTypeId = ((DataType) type).DataTypeId;
                base.category = type.Category;
            }
            base.instanceName = instanceName;
            this.instancePath = instancePath;
            if (string.IsNullOrEmpty(this.instancePath))
            {
                if (parent == null)
                {
                    this.instancePath = instanceName;
                }
                else if (type.IsPointer)
                {
                    this.instancePath = parent.InstancePath + "." + instanceName;
                }
                else if (!parent.IsPointer)
                {
                    this.instancePath = !parent.IsReference ? (parent.InstancePath + "." + instanceName) : parent.InstancePath;
                }
                else
                {
                    base.instanceName = parent.InstanceName + "^";
                    this.instancePath = parent.InstancePath + "^";
                }
            }
            base.SetContextMask(0);
            if (parent != null)
            {
                base.SetContextMask(((IContextMaskProvider) parent).ContextMask);
            }
            if (type != null)
            {
                base.flags = DataTypeFlagConverter.Convert(((DataType) type).Flags);
                base.size = type.Size;
                base.typeName = type.Name;
            }
            base.resolvedDataType = type;
        }

        private void calcAccess(ISymbol parent, Member member, out uint indexGroup, out uint indexOffset)
        {
            DataType dataType = (DataType) member.DataType;
            AdsDataTypeFlags memberFlags = member.MemberFlags;
            bool flag = memberFlags.HasFlag(AdsDataTypeFlags.None | AdsDataTypeFlags.Static);
            this.calcAccess(parent, dataType, member.Offset, member.Flags, memberFlags, out indexGroup, out indexOffset);
            if (indexGroup == 0xf017)
            {
                indexOffset = member.TypeHashValue;
            }
        }

        private void calcAccess(ISymbol parent, DataType memberType, int offset, AdsSymbolFlags symbolFlags, AdsDataTypeFlags flags, out uint indexGroup, out uint indexOffset)
        {
            ISymbol symbol = Unwrap(parent);
            bool isBitType = false;
            if (memberType != null)
            {
                isBitType = memberType.IsBitType;
            }
            bool flag2 = flags.HasFlag(AdsDataTypeFlags.None | AdsDataTypeFlags.Static);
            if ((flags & (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem)) == (AdsDataTypeFlags.None | AdsDataTypeFlags.PropItem))
            {
                indexGroup = 0xf017;
                indexOffset = 0;
            }
            else if (flag2)
            {
                indexGroup = 0xf019;
                indexOffset = 0;
            }
            else if (symbol.IsBitType | isBitType)
            {
                this.calcBitAccess(symbol, offset, symbolFlags, flags, out indexGroup, out indexOffset);
            }
            else if (this.IsDereferencedPointer)
            {
                indexGroup = !isBitType ? 0xf014 : 0xf01a;
                indexOffset = 0;
            }
            else if (base.IsReference || this.IsDereferencedReference)
            {
                indexGroup = !isBitType ? 0xf016 : 0xf01b;
                indexOffset = 0;
            }
            else
            {
                indexGroup = ((IAdsSymbol) symbol).IndexGroup;
                indexOffset = ((IAdsSymbol) symbol).IndexOffset + ((uint) offset);
            }
        }

        private void calcBitAccess(ISymbol parent, Member member, out uint indexGroup, out uint indexOffset)
        {
            int offset = member.Offset;
            this.calcBitAccess(parent, offset, member.Flags, ((DataType) member.DataType).Flags, out indexGroup, out indexOffset);
        }

        private void calcBitAccess(ISymbol parent, int bitOffset, AdsSymbolFlags symbolFlags, AdsDataTypeFlags flags, out uint indexGroup, out uint indexOffset)
        {
            ISymbol symbol = Unwrap(parent);
            uint num = ((IAdsSymbol) symbol).IndexGroup;
            uint num2 = ((IAdsSymbol) symbol).IndexOffset;
            if (!this.IsDereferencedPointer)
            {
                if (base.IsReference)
                {
                    goto TR_0002;
                }
                else if (!this.IsDereferencedReference)
                {
                    if (num > 0x4020)
                    {
                        if (num > 0x4040)
                        {
                            if ((num != 0xf020) && (num != 0xf030))
                            {
                                goto TR_0003;
                            }
                        }
                        else if ((num != 0x4030) && (num != 0x4040))
                        {
                            goto TR_0003;
                        }
                        goto TR_0004;
                    }
                    else if (((num == 0x4000) || (num == 0x4010)) || (num == 0x4020))
                    {
                        goto TR_0004;
                    }
                }
                else
                {
                    goto TR_0002;
                }
            }
            else
            {
                num = 0xf01a;
                num2 = 0;
                goto TR_0000;
            }
            goto TR_0003;
        TR_0000:
            indexGroup = num;
            indexOffset = num2;
            return;
        TR_0002:
            num = 0xf01b;
            num2 = 0;
            goto TR_0000;
        TR_0003:
            num2 = (num <= 0x100000) ? (num2 + ((uint) bitOffset)) : calcPidBitAddressing(num2, bitOffset);
            goto TR_0000;
        TR_0004:
            num++;
            num2 = (num2 * 8) + ((uint) bitOffset);
            goto TR_0000;
        }

        private static uint calcPidBitAddressing(uint indexOffset, int offset)
        {
            if (indexOffset <= 0x100000)
            {
                throw new ArgumentOutOfRangeException("indexOffset");
            }
            uint num2 = ((indexOffset & 0xffffff) * 8) + ((uint) offset);
            indexOffset = ((uint) (-1073741824 | (0x3f000000 & (((indexOffset & 0x3f000000) >> 0x18) << 0x18)))) | (0xffffff & num2);
            return indexOffset;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ISymbolCollection CreateSubSymbols(ISymbol parent)
        {
            ISymbolCollection target = null;
            if (this.subSymbols == null)
            {
                this.subSymbols = new WeakReference(this.OnCreateSubSymbols(parent));
            }
            target = (ISymbolCollection) this.subSymbols.Target;
            if (target == null)
            {
                target = this.OnCreateSubSymbols(parent);
                this.subSymbols.Target = target;
            }
            return target;
        }

        protected void EnsureRights(SymbolAccessRights requested)
        {
            if ((this.accessRights & requested) != requested)
            {
                throw new InsufficientAccessRights(this, requested);
            }
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
            Symbol symbol = (Symbol) obj;
            return this.instancePath.Equals(symbol.InstancePath);
        }

        public override int GetHashCode() => 
            ((0x5b * 10) + this.instancePath.GetHashCode());

        private string getInstanceName(string entryName)
        {
            int num = entryName.LastIndexOf('.');
            return ((num < 0) ? entryName : entryName.Substring(num + 1, entryName.Length - (num + 1)));
        }

        private string getInstancePath(string entryName) => 
            entryName;

        private List<ISymbol> getParentList()
        {
            List<ISymbol> list = new List<ISymbol>();
            for (ISymbol symbol = this.Parent; symbol != null; symbol = symbol.Parent)
            {
                list.Add(symbol);
            }
            return list;
        }

        private AdsSymbolFlags getSymbolFlags(ISymbol arrayParent, DataType elementType)
        {
            AdsSymbolFlags none = AdsSymbolFlags.None;
            ArrayType type = (ArrayType) ((DataType) arrayParent.DataType).ResolveType(DataTypeResolveStrategy.AliasReference);
            if (elementType != null)
            {
                none = DataTypeFlagConverter.Convert(elementType.Flags);
            }
            else if (type.ElementTypeId == AdsDatatypeId.ADST_BIT)
            {
                none |= AdsSymbolFlags.BitValue;
            }
            return (none | (((ISymbolFlagProvider) Unwrap(arrayParent)).Flags & (AdsSymbolFlags.None | AdsSymbolFlags.Persistent)));
        }

        private AdsSymbolFlags getSymbolFlags(ISymbol structParent, Field subSymbol) => 
            (subSymbol.Flags | (((ISymbolFlagProvider) Unwrap(structParent)).Flags & (AdsSymbolFlags.None | AdsSymbolFlags.Persistent)));

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        protected override void OnBound(IBinder binder)
        {
            IAdsBinder binder2 = binder as IAdsBinder;
            if (binder2 != null)
            {
                this.imageBaseAddress = binder2.ImageBaseAddress;
            }
        }

        internal virtual ISymbolCollection OnCreateSubSymbols(ISymbol parentSymbol) => 
            new SymbolCollection(InstanceCollectionMode.Names);

        internal virtual int OnGetSubSymbolCount(ISymbol parentSymbol) => 
            (this.IsContainerType ? this.SubSymbols.Count : 0);

        private object OnReadAnyValue(System.Type managedType, int timeout)
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
            int num = 0;
            IAccessorConnection connection = valueAccessor as IAccessorConnection;
            if ((timeout >= 0) && (connection != null))
            {
                using (new AdsTimeoutSetter(connection.Connection, timeout))
                {
                    num = valueAccessor.TryReadAnyValue(this, managedType, out obj2, out time);
                    goto TR_0004;
                }
            }
            num = valueAccessor.TryReadAnyValue(this, managedType, out obj2, out time);
        TR_0004:
            if (num != 0)
            {
                throw new SymbolException($"Cannot read (any) of Symbol '{this.InstancePath}'! Error: {num}", this);
            }
            return obj2;
        }

        protected virtual byte[] OnReadRawValue(int timeout)
        {
            DateTime time;
            if (!this.HasValue)
            {
                return null;
            }
            byte[] buffer = null;
            int num = 0;
            IAccessorConnection valueAccessor = this.ValueAccessor as IAccessorConnection;
            if ((timeout >= 0) && (valueAccessor != null))
            {
                using (new AdsTimeoutSetter(valueAccessor.Connection, timeout))
                {
                    num = this.ValueAccessor.TryReadValue(this, out buffer, out time);
                    goto TR_0003;
                }
            }
            num = this.ValueAccessor.TryReadValue(this, out buffer, out time);
        TR_0003:
            if (num != 0)
            {
                throw new SymbolException($"Cannot read RawValue of Symbol '{this.InstancePath}'! Error: {num}", this);
            }
            return buffer;
        }

        protected virtual object OnReadValue(int timeout)
        {
            DateTime time;
            if (!this.HasValue)
            {
                throw new CannotAccessVirtualSymbolException(this);
            }
            this.EnsureRights(SymbolAccessRights.Read);
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

        protected override void OnSetInstanceName(string instanceName)
        {
            string str = base.instanceName;
            base.instanceName = instanceName;
            int num = this.instancePath.LastIndexOf('.');
            this.instancePath = this.instancePath.Substring(0, num + 1) + instanceName;
        }

        private object OnUpdateAnyValue(object managedObject, int timeout)
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
            int num = 0;
            IAccessorConnection connection = valueAccessor as IAccessorConnection;
            if ((timeout >= 0) && (connection != null))
            {
                using (new AdsTimeoutSetter(connection.Connection, timeout))
                {
                    num = valueAccessor.TryUpdateAnyValue(this, ref managedObject, out time);
                    goto TR_0004;
                }
            }
            num = valueAccessor.TryUpdateAnyValue(this, ref managedObject, out time);
        TR_0004:
            if (num != 0)
            {
                throw new SymbolException($"Cannot read (any) of Symbol '{this.InstancePath}'! Error: {num}", this);
            }
            return managedObject;
        }

        private void OnWriteAnyValue(object managedValue, int timeout)
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
            int num = 0;
            IAccessorConnection connection = valueAccessor as IAccessorConnection;
            if ((timeout >= 0) && (connection != null))
            {
                using (new AdsTimeoutSetter(connection.Connection, timeout))
                {
                    num = valueAccessor.TryWriteAnyValue(this, managedValue, out time);
                    goto TR_0004;
                }
            }
            num = valueAccessor.TryWriteAnyValue(this, managedValue, out time);
        TR_0004:
            if (num != 0)
            {
                throw new SymbolException($"Cannot write (any) of Symbol '{this.InstancePath}'! Error: {num}", this);
            }
        }

        protected virtual void OnWriteRawValue(byte[] value, int timeout)
        {
            DateTime time;
            if (!this.HasValue)
            {
                throw new CannotAccessVirtualSymbolException(this);
            }
            IAccessorConnection valueAccessor = this.ValueAccessor as IAccessorConnection;
            int num = 0;
            if ((timeout >= 0) && (valueAccessor != null))
            {
                using (new AdsTimeoutSetter(valueAccessor.Connection, timeout))
                {
                    num = this.ValueAccessor.TryWriteValue(this, value, 0, out time);
                    goto TR_0003;
                }
            }
            num = this.ValueAccessor.TryWriteValue(this, value, 0, out time);
        TR_0003:
            if (num != 0)
            {
                throw new SymbolException($"Cannot write RawValue of Symbol '{this.InstancePath}'! Error: {num}", this);
            }
        }

        protected virtual void OnWriteValue(object value, int timeout)
        {
            DateTime time;
            if (!this.HasValue)
            {
                throw new CannotAccessVirtualSymbolException(this);
            }
            this.EnsureRights(SymbolAccessRights.Write);
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

        public static bool operator ==(Symbol o1, Symbol o2) => 
            (!Equals(o1, null) ? o1.Equals(o2) : Equals(o2, null));

        public static bool operator !=(Symbol o1, Symbol o2) => 
            !(o1 == o2);

        public object ReadAnyValue(System.Type managedType) => 
            this.OnReadAnyValue(managedType, -1);

        public object ReadAnyValue(System.Type managedType, int timeout) => 
            this.OnReadAnyValue(managedType, timeout);

        public byte[] ReadRawValue() => 
            this.OnReadRawValue(-1);

        public byte[] ReadRawValue(int timeout) => 
            this.OnReadRawValue(-1);

        public object ReadValue() => 
            this.OnReadValue(-1);

        public object ReadValue(int timeout) => 
            this.OnReadValue(timeout);

        public void SetParent(ISymbol parent)
        {
            this.parent = parent;
        }

        public override string ToString()
        {
            string str = null;
            if (!base.IsBitType)
            {
                str = $"{this.InstancePath} (IG: 0x{this.IndexGroup.ToString("x")}, IO: 0x{this.IndexOffset.ToString("x")}, Size: {base.ByteSize} bytes)'";
            }
            else
            {
                str = $"{this.InstancePath} (IG: 0x{this.IndexGroup.ToString("x")}, IO: 0x{this.IndexOffset.ToString("x")}, Size: {this.BitSize} bits)";
            }
            return str;
        }

        private static bool TryParseSubRange(IDataType baseType, TypeAttributeCollection attributes, IBinder binder, out ISubRangeType subRange)
        {
            string str;
            string str2;
            if (((attributes != null) && ((attributes.Count >= 2) && (baseType.Category == DataTypeCategory.Primitive))) && (attributes.TryGetValue("LowerBorder", out str2) & attributes.TryGetValue("UpperBorder", out str)))
            {
                object obj2;
                object obj3;
                IManagedMappableType type = (IManagedMappableType) baseType;
                System.Type managedType = type.ManagedType;
                System.Type type3 = type.ManagedType;
                if (managedType == typeof(byte))
                {
                    type3 = typeof(sbyte);
                }
                else if (managedType == typeof(ushort))
                {
                    type3 = typeof(short);
                }
                else if (managedType == typeof(uint))
                {
                    type3 = typeof(int);
                }
                else if (managedType == typeof(ulong))
                {
                    type3 = typeof(long);
                }
                if (DataTypeStringParser.TryParse(str, type3, out obj3) & DataTypeStringParser.TryParse(str2, type3, out obj2))
                {
                    object obj4;
                    object obj5;
                    if (managedType == type3)
                    {
                        obj4 = obj2;
                        obj5 = obj3;
                    }
                    else
                    {
                        PrimitiveTypeConverter converter = PrimitiveTypeConverter.Default;
                        byte[] data = converter.Marshal(obj2);
                        byte[] buffer2 = converter.Marshal(obj3);
                        converter.UnmarshalPrimitive(managedType, data, 0, data.Length, out obj4);
                        converter.UnmarshalPrimitive(managedType, buffer2, 0, buffer2.Length, out obj5);
                    }
                    string name = $"{baseType.Name} ({obj4}..{obj5})";
                    IDataType type4 = null;
                    if (binder.TryResolveType(name, out type4))
                    {
                        subRange = (ISubRangeType) type4;
                        return true;
                    }
                }
            }
            subRange = null;
            return false;
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

        internal static ISymbol Unwrap(ISymbol symbol)
        {
            IDynamicSymbol symbol3 = symbol as IDynamicSymbol;
            return ((symbol3 == null) ? symbol : symbol3.Unwrap());
        }

        public void UpdateAnyValue(ref object managedObject)
        {
            managedObject = this.OnUpdateAnyValue(managedObject, -1);
        }

        public void UpdateAnyValue(ref object managedObject, int timeout)
        {
            managedObject = this.OnUpdateAnyValue(managedObject, timeout);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void WriteAnyValue(object managedValue)
        {
            this.OnWriteAnyValue(managedValue, -1);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public void WriteAnyValue(object managedValue, int timeout)
        {
            this.OnWriteAnyValue(managedValue, timeout);
        }

        public void WriteRawValue(byte[] value)
        {
            this.OnWriteRawValue(value, -1);
        }

        public void WriteRawValue(byte[] value, int timeout)
        {
            this.OnWriteRawValue(value, timeout);
        }

        public void WriteValue(object value)
        {
            this.OnWriteValue(value, -1);
        }

        public void WriteValue(object value, int timeout)
        {
            this.OnWriteValue(value, timeout);
        }

        internal AdsDataTypeFlags MemberFlags =>
            this._memberFlags;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ISymbolFactoryServices FactoryServices =>
            this.factoryServices;

        public IAccessorRawValue ValueAccessor =>
            (!(this.factoryServices is ISymbolFactoryValueServices) ? null : ((ISymbolFactoryValueServices) this.factoryServices).ValueAccessor);

        public INotificationSettings NotificationSettings
        {
            get
            {
                if (this._notificationSettings != null)
                {
                    return this._notificationSettings;
                }
                INotificationSettings notificationSettings = null;
                IValueSymbol parent = this.Parent as IValueSymbol;
                if (parent != null)
                {
                    notificationSettings = parent.NotificationSettings;
                }
                if (notificationSettings != null)
                {
                    return notificationSettings;
                }
                IAccessorNotification valueAccessor = this.ValueAccessor as IAccessorNotification;
                return ((valueAccessor == null) ? TwinCAT.Ads.NotificationSettings.Default : valueAccessor.DefaultNotificationSettings);
            }
            set => 
                (this._notificationSettings = value);
        }

        public ISymbol Parent =>
            this.parent;

        public uint IndexGroup =>
            this.indexGroup;

        public uint IndexOffset =>
            this.indexOffset;

        [Obsolete("Use the InstancePath / InstanceName Properties instead")]
        public string Name =>
            this.instancePath;

        [Obsolete("Use the TypeName property instead!")]
        public string Type =>
            base.typeName;

        public override string InstancePath =>
            this.instancePath;

        public AmsAddress ImageBaseAddress =>
            this.imageBaseAddress;

        public ReadOnlySymbolCollection SubSymbols =>
            new ReadOnlySymbolCollection(this.SubSymbolsInternal);

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public ISymbolCollection SubSymbolsInternal
        {
            get
            {
                ISymbolCollection target = null;
                if (this.subSymbols == null)
                {
                    this.subSymbols = new WeakReference(this.OnCreateSubSymbols(this));
                }
                target = (ISymbolCollection) this.subSymbols.Target;
                if (target == null)
                {
                    target = this.OnCreateSubSymbols(this);
                    this.subSymbols.Target = target;
                }
                return target;
            }
        }

        public int SubSymbolCount
        {
            get
            {
                ISymbolCollection target = null;
                if (this.subSymbols != null)
                {
                    target = (ISymbolCollection) this.subSymbols.Target;
                }
                return ((target == null) ? this.OnGetSubSymbolCount(this) : target.Count);
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool SubSymbolsCreated =>
            ((this.subSymbols != null) && (this.subSymbols.Target != null));

        public bool IsDereferencedReference
        {
            get
            {
                for (ISymbol symbol = this; symbol.Parent != null; symbol = symbol.Parent)
                {
                    if (symbol.Parent.IsReference)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsDereferencedPointer
        {
            get
            {
                for (ISymbol symbol = this; symbol.Parent != null; symbol = symbol.Parent)
                {
                    if (symbol.Parent.IsPointer)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public SymbolAccessRights AccessRights =>
            this.accessRights;

        public IConnection Connection
        {
            get
            {
                IAccessorConnection valueAccessor = this.ValueAccessor as IAccessorConnection;
                return valueAccessor?.Connection;
            }
        }

        public virtual bool IsPrimitiveType =>
            ((base.DataType == null) ? PrimitiveTypeConverter.IsPrimitiveType(base.category) : base.DataType.IsPrimitive);

        public virtual bool IsContainerType =>
            ((base.DataType == null) ? PrimitiveTypeConverter.IsContainerType(base.category) : base.DataType.IsContainer);

        public bool IsRecursive
        {
            get
            {
                List<ISymbol> list = this.getParentList();
                list.Insert(0, this);
                int num = 0;
                while (num < (list.Count - 1))
                {
                    ISymbol symbol = list[num];
                    int num2 = num + 1;
                    while (true)
                    {
                        if (num2 >= list.Count)
                        {
                            num++;
                            break;
                        }
                        ISymbol symbol2 = list[num2];
                        if (ReferenceEquals(symbol.DataType, symbol2.DataType))
                        {
                            return true;
                        }
                        num2++;
                    }
                }
                return false;
            }
        }
    }
}

