namespace TCPClient.Services.TagValidators
{
    public class MessageTagValidator : TagFollowedByValueValidator
    {
        private const string MessageTag = "m";
        public MessageTagValidator() : base(MessageTag)
        {
        }
    }
}