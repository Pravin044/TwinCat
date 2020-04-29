namespace TwinCAT.TypeSystem
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    [DebuggerDisplay("Index: { _lowerBound } .. { UpperBound } (Elements: { _elementCount } )")]
    public class Dimension : IDimension
    {
        private int _lowerBound;
        private int _elementCount;

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public Dimension(int lowerBound, int elementCount)
        {
            if (elementCount < 0)
            {
                throw new ArgumentOutOfRangeException("ElementCount is negative!");
            }
            this._lowerBound = lowerBound;
            this._elementCount = elementCount;
        }

        public int LowerBound =>
            this._lowerBound;

        public int UpperBound =>
            ((this._lowerBound + this._elementCount) - 1);

        public int ElementCount =>
            this._elementCount;
    }
}

