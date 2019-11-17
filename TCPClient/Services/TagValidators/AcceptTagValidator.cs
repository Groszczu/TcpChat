namespace TCPClient.Services.TagValidators
{
    public static class AcceptTagValidator
    {
        private const string AcceptTag = "a";

        public static bool Validate(string tag)
        {
            return TagFollowedByNumberValidator.Validate(tag, AcceptTag);
        }

        public static string GetMatchedValue(string tag)
        {
            return TagFollowedByNumberValidator.GetMatchedValue(tag, AcceptTag);
        }
    }
}