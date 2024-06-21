using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace MetricCalculator;

public class MethodMetricCalculator
{
    public async Task Calculate(string solutionPath)
    {
        using var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionPath);

        foreach (var project in solution.Projects)
        {
            foreach (var document in project.Documents)
            {
                var root = await document.GetSyntaxRootAsync();

                if (root is null) {
                    continue;
                }
                
                var model = await document.GetSemanticModelAsync();

                var methodMetrics = new List<MethodMetrics>();

                var methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
                foreach (var method in methodDeclarations)
                {
                    var metrics = new MethodMetrics();
                    metrics.MethodName = method.Identifier.Text;
                    metrics.CYCLO = CalculateCyclomaticComplexity(method);
                    metrics.CYCLO_SWITCH = CalculateSwitchComplexity(method);
                    metrics.MLOC = CalculateMethodLinesOfCode(method);
                    metrics.MELOC = CalculateMethodEffectiveLinesOfCode(method);
                    metrics.NOP = CalculateNumberOfParameters(method);
                    metrics.NOLV = CalculateNumberOfLocalVariables(method);
                    metrics.NOTC = CalculateNumberOfTypeCasts(method);
                    metrics.MNOL = CalculateMaximumNestingOfLoops(method);
                    metrics.MNOR = CalculateMaximumNestingOfRecursions(method);
                    metrics.MNOC = CalculateMaximumNestingOfConditionals(method);
                    metrics.MNOA = CalculateMaximumNestingOfArrays(method);
                    metrics.NONL = CalculateNumberOfNestedLoops(method);
                    metrics.NOSL = CalculateNumberOfStatementsInLoops(method);
                    metrics.NOMO = CalculateNumberOfMethodsOverloaded(root, method);
                    metrics.NOPE = CalculateNumberOfParametersInExternalMethods(method, model);
                    metrics.NOLE = CalculateNumberOfExternalMethodsCalled(method, model);
                    metrics.MMNB = CalculateMaximumMethodsNestingBlocks(method);
                    metrics.NOUW = CalculateNumberOfUnusedVariables(method, model);
                    metrics.AID = CalculateAverageInvocationDepth(method);

                    methodMetrics.Add(metrics);
                }

                foreach (var metrics in methodMetrics)
                {
                    Console.WriteLine(metrics);
                }
            }
        }
    }

    private static int CalculateCyclomaticComplexity(MethodDeclarationSyntax method)
    {
        int complexity = 1;
        complexity += method.DescendantNodes().OfType<IfStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<DoStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<CaseSwitchLabelSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();
        complexity += method.DescendantNodes().OfType<CatchClauseSyntax>().Count();
        return complexity;
    }
    
    private static int CalculateSwitchComplexity(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes().OfType<SwitchStatementSyntax>().Sum(switchStmt => switchStmt.Sections.Count);
    }

    private static int CalculateMethodLinesOfCode(MethodDeclarationSyntax method)
    {
        var lines = method.GetLocation().GetLineSpan().EndLinePosition.Line - method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        return lines;
    }

    private static int CalculateMethodEffectiveLinesOfCode(MethodDeclarationSyntax method)
    {
        var lines = method.Body.Statements.Count;
        return lines;
    }

    private static int CalculateNumberOfParameters(MethodDeclarationSyntax method)
    {
        return method.ParameterList.Parameters.Count;
    }

    private static int CalculateNumberOfLocalVariables(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes().OfType<VariableDeclaratorSyntax>().Count();
    }

    private static int CalculateNumberOfTypeCasts(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes().OfType<CastExpressionSyntax>().Count();
    }

    private static int CalculateMaximumNestingOfLoops(MethodDeclarationSyntax method)
    {
        // This is a simplistic calculation for nesting
        int maxNesting = 0;
        int currentNesting = 0;

        void Visit(SyntaxNode node)
        {
            if (node is ForStatementSyntax || node is WhileStatementSyntax || node is DoStatementSyntax || node is ForEachStatementSyntax)
            {
                currentNesting++;
                if (currentNesting > maxNesting)
                {
                    maxNesting = currentNesting;
                }
                foreach (var child in node.ChildNodes())
                {
                    Visit(child);
                }
                currentNesting--;
            }
            else
            {
                foreach (var child in node.ChildNodes())
                {
                    Visit(child);
                }
            }
        }

        Visit(method);
        return maxNesting;
    }

    private static int CalculateMaximumNestingOfRecursions(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Count(inv => inv.Expression is IdentifierNameSyntax identifier && identifier.Identifier.Text == method.Identifier.Text);
    }

    private static int CalculateMaximumNestingOfConditionals(MethodDeclarationSyntax method)
    {
        int maxNesting = 0;
        int currentNesting = 0;

        void Visit(SyntaxNode node)
        {
            if (node is IfStatementSyntax)
            {
                currentNesting++;
                if (currentNesting > maxNesting)
                {
                    maxNesting = currentNesting;
                }
                foreach (var child in node.ChildNodes())
                {
                    Visit(child);
                }
                currentNesting--;
            }
            else
            {
                foreach (var child in node.ChildNodes())
                {
                    Visit(child);
                }
            }
        }

        Visit(method);
        return maxNesting;
    }

    private static int CalculateMaximumNestingOfArrays(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().Count();
    }

    private static int CalculateNumberOfNestedLoops(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes().OfType<ForStatementSyntax>().Count() +
               method.DescendantNodes().OfType<WhileStatementSyntax>().Count() +
               method.DescendantNodes().OfType<DoStatementSyntax>().Count() +
               method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
    }

    private static int CalculateNumberOfStatementsInLoops(MethodDeclarationSyntax method)
    {
        return method.DescendantNodes().OfType<ForStatementSyntax>().Sum(forStmt => forStmt.Statement.ChildNodes().Count()) +
               method.DescendantNodes().OfType<WhileStatementSyntax>().Sum(whileStmt => whileStmt.Statement.ChildNodes().Count()) +
               method.DescendantNodes().OfType<DoStatementSyntax>().Sum(doStmt => doStmt.Statement.ChildNodes().Count()) +
               method.DescendantNodes().OfType<ForEachStatementSyntax>().Sum(forEachStmt => forEachStmt.Statement.ChildNodes().Count());
    }

    private static int CalculateNumberOfMethodsOverloaded(SyntaxNode root, MethodDeclarationSyntax method)
    {
        var methodName = method.Identifier.Text;
        return root.DescendantNodes().OfType<MethodDeclarationSyntax>().Count(m => m.Identifier.Text == methodName);
    }

    private static int CalculateNumberOfParametersInExternalMethods(MethodDeclarationSyntax method, SemanticModel model)
    {
        return method.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Select(inv => model.GetSymbolInfo(inv).Symbol as IMethodSymbol)
            .Where(symbol => symbol != null && !symbol.ContainingType.Equals(model.GetDeclaredSymbol(method.Parent)))
            .Sum(symbol => symbol.Parameters.Length);
    }

    private static int CalculateNumberOfExternalMethodsCalled(MethodDeclarationSyntax method, SemanticModel model)
    {
        return method.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Select(inv => model.GetSymbolInfo(inv).Symbol as IMethodSymbol)
            .Count(symbol => symbol != null && !symbol.ContainingType.Equals(model.GetDeclaredSymbol(method.Parent)));
    }

    private static int CalculateMaximumMethodsNestingBlocks(MethodDeclarationSyntax method)
    {
        int maxNesting = 0;
        int currentNesting = 0;

        void Visit(SyntaxNode node)
        {
            if (node is BlockSyntax)
            {
                currentNesting++;
                if (currentNesting > maxNesting)
                {
                    maxNesting = currentNesting;
                }
                foreach (var child in node.ChildNodes())
                {
                    Visit(child);
                }
                currentNesting--;
            }
            else
            {
                foreach (var child in node.ChildNodes())
                {
                    Visit(child);
                }
            }
        }

        Visit(method);
        return maxNesting;
    }

    private static int CalculateNumberOfUnusedVariables(MethodDeclarationSyntax method, SemanticModel model)
    {
        var variables = method.DescendantNodes().OfType<VariableDeclaratorSyntax>();
        var usedVariables = new HashSet<string>(method.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Select(id => id.Identifier.Text));

        return variables.Count(variable => !usedVariables.Contains(variable.Identifier.Text));
    }

    private static double CalculateAverageInvocationDepth(MethodDeclarationSyntax method)
    {
        var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
        if (invocations.Count == 0)
        {
            return 0;
        }

        double totalDepth = invocations.Sum(invocation => GetInvocationDepth(invocation));
        return totalDepth / invocations.Count;
    }

    private static int GetInvocationDepth(InvocationExpressionSyntax invocation) {
        int depth = 0;
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