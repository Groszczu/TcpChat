namespace TCPClient.Services.TagValidators
{
    public class IdTagValidator : ITagValidator
    {
        public bool Validate(string tag)
        {
            return tag == "-id";
        }
    }
}