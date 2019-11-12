using System;
using System.IO;
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
                    _connectionThread = new Thread(async () =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        await HandleConnection();
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

        private async Task HandleConnection()
        {
            while (!_shutDown)
            {
                Console.WriteLine("Waiting for connection...");
                TcpClient newClient;
                try
                {
                    newClient = await _server.AcceptTcpClientAsync();
                }
                catch (SocketException)
                {
                    Console.WriteLine("Forcibly stopped waiting for connections");
                    return;
                }

                var newClientId = _clientIdsRepository.NewClientId();

                var newClientData = new ClientData(newClientId, newClient);
                lock (_lock)
                    _sessionsRepository.AddSessionRecord(newClientData, Guid.NewGuid());


                Console.WriteLine($"Connected with client {newClientId}");
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await ReceiveFromClient(newClientData);
                }).Start();
            }
        }

        private async Task ReceiveFromClient(object o)
        {
            if (!(o is ClientData client))
                throw new ArgumentException("Passed object is not ClientData type");

            var stream = client.Socket.GetStream();
            while (true)
            {
                Packet receivedPacket;
                try
                {
                    receivedPacket = await _packetFormatter.DeserializeAsync(stream);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Connection with client {client.Id} was forcibly closed");
                    EndConnection(client, stream);
                    break;
                }

                ProcessPacket(client, receivedPacket);

                if (!client.ToClose)
                    continue;

                EndConnection(client, stream);
                Console.WriteLine($"Client {client.Id} disconnected successfully");
                break;
            }
        }

        private void ProcessPacket(ClientData source, Packet data)
        {
            Guid sourceSessionId;
            lock (_lock) sourceSessionId = _sessionsRepository.GetSessionId(source);
            var errorPacket = new Packet(data.Operation.Value, Status.Unauthorized, sourceSessionId);
            try
            {
                ServerCommand command;
                lock (_lock)
                    command = data.Operation.Value switch
                    {
                        Operation.GetId => new ServerGetId(source, _sessionsRepository, _packetFormatter),
                        Operation.Invite => (data.Status.Value switch
                        {
                            Status.Ok => new ServerInvite(source, data.DestinationId.Value, _sessionsRepository,
                                _packetFormatter),
                            Status.Accept => new ServerAcceptInvite(source, data.DestinationId.Value,
                                _sessionsRepository, _packetFormatter),
                            Status.Decline => new ServerDeclineInvite(source, data.DestinationId.Value,
                                _sessionsRepository, _packetFormatter),
                            _ => (ICommand) null
                        }),
                        Operation.Message => new ServerSendMessage(source, _sessionsRepository, _packetFormatter,
                            data.Message.Value),
                        Operation.CloseSession => new ServerCloseAndOpenNewSessionCommand(source, _sessionsRepository,
                            _packetFormatter),
                        Operation.Disconnect => new ServerDisconnect(source, _sessionsRepository, _packetFormatter),
                        _ => (ICommand) null
                    } as ServerCommand;

                if (command != null)
                    _commandHandler.Handle(command);
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine($"Error message sent to client {source.Id}: \"{exception.Message}\"");
                errorPacket.SetMessage(exception.Message);
            }

            if (errorPacket.Message.IsSet)
                source.SendTo(_packetFormatter.Serialize(errorPacket));
        }

        private static IPAddress TryToGetLocalIpAddress()
        {
            IPAddress localIpAddress;
            try
            {
                localIpAddress = IPAddress.Parse(GetLocalIpAddress());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                throw;
            }

            return localIpAddress;
        }

        private void EndConnection(ClientData client, Stream stream)
        {
            lock (_lock)
                _sessionsRepository.RemoveClient(client);

            stream.Close();
            client.Socket.Close();
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