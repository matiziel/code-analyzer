using System.Collections.Immutable;
using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RefusedBequestAnalyzer : DiagnosticAnalyzer {
    public const string DiagnosticId = "RefusedBequest";
    private const string Title = "Potential Refused Bequest";
    private const string MessageFormat = "Class '{0}' inherits from '{1}' but does not use its members.";

    private const string Description =
        "This class inherits from a base class but does not use any of its members, which may indicate a Refused Bequest code smell.";

    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context) {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
    }
    
    private static void AnalyzeNode(SyntaxNodeAnalysisContext context) {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        
        // Get the class symbol and its base type
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        var baseType = classSymbol?.BaseType;

        // If the class does not have a base type or it's not a custom class, return
        if (baseType == null || baseType.SpecialType == SpecialType.System_Object)
            return;

        var methodDeclarations = classDeclaration.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(SyntaxKind.OverrideKeyword))
            .ToList();
        
        // Sprawdzenie, czy klasa ma metody
        if (!classDeclaration.Members.OfType<MethodDeclarationSyntax>().Any())
        {
            return; // Pomijamy klasy bez metod
        }

        var throwsNotImplementedException = false;
        var usesBaseMembers = false;
        
        foreach (var method in methodDeclarations) {
            if (!ThrowsNotImplementedException(method)) {
                continue;
            }
            throwsNotImplementedException = true;
            break;
        }
        
        var descendantNodes = classDeclaration.DescendantNodes();
        var containingType = context.ContainingSymbol.ContainingType;
        
        foreach (var node in descendantNodes)
        {
            if (node is IdentifierNameSyntax identifierName)
            {
                var symbol = semanticModel.GetSymbolInfo(identifierName).Symbol;

                if (symbol != null && SymbolBelongsToBaseClass(symbol, baseType))
                {
                    usesBaseMembers = true;
                    break;
                }
            }
            if (node is InvocationExpressionSyntax invocation)
            {
                var methodContainingType = GetMethodContainingType(semanticModel, invocation);
                
                if (!SymbolEqualityComparer.Default.Equals(methodContainingType, containingType)) {
                    usesBaseMembers = true;
                    break;
                }
            }
            // Sprawdzenie, czy klasa pochodna nadpisuje metody klasy bazowej
            if (node is MethodDeclarationSyntax methodDeclaration && methodDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword))
            {
                var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);

                if (methodSymbol?.OverriddenMethod != null && SymbolEqualityComparer.Default.Equals(methodSymbol.OverriddenMethod.ContainingType, baseType))
                {
                    usesBaseMembers = true;
                    break;
                }
            }
        }

        if (!throwsNotImplementedException && usesBaseMembers) {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(),
            classSymbol.Name, baseType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool ThrowsNotImplementedException(MethodDeclarationSyntax method)
    {
        var throwStatements = method.DescendantNodes().OfType<ThrowStatementSyntax>();

        foreach (var throwStatement in throwStatements)
        {
            if (throwStatement.Expression is ObjectCreationExpressionSyntax creationExpression &&
                creationExpression.Type.ToString() == "NotImplementedException")
            {
                return true;
            }
        }

        return false;
    }
    
    private static bool SymbolBelongsToBaseClass(ISymbol symbol, INamedTypeSymbol baseClassSymbol)
    {
        return symbol.ContainingType != null && symbol.ContainingType.Equals(baseClassSymbol);
    }
    
    private static INamedTypeSymbol GetMethodContainingType(
        SemanticModel semanticModel, InvocationExpressionSyntax invocation) {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation.Expression);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol;

        return methodSymbol?.ContainingType;
    }
}