namespace TwinCAT.TypeSystem
{
    using System;
    using TwinCAT.TypeSystem.Generic;

    public class ReadOnlyDataTypeCollection : ReadOnlyDataTypeCollection<IDataType>
    {
        public ReadOnlyDataTypeCollection(DataTypeCollection<IDataType> coll) : base(coll)
        {
        }
    }
}

