namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public abstract class RpcMarshaller : IRpcMarshaller
    {
        private IDataTypeResolver _resolver;

        public RpcMarshaller(IDataTypeResolver dataTypeResolver)
        {
            this._resolver = dataTypeResolver;
        }

        protected static int copyHelper(byte[] destination, int destinationOffset, byte[] source)
        {
            if ((source.Length + destinationOffset) > destination.Length)
            {
                throw new MarshalException("Data size mismatch!");
            }
            Array.Copy(source, 0, destination, destinationOffset, source.Length);
            return source.Length;
        }

        public virtual int GetInMarshallingSize(IRpcMethod method, object[] parameterValues)
        {
            int num = 0;
            RpcMethodParameterCollection parameters = null;
            IList<IDataType> types = null;
            this.GetInParameters(method, out parameters, out types);
            for (int i = 0; i < parameters.Count; i++)
            {
                num += this.OnGetParameterSize(i, parameters, parameterValues);
            }
            return num;
        }

        protected int GetInParameters(IRpcMethod method, out RpcMethodParameterCollection parameters, out IList<IDataType> types) => 
            this.GetParameters(method, MethodParamFlags.ByReference | MethodParamFlags.In, out parameters, out types);

        public virtual int GetOutMarshallingSize(IRpcMethod method, object[] outValues)
        {
            int num = 0;
            RpcMethodParameterCollection parameters = null;
            IList<IDataType> types = null;
            this.GetOutParameters(method, out parameters, out types);
            for (int i = 0; i < parameters.Count; i++)
            {
                num += this.OnGetParameterSize(i, parameters, outValues);
            }
            if (!string.IsNullOrEmpty(method.ReturnType))
            {
                num += this.OnGetMarshallingSize(method.ReturnType);
            }
            return num;
        }

        protected int GetOutParameters(IRpcMethod method, out RpcMethodParameterCollection parameters, out IList<IDataType> types) => 
            this.GetParameters(method, MethodParamFlags.ByReference | MethodParamFlags.Out, out parameters, out types);

        protected int GetParameters(IRpcMethod method, MethodParamFlags mask, out RpcMethodParameterCollection parameters, out IList<IDataType> types)
        {
            types = new List<IDataType>();
            parameters = new RpcMethodParameterCollection();
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                if ((method.Parameters[i].ParameterFlags & mask) > ((MethodParamFlags) 0))
                {
                    IDataType type = null;
                    if (!this._resolver.TryResolveType(method.Parameters[i].TypeName, out type))
                    {
                        throw new ArgumentOutOfRangeException("parameters");
                    }
                    types.Add(type);
                    parameters.Add(method.Parameters[i]);
                }
            }
            return types.Count;
        }

        public int MarshallParameters(IRpcMethod method, object[] parameterValues, byte[] buffer, int offset)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (parameterValues == null)
            {
                throw new ArgumentNullException("parameterValues");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("data");
            }
            if ((offset < 0) || (offset > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            return this.OnMarshalIn(method, parameterValues, buffer, offset);
        }

        protected virtual int OnGetMarshallingSize(string dataType)
        {
            IDataType type = null;
            if (!this._resolver.TryResolveType(dataType, out type))
            {
                throw new MarshalException($"DataType '{dataType}' cannot be resolved!");
            }
            return type.Size;
        }

        protected virtual int OnGetParameterSize(int parameterIndex, RpcMethodParameterCollection parameters, object[] parameterValues) => 
            parameters[parameterIndex].Size;

        protected virtual int OnMarshalIn(IRpcMethod method, object[] parameterValues, byte[] buffer, int offset)
        {
            int num = 0;
            if (method.Parameters.Count > 0)
            {
                RpcMethodParameterCollection parameters = null;
                IList<IDataType> types = null;
                this.GetInParameters(method, out parameters, out types);
                if (parameters.Count != parameterValues.Length)
                {
                    throw new ArgumentOutOfRangeException("parameterValues");
                }
                for (int i = 0; i < parameters.Count; i++)
                {
                    num += this.OnMarshalParameter(i, parameters, types, parameterValues, buffer, offset + num);
                }
            }
            return num;
        }

        protected virtual int OnMarshalOut(IRpcMethod method, byte[] data, int offset, out object[] values)
        {
            RpcMethodParameterCollection parameters = null;
            IList<IDataType> types = null;
            this.GetOutParameters(method, out parameters, out types);
            int num = 0;
            int num2 = offset;
            values = new object[method.Parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                num += this.OnUnmarshalParameter(i, parameters, types, data, num2, ref values);
                num2 = offset + num;
            }
            return num;
        }

        protected abstract int OnMarshalParameter(int i, RpcMethodParameterCollection inParameters, IList<IDataType> inParameterTypes, object[] parameterValues, byte[] buffer, int offset);
        protected abstract int OnUnmarshalParameter(int i, RpcMethodParameterCollection outParameters, IList<IDataType> outParameterTypes, byte[] buffer, int offset, ref object[] outValues);
        public int UnmarshalOutParameters(IRpcMethod method, byte[] data, int offset, out object[] values)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if ((offset < 0) | (offset > data.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            return this.OnMarshalOut(method, data, offset, out values);
        }

        public abstract int UnmarshalReturnValue(IRpcMethod method, IDataType returnType, byte[] buffer, int offset, out object returnValue);
        public int UnmarshalRpcMethod(IRpcMethod method, object[] parameterValues, byte[] data, out object returnValue)
        {
            object[] values = null;
            IDataType type;
            int offset = 0;
            this._resolver.TryResolveType(method.ReturnType, out type);
            offset += this.UnmarshalReturnValue(method, type, data, offset, out returnValue);
            return (offset + this.UnmarshalOutParameters(method, data, offset, out values));
        }
    }
}

