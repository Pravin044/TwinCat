namespace TwinCAT.TypeSystem.Generic
{
    using System;
    using System.Collections.Generic;
    using TwinCAT.TypeSystem;

    public interface INamespaceInternal<T> : INamespace<T> where T: IDataType
    {
        bool RegisterType(IDataType type);
        void RegisterTypes(IEnumerable<IDataType> types);

        DataTypeCollection DataTypesInternal { get; }
    }
}

