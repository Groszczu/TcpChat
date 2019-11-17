namespace TCPClient.Services.TagValidators
{
    public static class DisconnectTagValidator
    {
        public static bool Validate(string tag)
        {
            return tag == "-dn";
        }
    }
}