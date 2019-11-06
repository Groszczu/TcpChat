using System.Text.RegularExpressions;

namespace TCPClient.Services.TagValidators
{
    public abstract class TagFollowedByNumberValidator : ITagFollowedByValueValidator
    {
        private readonly Regex _regex;

        protected TagFollowedByNumberValidator(string expectedTag)
        {
            _regex = new Regex($"^-{expectedTag}\\s+(?<number>\\d+)$");
        }

        public virtual bool Validate(string tag)
        {
            return _regex.IsMatch(tag);
        }

        public string GetMatchedValue(string tag)
        {
            return _regex.Match(tag).Groups["number"].Value;
        }
    }
}