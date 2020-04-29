namespace TwinCAT.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using TwinCAT.ValueAccess;

    public class DynamicPointerValue : DynamicValue
    {
        public static string s_pointerDeref = "__deref";
        private object pointerValue;

        internal DynamicPointerValue(IDynamicSymbol symbol, byte[] data, int offset, DynamicValue parentValue) : base(symbol, data, offset, parentValue)
        {
        }

        internal DynamicPointerValue(IDynamicSymbol symbol, byte[] data, int offset, DateTime timeStamp, IAccessorValueFactory factory) : base(symbol, data, offset, timeStamp, factory)
        {
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            List<string> list = new List<string>();
            IDataType dataType = base._symbol.DataType;
            return new List<string> { s_pointerDeref };
        }

        internal object ResolvePointerValue()
        {
            if (this.pointerValue == null)
            {
                IAccessorDynamicValue dynamicValueAccessor = this.DynamicValueAccessor;
                if (dynamicValueAccessor != null)
                {
                    DateTime time;
                    this.pointerValue = dynamicValueAccessor.ReadValue(((IPointerInstance) base._symbol).Reference, out time);
                }
            }
            return this.pointerValue;
        }

        public override bool TryGetMemberValue(string name, out object result)
        {
            bool flag = false;
            result = null;
            if ((base.ResolvedType.Category == DataTypeCategory.Pointer) && (name == s_pointerDeref))
            {
                IDynamicSymbol reference = (IDynamicSymbol) ((IPointerInstance) base._symbol).Reference;
                result = this.ResolvePointerValue();
                flag = true;
            }
            return flag;
        }

        private IAccessorDynamicValue DynamicValueAccessor
        {
            get
            {
                IAccessorValueFactory2 valueFactory = base.valueFactory as IAccessorValueFactory2;
                return ((valueFactory == null) ? null : (valueFactory.ValueAccessor as IAccessorDynamicValue));
            }
        }
    }
}

