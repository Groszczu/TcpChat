namespace TCPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client("127.0.0.1", 13000);
            client.Run();
        }
    }
}
