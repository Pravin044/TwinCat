namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class TypeAttributeCollection : IList<ITypeAttribute>, ICollection<ITypeAttribute>, IEnumerable<ITypeAttribute>, IEnumerable
    {
        protected List<ITypeAttribute> list;

        public TypeAttributeCollection()
        {
            this.list = new List<ITypeAttribute>();
        }

        public TypeAttributeCollection(IEnumerable<ITypeAttribute> coll)
        {
            this.list = new List<ITypeAttribute>();
            if (coll != null)
            {
                foreach (ITypeAttribute attribute in coll)
                {
                    this.Add(new TypeAttribute(attribute));
                }
            }
        }

        public void Add(ITypeAttribute item)
        {
            this.list.Add(item);
        }

        public ReadOnlyTypeAttributeCollection AsReadOnly() => 
            new ReadOnlyTypeAttributeCollection(this);

        public void Clear()
        {
            this.list.Clear();
        }

        public bool Contains(string name)
        {
            StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            using (List<ITypeAttribute>.Enumerator enumerator = this.list.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    ITypeAttribute current = enumerator.Current;
                    if (ordinalIgnoreCase.Compare(name, current.Name) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Contains(ITypeAttribute item) => 
            this.list.Contains(item);

        public void CopyTo(ITypeAttribute[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ITypeAttribute> GetEnumerator() => 
            this.list.GetEnumerator();

        public int IndexOf(ITypeAttribute item) => 
            this.list.IndexOf(item);

        public void Insert(int index, ITypeAttribute item)
        {
            this.list.Insert(index, item);
        }

        public bool Remove(string name)
        {
            StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            ITypeAttribute attribute = null;
            int index = -1;
            int num2 = 0;
            while (true)
            {
                if (num2 < this.list.Count)
                {
                    if (ordinalIgnoreCase.Compare(name, this.list[num2].Name) != 0)
                    {
                        num2++;
                        continue;
                    }
                    attribute = this.list[num2];
                    index = num2;
                }
                if (attribute != null)
                {
                    this.list.RemoveAt(index);
                }
                return (attribute != null);
            }
        }

        public bool Remove(ITypeAttribute item) => 
            this.list.Remove(item);

        public void RemoveAt(int index)
        {
            ITypeAttribute attribute = this.list[index];
            this.list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.list.GetEnumerator();

        public bool TryGetAttribute(string name, out ITypeAttribute att)
        {
            StringComparer ordinalIgnoreCase = StringComparer.OrdinalIgnoreCase;
            att = null;
            using (List<ITypeAttribute>.Enumerator enumerator = this.list.GetEnumerator())
            {
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    ITypeAttribute current = enumerator.Current;
                    if (ordinalIgnoreCase.Compare(name, current.Name) == 0)
                    {
                        att = current;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetValue(string name, out string value)
        {
            ITypeAttribute attribute;
            value = null;
            bool flag = this.TryGetAttribute(name, out attribute);
            if (flag)
            {
                value = attribute.Value;
            }
            return flag;
        }

        public ITypeAttribute this[int index]
        {
            get => 
                this.list[index];
            set
            {
                ITypeAttribute attribute = this.list[index];
                this.list[index] = value;
            }
        }

        public string this[string name]
        {
            get
            {
                ITypeAttribute att = null;
                if (!this.TryGetAttribute(name, out att))
                {
                    throw new KeyNotFoundException();
                }
                return att.Value;
            }
        }

        public int Count =>
            this.list.Count;

        public bool IsReadOnly =>
            true;
    }
}

