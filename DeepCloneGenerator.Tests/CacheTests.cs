namespace DeepCloneGenerator.Tests;

public partial class CacheTests
{
    [Fact(DisplayName = "Reference loops are resolved correctly")]
    public void ReferenceLoopTest()
    {
        var original = new BaseClassWithChildren();

        original.Children.Add(new BaseClassWithParent(){Parent = original});

        var clone = original.DeepClone();

        clone.Should()
            .BeExactClone(original);
    }

    [Fact(DisplayName = "If two properties reference the same object, it is only cloned once")]
    public void TestWithTwinProperties()
    {
        var original = new ClassWithTwinProperties
        {
            SingleProp1 = new EmptyClass(),
            ListProp1 = new List<EmptyClass>{ new () },
            DictionaryProp1 = new Dictionary<string, EmptyClass>{ { "test" , new EmptyClass() } },
            EnumerableProp1 = new List<EmptyClass>{ new () },
            ArrayProp1 = [],
        };
        original.SingleProp2 = original.SingleProp1;
        original.ListProp2 = original.ListProp1;
        original.DictionaryProp2 = original.DictionaryProp1;
        original.EnumerableProp2 = original.EnumerableProp1;
        original.ArrayProp2 = original.ArrayProp1;

        var clone = original.DeepClone();

        clone.Should()
            .BeExactClone(original);

        clone.SingleProp1.Should().BeSameAs(clone.SingleProp2);
        clone.ListProp1.Should().BeSameAs(clone.ListProp2);
        clone.DictionaryProp1.Should().BeSameAs(clone.DictionaryProp2);
        clone.EnumerableProp1.Should().BeSameAs(clone.EnumerableProp2);
        clone.ArrayProp1.Should().BeSameAs(clone.ArrayProp2);
    }

    [GenerateDeepClone]
    private partial class BaseClassWithParent
    {
        public required BaseClassWithChildren Parent { get; init; }
    }

    [GenerateDeepClone]
    private partial class BaseClassWithChildren
    {
        public List<BaseClassWithParent> Children { get; init; } = new();
    }

    [GenerateDeepClone]
    private partial class ClassWithTwinProperties
    {
        public EmptyClass SingleProp1 { get; set; }
        public EmptyClass SingleProp2 { get; set; }

        public List<EmptyClass> ListProp1 { get; set; }
        public List<EmptyClass> ListProp2 { get; set; }

        public Dictionary<string, EmptyClass> DictionaryProp1 { get; set; }
        public Dictionary<string, EmptyClass> DictionaryProp2 { get; set; }

        public IEnumerable<EmptyClass> EnumerableProp1 { get; set; }
        public IEnumerable<EmptyClass> EnumerableProp2 { get; set; }

        public EmptyClass[] ArrayProp1 { get; set; }
        public EmptyClass[] ArrayProp2 { get; set; }
    }

    [GenerateDeepClone]
    private partial class EmptyClass
    {
        // Empty class
    }


}