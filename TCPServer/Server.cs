using System;
using System.Collections.Generic;
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

        private readonly Dictionary<int, ClientData> _clients = new Dictionary<int, ClientData>();

        public Server(IPacketFormatter packetFormatter, ISessionsRepository sessionsRepository)
        {
            _packetFormatter = packetFormatter;
            _sessionsRepository = sessionsRepository;
        }

        public async Task RunAsync(string ip, int port)
        {

            _server = new TcpListener(IPAddress.Parse(ip), port);
            _server.Start();
            while (!_shutDown)
            {
                Console.WriteLine("[SERVER CONSOLE]");
                Console.WriteLine("Enter '-h' to get help");
                Console.WriteLine("Enter option:");

                var option = Console.ReadLine();
                switch (option)
                {
                    case "-s":
                        await HandleConnection();
                        break;
                    case "-d": DisconnectAllClients();
                        break;
                    case "-q": TryToQuit();
                        break;
                    case "-h":
                        PrintHelp();
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

        private async Task HandleConnection()
        {
            while (!_shutDown)
            {
                Console.WriteLine("Waiting for connection...");
                var newClient = await _server.AcceptTcpClientAsync();
                var newClientId = _nextClientId++;
                
                var newClientData = new ClientData(newClientId, newClient);
                lock (_lock) _sessionsRepository.AddSession(newClientData, Guid.NewGuid());
               
                Console.WriteLine($"Connected with client {newClientId}");

                var receivingThread = new Thread(ReceiveFromClient);
                receivingThread.Start(newClientId);
            }
        }

        private void ReceiveFromClient(object o)
        {
            var clientId = (int) o;
            ClientData client;
            lock (_lock) client = _clients[clientId];
            while (true)
            {
                var stream = client.Socket.GetStream();
                var buffer = new byte[1024];
                stream.Read(buffer, 0, buffer.Length);

                ProcessPacket(client, _packetFormatter.Deserialize(buffer));
            }
        }

        private void ProcessPacket(ClientData source, Packet data)
        {
            switch (data.Operation)
            {
                case Operation.GetId: SendId(source, data);
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

        private void SendId(ClientData source, Packet data)
        {
            var stream = source.Socket.GetStream();
            var newPacket = new Packet(Operation.GetId, Status.Ok, _sessionsRepository.GetSessionId(source), data.Id.ToString());
            var buffer = _packetFormatter.Serialize(newPacket);
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
