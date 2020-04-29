namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;

    public class EnumValueCollection : IList<IEnumValue>, ICollection<IEnumValue>, IEnumerable<IEnumValue>, IEnumerable
    {
        private List<IEnumValue> _list;
        private Dictionary<string, IEnumValue> _nameValueDict;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public EnumValueCollection(IEnumerable<IEnumValue> coll)
        {
            this._list = new List<IEnumValue>();
            this._nameValueDict = new Dictionary<string, IEnumValue>(StringComparer.OrdinalIgnoreCase);
            foreach (IEnumValue value2 in coll)
            {
                this.Add(value2);
            }
        }

        internal EnumValueCollection(AdsDatatypeId typeId, AdsEnumInfoEntry[] coll)
        {
            this._list = new List<IEnumValue>();
            this._nameValueDict = new Dictionary<string, IEnumValue>(StringComparer.OrdinalIgnoreCase);
            if (coll == null)
            {
                throw new ArgumentNullException("coll");
            }
            foreach (AdsEnumInfoEntry entry in coll)
            {
                IEnumValue item = EnumValueFactory.Create(typeId, entry);
                this.Add(item);
            }
        }

        public void Add(IEnumValue item)
        {
            this._nameValueDict.Add(item.Name, item);
            this._list.Add(item);
        }

        public ReadOnlyEnumValueCollection AsReadOnly() => 
            new ReadOnlyEnumValueCollection(this);

        public void Clear()
        {
            this._nameValueDict.Clear();
            this._list.Clear();
        }

        public bool Contains(object value)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this._list[i].Primitive.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string name) => 
            this._nameValueDict.ContainsKey(name);

        public bool Contains(IEnumValue item) => 
            this._nameValueDict.ContainsKey(item.Name);

        public void CopyTo(IEnumValue[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IEnumValue> GetEnumerator() => 
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

        public object[] GetValues()
        {
            object[] objArray = new object[this.Count];
            for (int i = 0; i < this.Count; i++)
            {
                objArray[i] = this._list[i].Primitive;
            }
            return objArray;
        }

        public int IndexOf(IEnumValue item) => 
            this._list.IndexOf(item);

        public void Insert(int index, IEnumValue item)
        {
            this._nameValueDict.Add(item.Name, item);
            this._list.Insert(index, item);
        }

        public object Parse(string name)
        {
            object obj2 = null;
            if (!this.TryParse(name, out obj2))
            {
                throw new ArgumentOutOfRangeException("name");
            }
            return obj2;
        }

        public bool Remove(IEnumValue item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            IEnumValue value2 = this._list[index];
            this._list.RemoveAt(index);
            this._nameValueDict.Remove(value2.Name);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._list.GetEnumerator();

        public bool TryGetInfo(object val, out IEnumValue ei)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this._list[i].Primitive.Equals(val))
                {
                    ei = this._list[i];
                    return true;
                }
            }
            ei = null;
            return false;
        }

        public bool TryParse(string name, out object value)
        {
            IEnumValue value2 = null;
            if (this._nameValueDict.TryGetValue(name, out value2))
            {
                value = value2.Primitive;
                return true;
            }
            value = null;
            return false;
        }

        public IEnumValue this[int index]
        {
            get => 
                this._list[index];
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumValue this[string name] =>
            this._nameValueDict[name];

        public int Count =>
            this._list.Count;

        public bool IsReadOnly =>
            false;
    }
}

