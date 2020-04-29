namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class SymbolCollection<T> : InstanceCollection<T>, ISymbolCollection<T>, IInstanceCollection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T: class, ISymbol
    {
        internal SymbolCollection(InstanceCollectionMode mode) : base(mode)
        {
        }

        internal SymbolCollection(IEnumerable<T> coll, InstanceCollectionMode mode) : base(coll, mode)
        {
        }

        public ReadOnlySymbolCollection<T> AsReadOnly() => 
            new ReadOnlySymbolCollection<T>(this);

        public SymbolCollection<T> Clone() => 
            new SymbolCollection<T>(this, base.mode);

        public bool TryGetInstances(Func<T, bool> predicate, bool recurse, out IList<T> instances)
        {
            instances = new List<T>();
            foreach (T local in new SymbolIterator<T>(base._list, recurse))
            {
                if (predicate(local))
                {
                    instances.Add(local);
                }
            }
            if (instances.Count > 0)
            {
                return true;
            }
            instances = null;
            return false;
        }
    }
}

