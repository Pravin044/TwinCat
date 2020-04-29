namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TwinCAT.TypeSystem.Generic;

    [DebuggerDisplay("{ Name }, { DataTypes.Count } types")]
    public sealed class Namespace : INamespaceInternal<IDataType>, INamespace<IDataType>
    {
        private string _name;
        private DataTypeCollection _dataTypes;

        public Namespace(string name)
        {
            this._name = name;
            this._dataTypes = new DataTypeCollection();
        }

        bool INamespaceInternal<IDataType>.RegisterType(IDataType type)
        {
            if (this._dataTypes.Contains(type))
            {
                return false;
            }
            this._dataTypes.Add(type);
            return true;
        }

        void INamespaceInternal<IDataType>.RegisterTypes(IEnumerable<IDataType> types)
        {
            foreach (IDataType type in types)
            {
                ((INamespaceInternal<IDataType>) this).RegisterType(type);
            }
        }

        public string Name =>
            this._name;

        public ReadOnlyDataTypeCollection<IDataType> DataTypes =>
            new ReadOnlyDataTypeCollection(this._dataTypes);

        DataTypeCollection INamespaceInternal<IDataType>.DataTypesInternal =>
            this._dataTypes;
    }
}

