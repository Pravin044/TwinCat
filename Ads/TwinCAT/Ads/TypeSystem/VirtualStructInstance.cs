namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;
    using TwinCAT.TypeSystem.Generic;

    [DebuggerDisplay("Path = { InstancePath } (Virtual), Category = {category}")]
    internal sealed class VirtualStructInstance : StructInstance, IVirtualStructInstance, IStructInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        private SymbolCollection _virtualMembers;

        internal VirtualStructInstance(string instanceName, string instancePath, ISymbol parent, ISymbolFactoryServices services) : base(instanceName, instancePath, services)
        {
            this._virtualMembers = new SymbolCollection(InstanceCollectionMode.Names);
            base.category = DataTypeCategory.Struct;
        }

        public bool AddMember(ISymbol memberInstance, IVirtualStructInstance parent)
        {
            bool flag = false;
            if (this._virtualMembers.isUnique(memberInstance))
            {
                this._virtualMembers.Add(memberInstance);
                flag = true;
            }
            else
            {
                object[] args = new object[] { memberInstance, parent };
                Module.Trace.TraceWarning("Cannot add ambiguous instance '{0}' to virtual parent '{1}!", args);
                flag = false;
            }
            return flag;
        }

        internal override ISymbolCollection OnCreateSubSymbols(ISymbol parentInstance) => 
            this._virtualMembers;

        internal override int OnGetSubSymbolCount(ISymbol parentSymbol) => 
            this._virtualMembers.Count;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        protected override bool TryResolveType() => 
            false;

        public override int BitSize =>
            -1;

        public override bool HasValue =>
            false;
    }
}

