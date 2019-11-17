namespace TCPClient.Services.TagValidators
{
    public static class IdTagValidator
    {
        public static bool Validate(string tag)
        {
            return tag == "-id";
        }
    }
}