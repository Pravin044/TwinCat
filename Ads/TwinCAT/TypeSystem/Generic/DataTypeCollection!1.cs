namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class DataTypeCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IDataTypeContainer<T> where T: IDataType
    {
        protected List<T> list;
        protected Dictionary<string, T> nameDict;
        protected bool readOnly;

        public DataTypeCollection()
        {
            this.list = new List<T>();
            this.nameDict = new Dictionary<string, T>();
        }

        public DataTypeCollection(IEnumerable<T> types)
        {
            this.list = new List<T>();
            this.nameDict = new Dictionary<string, T>();
            this.AddRange(types);
        }

        public void Add(T item)
        {
            this.nameDict.Add(item.Name, item);
            this.list.Add(item);
        }

        public void AddRange(IEnumerable<T> types)
        {
            foreach (T local in types)
            {
                this.Add(local);
            }
        }

        public ReadOnlyDataTypeCollection<T> AsReadOnly() => 
            new ReadOnlyDataTypeCollection<T>((DataTypeCollection<T>) this);

        public void Clear()
        {
            this.nameDict.Clear();
            this.list.Clear();
        }

        public DataTypeCollection<T> Clone() => 
            new DataTypeCollection<T>(this);

        public bool Contains(T item) => 
            this.ContainsType(item.Name);

        public bool ContainsType(string name) => 
            this.nameDict.ContainsKey(name);

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator() => 
            this.list.GetEnumerator();

        public int IndexOf(T item) => 
            this.list.IndexOf(item);

        public void Insert(int index, T item)
        {
            this.nameDict.Add(item.Name, item);
            this.list.Add(item);
        }

        public T LookupType(string name)
        {
            T type = default(T);
            this.TryGetType(name, out type);
            return type;
        }

        public bool Remove(T item)
        {
            this.nameDict.Remove(item.Name);
            return this.list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            T local = this.list[index];
            this.nameDict.Remove(local.Name);
            this.list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.list.GetEnumerator();

        public bool TryGetType(string name, out T type) => 
            this.nameDict.TryGetValue(name, out type);

        public T this[int index]
        {
            get => 
                this.list[index];
            set
            {
                throw new NotImplementedException();
            }
        }

        public T this[string name] =>
            this.nameDict[name];

        public int Count =>
            this.list.Count;

        public bool IsReadOnly =>
            this.readOnly;
    }
}

