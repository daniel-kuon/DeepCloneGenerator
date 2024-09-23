using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DeepCloneGenerator;

public class CloneGeneratorContext
{
    private readonly List<(INamedTypeSymbol Symbol, bool SkipDefaultConstructorGeneration)> _classSymbols;

    public CloneGeneratorContext(List<(INamedTypeSymbol Symbol, bool SkipDefaultConstructorGeneration)> classSymbols, Compilation compilation)
    {
        _classSymbols = classSymbols;
        Compilation = compilation;
        var classesInAssemblyGeneratingClone = classSymbols
            .Select(c => c.Symbol.ToDisplayString())
            .ToImmutableHashSet();
        ClassesInAssemblyGeneratingClone = classesInAssemblyGeneratingClone;
    }

    public Compilation Compilation { get; }
    public IReadOnlyCollection<string> ClassesInAssemblyGeneratingClone { get; }

    public void Do(ref SourceProductionContext ctx)
    {
        foreach (var classSymbol in _classSymbols)
        {
            using var classContext = new CloneGeneratorClassContext(this, classSymbol.Symbol,classSymbol.SkipDefaultConstructorGeneration);
            classContext.Do(ref ctx);
        }
    }
}