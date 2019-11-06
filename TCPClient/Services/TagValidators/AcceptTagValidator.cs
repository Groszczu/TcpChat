namespace TCPClient.Services.TagValidators
{
    public class AcceptTagValidator : TagFollowedByNumberValidator
    {
        private const string AcceptTag = "a";
        public AcceptTagValidator() : base(AcceptTag)
        {
        }
    }
}