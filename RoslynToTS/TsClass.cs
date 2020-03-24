using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RoslynToTS
{
    public struct TsClass
    {
        private readonly INamedTypeSymbol symbol;
        private readonly SemanticModel model;

        public string Name => symbol.Name.CamelCase();

        public string Route => symbol.GetRoute();

        public IEnumerable<TsMethod> Methods
        {
            get
            {
                var model = this.model;
                return from m in symbol.GetMembers().OfType<IMethodSymbol>()
                       where m.DeclaredAccessibility == Accessibility.Public
                          && m.GetAttributes().Any(a => a.AttributeClass.BaseType.Name == "HttpMethodAttribute")
                       select new TsMethod(m, model);
            }
        }

        public TsClass(ClassDeclarationSyntax syntax, SemanticModel model)
        {
            this.model = model;
            symbol = model.GetDeclaredSymbol(syntax) as INamedTypeSymbol;
        }
    }
}
