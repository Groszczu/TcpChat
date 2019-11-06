namespace TCPClient.Services.TagValidators
{
    public class InviteTagValidator : TagFollowedByNumberValidator
    {
        private const string InviteTag = "i";
        public InviteTagValidator() : base(InviteTag)
        {
        }
    }
}