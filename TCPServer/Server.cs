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

        public Server(IPacketFormatter packetFormatter, ISessionsRepository sessionsRepository, ICommandHandler commandHandler)
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
                Console.WriteLine("\nEnter operation tag:");

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
            Console.WriteLine("-s".PadRight(20) + "start listening to clients");
            Console.WriteLine("-d".PadRight(20) + "disconnect all clients");
            Console.WriteLine("-h".PadRight(20) + "open help menu");
            Console.WriteLine("-q".PadRight(20) + "try to quit program");
        }

        private void HandleConnection()
        {
            while (!_shutDown)
            {
                Console.WriteLine("Waiting for connection...");
                var newClient = _server.AcceptTcpClient();
                var newClientId = _nextClientId++;

                var newClientData = new ClientData(newClientId, newClient);
                lock (_lock) _sessionsRepository.AddSession(newClientData, Guid.NewGuid());
                
                

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
            switch (data.Operation.Value)
            {
                case Operation.GetId:
                    {
                        try
                        {
                            lock (_lock) command = new ServerGetId(source, _sessionsRepository, _packetFormatter);
                        }
                        catch (InvalidOperationException exception)
                        {
                            Console.WriteLine(exception.Message);
                            command = null;
                            errorPacket.SetMessage(exception.Message);
                        }
                    }
                    break;
                case Operation.Invite: 
                    lock (_lock)
                    {
                        try
                        {
                            command = new ServerInvite(source, data.DestinationId.Value, _sessionsRepository,
                                _packetFormatter);
                        }
                        catch (InvalidOperationException exception)
                        {
                            Console.WriteLine(exception.Message);
                            command = null;
                            errorPacket.SetMessage(exception.Message);
                        }
                    }
                    break;
                //case Operation.AcceptInvite: AcceptInvite(source);
                //    break;
                //case Operation.Send: SendMessage(source, data.Message);
                //    break;
                case Operation.Disconnect:
                    lock (_lock)
                    {
                        //command = new ServerDisconnect(source)
                    }
                    break;
            }
            if (!errorPacket.Message.IsSet && command != null)
            {
                _commandHandler.Handle(command);
            }
            else
            {
                source.SendTo(_packetFormatter.Serialize(errorPacket));
            }
        }
    }
}