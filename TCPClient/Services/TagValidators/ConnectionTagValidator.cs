using System.Net;
using System.Text.RegularExpressions;

namespace TCPClient.Services.TagValidators
{
    public class ConnectionTagValidator : ITagValidator
    {
        private const string ConnectionTag = "cn";

        private const string ValidIpAddressRegex =
            @"(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])";

        private const string ValidPortRegex =
            @"([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])";

        private readonly Regex _regex = new Regex($"^-{ConnectionTag}\\s+(?<ip>{ValidIpAddressRegex})\\:(?<port>{ValidPortRegex})$");

        public bool Validate(string tag)
        {
            return _regex.IsMatch(tag);
        }

        public IPAddress GetIpAddress(string tag)
        {
            var ipString = _regex.Match(tag).Groups["ip"].Value;
            var ipParsed = IPAddress.Parse(ipString);
            
            return ipParsed;
        }

        public int GetPortNumber(string tag)
        {
            var portString = _regex.Match(tag).Groups["port"].Value;
            var portParsed = int.Parse(portString);
            
            return portParsed;
        }
    }
}