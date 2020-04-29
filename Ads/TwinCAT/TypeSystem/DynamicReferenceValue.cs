namespace TwinCAT.TypeSystem
{
    using System;
    using TwinCAT.ValueAccess;

    public class DynamicReferenceValue : DynamicValue
    {
        private object referenceValue;

        internal DynamicReferenceValue(IDynamicSymbol symbol, byte[] data, int offset, DynamicValue parentValue) : base(symbol, data, offset, parentValue)
        {
        }

        internal DynamicReferenceValue(IDynamicSymbol symbol, byte[] data, int offset, DateTime timeStamp, IAccessorValueFactory factory) : base(symbol, data, offset, timeStamp, factory)
        {
        }

        protected internal override object ReadMember(ISymbol memberInstance) => 
            base.ReadMember(memberInstance);

        private object ResolveReferenceValue()
        {
            if (this.referenceValue == null)
            {
                IAccessorDynamicValue dynamicValueAccessor = this.DynamicValueAccessor;
                if (dynamicValueAccessor != null)
                {
                    DateTime time;
                    this.referenceValue = dynamicValueAccessor.ReadValue(base._symbol, out time);
                }
            }
            return this.referenceValue;
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

