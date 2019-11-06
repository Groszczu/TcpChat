﻿using System;
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

        private Thread _connectionThread;

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
                        _connectionThread = new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            HandleConnection();
                        });
                        _connectionThread.Start();
                        break;
                    case "-d":
                        DisconnectAllClients();
                        Console.WriteLine("Disconnected all clients");
                        break;
                    case "-q":
                        // TODO: server disconnects all clients and quit correcly
                        DisconnectAllClients();
                        _server.Stop();
                        if (_connectionThread != null && _connectionThread.IsAlive)
                            _connectionThread.Join();
                        DisconnectAllClientsAndQuit();
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
                
                if (client.ToClose)
                {
                    stream.Close();
                    client.Socket.Close();
                    Console.WriteLine($"Client {client.Id} disconnected successfully");
                    break;
                }
            }

            Thread.CurrentThread.Join();
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
                            command = new ServerDisconnect(source, _sessionsRepository, _packetFormatter);
                        break;
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

        private void DisconnectAllClients()
        {
            lock (_lock)
                _sessionsRepository.RemoveAllClients();
        }

        private void DisconnectAllClientsAndQuit()
        {
            DisconnectAllClients();
            Environment.Exit(0);
        }

        private void PrintHelp()
        {
            Console.WriteLine("Options:");
            Console.WriteLine($"{"-s",-20}start listening to clients");
            Console.WriteLine($"{"-d",-20}disconnect all clients");
            Console.WriteLine($"{"-h",-20}open help menu");
            Console.WriteLine($"{"-q",-20}try to quit program");
        }
    }
}