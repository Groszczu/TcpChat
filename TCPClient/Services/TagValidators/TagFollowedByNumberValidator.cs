using System.Text.RegularExpressions;

namespace TCPClient.Services.TagValidators
{
    public abstract class TagFollowedByNumberValidator
    {
        public static bool Validate(string tag, string expectedTag)
        {
            var regex = new Regex($"^-{expectedTag}\\s+(?<number>\\d+)$");
            return regex.IsMatch(tag);
        }

        public static string GetMatchedValue(string tag, string expectedTag)
        {
            var regex = new Regex($"^-{expectedTag}\\s+(?<number>\\d+)$");
            return regex.Match(tag).Groups["number"].Value;
        }
    }
}