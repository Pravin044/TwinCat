namespace TwinCAT.TypeSystem.Generic
{
    using System.Collections;
    using System.Collections.Generic;

    public interface ISymbolCollection<T> : IInstanceCollection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T: ISymbol
    {
    }
}

