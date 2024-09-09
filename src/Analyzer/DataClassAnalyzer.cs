using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DataClassAnalyzer : DiagnosticAnalyzer {
    public const string DiagnosticId = "CFP001";
    private static readonly LocalizableString Title = "Class contains non-field and non-property members";

    private static readonly LocalizableString
        MessageFormat = "Class '{0}' contains non-field and non-property members.";

    private static readonly LocalizableString Description = "A class should only contain fields and properties.";
    private const string Category = "Design";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context) {
        // Register action for class declarations
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context) {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        // Check for any members that are not fields or properties
        var nonFieldOrPropertyMembers = classDeclaration.Members
            .Where(member => !(member is FieldDeclarationSyntax || member is PropertyDeclarationSyntax));

        // If any non-field and non-property members exist, report a diagnostic
        if (nonFieldOrPropertyMembers.Any()) {
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(),
                classDeclaration.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }
}