using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Analyzer;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace AnalyzerTests;

public class MyMethodCallAnalyzerTests
{
    [Fact]
    public async Task DetectsMethodCallFromAnotherClass()
    {
        var testCode = @"
public class Class1
{
    private readonly Class2 _class2; 
    
    public Class1(Class2 class2)
    {
        _class2 = class2;
    }
    
    public void Method1()
    {
        var class2 = new Class2();
        class2.Method2();
        _class2.Method2();
    }
}

public class Class2
{
    public void Method2() { }
}
";

        var expectedFirst = new DiagnosticResult(MyMethodCallAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
            .WithSpan(14, 9, 14, 23)
            .WithArguments("Method2");
        
        var expectedSecond = new DiagnosticResult(MyMethodCallAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
            .WithSpan(15, 9, 15, 24)
            .WithArguments("Method2");

        await VerifyAnalyzerAsync(testCode, expectedFirst, expectedSecond);
    }

    private static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<MyMethodCallAnalyzer, XUnitVerifier>
        {
            TestCode = source,
        };

        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }
}