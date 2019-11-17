namespace TCPClient.Services.TagValidators
{
    public static class HelpTagValidator
    {
        public static bool Validate(string tag)
        {
            return tag == "-h";
        }
    }
}