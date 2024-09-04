using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricCalculator;

public static class CohesionCalculator {
    public static int CalculateLackOfCohesion(this ClassDeclarationSyntax classDeclaration, SemanticModel model) {
        var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

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

    public static double CalculateLackOfCohesion3(
        this ClassDeclarationSyntax classDeclaration,
        SemanticModel semanticModel) {
        var fields = classDeclaration.Members.OfType<FieldDeclarationSyntax>()
            .SelectMany(f => f.Declaration.Variables)
            .Select(v => v.Identifier.Text)
            .ToList();

        var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

        var methodFieldMap = new Dictionary<MethodDeclarationSyntax, HashSet<string>>();

        foreach (var method in methods) {
            var accessedFields = new HashSet<string>();

            var identifiers = method.DescendantNodes().OfType<IdentifierNameSyntax>();

            foreach (var identifier in identifiers) {
                if (semanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol symbol &&
                    fields.Contains(symbol.Name)) {
                    accessedFields.Add(symbol.Name);
                }
            }

            methodFieldMap[method] = accessedFields;
        }

        var sharedFieldPairs = 0;
        var totalPairs = methods.Count * (methods.Count - 1) / 2;

        for (var i = 0; i < methods.Count; i++) {
            for (var j = i + 1; j < methods.Count; j++) {
                if (methodFieldMap[methods[i]].Intersect(methodFieldMap[methods[j]]).Any()) {
                    sharedFieldPairs++;
                }
            }
        }

        if (totalPairs == 0) return 0;
        return 1 - ((double)sharedFieldPairs / totalPairs);
    }

    public static int CalculateLackOfCohesion4(
        this ClassDeclarationSyntax classDeclaration,
        SemanticModel semanticModel) {
        // Get all fields in the class
        var fields = classDeclaration.Members
            .OfType<FieldDeclarationSyntax>()
            .SelectMany(f => f.Declaration.Variables)
            .Select(v => v.Identifier.Text)
            .ToList();

        // Get all methods in the class
        var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

        // Map each method to the fields it accesses
        var methodFieldMap = new Dictionary<MethodDeclarationSyntax, HashSet<string>>();

        foreach (var method in methods) {
            var accessedFields = new HashSet<string>();

            // Analyze method to find all identifiers used
            var identifiers = method.DescendantNodes().OfType<IdentifierNameSyntax>();

            // Check which identifiers correspond to fields
            foreach (var identifier in identifiers) {
                var symbol = semanticModel.GetSymbolInfo(identifier).Symbol as IFieldSymbol;
                if (symbol != null && fields.Contains(symbol.Name)) {
                    accessedFields.Add(symbol.Name);
                }
            }

            methodFieldMap[method] = accessedFields;
        }

        // Build a graph where each method is a node, and there is an edge if two methods share a field
        var graph = BuildMethodGraph(methods, methodFieldMap);

        // Count the number of connected components in the graph
        var visited = new HashSet<MethodDeclarationSyntax>();
        int componentCount = 0;

        foreach (var method in methods) {
            if (!visited.Contains(method)) {
                // Perform a depth-first search (DFS) or breadth-first search (BFS) to find all connected nodes (methods)
                TraverseGraph(method, graph, visited);
                componentCount++;
            }
        }

        // Return the number of connected components as LCOM4
        return componentCount;
    }

    // Build the graph, where nodes are methods, and edges exist if methods share fields
    private static Dictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>> BuildMethodGraph(
        List<MethodDeclarationSyntax> methods,
        Dictionary<MethodDeclarationSyntax, HashSet<string>> methodFieldMap) {
        var graph = new Dictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>();

        foreach (var method in methods) {
            graph[method] = new List<MethodDeclarationSyntax>();
        }

        for (int i = 0; i < methods.Count; i++) {
            for (int j = i + 1; j < methods.Count; j++) {
                // If the two methods share at least one field, create an edge between them
                if (methodFieldMap[methods[i]].Intersect(methodFieldMap[methods[j]]).Any()) {
                    graph[methods[i]].Add(methods[j]);
                    graph[methods[j]].Add(methods[i]);
                }
            }
        }

        return graph;
    }

    // Traverse the graph using DFS to visit all connected nodes
    private static void TraverseGraph(MethodDeclarationSyntax method,
        Dictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>> graph,
        HashSet<MethodDeclarationSyntax> visited) {
        var stack = new Stack<MethodDeclarationSyntax>();
        stack.Push(method);
        visited.Add(method);

        while (stack.Count > 0) {
            var currentMethod = stack.Pop();
            foreach (var neighbor in graph[currentMethod]) {
                if (!visited.Contains(neighbor)) {
                    visited.Add(neighbor);
                    stack.Push(neighbor);
                }
            }
        }
    }

    public static double CalculateTightClassCohesion(this ClassDeclarationSyntax classDeclaration) {
        var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

        if (methods.Count < 2) {
            return 1.0;
        }

        var fields = classDeclaration.Members.OfType<FieldDeclarationSyntax>()
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