namespace RoslynToTS
{
    public static class StringExtensions
    {
        public static string CamelCase(this string name) => char.ToLower(name[0]) + name[1..];
    }
}
