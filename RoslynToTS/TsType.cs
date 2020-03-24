using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynToTS
{
    public struct TsType: IEquatable<TsType>
    {
        private static readonly Dictionary<ITypeSymbol, string> anonymous = new Dictionary<ITypeSymbol, string>();
        private static readonly Queue<TsType> toPrint = new Queue<TsType>();
        private static readonly HashSet<TsType> userTypes = new HashSet<TsType>();
        private static readonly HashSet<TsType> enums = new HashSet<TsType>();

        public static IEnumerable<TsType> UserTypes
        {
            get
            {
                while (toPrint.Count > 0)
                    yield return toPrint.Dequeue();
                userTypes.Clear();
                anonymous.Clear();
            }
        }

        public static IEnumerable<TsType> Enums
        {
            get
            {
                foreach (var e in enums) yield return e;
                enums.Clear();
            }
        }

        private readonly ITypeSymbol symbol;

        public TsType(ITypeSymbol symbol)
        {
            this.symbol = symbol;
        }

        public string Name => GetName(symbol);

        private static string GetName(ITypeSymbol symbol)
        {
            if (TryGetIEnumerable(symbol, out var underlying))
            {
                var name = GetName(underlying);
                return name.EndsWith("| null") ?
                    "(" + name + ")[]" :
                    name + "[]";
            }

            if (symbol.IsAnonymousType)
                return MaybeNull(symbol, anonymous[Enqueue(symbol)]);                

            return symbol.Name switch
            {
                "Boolean" => "bool",
                "DateTime" => "Date",
                "DateTimeOffset" => "Date",
                "Decimal" => "number",
                "Double" => "number",
                "Int16" => "number",
                "Int32" => "number",
                "Int64" => "number",
                "Object" => "unknown",
                "Single" => "number",
                "String" => MaybeNull(symbol, "string"),
                "Void" => "void",

                "Nullable" => GetName(GetTypeArgument(symbol)) + " | null",

                _ => symbol.TypeKind switch
                {
                    TypeKind.Enum => Enqueue(symbol).Name,
                    TypeKind.Class => MaybeNull(symbol, Enqueue(symbol).Name),
                    _ => "unknown /* " + symbol.Name + " */",
                }
            };
        }

        // This only handles reference types (i.e. nullable annotations). 
        // Nullable<T> struct is not handled here.
        private static string MaybeNull(ITypeSymbol symbol, string name)
            => symbol.NullableAnnotation == NullableAnnotation.Annotated ? name + " | null" : name;

        public bool IsAnonymous => symbol.IsAnonymousType;

        public bool IsEnum => symbol.TypeKind == TypeKind.Enum;

        public IEnumerable<Property> Properties => from p in AllProperties(symbol)
                                                   where p.DeclaredAccessibility == Accessibility.Public
                                                   select new Property(p);

        private static IEnumerable<IPropertySymbol> AllProperties(ITypeSymbol symbol)
        {
            for (var iter = symbol; iter.SpecialType != SpecialType.System_Object; iter = iter.BaseType)
            {
                foreach (var p in iter.GetMembers().OfType<IPropertySymbol>())
                    yield return p;
            }
        }

        public IEnumerable<EnumValue> EnumValues => from m in symbol.GetMembers().OfType<IFieldSymbol>()
                                                    select new EnumValue(m);

        public bool TryGetUserType(out TsType value)
        {
            if (GetInterface(symbol) is ITypeSymbol type)
            {
                value = new TsType(type);
                return true;
            }
            value = default;
            return false;
        }

        private static ITypeSymbol GetInterface(ITypeSymbol symbol)
        {
            if (TryGetIEnumerable(symbol, out var value))
                return GetInterface(value);

            if (symbol.Name == "Nullable")
                return GetInterface(GetTypeArgument(symbol));
            
            if (symbol.SpecialType != SpecialType.None ||
                symbol.Name == "DateTimeOffset")    // DateTimeOffset is not a special type
                return null;
            
            return symbol;
        }

        private static ITypeSymbol GetTypeArgument(ITypeSymbol symbol) => (symbol as INamedTypeSymbol).TypeArguments[0];

        private static bool TryGetIEnumerable(ITypeSymbol symbol, out ITypeSymbol underlying)
        {
            if (symbol is IArrayTypeSymbol array)
            {
                underlying = array.ElementType;
                return true;
            }
            if (symbol.SpecialType != SpecialType.System_String &&
                symbol.AllInterfaces.Any(i => i.SpecialType == SpecialType.System_Collections_IEnumerable))
            {
                // A little wacky, as the type itself may not be generic, but good enough for us
                underlying = GetTypeArgument(symbol);
                return true;
            }
            underlying = null;
            return false;
        }

        private static ITypeSymbol Enqueue(ITypeSymbol symbol)
        {
            var type = new TsType(symbol);
            if (type.IsEnum)
                enums.Add(type);
            else if (type.TryGetUserType(out var user) && userTypes.Add(user))
            {
                if (symbol.IsAnonymousType)
                    anonymous.Add(symbol, "$" + (anonymous.Count + 1));
                toPrint.Enqueue(user);
            }
            return symbol;
        }

        public bool Equals(TsType other) => symbol.Equals(other.symbol);

        public override int GetHashCode() => symbol.GetHashCode();
    }
}
