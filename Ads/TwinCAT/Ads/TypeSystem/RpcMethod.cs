namespace TwinCAT.Ads.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public class RpcMethod : IRpcMethod
    {
        private string _name;
        private RpcMethodParameterCollection _parameters;
        private int _returnAlignSize;
        private string _returnType = string.Empty;
        private int _returnTypeSize;
        private int _vTableIndex = -1;
        private string _comment = string.Empty;

        internal RpcMethod(AdsMethodEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            this._name = entry.name;
            List<IRpcMethodParameter> coll = new List<IRpcMethodParameter>();
            if (entry.parameters != null)
            {
                for (int i = 0; i < entry.parameters.Length; i++)
                {
                    RpcMethodParameter item = new RpcMethodParameter(entry.parameters[i]);
                    coll.Add(item);
                }
            }
            this._parameters = new RpcMethodParameterCollection(coll);
            this._returnAlignSize = (int) entry.returnAlignSize;
            this._returnTypeSize = (int) entry.returnSize;
            this._returnType = entry.returnType;
            this._vTableIndex = (int) entry.vTableIndex;
            this._comment = entry.comment;
        }

        public override string ToString()
        {
            List<string> list = new List<string>();
            foreach (RpcMethodParameter parameter in this._parameters)
            {
                string str2 = null;
                if (parameter.ParameterFlags == MethodParamFlags.In)
                {
                    str2 = "in";
                }
                else if (parameter.ParameterFlags == MethodParamFlags.Out)
                {
                    str2 = "out";
                }
                else if (parameter.ParameterFlags == MethodParamFlags.ByReference)
                {
                    str2 = "ref";
                }
                string item = null;
                item = (str2 != null) ? $"[{str2}] {parameter.TypeName} {parameter.Name}" : $"{parameter.TypeName} {parameter.Name}";
                list.Add(item);
            }
            string str = string.Join<string>(",", list);
            return $"{this._returnType} {this._name}({str})";
        }

        public string Name =>
            this._name;

        public ReadOnlyMethodParameterCollection Parameters =>
            this._parameters.AsReadOnly();

        public int ReturnAlignSize =>
            this._returnAlignSize;

        public string ReturnType =>
            this._returnType;

        public int ReturnTypeSize =>
            this._returnTypeSize;

        public int VTableIndex =>
            this._vTableIndex;

        public string Comment =>
            this._comment;

        public bool IsVoid =>
            (this._returnTypeSize == 0);
    }
}

