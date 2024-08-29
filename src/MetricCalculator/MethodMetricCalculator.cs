using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricCalculator;

public class MethodMetricCalculator : IMetricCalculator<MethodMetrics> {
    public async Task<IEnumerable<MethodMetrics>> Calculate(string solutionPath) {
        var projects = await ProjectProvider.GetFromPath(solutionPath);

        var calculatedMetrics = new List<MethodMetrics>();

        foreach (var project in projects) {
            foreach (var document in project.Documents) {
                var root = await document.GetSyntaxRootAsync();

                if (root is null) {
                    continue;
                }

                var model = await document.GetSemanticModelAsync();

                var methodMetrics = root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Parent is not InterfaceDeclarationSyntax)
                    .Select(method => new MethodMetrics {
                        MethodName = method.Identifier.Text,
                        Cyclo = CalculateCyclomaticComplexity(method),
                        CycloSwitch = CalculateSwitchComplexity(method),
                        Mloc = CalculateMethodLinesOfCode(method),
                        Meloc = CalculateMethodEffectiveLinesOfCode(method),
                        Nop = CalculateNumberOfParameters(method),
                        Nolv = CalculateNumberOfLocalVariables(method),
                        Notc = CalculateNumberOfTypeCasts(method),
                        Mnol = CalculateMaximumNestingOfLoops(method),
                        Mnor = CalculateMaximumNestingOfRecursions(method),
                        Mnoc = CalculateMaximumNestingOfConditionals(method),
                        Mnoa = CalculateMaximumNestingOfArrays(method),
                        Nonl = CalculateNumberOfNestedLoops(method),
                        Nosl = CalculateNumberOfStatementsInLoops(method),
                        Nomo = CalculateNumberOfMethodsOverloaded(root, method),
                        Nope = CalculateNumberOfParametersInExternalMethods(method, model),
                        Nole = CalculateNumberOfExternalMethodsCalled(method, model),
                        Mmnb = CalculateMaximumMethodsNestingBlocks(method),
                        Nouw = CalculateNumberOfUnusedVariables(method, model),
                        Aid = CalculateAverageInvocationDepth(method),
                    }).ToList();

                calculatedMetrics.AddRange(methodMetrics);
            }
        }

