namespace TwinCAT.TypeSystem
{
    using System;

    public interface IStructType : IDataType, IBitSize
    {
        ReadOnlyMemberCollection Members { get; }

        string BaseTypeName { get; }

        IDataType BaseType { get; }

        ReadOnlyMemberCollection AllMembers { get; }

        bool HasRpcMethods { get; }
    }
}

