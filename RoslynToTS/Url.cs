using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoslynToTS
{
    public static class Url
    {
        public static string Join(params string[] fragments)
        {
            var sb = new StringBuilder();
            foreach (var fragment in fragments)
            {
                if (string.IsNullOrEmpty(fragment)) continue;
                if (fragment.StartsWith("/"))
                    sb.Clear().Append(fragment);
                else
                    sb.Append('/').Append(fragment);
            }
            sb.Replace("{", "${");  // parameters in route
            return sb.ToString();
        }

        internal static string GetRoute(this INamedTypeSymbol symbol)
        {
            return symbol.GetAttributes()
                         .FirstOrDefault(a => a.AttributeClass.Name == "RouteAttribute")
                         ?.GetArgument();
        }

        internal static string GetHttpUrl(this IMethodSymbol symbol)
        {
            return symbol.GetAttributes()
                         .FirstOrDefault(a => a.AttributeClass.BaseType.Name == "HttpMethodAttribute")
                         ?.GetArgument();
        }

        internal static string GetHttpVerb(this IMethodSymbol symbol)
        {
            var attr = symbol.GetAttributes()
                             .FirstOrDefault(a => a.AttributeClass.BaseType.Name == "HttpMethodAttribute");
            if (attr == null) return null;
            return attr.AttributeClass.Name switch
            {
                "HttpGetAttribute" => "GET",
                "HttpPostAttribute" => "POST",
                "HttpDeleteAttribute" => "DELETE",
                "HttpPutAttribute" => "PUT",
                "HttpHeadAttribute" => "HEAD",
                string s => "Unknown attribute " + s,  
            };
        }

        internal static string GetArgument(this AttributeData attr)
        {
            return attr.ConstructorArguments.Length == 0 ?
                null :
                attr.ConstructorArguments[0].Value?.ToString();
        }
    }
}
