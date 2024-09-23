namespace DeepCloneGenerator;

public interface ISourceGeneratedCloneable<out TSelf>
{
    TSelf DeepClone(Dictionary<object, object>? cache = null);
}

public interface ISourceGeneratedCloneableWithGenerics<out TSelf, T1>
    where TSelf : ISourceGeneratedCloneableWithGenerics<TSelf, T1>
{
    TSelf DeepClone(Func<T1, T1> arg1, Dictionary<object, object>? cache = null);
}

public interface ISourceGeneratedCloneableWithGenerics<out TSelf, T1, T2>
    where TSelf : ISourceGeneratedCloneableWithGenerics<TSelf, T1, T2>
{
    TSelf DeepClone(Func<T1, T1> arg1, Func<T2, T2> arg2, Dictionary<object, object>? cache = null);
}