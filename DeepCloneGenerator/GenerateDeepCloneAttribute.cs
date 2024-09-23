namespace DeepCloneGenerator;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class GenerateDeepCloneAttribute : Attribute {
    public bool AutoInherit { get; set; }
    public bool SkipDefaultConstructorGeneration { get; set; }

    public GenerateDeepCloneAttribute()
    {
    }
}