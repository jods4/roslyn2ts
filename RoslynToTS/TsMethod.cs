using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RoslynToTS
{
    public struct TsMethod
    {
        private readonly IMethodSymbol symbol;
        private readonly SemanticModel model;

        public string Name => symbol.Name.CamelCase();

        public string Verb => symbol.GetHttpVerb();

        public string Route => symbol.GetHttpUrl();

        private static readonly string[] fromAttributes = new[] { "FromRouteAttribute", "FromQueryAttribute", "FromBodyAttribute" };

        public IEnumerable<Parameter> Parameters => from p in symbol.Parameters
                                                    let attr = p.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name.StartsWith("From"))
                                                    where attr == null || fromAttributes.Contains(attr.AttributeClass.Name) 
                                                    select new Parameter(p.Name, attr, p.Type);

        public TsType Returns
        {
            get
            {
                var ret = symbol.ReturnType;
                if (ret.SpecialType == SpecialType.System_Object)
                    ret = AnalyzeCode();
                return new TsType(ret);
            }
        }
		
		public TsMethod(IMethodSymbol symbol, SemanticModel model)
        {
            this.symbol = symbol;
            this.model = model;
        }

        private ITypeSymbol AnalyzeCode()
        {
            var model = this.model;

            // A method, we so we'll assume a single declaration (no partial or weird stuff?)
            var syntax = symbol.DeclaringSyntaxReferences[0].GetSyntax();

            // Look for return statements, but be careful not to descend into lambdas or local functions (those have unrelated return statements)
            var type = syntax.DescendantNodes(node => !node.IsKind(SyntaxKind.ParenthesizedLambdaExpression)
                                                   && !node.IsKind(SyntaxKind.SimpleLambdaExpression)
                                                   && !node.IsKind(SyntaxKind.LocalFunctionStatement))
                             .OfType<ReturnStatementSyntax>()
                             .Where(r => r.Expression != null)
                             .Select(r => model.GetTypeInfo(r.Expression).Type)
                             // We don't try to merge different types into a union type.
                             // For now we just consider the first typed return as the shape of the API.
                             .FirstOrDefault(t => t != null && t.SpecialType != SpecialType.System_Object);
            
            // Its nullability is determined by the method signature: is it `object` or `object?`
            return type?.WithNullableAnnotation(symbol.ReturnType.NullableAnnotation)
                ?? symbol.ReturnType; // Object -> unknown
        }
    }
}