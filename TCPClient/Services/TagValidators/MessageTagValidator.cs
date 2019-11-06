using System.Text.RegularExpressions;

namespace TCPClient.Services.TagValidators
{
    public class MessageTagValidator : TagFollowedByValueValidator, ITagValidator
    {
        private const string MessageTag = "m";
        public MessageTagValidator() : base(MessageTag)
        {
        }
    }
}