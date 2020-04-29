namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dynamic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.ValueAccess;

    public class DynamicValue : DynamicObject, IValue, IStructValue, IArrayValue
    {
        private static Dictionary<string, string> s_staticProperties = null;
        private static Dictionary<string, string> s_staticMethods = null;
        private static Dictionary<string, string> s_propertyCollisions = new Dictionary<string, string>();
        private static Dictionary<string, string> s_methodCollisions = new Dictionary<string, string>();
        protected IAccessorValueFactory valueFactory;
        private DateTime _readUtcTimeStamp;
        protected IDynamicSymbol _symbol;
        private ValueUpdateMode _mode;
        private DynamicValue _parentValue;
        internal byte[] cachedData;
        internal int cachedDataOffset;

        internal DynamicValue(IDynamicSymbol symbol, byte[] data, int offset, DynamicValue parentValue) : this(symbol, data, offset, DateTime.MinValue, parentValue.valueFactory)
        {
            if (parentValue == null)
            {
                throw new ArgumentNullException("parentValue");
            }
            this._parentValue = parentValue;
            this._readUtcTimeStamp = this._parentValue._readUtcTimeStamp;
        }

        internal DynamicValue(IDynamicSymbol symbol, byte[] data, int offset, DateTime timeStamp, IAccessorValueFactory factory)
        {
            this._readUtcTimeStamp = DateTime.MinValue;
            this._mode = ValueUpdateMode.Immediately;
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            this.valueFactory = factory;
            this._symbol = symbol;
            this._parentValue = null;
            this.cachedData = data;
            this.cachedDataOffset = offset;
            this._readUtcTimeStamp = timeStamp;
        }

        private static List<string> createMemberNames(IStructType str)
        {
            List<string> list = new List<string>();
            foreach (IMember member in str.AllMembers)
            {
                string instanceName = member.InstanceName;
                if (!CollidingMethods.ContainsKey(instanceName))
                {
                    list.Add(instanceName);
                    continue;
                }
                list.Add(instanceName + "_1");
                s_propertyCollisions.Add(instanceName, instanceName + "_1");
            }
            if (str.HasRpcMethods)
            {
                foreach (IRpcMethod method in ((IRpcCallableType) str).RpcMethods)
                {
                    string name = method.Name;
                    if (!CollidingMethods.ContainsKey(name))
                    {
                        list.Add(name);
                        continue;
                    }
                    list.Add(name + "_1");
                    s_methodCollisions.Add(name, name + "_1");
                }
            }
            return list;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            List<string> list = new List<string>();
            IDataType dataType = this._symbol.DataType;
            if (this._symbol.Category == DataTypeCategory.Reference)
            {
                dataType = ((IResolvableType) this._symbol.DataType).ResolveType(DataTypeResolveStrategy.AliasReference);
            }
            if (dataType.Category == DataTypeCategory.Struct)
            {
                list = createMemberNames((IStructType) dataType);
            }
            return list;
        }

        public void Read()
        {
            DateTime time;
            byte[] buffer = null;
            int errorCode = this.ValueAccessor.TryReadValue(this._symbol, out buffer, out time);
            if (errorCode != 0)
            {
                throw new SymbolException(this.Symbol, errorCode);
            }
            Array.Copy(buffer, 0, this.cachedData, this.cachedDataOffset, this._symbol.Size);
            this._readUtcTimeStamp = time;
        }

        protected internal virtual object ReadMember(ISymbol memberInstance)
        {
            object obj2 = null;
            IMember member = ((IStructType) this.ResolvedType).Members[memberInstance.InstanceName];
            IDataType dataType = member.DataType;
            obj2 = !memberInstance.IsReference ? this.valueFactory.CreateValue(memberInstance, this.cachedData, this.cachedDataOffset + member.Offset, this) : this.valueFactory.CreateValue(memberInstance, new byte[memberInstance.GetValueMarshalSize()], 0, this);
            return this.valueFactory.CreateValue(memberInstance, this.cachedData, this.cachedDataOffset + member.Offset, this);
        }

        public object ResolveValue(bool resolveEnumToPrimitive)
        {
            object obj2 = null;
            if (!this.TryResolveValue(resolveEnumToPrimitive, out obj2))
            {
                throw new AdsException($"Cannot resolve type '{this.DataType.Name}' to primitive value!");
            }
            return obj2;
        }

        public override string ToString()
        {
            IDataType resolvedType = this.ResolvedType;
            if (resolvedType.Category == DataTypeCategory.Struct)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("...");
                return builder.ToString();
            }
            if (resolvedType.Category == DataTypeCategory.Array)
            {
                StringBuilder builder2 = new StringBuilder();
                builder2.Append("...");
                return builder2.ToString();
            }
            if (resolvedType.IsPrimitive)
            {
                return this.ResolveValue(false).ToString();
            }
            return "[INVALID]";
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;
            if (!this._symbol.IsPrimitiveType)
            {
                return base.TryConvert(binder, ref result);
            }
            object obj2 = this.valueFactory.CreatePrimitiveValue(this._symbol, this.cachedData, this.cachedDataOffset);
            try
            {
                result = Convert.ChangeType(obj2, binder.get_Type());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetArrayElementValues(out IEnumerable<object> elementValues)
        {
            if (this.ResolvedType.Category != DataTypeCategory.Array)
            {
                elementValues = null;
                return false;
            }
            ArrayElementValueIterator iterator = new ArrayElementValueIterator(this);
            elementValues = iterator;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) => 
            (!this.TryGetIndexValue(indexes, out result) ? base.TryGetIndex(binder, indexes, ref result) : true);

        public bool TryGetIndexValue(int[] indices, out object value)
        {
            IArrayInstance instance = (IArrayInstance) this._symbol;
            IArrayType dataType = (IArrayType) instance.DataType;
            IDataType elementType = dataType.ElementType;
            ArrayType.CheckIndices(indices, dataType, false);
            int elementPosition = ArrayType.GetElementPosition(indices, dataType);
            ISymbol symbol = instance.SubSymbols[elementPosition];
            int byteSize = symbol.ByteSize;
            if (byteSize <= 0)
            {
                throw new MarshalException($"Cannot determine size of array type '{dataType.Name}' element type '{elementType.Name}'");
            }
            int offset = this.cachedDataOffset + (elementPosition * byteSize);
            value = this.valueFactory.CreateValue(symbol, this.cachedData, offset, this);
            return true;
        }

        public bool TryGetIndexValue(object[] indexes, out object result)
        {
            bool flag = false;
            result = null;
            if (this._symbol.DataType.Category == DataTypeCategory.Array)
            {
                IDataType elementType = ((IArrayType) ((IArrayInstance) this._symbol).DataType).ElementType;
                int[] indices = new int[indexes.GetLength(0)];
                int index = 0;
                while (true)
                {
                    if (index >= indexes.GetLength(0))
                    {
                        flag = this.TryGetIndexValue(indices, out result);
                        break;
                    }
                    indices[index] = (int) indexes[index];
                    index++;
                }
            }
            return flag;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) => 
            (!this.TryGetMemberValue(binder.Name, out result) ? base.TryGetMember(binder, ref result) : true);

        public virtual bool TryGetMemberValue(string name, out object result)
        {
            bool flag = false;
            result = null;
            if (this.ResolvedType.Category == DataTypeCategory.Struct)
            {
                IList<ISymbol> symbols = null;
                if (CollidingProperties.ContainsKey(name))
                {
                    name = s_propertyCollisions[name];
                }
                if (this._symbol.SubSymbols.TryGetInstanceByName(name, out symbols))
                {
                    if (symbols.Count != 1)
                    {
                        throw new SymbolException($"Struct Instance members mismatch in StructInstance '{this.Symbol}'!", this.Symbol);
                    }
                    ISymbol memberInstance = symbols[0];
                    result = this.ReadMember(memberInstance);
                    flag = true;
                }
            }
            return flag;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result) => 
            base.TryInvoke(binder, args, ref result);

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if ((this._symbol.Category != DataTypeCategory.Struct) || !(this._symbol is IRpcCallableInstance))
            {
                return base.TryInvokeMember(binder, args, ref result);
            }
            IRpcCallableInstance instance = (IRpcCallableInstance) this._symbol;
            string name = binder.Name;
            if (CollidingMethods.ContainsKey(binder.Name))
            {
                name = CollidingMethods[binder.Name];
            }
            return (instance.TryInvokeRpcMethod(name, args, out result) == 0);
        }

        public bool TryResolveValue(bool resolveEnumToPrimitive, out object value)
        {
            IDataType resolvedType = this.ResolvedType;
            if (resolvedType.Category == DataTypeCategory.Enum)
            {
                IEnumValue value2 = EnumValueFactory.Create((IEnumType) resolvedType, this.CachedRaw, this.cachedDataOffset);
                value = !resolveEnumToPrimitive ? value2 : value2.Primitive;
                return true;
            }
            if (resolvedType.IsPrimitive)
            {
                new InstanceValueConverter().Unmarshal(this.Symbol, this.cachedData, this.cachedDataOffset, out value);
                return true;
            }
            value = null;
            return false;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) => 
            (!this.TrySetIndexValue(indexes, value) ? base.TrySetIndex(binder, indexes, value) : true);

        public bool TrySetIndexValue(object[] indexes, object value)
        {
            if (this._symbol.DataType.Category != DataTypeCategory.Array)
            {
                return false;
            }
            IDataType elementType = ((IArrayType) ((IArrayInstance) this._symbol).DataType).ElementType;
            int[] indices = new int[indexes.GetLength(0)];
            for (int i = 0; i < indexes.GetLength(0); i++)
            {
                indices[i] = (int) indexes[i];
            }
            this.WriteArrayElementCached(indices, value);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) => 
            (!this.TrySetMemberValue(binder.Name, value) ? base.TrySetMember(binder, value) : true);

        public bool TrySetMemberValue(string name, object value)
        {
            bool flag = false;
            if ((this._symbol.DataType.Category == DataTypeCategory.Struct) && (this._symbol.Category == DataTypeCategory.Struct))
            {
                if (CollidingProperties.ContainsKey(name))
                {
                    name = s_propertyCollisions[name];
                }
                IStructInstance instance = (IStructInstance) this._symbol;
                string instanceName = $"{instance.InstancePath}.{name}";
                IList<ISymbol> symbols = null;
                if (instance.MemberInstances.TryGetInstanceByName(instanceName, out symbols))
                {
                    ISymbol memberInstance = symbols[0];
                    flag = true;
                    if (memberInstance.IsPrimitiveType)
                    {
                        this.WriteMember(memberInstance, value);
                    }
                }
            }
            return flag;
        }

        public void Write()
        {
            DateTime time;
            int errorCode = this.ValueAccessor.TryWriteValue(this, out time);
            if (errorCode != 0)
            {
                throw new SymbolException(this.Symbol, errorCode);
            }
            this._readUtcTimeStamp = time;
        }

        private void WriteArrayElementCached(int[] indices, object value)
        {
            IArrayInstance instance = (IArrayInstance) this._symbol;
            IArrayType dataType = (IArrayType) instance.DataType;
            ArrayType.CheckIndices(indices, dataType, false);
            int elementPosition = ArrayType.GetElementPosition(indices, dataType);
            ISymbol symbol = instance.SubSymbols[elementPosition];
            if (symbol.IsPrimitiveType)
            {
                int byteSize = symbol.ByteSize;
                int destinationIndex = (elementPosition * byteSize) + this.cachedDataOffset;
                Array.Copy(new InstanceValueConverter().Marshal(symbol, value), 0, this.cachedData, destinationIndex, byteSize);
            }
        }

        protected virtual void WriteMember(ISymbol memberInstance, object value)
        {
            IMember member = ((IStructType) this.ResolvedType).Members[memberInstance.InstanceName];
            IDataType dataType = member.DataType;
            if (dataType.IsPrimitive)
            {
                Array.Copy(new DataTypeMarshaller().Marshal(dataType, null, value), 0, this.cachedData, this.cachedDataOffset + member.Offset, memberInstance.Size);
            }
            else
            {
                DynamicValue value2 = value as DynamicValue;
                if (value2 == null)
                {
                    throw new AdsException();
                }
                byte[] cachedData = value2.cachedData;
                Array.Copy(value2.cachedData, 0, this.cachedData, this.cachedDataOffset + member.Offset, memberInstance.Size);
            }
            if (this._mode == ValueUpdateMode.Immediately)
            {
                this.RootValue.Write();
            }
        }

        public DateTime UtcTimeStamp =>
            this._readUtcTimeStamp;

        public ISymbol Symbol =>
            this._symbol;

        private static Dictionary<string, string> CollidingProperties
        {
            get
            {
                if (s_staticProperties == null)
                {
                    s_staticProperties = new Dictionary<string, string>();
                    foreach (PropertyInfo info in typeof(DynamicValue).GetProperties())
                    {
                        s_staticProperties.Add(info.Name, info.Name);
                    }
                }
                return s_staticProperties;
            }
        }

        private static Dictionary<string, string> CollidingMethods
        {
            get
            {
                if (s_staticMethods == null)
                {
                    s_staticMethods = new Dictionary<string, string>();
                    foreach (MethodInfo info in typeof(DynamicValue).GetMethods())
                    {
                        if (!s_staticMethods.ContainsKey(info.Name))
                        {
                            s_staticMethods.Add(info.Name, info.Name);
                        }
                    }
                }
                return s_staticMethods;
            }
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public DynamicValue RootValue
        {
            get
            {
                DynamicValue value2 = null;
                for (DynamicValue value3 = this; value3 != null; value3 = value3._parentValue)
                {
                    value2 = value3;
                }
                return value2;
            }
        }

        public ValueUpdateMode UpdateMode =>
            this._mode;

        private IAccessorDynamicValue ValueAccessor =>
            ((IAccessorDynamicValue) ((IValueAccessorProvider) this._symbol).ValueAccessor);

        public TimeSpan Age =>
            ((TimeSpan) (DateTime.UtcNow - this.UtcTimeStamp));

        public IDataType DataType =>
            this._symbol.DataType;

        public byte[] CachedRaw
        {
            get
            {
                byte[] destinationArray = null;
                int byteSize = this.ResolvedType.ByteSize;
                if (byteSize >= 0)
                {
                    destinationArray = new byte[byteSize];
                    Array.Copy(this.cachedData, this.cachedDataOffset, destinationArray, 0, byteSize);
                }
                else
                {
                    byteSize = this.cachedData.Length - this.cachedDataOffset;
                    destinationArray = new byte[byteSize];
                    Array.Copy(this.cachedData, this.cachedDataOffset, destinationArray, 0, byteSize);
                }
                return destinationArray;
            }
        }

        public bool IsPrimitive =>
            this.ResolvedType.IsPrimitive;

        protected IDataType ResolvedType
        {
            get
            {
                IDataType dataType = this.DataType;
                IResolvableType type2 = this.DataType as IResolvableType;
                if (type2 != null)
                {
                    dataType = type2.ResolveType(DataTypeResolveStrategy.AliasReference);
                }
                return dataType;
            }
        }
    }
}

