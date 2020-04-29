namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem.Generic;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class DynamicSymbolFactory : ISymbolFactory, ISymbolFactoryOversampled
    {
        private bool nonCachedArrayElements;
        public static char[] DefaultInvalidChars = new char[] { '^', ' ', '(', ')', '-' };
        private ISymbolFactory inner;

        public DynamicSymbolFactory(ISymbolFactory inner, bool nonCachedArrayElements)
        {
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            this.inner = inner;
            this.nonCachedArrayElements = nonCachedArrayElements;
            this.inner.SetInvalidCharacters(DefaultInvalidChars);
        }

        private IValueSymbol create(IValueSymbol symbol)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            DataTypeCategory category = symbol.Category;
            switch (category)
            {
                case DataTypeCategory.Alias:
                {
                    IAliasInstance aliasInstance = symbol as IAliasInstance;
                    if (aliasInstance != null)
                    {
                        return new DynamicAliasInstance(aliasInstance);
                    }
                    Module.Trace.TraceWarning($"'{symbol.InstancePath}' cannot be resolved to alias");
                    return new DynamicSymbol(symbol);
                }
                case DataTypeCategory.Enum:
                    break;

                case DataTypeCategory.Array:
                    if (symbol is IArrayInstance)
                    {
                        return (!(symbol is IOversamplingArrayInstance) ? new DynamicArrayInstance((IArrayInstance) symbol) : new DynamicOversamplingArrayInstance((IOversamplingArrayInstance) symbol));
                    }
                    Module.Trace.TraceWarning($"'{symbol.InstancePath}' cannot be resolved to array");
                    return new DynamicSymbol(symbol);

                case DataTypeCategory.Struct:
                {
                    IStructInstance instance3 = symbol as IStructInstance;
                    if (instance3 != null)
                    {
                        return (!instance3.HasRpcMethods ? new DynamicStructInstance((IStructInstance) symbol) : new DynamicRpcStructInstance((IRpcStructInstance) symbol));
                    }
                    Module.Trace.TraceWarning($"'{symbol.InstancePath}' cannot be resolved to struct");
                    return new DynamicSymbol(symbol);
                }
                default:
                    switch (category)
                    {
                        case DataTypeCategory.Pointer:
                        {
                            IPointerInstance pointerInstance = symbol as IPointerInstance;
                            if (pointerInstance != null)
                            {
                                return new DynamicPointerInstance(pointerInstance);
                            }
                            Module.Trace.TraceWarning($"'{symbol.InstancePath}' cannot be resolved to pointer");
                            return new DynamicSymbol(symbol);
                        }
                        case DataTypeCategory.Union:
                        {
                            IUnionInstance unionInstance = symbol as IUnionInstance;
                            if (unionInstance != null)
                            {
                                return new DynamicUnionInstance(unionInstance);
                            }
                            Module.Trace.TraceWarning($"'{symbol.InstancePath}' cannot be resolved to union");
                            return new DynamicSymbol(symbol);
                        }
                        case DataTypeCategory.Reference:
                        {
                            IReferenceInstance refInstance = symbol as IReferenceInstance;
                            if (refInstance != null)
                            {
                                return new DynamicReferenceInstance(refInstance);
                            }
                            Module.Trace.TraceWarning($"'{symbol.InstancePath}' cannot be resolved to reference");
                            return new DynamicSymbol(symbol);
                        }
                        default:
                            break;
                    }
                    break;
            }
            return new DynamicSymbol(symbol);
        }

        public ISymbol CreateArrayElement(int[] currentIndex, ISymbol parent, IArrayType arrayType)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            IValueSymbol symbol = (IValueSymbol) this.inner.CreateArrayElement(currentIndex, parent, arrayType);
            ISymbol symbol2 = null;
            if (symbol != null)
            {
                symbol2 = this.create(symbol);
            }
            return symbol2;
        }

        public ISymbolCollection CreateArrayElementInstances(ISymbol parentInstance, IArrayType arrayType)
        {
            ISymbolCollection symbols = null;
            if (this.nonCachedArrayElements)
            {
                symbols = new ArrayElementSymbolCollection(parentInstance, arrayType, this);
            }
            else
            {
                symbols = new SymbolCollection(InstanceCollectionMode.Names);
                foreach (int[] numArray in new ArrayIndexIterator(arrayType))
                {
                    ISymbol item = this.CreateArrayElement(numArray, parentInstance, arrayType);
                    symbols.Add(item);
                }
                if (parentInstance is IOversamplingArrayInstance)
                {
                    symbols.Add(this.CreateOversamplingElement(parentInstance));
                }
            }
            return symbols;
        }

        public ISymbol CreateFieldInstance(IField field, ISymbol parent)
        {
            IValueSymbol symbol = (IValueSymbol) this.inner.CreateFieldInstance(field, parent);
            ISymbol symbol2 = null;
            if (symbol != null)
            {
                symbol2 = this.create(symbol);
            }
            return symbol2;
        }

        public ISymbolCollection CreateFieldInstances(ISymbol parentInstance, IDataType parentType)
        {
            if (parentInstance == null)
            {
                throw new ArgumentNullException("parentInstance");
            }
            if (parentType == null)
            {
                throw new ArgumentNullException("structType");
            }
            return this.OnCreateFieldInstances(parentInstance, parentType);
        }

        public ISymbol CreateInstance(ISymbolInfo entry, ISymbol parent)
        {
            IValueSymbol symbol = (IValueSymbol) this.inner.CreateInstance(entry, parent);
            return this.WrapSymbol(symbol);
        }

        public ISymbol CreateOversamplingElement(ISymbol parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            IValueSymbol symbol = (IValueSymbol) ((ISymbolFactoryOversampled) this.inner).CreateOversamplingElement(parent);
            ISymbol symbol2 = null;
            if (symbol != null)
            {
                symbol2 = this.create(symbol);
            }
            return symbol2;
        }

        public ISymbol CreateReferenceInstance(IPointerType type, ISymbol parent)
        {
            IValueSymbol symbol = (IValueSymbol) this.inner.CreateReferenceInstance(type, ((DynamicSymbol) parent).InnerSymbol);
            ISymbol symbol2 = null;
            if (symbol != null)
            {
                symbol2 = this.create(symbol);
            }
            return symbol2;
        }

        public ISymbol CreateVirtualStruct(string instanceName, string instancePath, ISymbol parent) => 
            new DynamicVirtualStructInstance((IVirtualStructInstance) this.inner.CreateVirtualStruct(instanceName, instancePath, parent));

        public void Initialize(ISymbolFactoryServices services)
        {
            this.inner.Initialize(services);
        }

        protected virtual ISymbolCollection OnCreateFieldInstances(ISymbol parentInstance, IDataType parentType)
        {
            SymbolCollection symbols = new SymbolCollection(InstanceCollectionMode.Names);
            IEnumerable<IField> members = null;
            if (parentType.Category == DataTypeCategory.Struct)
            {
                members = (IEnumerable<IField>) ((IStructType) parentType).Members;
            }
            else if (parentType.Category == DataTypeCategory.Union)
            {
                members = ((IUnionType) parentType).Fields;
            }
            if (members != null)
            {
                foreach (IField field in members)
                {
                    ISymbol item = null;
                    item = this.CreateFieldInstance(field, parentInstance);
                    if (item != null)
                    {
                        symbols.Add(item);
                        continue;
                    }
                    object[] args = new object[] { field.InstanceName, parentInstance.InstancePath };
                    Module.Trace.TraceWarning("Couldn't create field '{0}' for instance '{1}'!", args);
                }
            }
            return symbols;
        }

        public void SetInvalidCharacters(char[] invalidChars)
        {
            this.inner.SetInvalidCharacters(invalidChars);
        }

        public IValueSymbol WrapSymbol(IValueSymbol symbol)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            return this.create(symbol);
        }

        public char[] InvalidCharacters =>
            this.inner.InvalidCharacters;

        public bool HasInvalidCharacters =>
            this.inner.HasInvalidCharacters;

        public ISymbolFactoryServices FactoryServices =>
            this.inner.FactoryServices;
    }
}

