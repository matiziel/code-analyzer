using Microsoft.CodeAnalysis.Diagnostics;

namespace Common;

public static class SyntaxNodeAnalysisContextExtensions {
    
    public static HashSet<string> GetSystemAssemblies(this SyntaxNodeAnalysisContext context) => 
        context.Compilation.ReferencedAssemblyNames
            .Select(a => a.Name)
            .Where(a => a.StartsWith("Microsoft.") || a.StartsWith("System."))
            .ToHashSet();
}