using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core;
using TCPServer.Models;
using TCPServer.Models.Commands;
using TCPServer.Services;

namespace TCPServer
{
    public class Server
    {
        private readonly IPacketFormatter _packetFormatter;
        private readonly ISessionsRepository _sessionsRepository;
        private readonly ICommandHandler _commandHandler;
        private readonly IClientIdsRepository _clientIdsRepository;

        private readonly object _lock = new object();
        private TcpListener _server;
        private int _portNumber;
        private string _ipAddressString;


        private Thread _connectionThread;

        private bool _shutDown;

        public Server(IPacketFormatter packetFormatter, ISessionsRepository sessionsRepository,
            ICommandHandler commandHandler, IClientIdsRepository clientIdsRepository)
        {
            _packetFormatter = packetFormatter;
            _sessionsRepository = sessionsRepository;
            _commandHandler = commandHandler;
            _clientIdsRepository = clientIdsRepository;
        }

        public void Run()
        {
            var localIpAddress = TryToGetLocalIpAddress();
            _ipAddressString = localIpAddress.ToString();
            _portNumber = GetFreeTcpPort();
            _server = new TcpListener(localIpAddress, _portNumber);

            Console.WriteLine("[SERVER CONSOLE]");
            while (!_shutDown)
            {
                var tag = Console.ReadLine();
                ProcessInput(tag);
            }
        }

        private void ProcessInput(string tag)
        {
            switch (tag)
            {
                case "-s":
                    _server.Start();
                    Console.WriteLine($"Server IP: {_ipAddressString}");
                    Console.WriteLine($"Listening on port {_portNumber}");
                    _connectionThread = new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        HandleConnection();
                    });
                    _connectionThread.Start();
                    break;
                case "-q":
                    if (CanQuit())
                        _shutDown = true;
                    else
                        Console.WriteLine("Cannot quit because clients are connected");
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

        private void HandleConnection()
        {
            while (!_shutDown)
            {
                Console.WriteLine("Waiting for connection...");
                TcpClient newClient;
                try
                {
                    newClient = _server.AcceptTcpClient();
                }
                catch (SocketException)
                {
                    Console.WriteLine("Stopped waiting for connections");
                    return;
                }

                var newClientId = _clientIdsRepository.NewClientId();

                var newClientData = new ClientData(newClientId, newClient);
                lock (_lock) _sessionsRepository.AddSessionRecord(newClientData, Guid.NewGuid());


                Console.WriteLine($"Connected with client {newClientId}");
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    ReceiveFromClient(newClientData).GetAwaiter().GetResult();
                }).Start();
            }
        }

        private async Task ReceiveFromClient(object o)
        {
            if (!(o is ClientData client))
                throw new ArgumentException("Passed object is not ClientData type");

            while (true)
            {
                var stream = client.Socket.GetStream();

                var receivedPacket = await _packetFormatter.DeserializeAsync(stream);
                ProcessPacket(client, receivedPacket);

                if (!client.ToClose)
                    continue;

                stream.Close();
                client.Socket.Close();
                Console.WriteLine($"Client {client.Id} disconnected successfully");
                break;
            }

            Thread.CurrentThread.Join();
        }

        private void ProcessPacket(ClientData source, Packet data)
        {
            Guid sourceSessionId;
            lock (_lock) sourceSessionId = _sessionsRepository.GetSessionId(source);
            var errorPacket = new Packet(data.Operation.Value, Status.Unauthorized, sourceSessionId);
            try
            {
                ICommand command;
                switch (data.Operation.Value)
                {
                    case Operation.GetId:
                        lock (_lock)
                            command = new ServerGetId(source, _sessionsRepository, _packetFormatter);
                        break;
                    case Operation.Invite:
                        lock (_lock)
                            command = data.Status.Value switch
                            {
                                Status.Ok => new ServerInvite(source, data.DestinationId.Value, _sessionsRepository,
                                    _packetFormatter),
                                Status.Accept => new ServerAcceptInvite(source, data.DestinationId.Value,
                                    _sessionsRepository, _packetFormatter),
                                Status.Decline => new ServerDeclineInvite(source, data.DestinationId.Value,
                                    _sessionsRepository, _packetFormatter),
                                _ => (ICommand) null
                            };
                        break;
                    case Operation.Message:
                        lock (_lock)
                            command = new ServerSendMessage(source,
                                _sessionsRepository, _packetFormatter, data.Message.Value);
                        break;
                    case Operation.CloseSession:
                        lock (_lock)
                            command = new ServerCloseAndOpenNewSessionCommand(source, _sessionsRepository,
                                _packetFormatter);
                        break;
                    case Operation.Disconnect:
                        lock (_lock)
                            command = new ServerDisconnect(source, _sessionsRepository, _packetFormatter);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (command != null)
                    _commandHandler.Handle(command);
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine(exception.Message);
                errorPacket.SetMessage(exception.Message);
            }

            if (errorPacket.Message.IsSet)
            {
                source.SendTo(_packetFormatter.Serialize(errorPacket));
            }
        }

        private static IPAddress TryToGetLocalIpAddress()
        {
            IPAddress localIpAddress;
            try
            {
                localIpAddress = IPAddress.Parse(GetLocalIpAddress());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            return localIpAddress;
        }

        private static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system");
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();
            
            return port;
        }

        private bool CanQuit()
        {
            lock (_lock)
                return _sessionsRepository.IsEmpty();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Options:");
            Console.WriteLine($"{"-h",-20}open help menu");
            Console.WriteLine($"{"-s",-20}start listening");
            Console.WriteLine($"{"-q",-20}try to quit");
        }
    }
}