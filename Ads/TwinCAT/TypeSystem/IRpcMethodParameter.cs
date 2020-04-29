namespace TwinCAT.TypeSystem
{
    using System;

    public interface IRpcMethodParameter
    {
        int Size { get; }

        string Name { get; }

        string TypeName { get; }

        MethodParamFlags ParameterFlags { get; }
    }
}

