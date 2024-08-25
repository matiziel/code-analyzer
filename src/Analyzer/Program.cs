// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using Analyzer;
using Common;
using Microsoft.CodeAnalysis.Diagnostics;


var projectPath = "/home/mateusz/Documents/Projects/C#/my-interpreter/MyInterpreter/MyInterpreter/MyInterpreter.csproj";

var projects = await ProjectProvider.GetFromPath(projectPath);

if (!projects.Any()) {
    Console.WriteLine("Project not found.");
    return;
}

var project = projects.FirstOrDefault();

var compilation = await project.GetCompilationAsync();

var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
    // new FeatureEnvyAnalyzer()
    new RefusedBequestAnalyzer()
);

var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

foreach (var diagnostic in diagnostics) {
    Console.WriteLine($"{diagnostic.Location}: {diagnostic.GetMessage()}");
}