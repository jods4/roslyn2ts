using Microsoft.CodeAnalysis;

namespace RoslynToTS
{
    public struct EnumValue
    {
        private readonly IFieldSymbol symbol;

        public string Name => symbol.Name.CamelCase();

        public string Value => symbol.ConstantValue.ToString(); 

        public EnumValue(IFieldSymbol symbol)
        {
            this.symbol = symbol;
        }
    }
}
