namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReadOnlyInstanceCollection<T> : ReadOnlyCollection<T>, IInstanceCollection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T: IInstance
    {
        protected InstanceCollectionMode mode;

        public ReadOnlyInstanceCollection(IInstanceCollection<T> coll) : base(coll)
        {
            this.mode = coll.Mode;
        }

        public bool Contains(string instancePath) => 
            ((IInstanceCollection<T>) base.Items).Contains(instancePath);

        public bool ContainsName(string instanceName) => 
            ((IInstanceCollection<T>) base.Items).ContainsName(instanceName);

        public T GetInstance(string instancePath) => 
            ((IInstanceCollection<T>) base.Items).GetInstance(instancePath);

        public IList<T> GetInstanceByName(string instanceName) => 
            ((IInstanceCollection<T>) base.Items).GetInstanceByName(instanceName);

        public bool TryGetInstance(string instancePath, out T instance) => 
            ((IInstanceCollection<T>) base.Items).TryGetInstance(instancePath, out instance);

        public bool TryGetInstanceByName(string instanceName, out IList<T> symbols) => 
            ((IInstanceCollection<T>) base.Items).TryGetInstanceByName(instanceName, out symbols);

        public InstanceCollectionMode Mode =>
            this.mode;

        public T this[string instancePath] =>
            ((IInstanceCollection<T>) base.Items)[instancePath];
    }
}

