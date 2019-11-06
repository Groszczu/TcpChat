namespace TCPClient.Services.TagValidators
{
    public class DeclineTagValidator : TagFollowedByNumberValidator
    {
        private const string DeclineTag = "d";
        public DeclineTagValidator() : base(DeclineTag)
        {
        }
    }
}