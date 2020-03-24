using Microsoft.CodeAnalysis;

namespace RoslynToTS
{
    public struct Property
    {
        private readonly IPropertySymbol symbol;

        public string Name => symbol.Name.CamelCase();

        public TsType Type => new TsType(symbol.Type);

        public Property(IPropertySymbol symbol)
        {
            this.symbol = symbol;
        }
    }
}
