namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using TwinCAT.Ads;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem.Generic;

    public class Binder : IBinder, IDataTypeResolver, ITypeBinderEvents, ISymbolBinderEvents
    {
        private IInternalSymbolProvider _provider;
        private ISymbolFactory _symbolFactory;
        private int _platformPointerSize = -1;
        private bool _useVirtualInstances;
        [CompilerGenerated]
        private EventHandler<DataTypeEventArgs> TypesGenerated;
        [CompilerGenerated]
        private EventHandler<DataTypeNameEventArgs> TypeResolveError;

        public event EventHandler<DataTypeNameEventArgs> TypeResolveError
        {
            [CompilerGenerated] add
            {
                EventHandler<DataTypeNameEventArgs> typeResolveError = this.TypeResolveError;
                while (true)
                {
                    EventHandler<DataTypeNameEventArgs> a = typeResolveError;
                    EventHandler<DataTypeNameEventArgs> handler3 = (EventHandler<DataTypeNameEventArgs>) Delegate.Combine(a, value);
                    typeResolveError = Interlocked.CompareExchange<EventHandler<DataTypeNameEventArgs>>(ref this.TypeResolveError, handler3, a);
                    if (ReferenceEquals(typeResolveError, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<DataTypeNameEventArgs> typeResolveError = this.TypeResolveError;
                while (true)
                {
                    EventHandler<DataTypeNameEventArgs> source = typeResolveError;
                    EventHandler<DataTypeNameEventArgs> handler3 = (EventHandler<DataTypeNameEventArgs>) Delegate.Remove(source, value);
                    typeResolveError = Interlocked.CompareExchange<EventHandler<DataTypeNameEventArgs>>(ref this.TypeResolveError, handler3, source);
                    if (ReferenceEquals(typeResolveError, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<DataTypeEventArgs> TypesGenerated
        {
            [CompilerGenerated] add
            {
                EventHandler<DataTypeEventArgs> typesGenerated = this.TypesGenerated;
                while (true)
                {
                    EventHandler<DataTypeEventArgs> a = typesGenerated;
                    EventHandler<DataTypeEventArgs> handler3 = (EventHandler<DataTypeEventArgs>) Delegate.Combine(a, value);
                    typesGenerated = Interlocked.CompareExchange<EventHandler<DataTypeEventArgs>>(ref this.TypesGenerated, handler3, a);
                    if (ReferenceEquals(typesGenerated, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<DataTypeEventArgs> typesGenerated = this.TypesGenerated;
                while (true)
                {
                    EventHandler<DataTypeEventArgs> source = typesGenerated;
                    EventHandler<DataTypeEventArgs> handler3 = (EventHandler<DataTypeEventArgs>) Delegate.Remove(source, value);
                    typesGenerated = Interlocked.CompareExchange<EventHandler<DataTypeEventArgs>>(ref this.TypesGenerated, handler3, source);
                    if (ReferenceEquals(typesGenerated, source))
                    {
                        return;
                    }
                }
            }
        }

        protected Binder(IInternalSymbolProvider provider, ISymbolFactory symbolFactory, bool useVirtualInstances)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (symbolFactory == null)
            {
                throw new ArgumentNullException("symbolFactory");
            }
            if ((provider.SymbolsInternal != null) && (provider.SymbolsInternal.Mode != InstanceCollectionMode.PathHierarchy))
            {
                throw new ArgumentException($"Symbol provider has to be in Mode: {InstanceCollectionMode.PathHierarchy}", "provider");
            }
            this._provider = provider;
            this._useVirtualInstances = useVirtualInstances;
            this._symbolFactory = symbolFactory;
        }

        public ISymbol Bind(IHierarchicalSymbol subSymbol)
        {
            string str;
            string str2;
            IHierarchicalSymbol parent = (IHierarchicalSymbol) subSymbol.Parent;
            if (((parent == null) && this.UseVirtualInstances) && SymbolParser.TryParseParentPath(subSymbol, out str2, out str))
            {
                string str3;
                IList<int[]> list;
                SymbolParser.ArrayIndexType type;
                string indicesStr = null;
                bool flag2 = SymbolParser.TryParseArrayElement(str2, out str3, out indicesStr, out list, out type);
                ISymbol symbol = null;
                if (this._provider.SymbolsInternal.TryGetInstanceHierarchically(str2, out symbol))
                {
                    parent = (IHierarchicalSymbol) symbol;
                }
                else if (!flag2)
                {
                    parent = (IHierarchicalSymbol) this._symbolFactory.CreateVirtualStruct(str, str2, null);
                    this.Bind(parent);
                }
                else
                {
                    ISymbol symbol3 = null;
                    if (!this._provider.SymbolsInternal.TryGetInstance(str3, out symbol3))
                    {
                        object[] args = new object[] { subSymbol.InstancePath };
                        Module.Trace.TraceWarning("Cannot bind subSymbol '{0}'", args);
                    }
                    else
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            parent = (IHierarchicalSymbol) this._symbolFactory.CreateArrayElement(list[i], (IArrayInstance) symbol3, (IArrayType) symbol3.DataType);
                            this.Bind(parent);
                        }
                    }
                }
                if (parent != null)
                {
                    subSymbol.SetParent(parent);
                    IVirtualStructInstance instance = parent as IVirtualStructInstance;
                    if (instance != null)
                    {
                        instance.AddMember(subSymbol, instance);
                    }
                }
            }
            if (this._provider.SymbolsInternal.Contains(subSymbol))
            {
                ((IInstanceInternal) subSymbol).SetInstanceName(this.createUniquePathName(subSymbol));
            }
            try
            {
                if (subSymbol.Parent == null)
                {
                    this._provider.SymbolsInternal.Add(subSymbol);
                }
            }
            catch (ArgumentException exception1)
            {
                string message = $"Cannot bind symbol '{subSymbol.InstancePath}' because of double registration. Ignoring symbol!";
                Module.Trace.TraceWarning(message, exception1);
            }
            return parent;
        }

        internal string createUniquePathName(IInstance instance)
        {
            int num = 0;
            string instancePath = instance.InstancePath;
            string instanceSpecifier = instancePath;
            while (this._provider.SymbolsInternal.Contains(instanceSpecifier))
            {
                num++;
                instanceSpecifier = $"{instancePath}_{num}";
            }
            return instanceSpecifier;
        }

        public void OnTypeGenerated(IDataType type)
        {
            if (this.TypesGenerated != null)
            {
                DataTypeCollection<IDataType> types = new DataTypeCollection<IDataType> {
                    type
                };
                DataTypeEventArgs e = new DataTypeEventArgs(types.AsReadOnly());
                this.TypesGenerated(this, e);
            }
        }

        public void OnTypeResolveError(string typeName)
        {
            if (this.TypeResolveError != null)
            {
                DataTypeNameEventArgs e = new DataTypeNameEventArgs(typeName);
                this.TypeResolveError(this, e);
            }
        }

        public void OnTypesGenerated(IEnumerable<IDataType> types)
        {
            if (this.TypesGenerated != null)
            {
                DataTypeEventArgs e = new DataTypeEventArgs(types);
                this.TypesGenerated(this, e);
            }
        }

        public void RegisterType(IDataType type)
        {
            ((IBindable) type).Bind(this);
            ((NamespaceCollection) this._provider.NamespacesInternal).RegisterType(type);
        }

        public void RegisterTypes(IEnumerable<IDataType> types)
        {
            foreach (IDataType type in types)
            {
                this.RegisterType(type);
            }
        }

        internal void SetPlatformPointerSize(int sz)
        {
            this._platformPointerSize = sz;
            object[] args = new object[] { sz };
            Module.Trace.TraceInformation("Platform pointer size -> {0} bytes", args);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public bool TryResolveType(string name, out IDataType type)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException();
            }
            return (this._provider.NamespacesInternal.TryGetType(name, out type) || SymbolParser.TryParseType(name, this, out type));
        }

        public IInternalSymbolProvider Provider =>
            this._provider;

        public int PlatformPointerSize =>
            this._platformPointerSize;

        internal bool UseVirtualInstances =>
            this._useVirtualInstances;
    }
}

