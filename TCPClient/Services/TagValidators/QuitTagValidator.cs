namespace TCPClient.Services.TagValidators
{
    public static class QuitTagValidator
    {
        public static bool Validate(string tag)
        {
            return tag == "-q";
        }
    }
}