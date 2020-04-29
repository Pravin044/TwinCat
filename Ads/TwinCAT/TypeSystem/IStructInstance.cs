namespace TwinCAT.TypeSystem
{
    using System;

    public interface IStructInstance : ISymbol, IAttributedInstance, IInstance, IBitSize
    {
        ReadOnlySymbolCollection MemberInstances { get; }

        bool HasRpcMethods { get; }
    }
}

