using System.Collections.Immutable;
using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FeatureEnvyAnalyzer : DiagnosticAnalyzer {
    public const string DiagnosticId = nameof(FeatureEnvyAnalyzer);

    private static readonly LocalizableString Title = "Feature Envy Detected";

    private static readonly LocalizableString MessageFormat =
        "Method '{0}' in class '{1}' frequently calls methods from class '{2}'";

    private static readonly LocalizableString Description =
        "Feature Envy: Method frequently calls methods from another class.";

    private const string Category = "CodeSmell";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context) {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var invocationCount = new Dictionary<INamedTypeSymbol, int>();
        var containingType = context.ContainingSymbol.ContainingType;

        var systemAssemblies = context.GetSystemAssemblies();

        foreach (var invocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>()) {

            var methodContainingType = GetMethodContainingType(semanticModel, invocation);

            if (methodContainingType == null || methodContainingType.Equals(containingType)) {
                continue;
            }

            if (IsSystemMethod(methodContainingType, systemAssemblies)) {
                continue;
            }

            if (!invocationCount.ContainsKey(methodContainingType)) {
                invocationCount[methodContainingType] = 0;
            }

            invocationCount[methodContainingType]++;
        }

        foreach (var invocation in invocationCount) {
            // Arbitrary threshold for "frequent" calls, can be adjusted
            if (invocation.Value <= 3) {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(),
                methodDeclaration.Identifier.Text, containingType.Name, invocation.Key.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static INamedTypeSymbol GetMethodContainingType(
        SemanticModel semanticModel, InvocationExpressionSyntax invocation) {
        
        var symbolInfo = semanticModel.GetSymbolInfo(invocation.Expression);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
        
        return  methodSymbol?.ContainingType;
    }

    private static bool IsSystemMethod(INamedTypeSymbol methodContainingType, HashSet<string> systemAssemblies) {
        var assemblyName = methodContainingType.ContainingAssembly?.Name;

        return assemblyName == null || systemAssemblies.Contains(assemblyName);
    }
    
    
}