namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;

    public interface IStructValue : IValue
    {
        bool TryGetMemberValue(string name, out object value);
        bool TrySetMemberValue(string name, object value);
    }
}

