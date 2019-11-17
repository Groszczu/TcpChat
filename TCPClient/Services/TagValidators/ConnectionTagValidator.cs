using System.Net;
using System.Text.RegularExpressions;

namespace TCPClient.Services.TagValidators
{
    public static class ConnectionTagValidator
    {
        private const string ConnectionTag = "cn";

        private const string ValidIpAddressRegex =
            @"(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])";

        private const string ValidPortRegex =
            @"([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])";

        private static readonly Regex Regex = new Regex($"^-{ConnectionTag}\\s+(?<ip>{ValidIpAddressRegex})\\:(?<port>{ValidPortRegex})$");

        public static bool Validate(string tag)
        {
            return Regex.IsMatch(tag);
        }

        public static IPAddress GetIpAddress(string tag)
        {
            var ipString = Regex.Match(tag).Groups["ip"].Value;
            var ipParsed = IPAddress.Parse(ipString);
            
            return ipParsed;
        }

        public static int GetPortNumber(string tag)
        {
            var portString = Regex.Match(tag).Groups["port"].Value;
            var portParsed = int.Parse(portString);
            
            return portParsed;
        }
    }
}