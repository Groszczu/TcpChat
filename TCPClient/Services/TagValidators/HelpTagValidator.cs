namespace TCPClient.Services.TagValidators
{
    public class HelpTagValidator : ITagValidator
    {
        public bool Validate(string tag)
        {
            return tag == "-h";
        }
    }
}