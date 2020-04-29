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
    using TwinCAT.Ads.TypeSystem;

    public class RpcMethodCollection : IList<IRpcMethod>, ICollection<IRpcMethod>, IEnumerable<IRpcMethod>, IEnumerable
    {
        private List<IRpcMethod> _list;
        private Dictionary<string, IRpcMethod> _dict;
        private static RpcMethodCollection _empty;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public RpcMethodCollection()
        {
            this._list = new List<IRpcMethod>();
            this._dict = new Dictionary<string, IRpcMethod>(StringComparer.OrdinalIgnoreCase);
        }

        internal RpcMethodCollection(AdsMethodEntry[] coll)
        {
            this._list = new List<IRpcMethod>();
            this._dict = new Dictionary<string, IRpcMethod>(StringComparer.OrdinalIgnoreCase);
            if (coll != null)
            {
                for (int i = 0; i < coll.Length; i++)
                {
                    IRpcMethod item = new RpcMethod(coll[i]);
                    if (!this.Contains(item))
                    {
                        this.Add(item);
                    }
                    else
                    {
                        object[] args = new object[] { item.Name };
                        TwinCAT.Ads.Module.Trace.TraceWarning("RpcMethod '{0}' already contained in collection. Double definition in AdsMethodEntry", args);
                    }
                }
            }
        }

        public void Add(IRpcMethod item)
        {
            this._dict.Add(item.Name, item);
            this._list.Add(item);
        }

        public ReadOnlyRpcMethodCollection AsReadOnly() => 
            new ReadOnlyRpcMethodCollection(this);

        public void Clear()
        {
            this._dict.Clear();
            this._list.Clear();
        }

        public bool Contains(string methodName)
        {
            IRpcMethod method = null;
            return this.TryGetMethod(methodName, out method);
        }

        public bool Contains(IRpcMethod item) => 
            this._dict.ContainsKey(item.Name);

        public void CopyTo(IRpcMethod[] array, int arrayIndex)
        {
            this._list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IRpcMethod> GetEnumerator() => 
            this._list.GetEnumerator();

        public int IndexOf(IRpcMethod item) => 
            this._list.IndexOf(item);

        public void Insert(int index, IRpcMethod item)
        {
            this._dict.Add(item.Name, item);
            this._list.Insert(index, item);
        }

        public bool Remove(IRpcMethod item)
        {
            this._dict.Remove(item.Name);
            return this._list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            IRpcMethod method = this._list[index];
            this._dict.Remove(method.Name);
            this._list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._list.GetEnumerator();

        public bool TryGetMethod(int vTableIndex, out IRpcMethod method)
        {
            if ((vTableIndex < 0) || (vTableIndex >= this._list.Count))
            {
                method = null;
                return false;
            }
            method = this._list[vTableIndex];
            return true;
        }

        public bool TryGetMethod(string methodName, out IRpcMethod method) => 
            this._dict.TryGetValue(methodName, out method);

        internal static RpcMethodCollection Empty
        {
            get
            {
                if (_empty == null)
                {
                    _empty = new RpcMethodCollection(null);
                }
                return _empty;
            }
        }

        public IRpcMethod this[int index]
        {
            get => 
                this._list[index];
            set
            {
                throw new NotImplementedException();
            }
        }

        public IRpcMethod this[string methodName]
        {
            get
            {
                IRpcMethod method = null;
                if (!this.TryGetMethod(methodName, out method))
                {
                    throw new KeyNotFoundException();
                }
                return method;
            }
        }

        public int Count =>
            this._list.Count;

        public bool IsReadOnly =>
            false;
    }
}

