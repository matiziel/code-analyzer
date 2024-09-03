using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricCalculator;

public static class CohesionCalculator {
    public static int CalculateLackOfCohesion(this ClassDeclarationSyntax @class, SemanticModel model) {
        var methods = @class.Members.OfType<MethodDeclarationSyntax>().ToList();

        var methodPairsWithCommonFields = 0;
        var methodPairsWithoutCommonFields = 0;

        for (var i = 0; i < methods.Count; i++) {
            for (var j = i + 1; j < methods.Count; j++) {
                var method1Fields = methods[i].GetFieldsUsedByMethod(model);
                var method2Fields = methods[j].GetFieldsUsedByMethod(model);

                var hasCommonFields = method1Fields.Intersect(method2Fields, SymbolEqualityComparer.Default).Any();

                if (hasCommonFields) {
                    methodPairsWithCommonFields++;
                }
                else {
                    methodPairsWithoutCommonFields++;
                }
            }
        }

        var lcom = methodPairsWithoutCommonFields - methodPairsWithCommonFields;
        return lcom > 0 ? lcom : 0;
    }

    private static IEnumerable<ISymbol> GetFieldsUsedByMethod(
        this MethodDeclarationSyntax method, SemanticModel model) {
        
        var identifiers = method.DescendantNodes().OfType<IdentifierNameSyntax>();
        var fields = identifiers.Select(id => model.GetSymbolInfo(id).Symbol)
            .OfType<IFieldSymbol>()
            .Where(f => method.Parent != null && SymbolEqualityComparer.Default.Equals(f.ContainingType,
                model.GetDeclaredSymbol(method.Parent) as INamedTypeSymbol));

        return fields;
    }

    public static double CalculateLackOfCohesion3(this ClassDeclarationSyntax @class) {
        // LCOM3: Ulepszona miara LCOM.
        // Placeholder implementation. Replace with actual LCOM3 calculation.
        return 0.0;
    }

    public static double CalculateLackOfCohesion4(this ClassDeclarationSyntax @class) {
        // LCOM4: Dalsze ulepszenie miary LCOM.
        // Placeholder implementation. Replace with actual LCOM4 calculation.
        return 0.0;
    }

    public static double CalculateTightClassCohesion(this ClassDeclarationSyntax @class) {
        var methods = @class.Members.OfType<MethodDeclarationSyntax>().ToList();

        if (methods.Count < 2) {
            return 1.0;
        }

        var fields = @class.Members.OfType<FieldDeclarationSyntax>()
            .SelectMany(field => field.Declaration.Variables.Select(variable => variable.Identifier.Text))
            .ToList();

        var methodPairsWithSharedFields = 0;
        var possibleMethodPairs = 0;

        for (var i = 0; i < methods.Count - 1; i++) {
            for (var j = i + 1; j < methods.Count; j++) {
                possibleMethodPairs++;

                if (MethodsShareField(methods[i], methods[j], fields)) {
                    methodPairsWithSharedFields++;
                }
            }
        }

        return (double)methodPairsWithSharedFields / possibleMethodPairs;
    }

    private static bool MethodsShareField(MethodDeclarationSyntax method1, MethodDeclarationSyntax method2,
        List<string> fields) {
        var method1Fields = GetMethodFields(method1, fields);
        var method2Fields = GetMethodFields(method2, fields);

        return method1Fields.Intersect(method2Fields).Any();
    }

    private static List<string> GetMethodFields(MethodDeclarationSyntax method, List<string> fields) {
        var identifiers = method.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Select(identifier => identifier.Identifier.Text);

        return identifiers.Where(fields.Contains).ToList();
    }
}