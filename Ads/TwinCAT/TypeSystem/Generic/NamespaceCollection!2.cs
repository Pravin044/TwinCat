namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class NamespaceCollection<N, T> : IList<N>, ICollection<N>, IEnumerable<N>, IEnumerable, INamespaceCollection<N, T> where N: INamespace<T> where T: IDataType
    {
        protected List<N> list;
        protected Dictionary<string, N> namespaceDict;
        protected Dictionary<string, T> allTypes;
        protected bool readOnly;

        public NamespaceCollection()
        {
            this.list = new List<N>();
            this.namespaceDict = new Dictionary<string, N>(StringComparer.OrdinalIgnoreCase);
            this.allTypes = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        }

        public void Add(N item)
        {
            this.namespaceDict.Add(item.Name, item);
            this.list.Add(item);
        }

        public void Clear()
        {
            this.namespaceDict.Clear();
            this.list.Clear();
        }

        public bool Contains(N item) => 
            this.list.Contains(item);

        public bool ContainsNamespace(string name) => 
            this.namespaceDict.ContainsKey(name);

        public void CopyTo(N[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<N> GetEnumerator() => 
            this.list.GetEnumerator();

        public int IndexOf(N item) => 
            this.list.IndexOf(item);

        public void Insert(int index, N item)
        {
            this.namespaceDict.Add(item.Name, item);
            this.list.Insert(index, item);
        }

        public bool Remove(N item)
        {
            bool flag = this.list.Remove(item);
            this.namespaceDict.Remove(item.Name);
            return flag;
        }

        public void RemoveAt(int index)
        {
            N local = this.list[index];
            this.list.RemoveAt(index);
            this.namespaceDict.Remove(local.Name);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.list.GetEnumerator();

        public bool TryGetNamespace(string name, out N nspace) => 
            this.namespaceDict.TryGetValue(name, out nspace);

        public bool TryGetType(string typeName, out T dataType)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (typeName == string.Empty)
            {
                throw new ArgumentException();
            }
            using (IEnumerator<N> enumerator = this.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    N current = enumerator.Current;
                    if (current.DataTypes.TryGetType(typeName, out dataType))
                    {
                        return true;
                    }
                }
            }
            dataType = default(T);
            return false;
        }

        public bool TryGetTypeByFullName(string fullname, out T dataType) => 
            this.allTypes.TryGetValue(fullname, out dataType);

        public N this[int index]
        {
            get => 
                this.list[index];
            set
            {
                throw new NotImplementedException();
            }
        }

        public N this[string str] =>
            this.namespaceDict[str];

        public int Count =>
            this.list.Count;

        public bool IsReadOnly =>
            this.readOnly;

        public ReadOnlyDataTypeCollection<T> AllTypes =>
            this.AllTypesInternal.AsReadOnly();

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never), Browsable(false)]
        public DataTypeCollection<T> AllTypesInternal =>
            new DataTypeCollection<T>(this.allTypes.Values);
    }
}

