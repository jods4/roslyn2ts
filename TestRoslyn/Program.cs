using Microsoft.Build.Locator;
using RoslynToTS;
using System;
using System.Linq;

namespace TestRoslyn
{
    class Program
    {
        static void Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();
            var project = new Project(@"C:\Projects\Test\ConsoleApp1\TestApi\TestApi.csproj");
            project.Load();
            var c = project.Classes.First();
            var m = c.Methods.First(m => m.Name == "anonymous");
            var r = m.Returns;
        }
    }
}
