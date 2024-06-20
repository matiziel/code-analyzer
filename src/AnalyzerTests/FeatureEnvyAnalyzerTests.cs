using Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.CSharp.Testing;

namespace AnalyzerTests;


public class FeatureEnvyAnalyzerTests
{
    [Fact]
    public async Task DetectsFeatureEnvy()
    {
        var testCode = @"
public class Class1
{
    public void Method1()
    {
        var class2 = new Class2();
        class2.MethodA();
        class2.MethodB();
        class2.MethodC();
        class2.MethodD();
    }
}

public class Class2
{
    public void MethodA() { }
    public void MethodB() { }
    public void MethodC() { }
    public void MethodD() { }
}
";
        var expected = new DiagnosticResult(FeatureEnvyAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
            .WithSpan(4, 17, 4, 24)
            .WithArguments("Method1", "Class1", "Class2");

        await VerifyAnalyzerAsync(testCode, expected);
    }

    private static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<FeatureEnvyAnalyzer, XUnitVerifier>
        {
            TestCode = source,
        };

        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }
}