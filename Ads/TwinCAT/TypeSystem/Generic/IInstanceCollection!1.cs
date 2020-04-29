namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public interface IInstanceCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T: IInstance
    {
        bool Contains(string instancePath);
        bool ContainsName(string instanceName);
        T GetInstance(string instancePath);
        IList<T> GetInstanceByName(string instanceName);
        bool TryGetInstance(string instancePath, out T symbol);
        bool TryGetInstanceByName(string instanceName, out IList<T> symbols);

        T this[string instancePath] { get; }

        InstanceCollectionMode Mode { get; }
    }
}

