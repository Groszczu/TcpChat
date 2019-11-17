using System.Buffers.Text;
using System.ComponentModel;

namespace TCPClient.Services.TagValidators
{
    public static class InviteTagValidator 
    {
        private const string InviteTag = "i";

        public static bool Validate(string tag)
        {
            return TagFollowedByNumberValidator.Validate(tag, InviteTag);
        }

        public static string GetMatchedValue(string tag)
        {
            return TagFollowedByNumberValidator.GetMatchedValue(tag, InviteTag);
        }
    }
}