using Microsoft.Extensions.Options;

namespace TCPClient.Services.TagValidators
{
    public static class MessageTagValidator 
    {
        private const string MessageTag = "m";

        public static bool Validate(string tag)
        {
            return TagFollowedByValueValidator.Validate(tag, MessageTag);
        }

        public static string GetMatchedValue(string tag)
        {
            return TagFollowedByValueValidator.GetMatchedValue(tag, MessageTag);
        }
    }
}