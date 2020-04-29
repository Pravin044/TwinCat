namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public class RpcMethodParameter : IRpcMethodParameter
    {
        private int _size;
        private int _alignSize;
        private AdsDatatypeId _dataTypeId;
        private MethodParamFlags _flags;
        private Guid _typeGuid;
        private int _lengthParaIndex;
        private string _name;
        private string _typeName;
        private string _comment;

        internal RpcMethodParameter(AdsMethodParaInfo paraInfo)
        {
            this._name = paraInfo.name;
            this._size = (int) paraInfo.size;
            this._alignSize = (int) paraInfo.alignSize;
            this._dataTypeId = paraInfo.dataType;
            this._flags = paraInfo.flags;
            this._typeGuid = paraInfo.typeGuid;
            this._lengthParaIndex = paraInfo.lengthIsPara;
            this._name = paraInfo.name;
            this._typeName = paraInfo.type;
            this._comment = paraInfo.comment;
        }

        public int Size =>
            this._size;

        public int AlignSize =>
            this._alignSize;

        public MethodParamFlags ParameterFlags =>
            this._flags;

        public Guid TypeGuid =>
            this._typeGuid;

        public string Name =>
            this._name;

        public string TypeName =>
            this._typeName;

        public string Comment =>
            this._comment;
    }
}

