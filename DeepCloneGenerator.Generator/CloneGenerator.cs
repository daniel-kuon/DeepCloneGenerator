using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DeepCloneGenerator;

[Generator]
public class CloneGenerator : IIncrementalGenerator
{
    internal const string Namespace = "DeepCloneGenerator";
    internal const string AttributeName = "GenerateDeepCloneAttribute";
    internal const string IgnoreCloneAttribute = "CloneIgnoreAttribute";
    internal const string InterfaceName = "ISourceGeneratedCloneable";
    internal const string GenericInterfaceName = "ISourceGeneratedCloneableWithGenerics";
    internal const string CtorVariableName = "original";
    internal const string CacheDictionaryName = "cache";
    internal const string CacheDictionaryType = "System.Collections.Generic.Dictionary<object, object>";
    internal const string CacheDictionaryParameter = "System.Collections.Generic.Dictionary<object, object>? cache = null";
    internal const string CloneMethodName = "DeepClone";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                (s, _) => s is TypeDeclarationSyntax,
                transform: (ctx, _) => GetClassDeclarationForSourceGen(ctx))
            .Where(t => t.AttributeFound)
            .Select((t, _) => (t.Class, t.SkipDefaultConstructorGeneration));

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
    }

    private void GenerateCode(SourceProductionContext ctx, Compilation compilation, ImmutableArray<(TypeDeclarationSyntax Class, bool SkipDefaultConstructorGeneration)> classDeclarations)
    {
        var classSymbols = classDeclarations
            .Select(
                classDeclaration => (
                    Symbol: compilation.GetSemanticModel(classDeclaration.Class.SyntaxTree).GetDeclaredSymbol(classDeclaration.Class) as INamedTypeSymbol,
                    classDeclaration.SkipDefaultConstructorGeneration))
            .OfType<(INamedTypeSymbol Symbol, bool SkipDefaultConstructorGeneration)>()
            .ToList();

        var context = new CloneGeneratorContext(classSymbols, compilation);
        context.Do(ref ctx);
    }

    private static (TypeDeclarationSyntax Class, bool AttributeFound, bool SkipDefaultConstructorGeneration) GetClassDeclarationForSourceGen(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (TypeDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax) as INamedTypeSymbol;
        if (classSymbol == null)
        {
            return (classDeclarationSyntax, false, false);
        }

        var attributeInfo = GetAttributeInfo(classSymbol);
        if (attributeInfo.AttributeFound)
        {
            return (classDeclarationSyntax, attributeInfo.AttributeFound, attributeInfo.SkipDefaultConstructorGeneration);
        }

        // Traverse base types to find attribute
        var baseType = classSymbol.BaseType;
        while (baseType != null)
        {
            attributeInfo = GetAttributeInfo(baseType);
            if (attributeInfo.AttributeFound)
            {
                return (classDeclarationSyntax, attributeInfo.AttributeFound, attributeInfo.SkipDefaultConstructorGeneration);
            }
            baseType = baseType.BaseType;
        }

        return (classDeclarationSyntax, false, false);
    }

    private static (bool AttributeFound, bool AutoInherit, bool SkipDefaultConstructorGeneration) GetAttributeInfo(INamedTypeSymbol typeSymbol)
    {
        foreach (var attributeData in typeSymbol.GetAttributes())
        {
            var attributeClass = attributeData.AttributeClass?.ToDisplayString();
            if (attributeClass == $"{Namespace}.{AttributeName}")
            {
                var autoInherit = GetNamedBooleanArgumentValue(attributeData, "AutoInherit") ?? false;
                var skipDefaultConstructorGeneration = GetNamedBooleanArgumentValue(attributeData, "SkipDefaultConstructorGeneration") ?? false;

                return (true, autoInherit, skipDefaultConstructorGeneration);
            }
        }
        return (false, false, false);
    }

    private static bool? GetNamedBooleanArgumentValue(AttributeData attributeData, string argumentName)
    {
        var argument = attributeData.NamedArguments.FirstOrDefault(kvp => kvp.Key == argumentName);
        if (argument.Value.Value is bool value)
        {
            return value;
        }
        return null;
    }
}