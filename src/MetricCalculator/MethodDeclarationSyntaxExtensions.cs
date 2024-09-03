using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricCalculator;

public static class MethodDeclarationSyntaxExtensions {
    
        public static int CalculateCyclomaticComplexity(this MethodDeclarationSyntax method) {
        var complexity = 1;
        
        complexity += method.DescendantNodes().OfType<IfStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<CaseSwitchLabelSyntax>().Count();
        complexity += method.DescendantNodes().OfType<CasePatternSwitchLabelSyntax>().Count();
        complexity += method.DescendantNodes().OfType<DefaultSwitchLabelSyntax>().Count();
        complexity += method.DescendantNodes().OfType<SwitchStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ContinueStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<GotoStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();
        complexity += method.DescendantNodes().OfType<CatchClauseSyntax>().Count();
        
        complexity += CountLogicalOperators(method, "&&");
        complexity += CountLogicalOperators(method, "||");
        complexity += CountLogicalOperators(method, "??");

        return complexity;
    }
    
    public static int CalculateCyclomaticComplexityWithoutCases(this MethodDeclarationSyntax method) {
        var complexity = 1;
        
        complexity += method.DescendantNodes().OfType<IfStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<SwitchStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ContinueStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<GotoStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();
        complexity += method.DescendantNodes().OfType<CatchClauseSyntax>().Count();
        
        complexity += CountLogicalOperators(method, "&&");
        complexity += CountLogicalOperators(method, "||");
        complexity += CountLogicalOperators(method, "??");

        return complexity;
    }
    
    private static int CountLogicalOperators(MemberDeclarationSyntax method, string pattern)
    {
        var comments = method.DescendantTrivia();
        var commentOperatorCount = comments.Sum(comment => CountOccurrences(comment.ToString(), pattern));
        return CountOccurrences(method.ToString(), pattern) - commentOperatorCount;
    }
    
    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var i = 0;
        while ((i = text.IndexOf(pattern, i, StringComparison.Ordinal)) != -1)
        {
            i += pattern.Length;
            count++;
        }
        return count;
    }
    
    public static int CalculateMethodEffectiveLinesOfCode(this MethodDeclarationSyntax method) {
        var allCode = RemoveCommentsFromCode(method);
            
        var allLines = allCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var blankLines = CountBlankLines(allLines);
        var headerLines = CountHeaderLines(allLines);
        var openAndClosingBracketLines = 2;
            
        return Math.Max(allLines.Length - (blankLines + headerLines + openAndClosingBracketLines), 1);
    }
    
    public static string RemoveCommentsFromCode(this CSharpSyntaxNode node) {
        if (node == null) {
            return string.Empty;
        }
        var allCode = node.ToString();
        var allComments = node.DescendantTrivia().Where(t =>
            t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia));
        foreach (var comment in allComments) {
            allCode = allCode.Replace(comment.ToFullString(), "");
        }

        return allCode;
    }
    
    private static int CountHeaderLines(string[] allLines)
    {
        var counter = 0;
        foreach (var line in allLines)
        {
            if(line.Contains("{")) break;
            counter++;
        }

        return counter;
    }

    private static int CountBlankLines(string[] allLines)
    {
        return allLines.Select(t => t.Trim()).Count(line => line == "");
    }
    
    public static int CalculateNumberOfReturnStatements(this MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<ReturnStatementSyntax>().Count();
    }
    
    public static int CalculateNumberOfLoops(this MethodDeclarationSyntax method) {
        var count = method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
        count += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        count += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        count += method.DescendantNodes().OfType<DoStatementSyntax>().Count();
        return count;
    }
    
    public static int CalculateNumberOfComparisonOperators(this MethodDeclarationSyntax method) {
        var count = method.DescendantNodes().OfType<AssignmentExpressionSyntax>()
            .Count(n => n.IsKind(SyntaxKind.CoalesceAssignmentExpression));
        
        count += method.DescendantNodes().OfType<BinaryExpressionSyntax>()
            .Count(n => n.IsKind(SyntaxKind.EqualsExpression) || 
                        n.IsKind(SyntaxKind.NotEqualsExpression) ||
                        n.IsKind(SyntaxKind.LessThanExpression) ||
                        n.IsKind(SyntaxKind.LessThanOrEqualExpression) ||
                        n.IsKind(SyntaxKind.GreaterThanExpression) ||
                        n.IsKind(SyntaxKind.GreaterThanOrEqualExpression) ||
                        n.IsKind(SyntaxKind.CoalesceExpression));
        return count;
    }
    
    public static int CalculateNumberOfAssignments(this MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<AssignmentExpressionSyntax>().Count();
    }
    
    public static int CalculateMaximumMethodsNestingBlocks(this MethodDeclarationSyntax method) {
        return GetMaximumNesting(method, node => node is BlockSyntax);
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

}