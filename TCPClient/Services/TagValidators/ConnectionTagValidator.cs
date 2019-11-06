namespace TCPClient.Services.TagValidators
{
    public class ConnectionTagValidator : ITagValidator
    {
        public bool Validate(string tag)
        {
            return tag == "-cn";
        }
    }
}