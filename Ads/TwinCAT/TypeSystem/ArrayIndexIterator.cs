namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public class ArrayIndexIterator : IEnumerable<int[]>, IEnumerable
    {
        private int[] _lowerBounds;
        private int[] _upperBounds;
        private bool _zeroShift;

        internal ArrayIndexIterator(Array array)
        {
            int rank = array.Rank;
            this._lowerBounds = new int[rank];
            this._upperBounds = new int[rank];
            for (int i = 0; i < rank; i++)
            {
                this._lowerBounds[i] = array.GetLowerBound(i);
                this._upperBounds[i] = array.GetUpperBound(i);
            }
        }

        public ArrayIndexIterator(IArrayType arrayType) : this(arrayType, false)
        {
        }

        internal ArrayIndexIterator(int[] lowerBounds, int[] upperBounds) : this(lowerBounds, upperBounds, false)
        {
        }

        internal ArrayIndexIterator(IArrayType arrayType, bool zeroShift)
        {
            this._lowerBounds = arrayType.Dimensions.LowerBounds;
            this._upperBounds = arrayType.Dimensions.UpperBounds;
            this._zeroShift = zeroShift;
        }

        internal ArrayIndexIterator(int[] lowerBounds, int[] upperBounds, bool zeroShift)
        {
            this._lowerBounds = lowerBounds;
            this._upperBounds = upperBounds;
            this._zeroShift = zeroShift;
        }

        public IEnumerator<int[]> GetEnumerator()
        {
            <GetEnumerator>d__9 d__1 = new <GetEnumerator>d__9(0);
            d__1.<>4__this = this;
            return d__1;
        }

        internal int[] getIndexFactors()
        {
            int length = this._lowerBounds.Length;
            int[] numArray = new int[length];
            for (int i = length - 1; i >= 0; i--)
            {
                if (i == (length - 1))
                {
                    numArray[i] = 1;
                }
                else
                {
                    int num3 = (this._upperBounds[i + 1] - this._lowerBounds[i + 1]) + 1;
                    numArray[i] = numArray[i + 1] * num3;
                }
            }
            return numArray;
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        [CompilerGenerated]
        private sealed class <GetEnumerator>d__9 : IEnumerator<int[]>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private int[] <>2__current;
            public ArrayIndexIterator <>4__this;
            private int <currentRank>5__1;
            private int[] <actual>5__2;
            private int <highestRank>5__3;

            [DebuggerHidden]
            public <GetEnumerator>d__9(int <>1__state)
            {
                this.<>1__state = <>1__state;
            }

            private unsafe bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    if (this.<>4__this._lowerBounds.Length == 0)
                    {
                        return false;
                    }
                    this.<highestRank>5__3 = this.<>4__this._lowerBounds.Length - 1;
                    this.<actual>5__2 = new int[this.<>4__this._lowerBounds.Length];
                    Array.Copy(this.<>4__this._lowerBounds, this.<actual>5__2, this.<>4__this._lowerBounds.Length);
                    this.<currentRank>5__1 = this.<highestRank>5__3;
                }
                else
                {
                    if (num != 1)
                    {
                        return false;
                    }
                    this.<>1__state = -1;
                    bool flag = this.<actual>5__2[this.<currentRank>5__1] == this.<>4__this._upperBounds[this.<currentRank>5__1];
                    bool flag2 = false;
                    while (true)
                    {
                        if (!flag || (this.<currentRank>5__1 < 0))
                        {
                            if (this.<currentRank>5__1 < 0)
                            {
                                return false;
                            }
                            int* numPtr1 = (int*) ref this.<actual>5__2[this.<currentRank>5__1];
                            numPtr1[0]++;
                            if (flag2)
                            {
                                int index = this.<currentRank>5__1 + 1;
                                while (true)
                                {
                                    if (index > this.<highestRank>5__3)
                                    {
                                        this.<currentRank>5__1 = this.<highestRank>5__3;
                                        break;
                                    }
                                    this.<actual>5__2[index] = this.<>4__this._lowerBounds[index];
                                    index++;
                                }
                            }
                            break;
                        }
                        int num4 = this.<currentRank>5__1;
                        this.<currentRank>5__1 = num4 - 1;
                        if (this.<currentRank>5__1 >= 0)
                        {
                            flag = this.<actual>5__2[this.<currentRank>5__1] == this.<>4__this._upperBounds[this.<currentRank>5__1];
                            flag2 = true;
                        }
                    }
                }
                if (((this.<>4__this._upperBounds[this.<currentRank>5__1] - this.<>4__this._lowerBounds[this.<currentRank>5__1]) + 1) <= 0)
                {
                    return false;
                }
                int[] numArray = new int[this.<actual>5__2.Length];
                for (int i = 0; i < this.<actual>5__2.Length; i++)
                {
                    numArray[i] = !this.<>4__this._zeroShift ? this.<actual>5__2[i] : (this.<actual>5__2[i] - this.<>4__this._lowerBounds[i]);
                }
                this.<>2__current = numArray;
                this.<>1__state = 1;
                return true;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            int[] IEnumerator<int[]>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

