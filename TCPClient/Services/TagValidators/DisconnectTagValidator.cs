namespace TCPClient.Services.TagValidators
{
    public class DisconnectTagValidator : ITagValidator
    {
        public bool Validate(string tag)
        {
            return tag == "-dn";
        }
    }
}