namespace TwinCAT.ValueAccess
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class DynamicValueAccessor : RpcNotificationAccessorBase, IAccessorDynamicValue, IAccessorValue, IAccessorRawValue
    {
        private ValueCreationMode _mode;
        private IAccessorValue _inner;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public DynamicValueAccessor(IAccessorValue inner, IAccessorValueFactory factory, ValueCreationMode mode) : base(factory, ((ValueAccessor) inner).Connection)
        {
            this._mode = ValueCreationMode.Default;
            this._mode = mode;
            this._inner = inner;
            IAccessorNotification notification = inner as IAccessorNotification;
            if (notification != null)
            {
                base._notificationSettings = notification.DefaultNotificationSettings;
            }
        }

        public override void OnRegisterNotification(ISymbol symbol, SymbolNotificationType type, INotificationSettings settings)
        {
            IAccessorNotification notification = this._inner as IAccessorNotification;
            if (this._inner == null)
            {
                throw new ValueAccessorException($"Accessor '{this._inner}' doesn't support INotificationAccessor", this._inner);
            }
            notification.OnRegisterNotification(symbol, type, settings);
        }

        public override void OnUnregisterNotification(ISymbol symbol, SymbolNotificationType type)
        {
            IAccessorNotification notification = this._inner as IAccessorNotification;
            if (this._inner == null)
            {
                throw new ValueAccessorException($"Accessor '{this._inner}' doesn't support INotificationAccessor", this._inner);
            }
            notification.OnUnregisterNotification(symbol, type);
        }

        public object ReadArrayElement(IArrayInstance symbol, int[] indices, out DateTime utcReadTime)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (indices.Length == 0)
            {
                throw new ArgumentOutOfRangeException("indices");
            }
            int num = 0;
            object result = null;
            string str = string.Join<int>(",", indices);
            num = this.tryReadValue(symbol, indices, out result, out utcReadTime);
            if (num == 0)
            {
                return true;
            }
            Exception ex = new SymbolException($"Could not read ArrayInstance '{symbol.InstancePath}[{str}]'! Error: {num}", symbol);
            Module.Trace.TraceError(ex);
            throw ex;
        }

        public override int TryInvokeRpcMethod(IInstance instance, IRpcMethod method, object[] parameters, out object returnValue, out DateTime utcInvokeTime)
        {
            IAccessorRpc rpc = this._inner as IAccessorRpc;
            if (rpc != null)
            {
                return rpc.TryInvokeRpcMethod(instance, method, parameters, out returnValue, out utcInvokeTime);
            }
            returnValue = null;
            utcInvokeTime = DateTime.MinValue;
            return 0x701;
        }

        public override int TryReadArrayElementValue(ISymbol arrayInstance, int[] indices, out byte[] value, out DateTime utcReadTime) => 
            this._inner.TryReadArrayElementValue(arrayInstance, indices, out value, out utcReadTime);

        private int tryReadValue(ISymbol symbol, int[] indices, out object result, out DateTime utcReadTime)
        {
            IDataType dataType = symbol.DataType;
            IResolvableType type2 = dataType as IResolvableType;
            IArrayType type3 = null;
            type3 = (type2 == null) ? ((IArrayType) dataType) : ((IArrayType) type2.ResolveType(DataTypeResolveStrategy.AliasReference));
            IDataType elementType = type3.ElementType;
            byte[] buffer = null;
            int num = this.TryReadArrayElementValue(symbol, indices, out buffer, out utcReadTime);
            result = (num != 0) ? null : base.valueFactory.CreateValue(symbol, buffer, 0, utcReadTime);
            return num;
        }

        public override int TryReadValue(ISymbol symbolInstance, out byte[] value, out DateTime utcReadTime) => 
            this._inner.TryReadValue(symbolInstance, out value, out utcReadTime);

        public override int TryWriteArrayElementValue(ISymbol arrayInstance, int[] indices, byte[] value, int offset, out DateTime utcWriteTime) => 
            this._inner.TryWriteArrayElementValue(arrayInstance, indices, value, offset, out utcWriteTime);

        private int tryWriteValue(ISymbol symbol, byte[] value, int offset, out DateTime utcWriteTime)
        {
            if (symbol.IsBitType)
            {
                if (symbol.ByteSize <= (value.Length - offset))
                {
                    return this.TryWriteValue(symbol, value, offset, out utcWriteTime);
                }
                utcWriteTime = DateTime.MinValue;
                return 0x705;
            }
            if (symbol.Size >= (value.Length - offset))
            {
                return this.TryWriteValue(symbol, value, offset, out utcWriteTime);
            }
            utcWriteTime = DateTime.MinValue;
            return 0x705;
        }

        public int TryWriteValue(DynamicValue value, out DateTime utcWriteTime)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return this.TryWriteValue(value.Symbol, value.cachedData, value.cachedDataOffset, out utcWriteTime);
        }

        public override int TryWriteValue(ISymbol symbolInstance, byte[] value, int offset, out DateTime utcWriteTime) => 
            this._inner.TryWriteValue(symbolInstance, value, offset, out utcWriteTime);

        private void WriteArrayElement(IArrayInstance arrInstance, int[] indices, object value, out DateTime utcWriteTime)
        {
            if (arrInstance == null)
            {
                throw new ArgumentNullException("arrInstance");
            }
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            IArrayType dataType = (IArrayType) arrInstance.DataType;
            IDataType elementType = dataType.ElementType;
            ArrayType.CheckIndices(indices, dataType, false);
            int elementPosition = ArrayType.GetElementPosition(indices, dataType);
            ISymbol symbol = arrInstance.SubSymbols[elementPosition];
            this.WriteValue(symbol, value, out utcWriteTime);
        }

        public override void WriteValue(ISymbol symbol, object value, out DateTime utcWriteTime)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            int num = 0;
            ISymbol symbol2 = symbol;
            IDataType dataType = symbol2.DataType;
            IResolvableType type2 = dataType as IResolvableType;
            if (type2 != null)
            {
                dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            utcWriteTime = DateTime.MinValue;
            if (dataType.IsBitType)
            {
                num = this.tryWriteValue(symbol2, BitTypeConverter.Marshal(dataType.BitSize, value), 0, out utcWriteTime);
            }
            else if (!symbol2.IsPrimitiveType)
            {
                DynamicValue value2 = (DynamicValue) value;
                num = this.TryWriteValue(value2, out utcWriteTime);
            }
            else if (((dataType.Category == DataTypeCategory.Primitive) || (dataType.Category == DataTypeCategory.SubRange)) || (dataType.Category == DataTypeCategory.Pointer))
            {
                num = this.tryWriteValue(symbol2, PrimitiveTypeConverter.Default.Marshal(symbol2.DataType, value), 0, out utcWriteTime);
            }
            else if (dataType.Category == DataTypeCategory.String)
            {
                IStringType type3 = (IStringType) symbol2.DataType;
                byte[] buffer3 = new PrimitiveTypeConverter(type3.Encoding).Marshal(symbol2.DataType, value);
                if (buffer3.Length >= type3.Size)
                {
                    Exception ex = new AdsErrorException($"String is too large for symbol '{symbol2.InstancePath}' (Type: {symbol2.TypeName}).", AdsErrorCode.DeviceInvalidSize);
                    Module.Trace.TraceError(ex);
                    throw ex;
                }
                num = this.tryWriteValue(symbol2, buffer3, 0, out utcWriteTime);
            }
            else if (dataType.Category == DataTypeCategory.Array)
            {
                IArrayType type4 = (IArrayType) symbol2.DataType;
                num = this.tryWriteValue(symbol2, PrimitiveTypeConverter.Default.Marshal(symbol2.DataType, value), 0, out utcWriteTime);
            }
            else
            {
                if (dataType.Category != DataTypeCategory.Enum)
                {
                    Exception ex = new SymbolException($"Could not write Symbol '{symbol2.InstancePath}' (Type: {symbol2.TypeName}). Category mismatch!", symbol2);
                    Module.Trace.TraceError(ex);
                    throw ex;
                }
                IEnumType type5 = (IEnumType) symbol2.DataType;
                num = this.tryWriteValue(symbol2, PrimitiveTypeConverter.Default.Marshal(symbol2.DataType, value), 0, out utcWriteTime);
            }
            if (num != 0)
            {
                Exception ex = new SymbolException($"Could not write Symbol '{symbol2.InstancePath}' (Type: {symbol2.TypeName})! Error: {num}", symbol2);
                Module.Trace.TraceError(ex);
                throw ex;
            }
        }
    }
}

