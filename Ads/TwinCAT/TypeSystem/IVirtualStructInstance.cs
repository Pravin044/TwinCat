namespace TwinCAT.TypeSystem
{
    using System;

    public interface IVirtualStructInstance : IStructInstance, ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        bool AddMember(ISymbol memberInstance, IVirtualStructInstance parent);
    }
}

