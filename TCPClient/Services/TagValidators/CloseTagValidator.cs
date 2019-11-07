namespace TCPClient.Services.TagValidators
{
    public class CloseTagValidator : ITagValidator
    {
        public bool Validate(string tag)
        {
            return tag == "-c";
        }
    }
}