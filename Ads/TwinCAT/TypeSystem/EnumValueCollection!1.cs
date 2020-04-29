namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads.Internal;

    public class EnumValueCollection<T> : IList<EnumValue<T>>, ICollection<EnumValue<T>>, IEnumerable<EnumValue<T>>, IEnumerable where T: IConvertible
    {
        private List<EnumValue<T>> _list;
        private Dictionary<string, EnumValue<T>> _nameValueDict;

        internal EnumValueCollection(AdsEnumInfoEntry[] coll)
        {
            this._list = new List<EnumValue<T>>();
            this._nameValueDict = new Dictionary<string, EnumValue<T>>(StringComparer.OrdinalIgnoreCase);
            if (coll == null)
            {
                throw new ArgumentNullException("coll");
            }
            foreach (AdsEnumInfoEntry entry in coll)
            {
                EnumValue<T> item = new EnumValue<T>(entry);
                this.Add(item);
            }
        }

        public void Add(EnumValue<T> item)
        {
            this._nameValueDict.Add(item.Name, item);
            this._list.Add(item);
        }

        public ReadOnlyEnumValueCollection<T> AsReadOnly() => 
            new ReadOnlyEnumValueCollection<T>((EnumValueCollection<T>) this);

        public void Clear()
        {
            this._nameValueDict.Clear();
            this._list.Clear();
        }

        public bool Contains(string name) => 
            this._nameValueDict.ContainsKey(name);

        public bool Contains(EnumValue<T> item) => 
            this._nameValueDict.ContainsKey(item.Name);

        public bool Contains(T value)
        {
            for (int i = 0; i < this.Count; i++)
            {
                T primitive = this._list[i].Primitive;
                if (primitive.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(EnumValue<T>[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<EnumValue<T>> GetEnumerator() => 
            this._list.GetEnumerator();

        public string[] GetNames()
        {
            string[] strArray = new string[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                strArray[i] = this._list[i].Name;
            }
            return strArray;
        }

        public T[] GetValues()
        {
            T[] localArray = new T[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                localArray[i] = this._list[i].Primitive;
            }
            return localArray;
        }

        public int IndexOf(EnumValue<T> item) => 
            this._list.IndexOf(item);

        public void Insert(int index, EnumValue<T> item)
        {
            this._nameValueDict.Add(item.Name, item);
            this._list.Insert(index, item);
        }

        public static explicit operator EnumValueCollection(EnumValueCollection<T> coll)
        {
            IEnumValue[] valueArray = new IEnumValue[coll.Count];
            for (int i = 0; i < coll.Count; i++)
            {
                valueArray[i] = coll[i];
            }
            return new EnumValueCollection(valueArray);
        }

        public T Parse(string name)
        {
            T local = default(T);
            if (this.TryParse(name, out local))
            {
                return local;
            }
            string str = name;
            string[] names = this.GetNames();
            string str2 = string.Join(",", names);
            throw new ArgumentOutOfRangeException("name", name, $"The value '{name}' is not one of the valid enum values '{str2}'");
        }

        public bool Remove(EnumValue<T> item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            EnumValue<T> value2 = this._list[index];
            this._list.RemoveAt(index);
            this._nameValueDict.Remove(value2.Name);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._list.GetEnumerator();

        public bool TryGetInfo(T val, out EnumValue<T> ei)
        {
            for (int i = 0; i < this.Count; i++)
            {
                T primitive = this._list[i].Primitive;
                if (primitive.Equals(val))
                {
                    ei = this._list[i];
                    return true;
                }
            }
            ei = null;
            return false;
        }

        public bool TryParse(string name, out T value)
        {
            EnumValue<T> value2 = null;
            if (this._nameValueDict.TryGetValue(name, out value2))
            {
                value = value2.Primitive;
                return true;
            }
            value = default(T);
            return false;
        }

        public EnumValue<T> this[int index]
        {
            get => 
                this._list[index];
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count =>
            this._list.Count;

        public bool IsReadOnly =>
            false;
    }
}

