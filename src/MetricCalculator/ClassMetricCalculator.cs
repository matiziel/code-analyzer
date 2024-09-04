using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetricCalculator;

public class ClassMetricCalculator : IMetricCalculator<ClassMetrics> {
    public async Task<IEnumerable<ClassMetrics>> Calculate(string solutionPath) {
        var projects = await ProjectProvider.GetFromPath(solutionPath);

        var calculatedMetrics = new List<ClassMetrics>();

        foreach (var project in projects) {
            foreach (var document in project.Documents) {
                var root = await document.GetSyntaxRootAsync();

                if (root is null) {
                    continue;
                }

                var model = await document.GetSemanticModelAsync();

                var classMetrics = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(classDeclaration => new ClassMetrics {
                        ClassName = classDeclaration.Identifier.Text,
                        Cloc = CalculateLinesOfCode(classDeclaration),
                        Celoc = CalculateEffectiveLinesOfCode(classDeclaration),
                        Nmd = CalculateNumberOfMethodsDeclared(classDeclaration),
                        Nad = CalculateNumberOfAttributesDeclared(classDeclaration),
                        NmdNad = CalculateNumberOfMethodsAndAttributes(classDeclaration),
                        Wmc = CalculateWeightedMethods(classDeclaration),
                        WmcNoCase = CalculateWeightedMethodsWihoutCase(classDeclaration),
                        Lcom = classDeclaration.CalculateLackOfCohesion(model),
                        Lcom3 = classDeclaration.CalculateLackOfCohesion3(model),
                        Lcom4 = classDeclaration.CalculateLackOfCohesion4(model),
                        Tcc = classDeclaration.CalculateTightClassCohesion(),
                        Atfd = CalculateAccessToForeignData(classDeclaration, model),
                        Cnor = CalculateNumberReturnStatements(classDeclaration),
                        Cnol = CalculateNumberOfLoops(classDeclaration),
                        Cnoc = CalculateNumberOfComparisonOperators(classDeclaration),
                        Cnoa = CalculateNumberOfAssignments(classDeclaration),
                        Nopm = CalculateNumberOfPrivateMethods(classDeclaration),
                        Nopf = CalculateNumberOfProtectedFields(classDeclaration),
                        Cmnb = CalculateMaxNestedBlocks(classDeclaration),
                        Rfc = CalculateUniqueMethodInvocations(classDeclaration, model),
                        Cbo = CalculateDependencies(classDeclaration, model),
                        Dit = CalculateDepthOfInheritanceTree(classDeclaration, model),
                        Dcc = CalculateDirectClassCoupling(classDeclaration, model),
                        Atfd10 = CalculateAccessToForeignDataDirectly(classDeclaration, model),
                        Nic = CalculateNumberOfInnerClasses(classDeclaration),
                        Woc = CalculateWeightOfClass(classDeclaration),
                        Nopa = CalculateNumberOfPublicAttributes(classDeclaration),
                        Nopp = CalculateNumberOfPublicProperties(classDeclaration),
                        Wmcnamm = CalculateWmcNotCountingAccessorMethods(classDeclaration, model),
                        Bur = CalculateBaseClassUsageRatio(classDeclaration, model),
                        BOvR = CalculateBaseOverriddenMethodsRatio(classDeclaration, model),
                    }).ToList();

                calculatedMetrics.AddRange(classMetrics);
            }
        }

        return calculatedMetrics;
    }

    private int CalculateLinesOfCode(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.SyntaxTree.GetLineSpan(classDeclaration.Span).EndLinePosition.Line -
            classDeclaration.SyntaxTree.GetLineSpan(classDeclaration.Span).StartLinePosition.Line + 1;
    }

    private int CalculateEffectiveLinesOfCode(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Sum(t => t.CalculateMethodEffectiveLinesOfCode());
    }

    private int CalculateNumberOfMethodsDeclared(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>().Count();
    }

    private int CalculateNumberOfAttributesDeclared(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<FieldDeclarationSyntax>().Count();
    }

    private int CalculateNumberOfMethodsAndAttributes(ClassDeclarationSyntax classDeclaration) {
        return CalculateNumberOfMethodsDeclared(classDeclaration) +
               CalculateNumberOfAttributesDeclared(classDeclaration);
    }

    private int CalculateWeightedMethods(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Select(t => t.CalculateCyclomaticComplexity())
            .Sum();
    }

    private int CalculateWeightedMethodsWihoutCase(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Select(t => t.CalculateCyclomaticComplexityWithoutCases())
            .Sum();
    }

