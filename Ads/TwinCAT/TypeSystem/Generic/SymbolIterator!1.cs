namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using TwinCAT.TypeSystem;

    public class SymbolIterator<T> : IEnumerable<T>, IEnumerable where T: ISymbol
    {
        private IEnumerable<T> _symbols;
        private SymbolIterationMask _mask;
        private bool _symbolRecursionDetection;
        private bool _recurse;
        private Func<T, bool> _filter;

        public SymbolIterator(IInstanceCollection<T> coll) : this(coll, null)
        {
        }

        public SymbolIterator(IEnumerable<T> coll, bool recurse) : this(coll, recurse, null)
        {
        }

        public SymbolIterator(IInstanceCollection<T> coll, Func<T, bool> predicate) : this(coll, false, predicate)
        {
            switch (coll.Mode)
            {
                case InstanceCollectionMode.Names:
                    this._recurse = false;
                    return;

                case InstanceCollectionMode.Path:
                    this._recurse = false;
                    return;

                case InstanceCollectionMode.PathHierarchy:
                    this._recurse = true;
                    return;
            }
            throw new NotSupportedException();
        }

        public SymbolIterator(IEnumerable<T> coll, bool recurse, Func<T, bool> predicate)
        {
            this._mask = SymbolIterationMask.All;
            this._symbolRecursionDetection = true;
            this._symbols = coll;
            this._recurse = recurse;
            this._filter = predicate;
        }

        public IEnumerator<T> GetEnumerator() => 
            this.getFilteredEnumerable(this._symbols).GetEnumerator();

        private IEnumerable<T> getFilteredEnumerable(IEnumerable<T> parentColl)
        {
            <getFilteredEnumerable>d__22<T> d__1 = new <getFilteredEnumerable>d__22<T>(-2);
            d__1.<>4__this = (SymbolIterator<T>) this;
            d__1.<>3__parentColl = parentColl;
            return d__1;
        }

        private bool iterateSubSymbols(IDataType type)
        {
            if (this._mask != SymbolIterationMask.All)
            {
                switch (type.Category)
                {
                    case DataTypeCategory.Alias:
                    {
                        IAliasType type2 = (IAliasType) type;
                        return ((type2.BaseType == null) || this.iterateSubSymbols(type2.BaseType));
                    }
                    case DataTypeCategory.Array:
                        return ((this._mask & SymbolIterationMask.Arrays) > SymbolIterationMask.None);

                    case DataTypeCategory.Struct:
                        return ((this._mask & SymbolIterationMask.Structures) > SymbolIterationMask.None);

                    case DataTypeCategory.Pointer:
                        return ((this._mask & SymbolIterationMask.Pointer) > SymbolIterationMask.None);

                    case DataTypeCategory.Union:
                        return ((this._mask & SymbolIterationMask.Unions) > SymbolIterationMask.None);

                    case DataTypeCategory.Reference:
                        return ((this._mask & SymbolIterationMask.References) > SymbolIterationMask.None);
                }
            }
            return true;
        }

        private bool iterateSubSymbols(T parent)
        {
            if (this._mask != SymbolIterationMask.All)
            {
                switch (parent.Category)
                {
                    case DataTypeCategory.Alias:
                    {
                        IAliasType dataType = (IAliasType) ((IAliasInstance) parent).DataType;
                        return ((dataType.BaseType == null) || this.iterateSubSymbols(dataType.BaseType));
                    }
                    case DataTypeCategory.Array:
                        return ((this._mask & SymbolIterationMask.Arrays) > SymbolIterationMask.None);

                    case DataTypeCategory.Struct:
                        return ((this._mask & SymbolIterationMask.Structures) > SymbolIterationMask.None);

                    case DataTypeCategory.Pointer:
                        return ((this._mask & SymbolIterationMask.Pointer) > SymbolIterationMask.None);

                    case DataTypeCategory.Union:
                        return ((this._mask & SymbolIterationMask.Unions) > SymbolIterationMask.None);

                    case DataTypeCategory.Reference:
                        return ((this._mask & SymbolIterationMask.References) > SymbolIterationMask.None);
                }
            }
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        [Obsolete("Use SymbolIterator<T>.Mask property instead")]
        public bool IterateArrayElements
        {
            get => 
                ((this._mask & SymbolIterationMask.Arrays) > SymbolIterationMask.None);
            set
            {
                if (value)
                {
                    this._mask |= SymbolIterationMask.Arrays;
                }
                else
                {
                    this._mask &= ~SymbolIterationMask.Arrays;
                }
            }
        }

        [Obsolete("Use SymbolIterator<T>.Mask property instead")]
        public bool IterateStructMembers
        {
            get => 
                ((this._mask & SymbolIterationMask.Structures) > SymbolIterationMask.None);
            set
            {
                if (value)
                {
                    this._mask |= SymbolIterationMask.Structures;
                }
                else
                {
                    this._mask &= ~SymbolIterationMask.Structures;
                }
            }
        }

        public SymbolIterationMask Mask
        {
            get => 
                this._mask;
            set => 
                (this._mask = value);
        }

        public bool SymbolRecursionDetection
        {
            get => 
                this._symbolRecursionDetection;
            set => 
                (this._symbolRecursionDetection = value);
        }

        [CompilerGenerated]
        private sealed class <getFilteredEnumerable>d__22 : IEnumerable<T>, IEnumerable, IEnumerator<T>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private T <>2__current;
            private int <>l__initialThreadId;
            private IEnumerable<T> parentColl;
            public IEnumerable<T> <>3__parentColl;
            public SymbolIterator<T> <>4__this;
            private T <parent>5__1;
            private IEnumerator<T> <>7__wrap1;
            private IEnumerator<T> <>7__wrap2;

            [DebuggerHidden]
            public <getFilteredEnumerable>d__22(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private void <>m__Finally2()
            {
                this.<>1__state = -3;
                if (this.<>7__wrap2 != null)
                {
                    this.<>7__wrap2.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    bool flag2;
                    int num1;
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap1 = this.parentColl.GetEnumerator();
                            this.<>1__state = -3;
                            break;

                        case 1:
                            this.<>1__state = -3;
                            goto TR_000E;

                        case 2:
                            this.<>1__state = -4;
                            goto TR_0008;

                        default:
                            return false;
                    }
                    goto TR_0014;
                TR_0005:
                    this.<parent>5__1 = default(T);
                    goto TR_0014;
                TR_0008:
                    if (this.<>7__wrap2.MoveNext())
                    {
                        T current = this.<>7__wrap2.Current;
                        this.<>2__current = current;
                        this.<>1__state = 2;
                        flag = true;
                    }
                    else
                    {
                        this.<>m__Finally2();
                        this.<>7__wrap2 = null;
                        goto TR_0005;
                    }
                    return flag;
                TR_000E:
                    flag2 = this.<>4__this._symbolRecursionDetection && this.<parent>5__1.IsRecursive;
                    if (!this.<>4__this._recurse || !this.<>4__this.iterateSubSymbols(this.<parent>5__1))
                    {
                        num1 = 0;
                    }
                    else
                    {
                        num1 = (int) !flag2;
                    }
                    if (num1 == 0)
                    {
                        goto TR_0005;
                    }
                    else
                    {
                        this.<>7__wrap2 = this.<>4__this.getFilteredEnumerable((IEnumerable<T>) this.<parent>5__1.SubSymbols).GetEnumerator();
                        this.<>1__state = -4;
                    }
                    goto TR_0008;
                TR_0014:
                    while (true)
                    {
                        if (this.<>7__wrap1.MoveNext())
                        {
                            this.<parent>5__1 = this.<>7__wrap1.Current;
                            if ((this.<>4__this._filter == null) || this.<>4__this._filter(this.<parent>5__1))
                            {
                                this.<>2__current = this.<parent>5__1;
                                this.<>1__state = 1;
                                flag = true;
                            }
                            else
                            {
                                goto TR_000E;
                            }
                        }
                        else
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            flag = false;
                        }
                        break;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                SymbolIterator<T>.<getFilteredEnumerable>d__22 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Thread.CurrentThread.ManagedThreadId))
                {
                    d__ = new SymbolIterator<T>.<getFilteredEnumerable>d__22(0) {
                        <>4__this = this.<>4__this
                    };
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = (SymbolIterator<T>.<getFilteredEnumerable>d__22) this;
                }
                d__.parentColl = this.<>3__parentColl;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                switch (num)
                {
                    case -4:
                    case -3:
                    case 1:
                    case 2:
                        try
                        {
                            if ((num == -4) || (num == 2))
                            {
                                try
                                {
                                }
                                finally
                                {
                                    this.<>m__Finally2();
                                }
                            }
                        }
                        finally
                        {
                            this.<>m__Finally1();
                        }
                        break;

                    case -2:
                    case -1:
                    case 0:
                        break;

                    default:
                        return;
                }
            }

            T IEnumerator<T>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

