namespace TwinCAT.TypeSystem
{
    using System;

    public interface ISymbolInfo
    {
        string InstancePath { get; }

        string TypeName { get; }
    }
}

