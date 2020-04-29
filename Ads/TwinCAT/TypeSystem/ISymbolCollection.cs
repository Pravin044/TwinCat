namespace TwinCAT.TypeSystem
{
    using System.Collections;
    using System.Collections.Generic;
    using TwinCAT.TypeSystem.Generic;

    public interface ISymbolCollection : ISymbolCollection<ISymbol>, IInstanceCollection<ISymbol>, IList<ISymbol>, ICollection<ISymbol>, IEnumerable<ISymbol>, IEnumerable
    {
    }
}

