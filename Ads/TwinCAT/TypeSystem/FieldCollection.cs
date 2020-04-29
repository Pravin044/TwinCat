namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using TwinCAT.TypeSystem.Generic;

    public class FieldCollection : InstanceCollection<IField>
    {
        public FieldCollection() : base(InstanceCollectionMode.Names)
        {
        }

        public FieldCollection(IEnumerable<IField> coll) : base(coll, InstanceCollectionMode.Names)
        {
        }

        public ReadOnlyFieldCollection AsReadOnly() => 
            new ReadOnlyFieldCollection(this);

        public FieldCollection Clone() => 
            new FieldCollection(this);
    }
}

