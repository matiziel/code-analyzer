using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

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
                    .Select(@class => new ClassMetrics {
                        ClassName = @class.Identifier.Text,
                        Cloc = CalculateLinesOfCode(@class),
                        Celoc = CalculateLinesOfCode(@class),
                        Nmd = CalculateNumberOfMethodsDeclared(@class),
                        Nad = CalculateNumberOfAttributesDeclared(@class),
                        NmdNad = CalculateNumberOfMethodsAndAttributes(@class),
                        Wmc = CalculateWeightedMethods(@class),
                        WmcNoCase = CalculateWmcNoCase(@class),
                        Lcom = CalculateLackOfCohesion(@class),
                        Lcom3 = CalculateLackOfCohesion3(@class),
                        Lcom4 = CalculateLackOfCohesion4(@class),
                        Tcc = CalculateTightClassCohesion(@class),
                        Atfd = CalculateAccessToForeignData(@class),
                        Cnor = CalculateNumberOfConstructors(@class),
                        Cnol = CalculateNumberOfOverloadedMethods(@class),
                        Cnoc = CalculateNumberOfChildClasses(@class),
                        Cnoa = CalculateNumberOfAncestorClasses(@class),
                        Nopm = CalculateNumberOfPublicMethods(@class),
                        Nopf = CalculateNumberOfPublicFields(@class),
                        Cmnb = CalculateCyclomaticComplexity(@class),
                        Rfc = CalculateResponseForClass(@class),
                        Cbo = CalculateCouplingBetweenObjects(@class),
                        Dit = CalculateDepthOfInheritanceTree(@class),
                        Dcc = CalculateDirectClassCoupling(@class),
                        Atfd10 = CalculateAtfdWithThreshold(@class, 10),
                        Nic = CalculateNumberOfInnerClasses(@class),
                        Woc = CalculateWeightOfClass(@class),
                        Nopa = CalculateNumberOfPublicAttributes(@class),
                        Nopp = CalculateNumberOfPrivateAttributes(@class),
                        Wmcnamm = CalculateWmcNotCountingAccessorMethods(@class),
                        BOvR = CalculateBaseOverriddenMethodsRatio(@class),
                    }).ToList();

                calculatedMetrics.AddRange(classMetrics);
            }
        }

        return calculatedMetrics;
    }

    private int CalculateLinesOfCode(ClassDeclarationSyntax @class) {
        return @class.SyntaxTree.GetLineSpan(@class.Span).EndLinePosition.Line -
            @class.SyntaxTree.GetLineSpan(@class.Span).StartLinePosition.Line + 1;
    }

    private int CalculateLinesOfComments(ClassDeclarationSyntax @class) {
        return @class
            .DescendantTrivia()
            .Count(x => x.IsKind(SyntaxKind.SingleLineCommentTrivia) || x.IsKind(SyntaxKind.MultiLineCommentTrivia));
    }

    private int CalculateNumberOfMethodsDeclared(ClassDeclarationSyntax @class) {
        return @class.Members.OfType<MethodDeclarationSyntax>().Count();
    }
    
    private int CalculateNumberOfAttributesDeclared(ClassDeclarationSyntax @class)
    {
        return @class.Members.OfType<FieldDeclarationSyntax>().Count();
    }

    private int CalculateNumberOfMethodsAndAttributes(ClassDeclarationSyntax @class)
    {
        return CalculateNumberOfMethodsDeclared(@class) + CalculateNumberOfAttributesDeclared(@class);
    }

    private int CalculateWeightedMethods(ClassDeclarationSyntax @class)
    {
        // WMC: Liczba metod w klasie. W bardziej złożonych przypadkach można uwzględniać złożoność cyklomatyczną każdej metody.
        return @class.Members.OfType<MethodDeclarationSyntax>().Count();
    }

    private int CalculateWmcNoCase(ClassDeclarationSyntax @class)
    {
        // WMC_NO_CASE: Liczba metod w klasie, nie licząc akcesorów (gettery, settery).
        return @class.Members.OfType<MethodDeclarationSyntax>()
                .Count(method => !(method.Modifiers.Any(SyntaxKind.StaticKeyword) || method.Identifier.Text.StartsWith("get") || method.Identifier.Text.StartsWith("set")));
    }

    private double CalculateLackOfCohesion(ClassDeclarationSyntax @class)
    {
        // LCOM: Prosta miara spójności klasy (np. różnica między liczbą metod a liczbą pól dzielonych między metody).
        // Placeholder implementation. Replace with actual LCOM calculation.
        return 0.0;
    }

    private double CalculateLackOfCohesion3(ClassDeclarationSyntax @class)
    {
        // LCOM3: Ulepszona miara LCOM.
        // Placeholder implementation. Replace with actual LCOM3 calculation.
        return 0.0;
    }

    private double CalculateLackOfCohesion4(ClassDeclarationSyntax @class)
    {
        // LCOM4: Dalsze ulepszenie miary LCOM.
        // Placeholder implementation. Replace with actual LCOM4 calculation.
        return 0.0;
    }

    private double CalculateTightClassCohesion(ClassDeclarationSyntax @class)
    {
        // TCC: Miara spójności klasy na podstawie par metod współdzielących pola.
        // Placeholder implementation. Replace with actual TCC calculation.
        return 0.0;
    }

    private double CalculateAccessToForeignData(ClassDeclarationSyntax @class)
    {
        // ATFD: Dostęp do danych z innych klas (obcy dostęp).
        // Placeholder implementation. Replace with actual ATFD calculation.
        return 0.0;
    }

    private int CalculateNumberOfConstructors(ClassDeclarationSyntax @class)
    {
        return @class.Members.OfType<ConstructorDeclarationSyntax>().Count();
    }

    private int CalculateNumberOfOverloadedMethods(ClassDeclarationSyntax @class)
    {
        return @class.Members.OfType<MethodDeclarationSyntax>()
                .GroupBy(m => m.Identifier.Text)
                .Count(g => g.Count() > 1);
    }

    private int CalculateNumberOfChildClasses(ClassDeclarationSyntax @class)
    {
        // CNOC: Liczba klas dziedziczących po tej klasie.
        // Placeholder implementation. Replace with actual CNOC calculation.
        return 0;
    }

    private int CalculateNumberOfAncestorClasses(ClassDeclarationSyntax @class)
    {
        // CNOA: Liczba klas nadrzędnych (przodków).
        // Placeholder implementation. Replace with actual CNOA calculation.
        return 0;
    }

    private int CalculateNumberOfPublicMethods(ClassDeclarationSyntax @class)
    {
        return @class.Members.OfType<MethodDeclarationSyntax>()
                .Count(m => m.Modifiers.Any(SyntaxKind.PublicKeyword));
    }

    private int CalculateNumberOfPublicFields(ClassDeclarationSyntax @class)
    {
        return @class.Members.OfType<FieldDeclarationSyntax>()
                .Count(f => f.Modifiers.Any(SyntaxKind.PublicKeyword));
    }

    private int CalculateCyclomaticComplexity(ClassDeclarationSyntax @class)
    {
        // CMNB: Złożoność cyklomatyczna klasy.
        // Placeholder implementation. Replace with actual Cyclomatic Complexity calculation.
        return 0;
    }

    private int CalculateResponseForClass(ClassDeclarationSyntax @class)
    {
        // RFC: Odpowiedź klasy (liczba metod, które mogą być wywołane z instancji klasy).
        // Placeholder implementation. Replace with actual RFC calculation.
        return 0;
    }

    private int CalculateCouplingBetweenObjects(ClassDeclarationSyntax @class)
    {
        // CBO: Coupling Between Objects - Sprzężenie między obiektami.
        // Placeholder implementation. Replace with actual CBO calculation.
        return 0;
    }

    private int CalculateDepthOfInheritanceTree(ClassDeclarationSyntax @class)
    {
        // DIT: Głębokość drzewa dziedziczenia.
        // Placeholder implementation. Replace with actual DIT calculation.
        return 0;
    }

    private double CalculateDirectClassCoupling(ClassDeclarationSyntax @class)
    {
        // DCC: Bezpośrednie sprzężenie klas.
        // Placeholder implementation. Replace with actual DCC calculation.
        return 0.0;
    }

    private int CalculateAtfdWithThreshold(ClassDeclarationSyntax @class, int threshold)
    {
        // ATFD_10: Dostęp do danych z innych klas z progiem 10.
        // Placeholder implementation. Replace with actual ATFD with threshold calculation.
        return 0;
    }

    private int CalculateNumberOfInnerClasses(ClassDeclarationSyntax @class)
    {
        return @class.Members.OfType<ClassDeclarationSyntax>().Count();
    }

    private double CalculateWeightOfClass(ClassDeclarationSyntax @class)
    {
        var totalMethods = CalculateNumberOfMethodsDeclared(@class);
        var publicMethods = CalculateNumberOfPublicMethods(@class);
        return totalMethods == 0 ? 0 : (double)publicMethods / totalMethods;
    }

    private int CalculateNumberOfPublicAttributes(ClassDeclarationSyntax @class)
    {
        return @class.Members.OfType<FieldDeclarationSyntax>()
                .Count(f => f.Modifiers.Any(SyntaxKind.PublicKeyword));
    }
    
    private int CalculateNumberOfPrivateAttributes(ClassDeclarationSyntax @class) {
        throw new NotImplementedException();
    }

    private int CalculateWmcNotCountingAccessorMethods(ClassDeclarationSyntax @class) {
        throw new NotImplementedException();
    }
    
    private double CalculateBaseOverriddenMethodsRatio(ClassDeclarationSyntax @class) {
        throw new NotImplementedException();
    }
}