        return calculatedMetrics;
    }

    private static int CalculateCyclomaticComplexity(MethodDeclarationSyntax method) {
        var complexity = 1;
        complexity += method.DescendantNodes().OfType<IfStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<DoStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<CaseSwitchLabelSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();
        complexity += method.DescendantNodes().OfType<CatchClauseSyntax>().Count();
        return complexity;
    }

    private static int CalculateSwitchComplexity(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<SwitchStatementSyntax>().Sum(s => s.Sections.Count);
    }

    private static int CalculateMethodLinesOfCode(MethodDeclarationSyntax method) {
        var lines = method.GetLocation().GetLineSpan().EndLinePosition.Line -
            method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        return lines;
    }

    private static int CalculateMethodEffectiveLinesOfCode(MethodDeclarationSyntax method) {
        if (method.ExpressionBody is not null) {
            return 1;
        }

        return method.Body?.Statements.Count ?? 0;
    }

    private static int CalculateNumberOfParameters(MethodDeclarationSyntax method) {
        return method.ParameterList.Parameters.Count;
    }

    private static int CalculateNumberOfLocalVariables(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<VariableDeclaratorSyntax>().Count();
    }

    private static int CalculateNumberOfTypeCasts(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<CastExpressionSyntax>().Count();
    }

    private static int CalculateMaximumNestingOfLoops(MethodDeclarationSyntax method) {
        return GetMaximumNesting(method,
            n => n is ForStatementSyntax or WhileStatementSyntax or DoStatementSyntax or ForEachStatementSyntax);
    }

    private static int GetMaximumNesting(MethodDeclarationSyntax method, Func<SyntaxNode, bool> condition) {
        var maxNesting = 0;
        var currentNesting = 0;

        void Visit(SyntaxNode node) {
            if (condition.Invoke(node)) {
                currentNesting++;
                if (currentNesting > maxNesting) {
                    maxNesting = currentNesting;
                }

                foreach (var child in node.ChildNodes()) {
                    Visit(child);
                }

                currentNesting--;
            }
            else {
                foreach (var child in node.ChildNodes()) {
                    Visit(child);
                }
            }
        }

        Visit(method);
        return maxNesting;
    }

    private static int CalculateMaximumNestingOfRecursions(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Count(i =>
                i.Expression is IdentifierNameSyntax identifier &&
                identifier.Identifier.Text == method.Identifier.Text);
    }

    private static int CalculateMaximumNestingOfConditionals(MethodDeclarationSyntax method) {
        return GetMaximumNesting(method, node => node is IfStatementSyntax);
    }

    private static int CalculateMaximumNestingOfArrays(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().Count();
    }

    private static int CalculateNumberOfNestedLoops(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<ForStatementSyntax>().Count() +
               method.DescendantNodes().OfType<WhileStatementSyntax>().Count() +
               method.DescendantNodes().OfType<DoStatementSyntax>().Count() +
               method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
    }

    private static int CalculateNumberOfStatementsInLoops(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<ForStatementSyntax>()
                   .Sum(forStmt => forStmt.Statement.ChildNodes().Count()) +
               method.DescendantNodes().OfType<WhileStatementSyntax>()
                   .Sum(whileStmt => whileStmt.Statement.ChildNodes().Count()) +
               method.DescendantNodes().OfType<DoStatementSyntax>()
                   .Sum(doStmt => doStmt.Statement.ChildNodes().Count()) +
               method.DescendantNodes().OfType<ForEachStatementSyntax>()
                   .Sum(forEachStmt => forEachStmt.Statement.ChildNodes().Count());
    }

    private static int CalculateNumberOfMethodsOverloaded(SyntaxNode root, MethodDeclarationSyntax method) {
        var methodName = method.Identifier.Text;
        return root.DescendantNodes().OfType<MethodDeclarationSyntax>().Count(m => m.Identifier.Text == methodName);
    }

    private static int
        CalculateNumberOfParametersInExternalMethods(MethodDeclarationSyntax method, SemanticModel model) {
        return method.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Select(inv => model.GetSymbolInfo(inv).Symbol as IMethodSymbol)
            .Where(symbol => symbol != null && !SymbolEqualityComparer.Default.Equals(symbol.ContainingType, model.GetDeclaredSymbol(method.Parent)))
            .Sum(symbol => symbol.Parameters.Length);
    }

    private static int CalculateNumberOfExternalMethodsCalled(MethodDeclarationSyntax method, SemanticModel model) {
        return method.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Select(inv => model.GetSymbolInfo(inv).Symbol as IMethodSymbol)
            .Count(symbol => symbol != null && !SymbolEqualityComparer.Default.Equals(symbol.ContainingType, model.GetDeclaredSymbol(method.Parent)));
    }

    private static int CalculateMaximumMethodsNestingBlocks(MethodDeclarationSyntax method) {
        return GetMaximumNesting(method, node => node is BlockSyntax);
    }

    private static int CalculateNumberOfUnusedVariables(MethodDeclarationSyntax method, SemanticModel model) {
        var variables = method.DescendantNodes().OfType<VariableDeclaratorSyntax>();
        var usedVariables = new HashSet<string>(method.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Select(id => id.Identifier.Text));

        return variables.Count(variable => !usedVariables.Contains(variable.Identifier.Text));
    }

    private static double CalculateAverageInvocationDepth(MethodDeclarationSyntax method) {
        var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
        if (invocations.Count == 0) {
            return 0;
        }

        double totalDepth = invocations.Sum(GetInvocationDepth);
        return totalDepth / invocations.Count;
    }

    private static int GetInvocationDepth(InvocationExpressionSyntax invocation) {
        var depth = 0;
        var parent = invocation.Parent;

        while (parent != null) {
            if (parent is InvocationExpressionSyntax) {
                depth++;
            }

            parent = parent.Parent;
        }

        return depth;
    }
}