namespace TwinCAT.TypeSystem
{
    using System;

    public interface ISymbolFactory
    {
        ISymbol CreateArrayElement(int[] currentIndex, ISymbol parent, IArrayType arrayType);
        ISymbolCollection CreateArrayElementInstances(ISymbol parentInstance, IArrayType arrayType);
        ISymbol CreateFieldInstance(IField field, ISymbol parent);
        ISymbolCollection CreateFieldInstances(ISymbol parentInstance, IDataType parentType);
        ISymbol CreateInstance(ISymbolInfo entry, ISymbol parent);
        ISymbol CreateReferenceInstance(IPointerType type, ISymbol parent);
        ISymbol CreateVirtualStruct(string instanceName, string instancePath, ISymbol parent);
        void Initialize(ISymbolFactoryServices services);
        void SetInvalidCharacters(char[] invalidChars);

        ISymbolFactoryServices FactoryServices { get; }

        char[] InvalidCharacters { get; }

        bool HasInvalidCharacters { get; }
    }
}

