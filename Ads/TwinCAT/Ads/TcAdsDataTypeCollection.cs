namespace TwinCAT.Ads
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem;

    internal class TcAdsDataTypeCollection : IList<ITcAdsDataType>, ICollection<ITcAdsDataType>, IEnumerable<ITcAdsDataType>, IEnumerable
    {
        private List<ITcAdsDataType> _list;
        private Dictionary<string, ITcAdsDataType> _table;

        public TcAdsDataTypeCollection()
        {
            this._list = new List<ITcAdsDataType>();
            this._table = new Dictionary<string, ITcAdsDataType>(StringComparer.OrdinalIgnoreCase);
        }

        public TcAdsDataTypeCollection(IEnumerable<ITcAdsDataType> coll)
        {
            this._list = new List<ITcAdsDataType>();
            this._table = new Dictionary<string, ITcAdsDataType>(StringComparer.OrdinalIgnoreCase);
            this.AddRange(coll);
        }

        public void Add(ITcAdsDataType item)
        {
            try
            {
                this._table.Add(item.Name, item);
                this._list.Add(item);
            }
            catch (ArgumentException)
            {
                object[] args = new object[] { item.Name };
                TwinCAT.Ads.Module.Trace.TraceWarning("DataType '{0}' already in collection", args);
            }
        }

        public void AddRange(IEnumerable<ITcAdsDataType> coll)
        {
            foreach (ITcAdsDataType type in coll)
            {
                this.Add(type);
            }
        }

        public ReadOnlyTcAdsDataTypeCollection AsReadOnly() => 
            new ReadOnlyTcAdsDataTypeCollection(this);

        public void Clear()
        {
            this._table.Clear();
            this._list.Clear();
        }

        public TcAdsDataTypeCollection Clone() => 
            new TcAdsDataTypeCollection(this);

        public bool Contains(string typeName)
        {
            ITcAdsDataType ret = null;
            return this.TryGetDataType(typeName, out ret);
        }

        public bool Contains(ITcAdsDataType item) => 
            this._table.ContainsKey(item.Name);

        public void CopyTo(ITcAdsDataType[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ITcAdsDataType> GetEnumerator() => 
            this._list.GetEnumerator();

        public int IndexOf(ITcAdsDataType item) => 
            this._list.IndexOf(item);

        public void Insert(int index, ITcAdsDataType item)
        {
            this._table.Add(item.Name, item);
            this._list.Add(item);
        }

        public bool Remove(ITcAdsDataType item)
        {
            bool flag = this._table.Remove(item.Name);
            this._list.Remove(item);
            return flag;
        }

        public void RemoveAt(int index)
        {
            ITcAdsDataType type = this._list[index];
            this._list.RemoveAt(index);
            this._table.Remove(type.Name);
        }

        internal ITcAdsDataType ResolveType(ITcAdsDataType dataType, DataTypeResolveStrategy strategy)
        {
            ITcAdsDataType ret = dataType;
            if (strategy == DataTypeResolveStrategy.Alias)
            {
                while ((ret != null) && (ret.Category == DataTypeCategory.Alias))
                {
                    this.TryGetDataType(ret.BaseTypeName, out ret);
                }
            }
            else if (strategy == DataTypeResolveStrategy.AliasReference)
            {
                while ((ret != null) && ((ret.Category == DataTypeCategory.Alias) || (ret.Category == DataTypeCategory.Reference)))
                {
                    if (ret.Category == DataTypeCategory.Alias)
                    {
                        this.TryGetDataType(ret.BaseTypeName, out ret);
                    }
                    else
                    {
                        string str;
                        if (DataTypeStringParser.TryParseReference(ret.Name, out str))
                        {
                            this.TryGetDataType(str, out ret);
                        }
                        else
                        {
                            ret = null;
                        }
                    }
                }
            }
            return ret;
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._list.GetEnumerator();

        public bool TryGetDataType(string typeName, out ITcAdsDataType ret) => 
            this._table.TryGetValue(typeName, out ret);

        public ITcAdsDataType this[int index]
        {
            get => 
                this._list[index];
            set
            {
                throw new NotImplementedException();
            }
        }

        public ITcAdsDataType this[string typeName]
        {
            get
            {
                ITcAdsDataType ret = null;
                if (!this.TryGetDataType(typeName, out ret))
                {
                    throw new KeyNotFoundException();
                }
                return ret;
            }
        }

        public int Count =>
            this._list.Count;

        public bool IsReadOnly =>
            false;
    }
}

