using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Microsoft.VisualBasic;
using TCPClient.Models.Commands;

namespace TCPClient
{
    public class Client
    {
        private readonly IPacketFormatter _packetFormatter;
        private readonly ICommandHandler _commandHandler;
        private ISender _byteSender;
        private TcpClient _client;
        private Guid _sessionId = Guid.Empty;
        private int _id;
        private Status _status = Status.Ok;

        private IPAddress _ip;
        private int _port;

        private NetworkStream _stream;

        private Thread _receivingThread;
        private readonly ManualResetEvent _reset = new ManualResetEvent(false);

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

        public Client(IPacketFormatter packetFormatter, ICommandHandler commandHandler)
        {
            _packetFormatter = packetFormatter;
            _commandHandler = commandHandler;
        }

        public void Run(string ip, int port)
        {
            Console.WriteLine("[CLIENT CONSOLE]");
            _client = new TcpClient();
            _ip = IPAddress.Parse(ip);
            _port = port;
            while (true)
            {
                Console.WriteLine("\nEnter operation tag:");
                var tag = Console.ReadLine();
                switch (tag)
                {
                    case "-cn":
                        TryToConnect();
                        break;
                    case "-id":
                        _commandHandler.Handle(new ClientGetId(_sessionId, _byteSender, _packetFormatter));
                        _reset.WaitOne();
                        break;
                    case var someVal when someVal != null && new Regex(@"^-i\s+.*$").IsMatch(someVal):
                        var correctInviteRegex = new Regex(@"^-i\s+(?<id>\d+)$");
                        if (correctInviteRegex.IsMatch(tag))
                        {
                            var destinationId = int.Parse(correctInviteRegex.Match(tag).Groups["id"].Value);
                            _commandHandler.Handle(new ClientInvite(destinationId, _sessionId, _byteSender,
                                _packetFormatter));
                        }
                        else
                        {
                            InvalidInputMessage("Invite", "-i [id]");
                        }
                        break;
                    case var someVal when someVal != null && new Regex(@"^-a\s+.*$").IsMatch(someVal):
                        var correctAcceptInviteRegex = new Regex(@"^-a\s+(?<id>\d+)$");
                        if (tag != null && correctAcceptInviteRegex.IsMatch(tag))
                        {
                            var inviterId = int.Parse(correctAcceptInviteRegex.Match(tag).Groups["id"].Value);
                            _commandHandler.Handle(new ClientAcceptInvite(inviterId, _sessionId, _byteSender,
                                _packetFormatter));
                        }
                        else
                        {
                            InvalidInputMessage("Accept invite", "-a [id]");
                        }
                        break;
                    //case "-c": CloseSession();
                    //   break;
                    case "-q":
                        TryToQuit();
                        break;
                    case "-h":
                        PrintHelp();
                        break;
                    default:
                        Console.WriteLine("Invalid operation tag");
                        Console.WriteLine("Enter '-h' to get help");
                        break;
                }
            }
        }

        private void InvalidInputMessage(string operationName, string pattern)
        {
            Console.WriteLine("Invalid input");
            Console.WriteLine($"{operationName} operation should be organized like shown below:");
            Console.WriteLine(pattern);
        }

        private void TryToQuit()
        {
            if (IsConnected())
                Disconnect();
            Environment.Exit(0);
        }

        private bool IsConnected()
        {
            return _client.Connected;
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

            if (_client.Connected)
            {
                Console.WriteLine("Connected successfully");
                _stream = _client.GetStream();
                _byteSender = new ByteSender(_stream);

                _receivingThread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    ReceiveAndPrint().GetAwaiter().GetResult();
                });

                _receivingThread.Start();
            }
            else
            {
                Console.WriteLine($"Filed to connect in {maxAttempts} attempts");
            }
        }

        private async Task ReceiveAndPrint()
        {
            while (_client.Connected)
            {
                var receivedPacket = await _packetFormatter.DeserializeAsync(_stream);
                ProcessPacket(receivedPacket);
            }
        }

        private void ProcessPacket(Packet data)
        {
            Console.WriteLine(data.Message.Value);
            if (data.Status.Value == Status.Unauthorized)
                return;
            switch (data.Operation.Value)
            {
                case Operation.GetId:
                    _sessionId = data.Id.Value;
                    break;
                case Operation.Invite:

                    break;
                //case Operation.AcceptInvite: AcceptInvite(source);
                //    break;
                //case Operation.Send: SendMessage(source, data.Message);
                //    break;
                //case Operation.Disconnect: Disconnect(source) ;
                //    break;
            }

            _reset.Set();
        }

        private void Disconnect()
        {
            Console.WriteLine("Disconnecting from server...");
            _client.Client.Shutdown(SocketShutdown.Send);
            _receivingThread?.Join();
            _stream.Close();
            _client.Close();
            Console.WriteLine("Disconnected from server");
        }
    }
}