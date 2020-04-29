namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads.TypeSystem;
    using TwinCAT.TypeSystem;

    public abstract class InstanceCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IInstanceCollection<T> where T: class, IInstance
    {
        protected List<T> _list;
        protected Dictionary<string, T> _pathDict;
        protected InstanceCollectionMode mode;

        protected InstanceCollection(InstanceCollectionMode mode)
        {
            this._list = new List<T>();
            this._pathDict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            this.mode = InstanceCollectionMode.Path;
            this.mode = mode;
        }

        protected InstanceCollection(IEnumerable<T> coll, InstanceCollectionMode mode) : this(mode)
        {
            this.AddRange(coll);
        }

        public void Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (this.mode == InstanceCollectionMode.Names)
            {
                this._pathDict.Add(item.InstanceName, item);
                this._list.Add(item);
            }
            else if (this.mode == InstanceCollectionMode.Path)
            {
                this._pathDict.Add(item.InstancePath, item);
                this._list.Add(item);
            }
            else
            {
                string str;
                string str2;
                IList<int[]> list;
                SymbolParser.ArrayIndexType type;
                if (this.mode != InstanceCollectionMode.PathHierarchy)
                {
                    throw new NotSupportedException();
                }
                string indicesStr = null;
                if (!SymbolParser.TryParseParentPath(item, out str, out str2) && !SymbolParser.TryParseArrayElement(item.InstancePath, out str2, out indicesStr, out list, out type))
                {
                    this._list.Add(item);
                    string instanceName = item.InstanceName;
                    if (item.InstancePath[0] == '.')
                    {
                        instanceName = instanceName.Insert(0, ".");
                    }
                    this._pathDict.Add(instanceName, item);
                }
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T local in items)
            {
                this.Add(local);
            }
        }

        public ReadOnlyInstanceCollection<T> AsReadOnly() => 
            new ReadOnlyInstanceCollection<T>(this);

        public void Clear()
        {
            this._pathDict.Clear();
            this._list.Clear();
        }

        public bool Contains(T item) => 
            this._pathDict.ContainsKey(item.InstancePath);

        public bool Contains(string instanceSpecifier)
        {
            T symbol = default(T);
            return this.TryGetInstance(instanceSpecifier, out symbol);
        }

        public bool ContainsName(string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                throw new ArgumentNullException("instanceName");
            }
            string actName = instanceName;
            bool flag = instanceName.EndsWith("^");
            if (flag)
            {
                actName = instanceName.Substring(0, instanceName.Length - 1);
            }
            T local = default(T);
            local = Enumerable.SingleOrDefault<T>(this._list, p => string.CompareOrdinal(p.InstanceName, actName) == 0);
            foreach (T local2 in this._list)
            {
                if (string.CompareOrdinal(local2.InstanceName, actName) == 0)
                {
                    local = local2;
                    break;
                }
            }
            if ((local != null) & flag)
            {
                IPointerInstance instance = local as IPointerInstance;
                local = (instance == null) ? default(T) : ((T) instance.Reference);
            }
            return (local != null);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        internal string createUniquepathName(IInstance instance)
        {
            string str = (this.mode != InstanceCollectionMode.Names) ? instance.InstancePath : instance.InstanceName;
            int num = 0;
            string str2 = str;
            string key = str;
            while (this._pathDict.ContainsKey(key))
            {
                num++;
                key = $"{str2}_{num}";
            }
            return key;
        }

        public IEnumerator<T> GetEnumerator() => 
            this._list.GetEnumerator();

        public T GetInstance(string instanceSpecifier)
        {
            T symbol = default(T);
            if (!this.TryGetInstance(instanceSpecifier, out symbol))
            {
                throw new ArgumentException($"InstancePath '{instanceSpecifier}' not found!", "instancePath");
            }
            return symbol;
        }

        public IList<T> GetInstanceByName(string instanceName)
        {
            IList<T> instances = null;
            if (!this.TryGetInstanceByName(instanceName, out instances))
            {
                throw new ArgumentException($"Name '{instanceName}' not found!", "instanceName");
            }
            return instances;
        }

        public int IndexOf(T item) => 
            this._list.IndexOf(item);

        public void Insert(int index, T instance)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (instance == null)
            {
                throw new ArgumentNullException();
            }
            if (index > this.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (this.mode == InstanceCollectionMode.PathHierarchy)
            {
                string str;
                string str2;
                IList<int[]> list;
                SymbolParser.ArrayIndexType type;
                if (SymbolParser.TryParseParentPath(instance, out str, out str2))
                {
                    throw new ArgumentOutOfRangeException("Instance '{0}'is not a root object. Cannot Insert!", instance.InstancePath);
                }
                string indicesStr = null;
                if (SymbolParser.TryParseArrayElement(instance.InstancePath, out str2, out indicesStr, out list, out type))
                {
                    throw new ArgumentOutOfRangeException("Instance '{0}'is not a root object. Cannot Insert!", instance.InstancePath);
                }
            }
            this._pathDict.Add(instance.InstancePath, instance);
            this._list.Insert(index, instance);
        }

        internal bool isUnique(IInstance instance) => 
            ((this.mode != InstanceCollectionMode.Names) ? !this._pathDict.ContainsKey(instance.InstancePath) : !this._pathDict.ContainsKey(instance.InstanceName));

        public bool Remove(T item)
        {
            string instancePath = item.InstancePath;
            string instanceName = item.InstanceName;
            int index = this.IndexOf(item);
            if (index < 0)
            {
                return false;
            }
            this.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            T local = this._list[index];
            this._list.RemoveAt(index);
            this._pathDict.Remove(local.InstancePath);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._list.GetEnumerator();

        public bool TryGetInstance(string instanceSpecifier, out T symbol)
        {
            if (instanceSpecifier == null)
            {
                throw new ArgumentNullException("instancePath");
            }
            if (instanceSpecifier.Length == 0)
            {
                throw new ArgumentException();
            }
            switch (this.mode)
            {
                case InstanceCollectionMode.Names:
                    return this._pathDict.TryGetValue(instanceSpecifier, out symbol);

                case InstanceCollectionMode.Path:
                    return this._pathDict.TryGetValue(instanceSpecifier, out symbol);

                case InstanceCollectionMode.PathHierarchy:
                    return this.TryGetInstanceHierarchically(instanceSpecifier, out symbol);
            }
            throw new NotSupportedException();
        }

        public virtual bool TryGetInstanceByName(string instanceName, out IList<T> instances)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                throw new ArgumentNullException("instanceName2");
            }
            List<T> list = null;
            bool flag = false;
            bool flag2 = instanceName.EndsWith("^");
            string actName = instanceName;
            if (flag2)
            {
                actName = instanceName.Substring(0, instanceName.Length - 1);
            }
            if (this.mode == InstanceCollectionMode.Path)
            {
                StringComparer cmp = StringComparer.OrdinalIgnoreCase;
                list = new List<T>(from p in this._pathDict.Values
                    where cmp.Compare(p.InstanceName, actName) == 0
                    select p);
                if (flag2)
                {
                    list = new List<T>(from pi in Enumerable.Cast<IPointerInstance>((IEnumerable) (from p in list
                        where p is IPointerInstance
                        select p)) select (T) pi.Reference);
                }
            }
            else
            {
                T reference = default(T);
                if (this._pathDict.TryGetValue(actName, out reference))
                {
                    list = new List<T>();
                    if (!flag2)
                    {
                        list.Add(reference);
                    }
                    else
                    {
                        IPointerInstance instance = reference as IPointerInstance;
                        if (instance != null)
                        {
                            reference = (T) instance.Reference;
                            if (reference != null)
                            {
                                list.Add(reference);
                            }
                        }
                    }
                }
            }
            if ((list == null) || (list.Count <= 0))
            {
                instances = null;
                flag = false;
            }
            else
            {
                instances = list.AsReadOnly();
                flag = true;
            }
            return flag;
        }

        internal bool TryGetInstanceHierarchically(string instancePath, out T symbol)
        {
            if (instancePath == null)
            {
                throw new ArgumentNullException("instancePath");
            }
            if (instancePath.Length == 0)
            {
                throw new ArgumentException();
            }
            if ((this.mode == InstanceCollectionMode.Path) && this._pathDict.TryGetValue(instancePath, out symbol))
            {
                return true;
            }
            bool flag = false;
            string[] destinationArray = null;
            if ((instancePath.Length > 0) && (instancePath[0] == '.'))
            {
                flag = true;
            }
            if (!flag)
            {
                char[] separator = new char[] { '.' };
                destinationArray = instancePath.Split(separator);
            }
            else
            {
                char[] separator = new char[] { '.' };
                string[] sourceArray = instancePath.Split(separator);
                if (sourceArray.Length > 1)
                {
                    sourceArray[1] = sourceArray[1].Insert(0, ".");
                }
                destinationArray = new string[sourceArray.Length - 1];
                Array.Copy(sourceArray, 1, destinationArray, 0, sourceArray.Length - 1);
            }
            return InstanceCollection<T>.TryGetSubItem(this, destinationArray, 0, out symbol);
        }

        internal static bool TryGetSubItem(IInstanceCollection<T> coll, string[] pathSplit, int splitIndex, out T symbol)
        {
            SymbolParser.ArrayIndexType type;
            T local = default(T);
            symbol = default(T);
            string nameWithIndices = pathSplit[splitIndex];
            string instanceName = null;
            IList<int[]> jaggedIndices = null;
            string indicesStr = null;
            bool flag = SymbolParser.TryParseArrayElement(nameWithIndices, out instanceName, out indicesStr, out jaggedIndices, out type);
            if (flag)
            {
                nameWithIndices = instanceName;
            }
            bool flag2 = nameWithIndices[nameWithIndices.Length - 1] == '^';
            if (flag2)
            {
                nameWithIndices = nameWithIndices.Substring(0, nameWithIndices.Length - 1);
            }
            IList<T> list2 = null;
            if (coll.TryGetInstanceByName(nameWithIndices, out list2))
            {
                local = list2[0];
                if (flag)
                {
                    ISymbol symbol2 = null;
                    IArrayInstance instance = local as IArrayInstance;
                    IReferenceInstanceAccess access = local as IReferenceInstanceAccess;
                    if (instance != null)
                    {
                        instance.TryGetElement(jaggedIndices, out symbol2);
                    }
                    else if (access != null)
                    {
                        access.TryGetElement(jaggedIndices, out symbol2);
                    }
                    local = (T) symbol2;
                }
                if ((local == null) || (splitIndex >= (pathSplit.Length - 1)))
                {
                    symbol = !((splitIndex == (pathSplit.Length - 1)) & flag2) ? local : ((ISymbolInternal) local).SubSymbolsInternal[0];
                }
                else
                {
                    ISymbol symbol3 = local as ISymbol;
                    if (symbol3.IsContainerType)
                    {
                        ISymbolCollection subSymbolsInternal = ((ISymbolInternal) symbol3).SubSymbolsInternal;
                        if (flag2)
                        {
                            subSymbolsInternal = ((ISymbolInternal) subSymbolsInternal[0]).SubSymbolsInternal;
                        }
                        if ((subSymbolsInternal != null) && (subSymbolsInternal.Count > 0))
                        {
                            T local2 = default(T);
                            splitIndex++;
                            if (InstanceCollection<T>.TryGetSubItem((IInstanceCollection<T>) subSymbolsInternal, pathSplit, splitIndex, out local2))
                            {
                                symbol = local2;
                            }
                            else
                            {
                                symbol = default(T);
                            }
                        }
                    }
                }
            }
            return (((T) symbol) != null);
        }

        public T this[int index]
        {
            get => 
                this._list[index];
            set
            {
                throw new NotImplementedException();
            }
        }

        public T this[string instanceSpecifier]
        {
            get
            {
                T symbol = default(T);
                if (!this.TryGetInstance(instanceSpecifier, out symbol))
                {
                    throw new KeyNotFoundException($"InstancePath '{instanceSpecifier}' not found in InstanceCollection!");
                }
                return symbol;
            }
        }

        public int Count =>
            this._list.Count;

        public bool IsReadOnly =>
            true;

        public InstanceCollectionMode Mode =>
            this.mode;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly InstanceCollection<T>.<>c <>9;
            public static Func<T, bool> <>9__34_1;
            public static Func<IPointerInstance, T> <>9__34_2;

            static <>c()
            {
                InstanceCollection<T>.<>c.<>9 = new InstanceCollection<T>.<>c();
            }

            internal bool <TryGetInstanceByName>b__34_1(T p) => 
                (p is IPointerInstance);

            internal T <TryGetInstanceByName>b__34_2(IPointerInstance pi) => 
                ((T) pi.Reference);
        }
    }
}

