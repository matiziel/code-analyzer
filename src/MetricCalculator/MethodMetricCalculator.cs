using System.Text.RegularExpressions;
using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricCalculator;

public class MethodMetricCalculator : IMetricCalculator<MethodMetrics> {
    public async Task<IEnumerable<MethodMetrics>> Calculate(string solutionPath, Dictionary<string, int> annotations = null) {
        var projects = await ProjectProvider.GetFromPath(solutionPath);

        var calculatedMetrics = new List<MethodMetrics>();

        foreach (var project in projects) {
            foreach (var document in project.Documents) {
                var root = await document.GetSyntaxRootAsync();

                if (root is null) {
                    continue;
                }

                var model = await document.GetSemanticModelAsync();

                var methods = root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>();

                if (annotations is not null) {
                    methods = methods.Where(x =>
                        annotations.Keys.Contains(model.GetDeclaredSymbol(x)?.ToDisplayString()));
                }

                var methodMetrics = root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Parent is not InterfaceDeclarationSyntax)
                    .Select(method => new MethodMetrics {
                        MethodName = model.GetDeclaredSymbol(method)?.ToDisplayString() ?? method.Identifier.Text,
                        Cyclo = method.CalculateCyclomaticComplexity(),
                        CycloSwitch = method.CalculateCyclomaticComplexityWithoutCases(),
                        Mloc = CalculateMethodLinesOfCode(method),
                        Meloc = method.CalculateMethodEffectiveLinesOfCode(),
                        Nop = CalculateNumberOfParameters(method),
                        Nolv = CalculateNumberOfLocalVariables(method),
                        Notc = CalculateNumberOfTryCatchBlocks(method),
                        Mnol = method.CalculateNumberOfLoops(),
                        Mnor = method.CalculateNumberOfReturnStatements(),
                        Mnoc = method.CalculateNumberOfComparisonOperators(),
                        Mnoa = method.CalculateNumberOfAssignments(),
                        Nonl = CalculateNumberOfNumericLiterals(method),
                        Nosl = CalculateNumberOfStringLiterals(method),
                        Nomo = CalculateNumberOfMathOperations(root, method),
                        Nope = CalculateNumberOfParenthesizedExpressions(method),
                        Nole = CalculateNumberOfLambdaExpressions(method),
                        Mmnb = method.CalculateMaximumMethodsNestingBlocks(),
                        Nouw = CalculateNumberOfUniqueWords(method),
                        Aid = CalculateAccessToForeignData(method, model),
                    }).ToList();

                calculatedMetrics.AddRange(methodMetrics);
            }
        }

