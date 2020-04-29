namespace TwinCAT.TypeSystem
{
    using System;

    public class DataTypeNameEventArgs : EventArgs
    {
        public readonly string TypeName;

        public DataTypeNameEventArgs(string typeName)
        {
            this.TypeName = typeName;
        }
    }
}

