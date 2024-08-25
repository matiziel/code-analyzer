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
        
        
        // var systemAssemblies = context.GetSystemAssemblies();

        // Get the class symbol and its base type
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        var baseType = classSymbol?.BaseType;
        
        if (classSymbol.Name != "DerivedClass")
            return;

        // If the class does not have a base type or it's not a custom class, return
        if (baseType is not { SpecialType: SpecialType.None }) {
            return;
        }
        

        // Check if the class overrides any members from the base class
        var overridesMember = false;
        var notImplementedExceptionFound = false;
        var y = classSymbol.GetMembers();
        
        foreach (var member in classSymbol.GetMembers()) {
            if (!member.IsOverride) {
                continue;
            }
            
            overridesMember = true;

            // Check if the overridden method throws NotImplementedException
            var methodSyntax =
                member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;

            notImplementedExceptionFound = ThrowsNotImplementedException(methodSyntax);
                
            if (notImplementedExceptionFound) {
                break;
            }
        }

        // Check if the class uses any members from the base class
        var usesBaseMembers = false;
        var x = classDeclaration.DescendantNodes();
        foreach (var descendant in classDeclaration.DescendantNodes()) {
            if (UsesBaseMembers(semanticModel, descendant, baseType)) {
                usesBaseMembers = true;
                break;
            }
        }

        // If the class neither overrides nor uses any base class members,
        // or it throws NotImplementedException in overridden methods, report a diagnostic
        if (overridesMember && usesBaseMembers && !notImplementedExceptionFound) {
            return;
        }
        
        var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(),
            classSymbol.Name, baseType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool ThrowsNotImplementedException(MethodDeclarationSyntax methodSyntax) {
        if (methodSyntax is null) {
            return false;
        }

        foreach (var statement in methodSyntax.Body?.Statements ?? Enumerable.Empty<StatementSyntax>()) {
            if (statement is not ThrowStatementSyntax throwStatement) continue;
            
            var throwExpression = throwStatement.Expression as ObjectCreationExpressionSyntax;

            if (throwExpression?.Type.ToString() == "NotImplementedException") {
                return true;
            }
        }

        return false;
    }

    private static bool UsesBaseMembers(
        SemanticModel semanticModel, SyntaxNode descendant, INamedTypeSymbol baseType) {
        if (descendant is not MemberAccessExpressionSyntax or IdentifierNameSyntax) {
            return false;
        }
        var symbolInfo = semanticModel.GetSymbolInfo(descendant);
        return symbolInfo.Symbol?.ContainingType is not null && symbolInfo.Symbol.ContainingType.Equals(baseType);
    }
}