        return calculatedMetrics;
    }

    private static int CalculateMethodLinesOfCode(MethodDeclarationSyntax method) {
        var lines = method.GetLocation().GetLineSpan().EndLinePosition.Line -
            method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        return lines;
    }

    private static int CalculateNumberOfParameters(MethodDeclarationSyntax method) {
        return method.ParameterList.Parameters.Count;
    }

    private static int CalculateNumberOfLocalVariables(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<VariableDeclaratorSyntax>().Count();
    }

    private static int CalculateNumberOfTryCatchBlocks(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<TryStatementSyntax>().Count();
    }



    private static int CalculateNumberOfNumericLiterals(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<LiteralExpressionSyntax>()
            .Count(n => n.IsKind(SyntaxKind.NumericLiteralExpression));
    }

    private static int CalculateNumberOfStringLiterals(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<LiteralExpressionSyntax>()
            .Count(n => n.IsKind(SyntaxKind.StringLiteralExpression));
    }

    private static int CalculateNumberOfMathOperations(SyntaxNode root, MethodDeclarationSyntax method) {
        int count = CountBinaryExpressions(method);
        count += CountUnaryExpressions(method);
        return count;
    }

    private static int CountBinaryExpressions(MemberDeclarationSyntax method) {
        return method.DescendantNodes().OfType<BinaryExpressionSyntax>()
            .Count(n => n.IsKind(SyntaxKind.AddExpression) ||
                        n.IsKind(SyntaxKind.SubtractExpression) ||
                        n.IsKind(SyntaxKind.MultiplyExpression) ||
                        n.IsKind(SyntaxKind.DivideExpression) ||
                        n.IsKind(SyntaxKind.ModuloExpression) ||
                        n.IsKind(SyntaxKind.LeftShiftExpression) ||
                        n.IsKind(SyntaxKind.RightShiftExpression));
    }

    private static int CountUnaryExpressions(MemberDeclarationSyntax method) {
        int count = method.DescendantNodes().OfType<PrefixUnaryExpressionSyntax>()
            .Count(n => n.IsKind(SyntaxKind.PreIncrementExpression) ||
                        n.IsKind(SyntaxKind.PreDecrementExpression) ||
                        n.IsKind(SyntaxKind.UnaryPlusExpression) ||
                        n.IsKind(SyntaxKind.UnaryMinusExpression));
        count += method.DescendantNodes().OfType<PostfixUnaryExpressionSyntax>()
            .Count(n => n.IsKind(SyntaxKind.PostIncrementExpression) ||
                        n.IsKind(SyntaxKind.PostDecrementExpression));
        return count;
    }

    private static int CalculateNumberOfParenthesizedExpressions(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<ExpressionSyntax>()
            .Count(n => n.IsKind(SyntaxKind.ParenthesizedExpression));
    }

    private static int CalculateNumberOfLambdaExpressions(MethodDeclarationSyntax method) {
        return method.DescendantNodes().OfType<LambdaExpressionSyntax>().Count();
    }

    private static int CalculateNumberOfUniqueWords(MemberDeclarationSyntax method) {
        if (!(method.Kind().Equals(SyntaxKind.MethodDeclaration) ||
              method.Kind().Equals(SyntaxKind.ConstructorDeclaration))) return 0;

        var baseMethod = (BaseMethodDeclarationSyntax)method;
        var methodBody = baseMethod.Body.RemoveCommentsFromCode();
        methodBody = RemoveSymbols(methodBody);
        var words = GetWords(methodBody);
        words = BreakWords(words);
        words = FilterWords(words);
        return words.Distinct(StringComparer.CurrentCultureIgnoreCase).Count();
    }



    private static List<string> GetWords(string methodBody) {
        return Regex.Split(methodBody, "[\\s+]").Select(word => word.Trim()).ToList();
    }

    private static string RemoveSymbols(string words) {
        return words
            .Replace("(", " ")
            .Replace(")", " ")
            .Replace("{", " ")
            .Replace("}", " ")
            .Replace("=", " ")
            .Replace(">", " ")
            .Replace("<", " ")
            .Replace("&", " ")
            .Replace("|", " ")
            .Replace("!", " ")
            .Replace("+", " ")
            .Replace("*", " ")
            .Replace("/", " ")
            .Replace("-", " ")
            .Replace(";", " ")
            .Replace(":", " ")
            .Replace(",", " ")
            .Replace("[", " ")
            .Replace("]", " ")
            .Replace("\"", " ")
            .Replace(".", " ")
            .Replace("?", " ");
    }

    private static List<string> FilterWords(List<string> words) {
        return words.Where(word => !string.IsNullOrEmpty(word))
            .Where(word => !Regex.IsMatch(word, "[0-9]+"))
            .Where(word => !GetKeywords().Contains(word))
            .ToList();
    }

    private static List<string> BreakWords(List<string> words) {
        var individualWords = new List<string>();
        foreach (var word in words) {
            var wordParts = Regex.Split(word, "[_ ]").ToList();
            var camelCaseWords = GetCamelCaseWords(wordParts);
            individualWords.AddRange(camelCaseWords);
        }

        return individualWords;
    }

    private static List<string> GetCamelCaseWords(List<string> words) {
        var camelCaseWords = new List<string>();
        foreach (var word in words) {
            var wordParts = Regex.Split(word, "[A-Z]");
            var matches = Regex.Matches(word, "[A-Z]");
            for (int i = 0; i < wordParts.Length - 1; i++) {
                wordParts[i + 1] = matches[i] + wordParts[i + 1];
            }

            camelCaseWords.AddRange(wordParts);
        }

        return camelCaseWords;
    }

    private static List<string> GetKeywords() {
        List<string> keywords = new List<string>();
        keywords.Add("abstract");
        keywords.Add("as");
        keywords.Add("base");
        keywords.Add("bool");
        keywords.Add("break");
        keywords.Add("byte");
        keywords.Add("case");
        keywords.Add("catch");
        keywords.Add("char");
        keywords.Add("checked");
        keywords.Add("class");
        keywords.Add("const");
        keywords.Add("continue");
        keywords.Add("decimal");
        keywords.Add("default");
        keywords.Add("delegate");
        keywords.Add("do");
        keywords.Add("double");
        keywords.Add("else");
        keywords.Add("enum");
        keywords.Add("event");
        keywords.Add("explicit");
        keywords.Add("extern");
        keywords.Add("false");
        keywords.Add("finally");
        keywords.Add("fixed");
        keywords.Add("float");
        keywords.Add("for");
        keywords.Add("foreach");
        keywords.Add("goto");
        keywords.Add("if");
        keywords.Add("implicit");
        keywords.Add("in");
        keywords.Add("int");
        keywords.Add("interface");
        keywords.Add("internal");
        keywords.Add("is");
        keywords.Add("lock");
        keywords.Add("long");
        keywords.Add("namespace");
        keywords.Add("new");
        keywords.Add("null");
        keywords.Add("object");
        keywords.Add("operator");
        keywords.Add("out");
        keywords.Add("override");
        keywords.Add("params");
        keywords.Add("private");
        keywords.Add("protected");
        keywords.Add("public");
        keywords.Add("readonly");
        keywords.Add("record");
        keywords.Add("ref");
        keywords.Add("return");
        keywords.Add("sbyte");
        keywords.Add("sealed");
        keywords.Add("short");
        keywords.Add("sizeof");
        keywords.Add("stackalloc");
        keywords.Add("static");
        keywords.Add("string");
        keywords.Add("struct");
        keywords.Add("switch");
        keywords.Add("this");
        keywords.Add("throw");
        keywords.Add("true");
        keywords.Add("try");
        keywords.Add("typeof");
        keywords.Add("uint");
        keywords.Add("ulong");
        keywords.Add("unchecked");
        keywords.Add("unsafe");
        keywords.Add("ushort");
        keywords.Add("using");
        keywords.Add("virtual");
        keywords.Add("void");
        keywords.Add("volatile");
        keywords.Add("while");
        return keywords;
    }

    private int CalculateAccessToForeignData(MethodDeclarationSyntax method, SemanticModel semanticModel)
    {
        var foreignDataClasses = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        var variables = method.Body?.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Select(variable => semanticModel.GetDeclaredSymbol(variable))
            .OfType<IFieldSymbol>()
            .Select(field => field.Type as INamedTypeSymbol)
            .Where(type => type != null);

        if (variables != null)
        {
            foreach (var type in variables)
            {
                if (!SymbolEqualityComparer.Default.Equals(type, semanticModel.GetDeclaredSymbol(method)?.ContainingType))
                {
                    foreignDataClasses.Add(type);
                }
            }
        }

        var parameters = method.ParameterList.Parameters
            .Select(param => semanticModel.GetDeclaredSymbol(param))
            .Select(param => param?.Type as INamedTypeSymbol)
            .Where(type => type != null);

        foreach (var type in parameters)
        {
            if (!SymbolEqualityComparer.Default.Equals(type, semanticModel.GetDeclaredSymbol(method)?.ContainingType))
            {
                foreignDataClasses.Add(type);
            }
        }

        var memberAccesses = method.Body?.DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .Select(memberAccess => semanticModel.GetSymbolInfo(memberAccess).Symbol)
            .OfType<IMethodSymbol>()
            .Select(member => member.ContainingType)
            .Where(type => type != null && !SymbolEqualityComparer.Default.Equals(type, semanticModel.GetDeclaredSymbol(method)?.ContainingType));

        if (memberAccesses != null)
        {
            foreach (var type in memberAccesses)
            {
                foreignDataClasses.Add(type);
            }
        }

        return foreignDataClasses.Count;
    }
}