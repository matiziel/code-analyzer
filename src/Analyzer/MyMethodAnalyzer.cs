using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MyMethodCallAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MyMethodCallAnalyzer";
    private static readonly LocalizableString Title = "Method Call from Another Class";
    private static readonly LocalizableString MessageFormat = "Method '{0}' is called from another class.";
    private static readonly LocalizableString Description = "Counts method calls from another class.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId, Title, MessageFormat, Category,
        DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;

        if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccessExpr)
        {
            return;
        }

        var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccessExpr);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

        // Dodatkowe sprawdzenie, jeśli symbolInfo.Symbol jest null, sprawdź kandydatów
        if (methodSymbol == null && symbolInfo.CandidateSymbols.Length > 0)
        {
            methodSymbol = symbolInfo.CandidateSymbols[0] as IMethodSymbol;
        }

        if (methodSymbol == null)
        {
            return;
        }

        var containingType = methodSymbol.ContainingType;
        var currentType = context.ContainingSymbol?.ContainingType;

        if (containingType == null || currentType == null)
        {
            return;
        }

        if (!containingType.Equals(currentType))
        {
            var diagnostic = Diagnostic.Create(Rule, memberAccessExpr.GetLocation(), methodSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}