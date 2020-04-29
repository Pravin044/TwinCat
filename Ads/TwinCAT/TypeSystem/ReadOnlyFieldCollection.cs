namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem.Generic;

    public class ReadOnlyFieldCollection : ReadOnlyInstanceCollection<IField>
    {
        public ReadOnlyFieldCollection(FieldCollection members) : base(members)
        {
        }

        public bool TryGetMember(string fieldName, out IField symbol) => 
            ((FieldCollection) base.Items).TryGetInstance(fieldName, out symbol);
    }
}

