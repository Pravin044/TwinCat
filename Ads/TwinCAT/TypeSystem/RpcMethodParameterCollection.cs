namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    public class RpcMethodParameterCollection : IList<IRpcMethodParameter>, ICollection<IRpcMethodParameter>, IEnumerable<IRpcMethodParameter>, IEnumerable
    {
        private List<IRpcMethodParameter> _list;
        private Dictionary<string, IRpcMethodParameter> _dict;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public RpcMethodParameterCollection()
        {
            this._list = new List<IRpcMethodParameter>();
            this._dict = new Dictionary<string, IRpcMethodParameter>(StringComparer.OrdinalIgnoreCase);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public RpcMethodParameterCollection(IEnumerable<IRpcMethodParameter> coll)
        {
            this._list = new List<IRpcMethodParameter>();
            this._dict = new Dictionary<string, IRpcMethodParameter>(StringComparer.OrdinalIgnoreCase);
            foreach (IRpcMethodParameter parameter in coll)
            {
                this.Add(parameter);
            }
        }

        public void Add(IRpcMethodParameter item)
        {
            this._dict.Add(item.Name, item);
            this._list.Add(item);
        }

        public ReadOnlyMethodParameterCollection AsReadOnly() => 
            new ReadOnlyMethodParameterCollection(this);

        public void Clear()
        {
            this._dict.Clear();
            this._list.Clear();
        }

        public bool Contains(IRpcMethodParameter item) => 
            this._dict.ContainsKey(item.Name);

        public void CopyTo(IRpcMethodParameter[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IRpcMethodParameter> GetEnumerator() => 
            this._list.GetEnumerator();

        public int IndexOf(IRpcMethodParameter item) => 
            this._list.IndexOf(item);

        public void Insert(int index, IRpcMethodParameter item)
        {
            this._dict.Add(item.Name, item);
            this._list.Insert(index, item);
        }

        public bool Remove(IRpcMethodParameter item)
        {
            this._dict.Remove(item.Name);
            return this._list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            IRpcMethodParameter parameter = this[index];
            this._dict.Remove(parameter.Name);
            this._list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._list.GetEnumerator();

        public IRpcMethodParameter this[int index]
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

