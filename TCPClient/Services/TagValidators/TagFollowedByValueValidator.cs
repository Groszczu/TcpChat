﻿using System.Text.RegularExpressions;

namespace TCPClient.Services.TagValidators
{
    public abstract class TagFollowedByValueValidator : ITagFollowedByValueValidator
    {
        private readonly Regex _regex;

        protected TagFollowedByValueValidator(string expectedTag)
        {
            _regex = new Regex($"^-{expectedTag}\\s+(?<value>.+)$");
        }

        public virtual bool Validate(string tag)
        {
            return _regex.IsMatch(tag);
        }

        public string GetMatchedValue(string tag)
        {
            return _regex.Match(tag).Groups["value"].Value;
        }
    }
}