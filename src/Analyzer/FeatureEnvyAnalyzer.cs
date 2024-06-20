using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FeatureEnvyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "FeatureEnvyAnalyzer";
    private static readonly LocalizableString Title = "Feature Envy Detected";
    private static readonly LocalizableString MessageFormat = "Method '{0}' in class '{1}' frequently calls methods from class '{2}'";
    private static readonly LocalizableString Description = "Feature Envy: Method frequently calls methods from another class";
    private const string Category = "CodeSmell";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var invocationCount = new Dictionary<INamedTypeSymbol, int>();
        var containingType = context.ContainingSymbol.ContainingType;
        
        var systemAssemblies = context.Compilation.ReferencedAssemblyNames
            .Select(a => a.Name)
            .Where(a => a.StartsWith("Microsoft.") || a.StartsWith("System."))
            .ToHashSet();

        foreach (var invocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation.Expression);
            var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

            if (methodSymbol == null) continue;

            var methodContainingType = methodSymbol.ContainingType;

            if (methodContainingType == null || methodContainingType.Equals(containingType)) continue;
            
            var assemblyName = methodContainingType.ContainingAssembly?.Name;
            if (assemblyName == null || systemAssemblies.Contains(assemblyName))
            {
                continue;
            }

            if (!invocationCount.ContainsKey(methodContainingType))
            {
                invocationCount[methodContainingType] = 0;
            }

            invocationCount[methodContainingType]++;
        }

        foreach (var kvp in invocationCount)
        {
            if (kvp.Value > 3) // Arbitrary threshold for "frequent" calls, can be adjusted
            {
                var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(),
                    methodDeclaration.Identifier.Text, containingType.Name, kvp.Key.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}