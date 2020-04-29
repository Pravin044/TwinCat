namespace TwinCAT.Ads
{
    using System;
    using System.ComponentModel;
    using System.Text;

    public interface IAdsAnyAccess
    {
        object ReadAny(int variableHandle, Type type);
        object ReadAny(int variableHandle, Type type, int[] args);
        object ReadAny(uint indexGroup, uint indexOffset, Type type);
        object ReadAny(uint indexGroup, uint indexOffset, Type type, int[] args);
        string ReadAnyString(int variableHandle, int len, Encoding encoding);
        string ReadAnyString(uint indexGroup, uint indexOffset, int len, Encoding encoding);
        void WriteAny(int variableHandle, object value);
        void WriteAny(int variableHandle, object value, int[] args);
        void WriteAny(uint indexGroup, uint indexOffset, object value);
        void WriteAny(uint indexGroup, uint indexOffset, object value, int[] args);
        [Obsolete("This method is potentially unsafe!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        void WriteAnyString(int variableHandle, string value, int length, Encoding encoding);
        [Obsolete("This method is potentially unsafe!"), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        void WriteAnyString(uint indexGroup, uint indexOffset, string value, int length, Encoding encoding);
    }
}

