namespace TwinCAT.TypeSystem
{
    using System;

    public interface IRpcMethod
    {
        string Name { get; }

        ReadOnlyMethodParameterCollection Parameters { get; }

        string ReturnType { get; }

        int ReturnTypeSize { get; }

        string Comment { get; }

        bool IsVoid { get; }
    }
}

