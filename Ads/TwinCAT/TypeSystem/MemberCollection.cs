namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using TwinCAT.TypeSystem.Generic;

    public class MemberCollection : InstanceCollection<IMember>
    {
        public MemberCollection() : base(InstanceCollectionMode.Names)
        {
        }

        public MemberCollection(IEnumerable<IMember> coll) : base(coll, InstanceCollectionMode.Names)
        {
        }

        public ReadOnlyMemberCollection AsReadOnly() => 
            new ReadOnlyMemberCollection(this);

        public MemberCollection Clone() => 
            new MemberCollection(this);
    }
}

