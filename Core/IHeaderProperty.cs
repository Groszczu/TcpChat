namespace Core
{
    public interface IHeaderProperty
    {
        object ObjectValue { get; }
        string Key { get; }
        bool IsSet { get; }
    }
}