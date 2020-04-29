namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using TwinCAT.TypeSystem.Generic;

    public class DataTypeCollection : DataTypeCollection<IDataType>
    {
        public DataTypeCollection()
        {
        }

        public DataTypeCollection(IEnumerable<IDataType> coll) : base(coll)
        {
        }

        public ReadOnlyDataTypeCollection AsReadOnly() => 
            new ReadOnlyDataTypeCollection(this);

        public DataTypeCollection Clone() => 
            new DataTypeCollection(this);
    }
}

