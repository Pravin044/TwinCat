namespace TwinCAT.TypeSystem
{
    using System;

    internal interface ISymbolValueChangeNotify
    {
        void OnRawValueChanged(RawValueChangedArgs args);
        void OnValueChanged(ValueChangedArgs args);
    }
}