    private int CalculateAccessToForeignData(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel) {
        var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>();

        var accessToForeignDataCount = 0;

        foreach (var method in methods) {
            var memberAccesses = method.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

            foreach (var memberAccess in memberAccesses) {
                var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);

                if (symbolInfo.Symbol == null) {
                    continue;
                }

                var containingType = symbolInfo.Symbol.ContainingType;
                var currentClass = semanticModel.GetDeclaredSymbol(classDeclaration);

                if (containingType != null && currentClass != null &&
                    !SymbolEqualityComparer.Default.Equals(containingType, currentClass)) {
                    accessToForeignDataCount++;
                }
            }
        }

        return accessToForeignDataCount;
    }

    private int CalculateNumberReturnStatements(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Sum(t => t.CalculateNumberOfReturnStatements());
    }

    private int CalculateNumberOfLoops(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Sum(t => t.CalculateNumberOfLoops());
    }

    private int CalculateNumberOfComparisonOperators(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Sum(t => t.CalculateNumberOfComparisonOperators());
    }

    private int CalculateNumberOfAssignments(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Sum(t => t.CalculateNumberOfAssignments());
    }

    private int CalculateNumberOfPrivateMethods(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Count(f => f.Modifiers.Any(SyntaxKind.PrivateKeyword));
    }

    private int CalculateNumberOfProtectedFields(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<FieldDeclarationSyntax>()
            .Count(f => f.Modifiers.Any(SyntaxKind.ProtectedKeyword));
    }

    private int CalculateMaxNestedBlocks(ClassDeclarationSyntax classDeclaration) {
        var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

        return methods.Any() ? methods.Max(t => t.CalculateMaximumMethodsNestingBlocks()) : 0;
    }

    private int CalculateUniqueMethodInvocations(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel) {
        var uniqueMethodInvocations = new HashSet<string>();

        var methodInvocations = classDeclaration
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>();

        foreach (var invocation in methodInvocations) {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);

            if (symbolInfo.Symbol is IMethodSymbol methodSymbol) {
                uniqueMethodInvocations.Add(methodSymbol.ToDisplayString());
            }
        }

        return uniqueMethodInvocations.Count;
    }

    private int CalculateDependencies(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel) {
        var dependencies = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        var fieldTypes = classDeclaration.Members
            .OfType<FieldDeclarationSyntax>()
            .SelectMany(field => field.Declaration.Type.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>());

        var methodTypes = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .SelectMany(method => method.ReturnType
                .DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
            .Concat(classDeclaration.Members
                .OfType<MethodDeclarationSyntax>()
                .SelectMany(method => method.ParameterList.Parameters
                    .SelectMany(param =>
                        param.Type?.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>() ??
                        Enumerable.Empty<IdentifierNameSyntax>())));

        var propertyTypes = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .SelectMany(prop => prop.Type.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>());

        var allTypes = fieldTypes.Concat(methodTypes).Concat(propertyTypes);

        foreach (var type in allTypes) {
            if (semanticModel.GetSymbolInfo(type).Symbol is INamedTypeSymbol symbolInfo) {
                dependencies.Add(symbolInfo);
            }
        }

        return dependencies.Count;
    }

    private int CalculateDepthOfInheritanceTree(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel) {
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (classSymbol == null) {
            return 0;
        }

        var depth = 0;
        var currentClass = classSymbol.BaseType;

        while (currentClass != null && currentClass.SpecialType != SpecialType.System_Object) {
            depth++;
            currentClass = currentClass.BaseType;
        }

        return depth;
    }

    private double CalculateDirectClassCoupling(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel) {
        var directClassCouplings = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        var fieldTypes = classDeclaration.Members
            .OfType<FieldDeclarationSyntax>()
            .Select(field => semanticModel.GetTypeInfo(field.Declaration.Type).Type)
            .OfType<INamedTypeSymbol>();

        var propertyTypes = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(prop => semanticModel.GetTypeInfo(prop.Type).Type)
            .OfType<INamedTypeSymbol>();

        var methodTypes = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .SelectMany(method =>
                method.ParameterList.Parameters.Select(param => semanticModel.GetTypeInfo(param.Type!).Type)
                    .Concat(new[] { semanticModel.GetTypeInfo(method.ReturnType).Type }))
            .OfType<INamedTypeSymbol>();

        var allTypes = fieldTypes.Concat(propertyTypes).Concat(methodTypes);

        foreach (var typeSymbol in allTypes) {
            if (typeSymbol is { IsValueType: false, IsAnonymousType: false, IsImplicitlyDeclared: false }) {
                directClassCouplings.Add(typeSymbol);
            }
        }

        return directClassCouplings.Count;
    }

    private int CalculateAccessToForeignDataDirectly(ClassDeclarationSyntax classDeclaration,
        SemanticModel semanticModel) {
        var accessToForeignData = 0;

        var memberAccesses = classDeclaration.DescendantNodes().OfType<MemberAccessExpressionSyntax>();

        foreach (var memberAccess in memberAccesses) {
            var symbolInfo = semanticModel.GetSymbolInfo(memberAccess).Symbol;

            switch (symbolInfo) {
                case IFieldSymbol fieldSymbol: {
                    if (!IsMemberOfCurrentClass(fieldSymbol.ContainingType, classDeclaration, semanticModel)) {
                        accessToForeignData++;
                    }

                    break;
                }
                case IMethodSymbol methodSymbol: {
                    if (!IsMemberOfCurrentClass(methodSymbol.ContainingType, classDeclaration, semanticModel)) {
                        accessToForeignData++;
                    }

                    break;
                }
            }
        }

        return accessToForeignData;
    }

    private bool IsMemberOfCurrentClass(INamedTypeSymbol containingType, ClassDeclarationSyntax classDeclaration,
        SemanticModel semanticModel) {
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        return SymbolEqualityComparer.Default.Equals(containingType, classSymbol);
    }

    private int CalculateNumberOfInnerClasses(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<ClassDeclarationSyntax>().Count();
    }

    private double CalculateWeightOfClass(ClassDeclarationSyntax classDeclaration) {
        var totalMethods = CalculateNumberOfMethodsDeclared(classDeclaration);
        var publicMethods = CalculateNumberOfPublicMethods(classDeclaration);
        return totalMethods == 0 ? 0 : (double)publicMethods / totalMethods;
    }

    private int CalculateNumberOfPublicMethods(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .Count(m => m.Modifiers.Any(SyntaxKind.PublicKeyword));
    }

    private int CalculateNumberOfPublicAttributes(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<FieldDeclarationSyntax>()
            .Count(f => f.Modifiers.Any(SyntaxKind.PublicKeyword));
    }

    private int CalculateNumberOfPublicProperties(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.Members.OfType<PropertyDeclarationSyntax>()
            .Count(field => field.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)));
    }

    private int CalculateWmcNotCountingAccessorMethods(ClassDeclarationSyntax classDeclaration,
        SemanticModel semanticModel) {
        return classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Select(x => semanticModel.GetDeclaredSymbol(x))
            .Count(x => x is not null && !IsAccessorMethod(x));
    }

    private bool IsAccessorMethod(IMethodSymbol methodSymbol) {
        return methodSymbol.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet;
    }

    private double CalculateBaseOverriddenMethodsRatio(ClassDeclarationSyntax classDeclaration,
        SemanticModel semanticModel) {
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol?.BaseType == null) {
            return 0.0;
        }

        var baseType = classSymbol.BaseType;

        var baseMethods = baseType.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary &&
                        !m.IsStatic &&
                        m.DeclaredAccessibility != Accessibility.Private)
            .ToList();

        if (baseMethods.Count == 0) {
            return 0.0;
        }

        var overriddenMethodCount = 0;

        foreach (var baseMethod in baseMethods) {
            var overriddenMethod = classSymbol.GetMembers().OfType<IMethodSymbol>()
                .FirstOrDefault(m =>
                    m.Name == baseMethod.Name &&
                    m.Parameters.Length == baseMethod.Parameters.Length &&
                    m.Parameters
                        .Select(p => p.Type)
                        .SequenceEqual(baseMethod.Parameters.Select(p => p.Type), SymbolEqualityComparer.Default) &&
                    SymbolEqualityComparer.Default.Equals(m.OverriddenMethod?.ContainingType, baseType));

            if (overriddenMethod is not null) {
                overriddenMethodCount++;
            }
        }

        return (double)overriddenMethodCount / baseMethods.Count;
    }

    private double CalculateBaseClassUsageRatio(ClassDeclarationSyntax classDeclaration, SemanticModel model) {
        var baseType = model.GetDeclaredSymbol(classDeclaration)?.BaseType;
        if (baseType == null) {
            return 0.0;
        }

        var baseMembers = baseType.GetMembers()
            .Where(member => member.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected)
            .ToList();

        if (!baseMembers.Any()) {
            return 0.0;
        }

        var usedBaseMembers = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

        var invocations = classDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
        foreach (var invocation in invocations) {
            var symbol = model.GetSymbolInfo(invocation).Symbol;
            if (symbol != null && baseMembers.Contains(symbol.OriginalDefinition)) {
                usedBaseMembers.Add(symbol.OriginalDefinition);
            }
        }

        var memberAccesses = classDeclaration.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
        foreach (var memberAccess in memberAccesses) {
            var symbol = model.GetSymbolInfo(memberAccess).Symbol;
            if (symbol != null && baseMembers.Contains(symbol.OriginalDefinition)) {
                usedBaseMembers.Add(symbol.OriginalDefinition);
            }
        }

        return (double)usedBaseMembers.Count / baseMembers.Count;
    }
}