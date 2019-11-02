using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Core;
using Microsoft.VisualBasic;

namespace TCPClient
{
    
    public class Client
    {
        private readonly IPacketFormatter _packetFormatter;
        private TcpClient _client;
        private Guid _sessionId = Guid.Empty;
        private Status _status = Status.Ok;

        private IPAddress _ip;
        private int _port;

        private NetworkStream _stream;
        private byte[] _buffer = new byte[1024];

        private Thread _receivingThread;

        private DateTime _connectionTime;
        public DateTime ConnectionTime
        {
            get => _connectionTime;
            private set
            {
                if (value.CompareTo(_connectionTime) > 0)
                {
                    _connectionTime = value;
                }
            }
        }
        public Client(IPacketFormatter packetFormatter)
        {
            _packetFormatter = packetFormatter;
        }

        public void Run(string ip, int port)
        {
            _client = new TcpClient();
            _ip = IPAddress.Parse(ip);
            _port = port;
            while (true)
            {

                Console.WriteLine("[CLIENT CONSOLE]");
                Console.WriteLine("Enter '-h' to get help");
                Console.WriteLine("Enter option:");

                var option = Console.ReadLine();
                switch (option)
                {
                    case "-cn":
                        TryToConnect();
                        break;
                    case "-id": GetSessionId();
                        break;
                    case var someVal when new Regex(@"^-i.*$").IsMatch(someVal):
                        var correctInviteRegex = new Regex(@"^-i\s+(?<id>\d+)$");
                        if (correctInviteRegex.IsMatch(option))
                        {
                            InviteUserById(int.Parse(correctInviteRegex.Match(option).Groups["id"].Value));
                        }
                        else
                        {
                            PrintInvalidInviteMessage();
                        }
                        break;
                    //case "-a": AcceptInvite();
                    //   break;
                    //case "-c": CloseSession();
                    //   break;
                    //  case "-q": TryToQuit();
                    //   break;
                    case "-h":
                        PrintHelp();
                        break;
                }
            }
        }

        private void GetSessionId()
        {
            var packet = new Packet(Operation.GetId, _status, _sessionId, "");
            _buffer = _packetFormatter.Serialize(packet);
            _stream.Write(_buffer, 0, _buffer.Length);
        }

        private void InviteUserById(int id)
        {
            Console.WriteLine(id);
            if (!IsConnected())
            {
                Console.WriteLine("Please connect to the server first");
                return;
            }

            //Send(MakeHeader());


        }

        private bool IsConnected()
        {
            return _client.Connected;
        }

        private void PrintInvalidInviteMessage()
        {
            Console.WriteLine("Invalid input");
            Console.WriteLine("Invite option should be organized like shown below:");
            Console.WriteLine("-i [id]");
        }

        private void PrintHelp()
        {
            Console.WriteLine("Options:");
            Console.WriteLine("-cn".PadRight(20) + "try to connect to server");
            Console.WriteLine("-id".PadRight(20) + "get your session id");
            Console.WriteLine("-i".PadRight(20) + "user to your session " +
                              "(only if there is other user connected to the server");
            Console.WriteLine("-a".PadRight(20) + "accept invite to other session");
            Console.WriteLine("-c".PadRight(20) + "close current session");
            Console.WriteLine("-q".PadRight(20) + "quit program");
        }

        private void TryToConnect(int maxAttempts = 3)
        { 
            var attempt = 0;
            while (!_client.Connected && ++attempt <= maxAttempts)
            {
                try
                {
                    Console.WriteLine("Connecting to the server...");
                    _client.Connect(_ip, _port);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Unable to connect");
                }
            }

            Console.Clear();
            if (_client.Connected)
            {
                Console.WriteLine("Connected successfully");
                _stream = _client.GetStream();

                _receivingThread = new Thread(ReceiveAndPrint);
                _receivingThread.Start();

                ConnectionTime = DateTime.Now;
            }
            else
            {
                Console.WriteLine($"Filed to connect in {maxAttempts} attempts");
            }
        }

        private void Send(string message)
        {
            if (!_client.Connected) return;
            _buffer = Encoding.ASCII.GetBytes(message);
            _stream.Write(_buffer, 0, _buffer.Length);
        }

        private void ReceiveAndPrint()
        {
            while (_client.Connected)
            {
                _buffer = new byte[1024];
                var byteCount = _stream.Read(_buffer, 0, _buffer.Length);

                if (byteCount == 0)
                {
                    return;
                }
                var receivedMassage = _packetFormatter.Deserialize(_buffer);
                Console.WriteLine("Message: " + receivedMassage);
            }
        }

        private void Disconnect()
        {
            Console.WriteLine("Disconnecting from server...");
            _client.Client.Shutdown(SocketShutdown.Send);
            _receivingThread.Join();
            _stream.Close();
            _client.Close();
            Console.WriteLine("Disconnected from server");
        }
    }
}
