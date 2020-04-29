namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;

    public class DataTypeEventArgs : EventArgs
    {
        public readonly IEnumerable<IDataType> DataTypes;

        public DataTypeEventArgs(IEnumerable<IDataType> types)
        {
            this.DataTypes = types;
        }
    }
}

