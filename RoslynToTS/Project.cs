using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoslynToTS
{
    public class Project : IDisposable
    {
        private readonly string path;
        private readonly MSBuildWorkspace workspace = MSBuildWorkspace.Create();

        public List<TsClass> Classes { get; private set; }

        public Project(string path)
        {
            this.path = path;
        }

        public void Load()
        {
            workspace.WorkspaceFailed += Workspace_WorkspaceFailed;
            var project = workspace.OpenProjectAsync(path).Result;
            var compilation = project.GetCompilationAsync().Result;

            Classes = (from tree in compilation.SyntaxTrees
                       from declaration in tree.GetRoot()
                                               .DescendantNodes()
                                               .OfType<ClassDeclarationSyntax>()
                       where declaration.Identifier.ValueText.EndsWith("Controller")
                       let model = compilation.GetSemanticModel(tree)
                       select new TsClass(declaration, model)
                       ).ToList();
        }

        private void Workspace_WorkspaceFailed(object sender, Microsoft.CodeAnalysis.WorkspaceDiagnosticEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            workspace.Dispose();
        }
    }
}
