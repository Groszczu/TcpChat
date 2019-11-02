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
using TCPServer.Services;

namespace TCPServer
{
    public class Server
    {
        private readonly IPacketFormatter _packetFormatter;
        private readonly ISessionsRepository _sessionsRepository;
        private readonly object _lock = new object();
        private TcpListener _server;

        private bool _shutDown = false;

        private static int _nextClientId = 1;

        public Server(IPacketFormatter packetFormatter, ISessionsRepository sessionsRepository)
        {
            _packetFormatter = packetFormatter;
            _sessionsRepository = sessionsRepository;
        }

        public void Run(string ip, int port)
        {
            Console.WriteLine("[SERVER CONSOLE]");
            _server = new TcpListener(IPAddress.Parse(ip), port);
            _server.Start();
            while (!_shutDown)
            {
                Console.WriteLine("Enter operation tag:");

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

                ReceiveFromClient(newClientData).GetAwaiter().GetResult();
            }
        }

        private async Task ReceiveFromClient(object o)
        {
            if (!(o is ClientData client))
                throw new ArgumentException("Passed object is not ClientData type");
            
            while (true)
            {
                var stream = client.Socket.GetStream();

                ProcessPacket(client, await _packetFormatter.DeserializeAsync(stream));
            }
        }

        private void ProcessPacket(ClientData source, Packet data)
        {
            switch (data.Operation)
            {
                case Operation.GetId:
                    SendSessionIdAndOtherClientsIds(source, data);
                    break;
                //case Operation.Invite: SendInvite(source);
                //    break;
                //case Operation.AcceptInvite: AcceptInvite(source);
                //    break;
                //case Operation.Send: SendMessage(source, data.Message);
                //    break;
                //case Operation.Disconnect: Disconnect(source) ;
                //    break;
            }
        }

        private void SendSessionIdAndOtherClientsIds(ClientData source, Packet data)
        {
            Guid sessionId;
            lock (_lock) sessionId = _sessionsRepository.GetSessionId(source);
            
            var message = new StringBuilder($"Your client ID: {source.Id}, Your session ID: \'{sessionId}\'");
            AppendOtherClientsIdsToStringBuilder(source, message);
            
            var newPacket = new Packet(Operation.GetId, Status.Ok, sessionId, message.ToString());
            var buffer = _packetFormatter.Serialize(newPacket);
            var stream = source.Socket.GetStream();
            stream.Write(buffer, 0, buffer.Length);
        }

        private void AppendOtherClientsIdsToStringBuilder(ClientData clientData, StringBuilder stringBuilder)
        {
            stringBuilder.Append(" Other available clients IDs: ");
            var delimiter = string.Empty;
            lock (_lock)
            {
                var otherClients = _sessionsRepository.GetAllClients().Where(client => client.Id != clientData.Id);
            
                var thereIsOtherClient = false;
                foreach (var client in otherClients)
                {
                    thereIsOtherClient = true;
                    stringBuilder.Append(delimiter + client.Id.ToString());
                    delimiter = ", ";
                }
            
                if (!thereIsOtherClient)
                {
                    stringBuilder.Append("There is no other client");
                }
            }
        }
    }
}