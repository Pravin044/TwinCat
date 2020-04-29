namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public interface IArrayValue : IValue
    {
        bool TryGetArrayElementValues(out IEnumerable<object> elementValues);
        bool TryGetIndexValue(int[] indices, out object value);
        bool TrySetIndexValue(object[] indexes, object value);
    }
}

