namespace TwinCAT.TypeSystem
{
    using System;
    using TwinCAT.TypeSystem.Generic;

    public class NamespaceCollection : NamespaceCollection<INamespace<IDataType>, IDataType>
    {
        public ReadOnlyNamespaceCollection AsReadOnly() => 
            new ReadOnlyNamespaceCollection(this);

        public void RegisterType(IDataType type)
        {
            INamespace<IDataType> nspace = null;
            INamespaceInternal<IDataType> item = null;
            if (base.TryGetNamespace(type.Namespace, out nspace))
            {
                item = (INamespaceInternal<IDataType>) nspace;
            }
            else
            {
                item = new Namespace(type.Namespace);
                base.Add(item);
            }
            if (item.RegisterType(type))
            {
                base.allTypes.Add(type.FullName, type);
            }
        }
    }
}

