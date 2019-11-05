namespace Core
{
    public interface IHeaderProperty
    {
        object ObjectValue { get; }
        string Key { get; }
        int MaximumLengthInBytes { get; }
        bool IsSet { get; }
    }
}