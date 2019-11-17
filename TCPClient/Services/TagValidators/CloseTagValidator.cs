namespace TCPClient.Services.TagValidators
{
    public static class CloseTagValidator
    {
        public static bool Validate(string tag)
        {
            return tag == "-c";
        }
    }
}