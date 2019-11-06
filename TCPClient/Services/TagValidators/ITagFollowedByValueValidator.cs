namespace TCPClient.Services.TagValidators
{
    public interface ITagFollowedByValueValidator : ITagValidator
    {
        string GetMatchedValue(string tag);
    }
}