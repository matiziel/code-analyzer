// See https://aka.ms/new-console-template for more information
using System.Collections.Immutable;
using Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

var projectPath = "/home/mateusz/Documents/Projects/C#/my-interpreter/MyInterpreter/MyInterpreter/MyInterpreter.csproj";
        
if (!File.Exists(projectPath) && !Directory.Exists(projectPath))
{
    Console.WriteLine("Invalid path.");
    return;
}

using var workspace = MSBuildWorkspace.Create();
Project? project;

if (Directory.Exists(projectPath))
{
    var solution = await workspace.OpenSolutionAsync(projectPath);
    project = solution.Projects.FirstOrDefault();
}
else
{
    project = await workspace.OpenProjectAsync(projectPath);
}

if (project == null)
{
    Console.WriteLine("Project not found.");
    return;
}

var compilation = await project.GetCompilationAsync();

var analyzer = new FeatureEnvyAnalyzer();
var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(analyzer);

var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

foreach (var diagnostic in diagnostics)
{
    Console.WriteLine($"{diagnostic.Location}: {diagnostic.GetMessage()}");
}