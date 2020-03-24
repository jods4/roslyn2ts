using Microsoft.CodeAnalysis;

namespace RoslynToTS
{
    public class Parameter
    {
        private readonly AttributeData from;

        public string Name { get; }
        public TsType Type { get; }
        public bool IsQuery => from == null || from.AttributeClass.Name == "FromQueryAttribute";
        public bool IsBody => from?.AttributeClass.Name == "FromBodyAttribute";

        public Parameter(string name, AttributeData from, ITypeSymbol type)
        {
            this.Name = name;
            this.from = from;
            this.Type = new TsType(type);
        }
    }
}
