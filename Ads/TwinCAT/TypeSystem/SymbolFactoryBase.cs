namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem.Generic;

    public abstract class SymbolFactoryBase : ISymbolFactory
    {
        protected ISymbolFactoryServices services;
        protected bool nonCachedArrayElements;
        protected bool initialized;
        public static char[] DefaultInvalidChars = new char[] { '^' };
        protected char[] invalidCharacters = DefaultInvalidChars;

        public SymbolFactoryBase(bool nonCachedArrayElements)
        {
            this.nonCachedArrayElements = nonCachedArrayElements;
        }

        protected string CombinePath(IField member, ISymbol parent)
        {
            string[] textArray1 = new string[] { parent.InstancePath, member.InstanceName };
            return string.Join(".", textArray1);
        }

        public ISymbol CreateArrayElement(int[] indices, ISymbol parent, IArrayType arrayType)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if (indices == null)
            {
                throw new ArgumentNullException("indices");
            }
            if (arrayType == null)
            {
                throw new ArgumentNullException("arrayType");
            }
            if (indices.Length == 0)
            {
                throw new ArgumentOutOfRangeException("indices");
            }
            return this.OnCreateArrayElement(indices, parent, arrayType);
        }

        public ISymbolCollection CreateArrayElementInstances(ISymbol parentInstance, IArrayType arrayType)
        {
            if (parentInstance == null)
            {
                throw new ArgumentNullException("parentInstance");
            }
            if (arrayType == null)
            {
                throw new ArgumentNullException("arrayType");
            }
            return this.OnCreateArrayElementInstances(parentInstance, arrayType);
        }

        public ISymbol CreateFieldInstance(IField field, ISymbol parent)
        {
            if (field == null)
            {
                throw new ArgumentNullException("member");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            return this.OnCreateFieldInstance(field, parent);
        }

        public ISymbolCollection CreateFieldInstances(ISymbol parentInstance, IDataType structType)
        {
            if (parentInstance == null)
            {
                throw new ArgumentNullException("parentInstance");
            }
            if (structType == null)
            {
                throw new ArgumentNullException("structType");
            }
            return this.OnCreateFieldInstances(parentInstance, structType);
        }

        public ISymbol CreateInstance(ISymbolInfo entry, ISymbol parent)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            ISymbol symbol = this.OnCreateSymbol(entry, parent);
            ((IBindable) symbol).Bind(this.FactoryServices.Binder);
            return symbol;
        }

        public ISymbol CreateReferenceInstance(IPointerType type, ISymbol parent)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            return this.OnCreateReference(type, parent);
        }

        public ISymbol CreateVirtualStruct(string instanceName, string instancePath, ISymbol parent)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException("instanceName");
            }
            if (instancePath == null)
            {
                throw new ArgumentNullException("instancePath");
            }
            if (instanceName == string.Empty)
            {
                throw new ArgumentOutOfRangeException("instanceName");
            }
            if (instancePath == string.Empty)
            {
                throw new ArgumentOutOfRangeException("instancePath");
            }
            return this.OnCreateVirtualStruct(instanceName, instancePath, parent);
        }

        public void Initialize(ISymbolFactoryServices services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            this.services = services;
            this.initialized = true;
        }

        protected abstract IAliasInstance OnCreateAlias(ISymbolInfo entry, IAliasType aliasType, ISymbol parent);
        protected abstract ISymbol OnCreateArrayElement(int[] currentIndex, ISymbol parent, IArrayType arrayType);
        protected virtual ISymbolCollection OnCreateArrayElementInstances(ISymbol parentInstance, IArrayType arrayType)
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
            }
            return symbols;
        }

        protected abstract IArrayInstance OnCreateArrayInstance(ISymbolInfo entry, IArrayType type, ISymbol parent);
        protected abstract ISymbol OnCreateFieldInstance(IField member, ISymbol parent);
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

        protected abstract IPointerInstance OnCreatePointerInstance(ISymbolInfo entry, IPointerType structType, ISymbol parent);
        protected abstract ISymbol OnCreatePrimitive(ISymbolInfo entry, IDataType dataType, ISymbol parent);
        protected abstract ISymbol OnCreateReference(IPointerType type, ISymbol parent);
        protected abstract IReferenceInstance OnCreateReferenceInstance(ISymbolInfo entry, IReferenceType referenceType, ISymbol parent);
        protected abstract ISymbol OnCreateString(ISymbolInfo entry, IStringType stringType, ISymbol parent);
        protected abstract IStructInstance OnCreateStruct(ISymbolInfo entry, IStructType structType, ISymbol parent);
        protected virtual ISymbol OnCreateSymbol(ISymbolInfo entry, ISymbol parent)
        {
            IDataType type = null;
            return (!this.TryResolveType(entry.TypeName, out type) ? this.OnCreatePrimitive(entry, type, parent) : ((type.Category != DataTypeCategory.String) ? ((type.Category != DataTypeCategory.Struct) ? ((type.Category != DataTypeCategory.Array) ? ((type.Category != DataTypeCategory.Union) ? ((type.Category != DataTypeCategory.Reference) ? ((type.Category != DataTypeCategory.Pointer) ? ((type.Category != DataTypeCategory.Alias) ? this.OnCreatePrimitive(entry, type, parent) : this.OnCreateAlias(entry, (IAliasType) type, parent)) : this.OnCreatePointerInstance(entry, (IPointerType) type, parent)) : this.OnCreateReferenceInstance(entry, (IReferenceType) type, parent)) : this.OnCreateUnion(entry, (IUnionType) type, parent)) : this.OnCreateArrayInstance(entry, (IArrayType) type, parent)) : this.OnCreateStruct(entry, (IStructType) type, parent)) : this.OnCreateString(entry, (IStringType) type, parent)));
        }

        protected abstract IUnionInstance OnCreateUnion(ISymbolInfo entry, IUnionType unionType, ISymbol parent);
        protected abstract ISymbol OnCreateVirtualStruct(string instanceName, string instancePath, ISymbol parent);
        public void SetInvalidCharacters(char[] invalidChars)
        {
            this.invalidCharacters = invalidChars;
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        protected bool TryResolveType(string typeName, out IDataType type)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (typeName != string.Empty)
            {
                return this.services.Binder.TryResolveType(typeName, out type);
            }
            type = null;
            return false;
        }

        internal bool NonCachedArrayElements =>
            this.nonCachedArrayElements;

        public bool IsInitialized =>
            this.initialized;

        public ISymbolFactoryServices FactoryServices =>
            this.services;

        public char[] InvalidCharacters =>
            this.invalidCharacters;

        public bool HasInvalidCharacters =>
            (this.invalidCharacters.Length != 0);
    }
}

