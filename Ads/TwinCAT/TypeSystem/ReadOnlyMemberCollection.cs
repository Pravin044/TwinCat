namespace TwinCAT.TypeSystem
{
    using System;
    using System.Runtime.InteropServices;
    using TwinCAT.TypeSystem.Generic;

    public class ReadOnlyMemberCollection : ReadOnlyInstanceCollection<IMember>
    {
        public ReadOnlyMemberCollection(MemberCollection members) : base(members)
        {
        }

        public bool TryGetMember(string memberName, out IMember symbol) => 
            ((MemberCollection) base.Items).TryGetInstance(memberName, out symbol);
    }
}

