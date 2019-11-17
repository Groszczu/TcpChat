using System.Text.RegularExpressions;

namespace TCPClient.Services.TagValidators
{
    public abstract class TagFollowedByValueValidator
    {
        public static bool Validate(string tag, string expectedTag)
        {
            var regex = new Regex($"^-{expectedTag}\\s+(?<value>.+)$");
            return regex.IsMatch(tag);
        }

        public static string GetMatchedValue(string tag, string expectedTag)
        {
            var regex = new Regex($"^-{expectedTag}\\s+(?<value>.+)$");
            return regex.Match(tag).Groups["value"].Value;
        }
    }
}