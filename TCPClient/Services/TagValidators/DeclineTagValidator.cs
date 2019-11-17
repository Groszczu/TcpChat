namespace TCPClient.Services.TagValidators
{
    public static class DeclineTagValidator
    {
        private const string DeclineTag = "d";

        public static bool Validate(string tag)
        {
            return TagFollowedByNumberValidator.Validate(tag, DeclineTag);
        }

        public static string GetMatchedValue(string tag)
        {
            return TagFollowedByNumberValidator.GetMatchedValue(tag, DeclineTag);
        }
    }
}