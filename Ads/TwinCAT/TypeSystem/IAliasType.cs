namespace TwinCAT.TypeSystem
{
    using System;

    public interface IAliasType : IDataType, IBitSize
    {
        string BaseTypeName { get; }

        IDataType BaseType { get; }
    }
}

