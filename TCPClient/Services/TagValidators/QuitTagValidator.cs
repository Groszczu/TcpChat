namespace TCPClient.Services.TagValidators
{
    public class QuitTagValidator : ITagValidator
    {
        public bool Validate(string tag)
        {
            return tag == "-q";
        }
    }
}