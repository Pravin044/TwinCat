namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public class ArrayElementValueIterator : IEnumerable<object>, IEnumerable
    {
        private IArrayValue _arrayValue;
        private IArrayInstance _array;
        private IArrayType _type;
        private ArrayIndexIterator _indexIter;

        public ArrayElementValueIterator(IArrayValue arrayValue)
        {
            this._arrayValue = arrayValue;
            this._array = (IArrayInstance) arrayValue.Symbol;
            this._type = (IArrayType) arrayValue.DataType;
            this._indexIter = new ArrayIndexIterator(this._type);
        }

        public IEnumerator<object> GetEnumerator()
        {
            <GetEnumerator>d__5 d__1 = new <GetEnumerator>d__5(0);
            d__1.<>4__this = this;
            return d__1;
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__5 : IEnumerator<object>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private object <>2__current;
            public ArrayElementValueIterator <>4__this;
            private IEnumerator<int[]> <>7__wrap1;

            [DebuggerHidden]
            public <GetEnumerator>d__5(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<>7__wrap1 = this.<>4__this._indexIter.GetEnumerator();
                        this.<>1__state = -3;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    if (!this.<>7__wrap1.MoveNext())
                    {
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        flag = false;
                    }
                    else
                    {
                        int[] current = this.<>7__wrap1.Current;
                        object obj2 = null;
                        bool flag2 = this.<>4__this._arrayValue.TryGetIndexValue(current, out obj2);
                        this.<>2__current = obj2;
                        this.<>1__state = 1;
                        flag = true;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            object IEnumerator<object>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

