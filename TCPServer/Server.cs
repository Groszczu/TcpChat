using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
        private readonly object _lock = new object();
        private TcpListener _server;

        private bool _shutDown = false;

        private static int _nextClientId = 1;

        public Server(IPacketFormatter packetFormatter, ISessionsRepository sessionsRepository,
            ICommandHandler commandHandler)
        {
            _packetFormatter = packetFormatter;
            _sessionsRepository = sessionsRepository;
            _commandHandler = commandHandler;
        }

        public void Run(string ip, int port)
        {
            Console.WriteLine("[SERVER CONSOLE]");
            _server = new TcpListener(IPAddress.Parse(ip), port);
            _server.Start();
            while (!_shutDown)
            {
                var tag = Console.ReadLine();
                switch (tag)
                {
                    case "-s":
                        HandleConnection();
                        break;
                    case "-d":
                        DisconnectAllClients();
                        break;
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

        private void TryToQuit()
        {
            _shutDown = true;
            _server.Stop();
        }

        private void DisconnectAllClients()
        {
        }

        private void PrintHelp()
        {
            Console.WriteLine("Options:");
            Console.WriteLine($"{"-s",-20}start listening to clients");
            Console.WriteLine($"{"-d",-20}disconnect all clients");
            Console.WriteLine($"{"-h",-20}open help menu");
            Console.WriteLine($"{"-q",-20}try to quit program");
        }

        private void HandleConnection()
        {
            while (!_shutDown)
            {
                Console.WriteLine("Waiting for connection...");
                var newClient = _server.AcceptTcpClient();
                var newClientId = _nextClientId++;

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
            }
        }

        private void ProcessPacket(ClientData source, Packet data)
        {
            ICommand command = null;
            Guid sourceSessionId;
            lock (_lock) sourceSessionId = _sessionsRepository.GetSessionId(source);
            var errorPacket = new Packet(data.Operation.Value, Status.Unauthorized, sourceSessionId);
            try
            {
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
                    case Operation.Disconnect:
                        lock (_lock)
                        {
                            //command = new ServerDisconnect(source)
                        }

                        break;
                }
                if (command != null)
                    _commandHandler.Handle(command);
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine(exception.Message);
                command = null;
                errorPacket.SetMessage(exception.Message);
            }

            if (errorPacket.Message.IsSet)
            {
                source.SendTo(_packetFormatter.Serialize(errorPacket));
            }
        }
    }
}