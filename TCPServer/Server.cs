using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using Core;

namespace TCPServer
{
    class ClientData
    {
        public int Id { get; }
        public int SessionId { get; }
        public TcpClient Socket { get; }

        public ClientData(int id, int sessionId, TcpClient socket)
        {
            Id = id;
            SessionId = sessionId;
            Socket = socket;
        }

    }
    
    class Server
    {
        private readonly object _lock = new object();
        private readonly TcpListener _server;

        private bool _shutDown = false;

        private static int _nextClientId = 1;
        private static int _nextSessionId = 100;

        private readonly Dictionary<int, ClientData> _clients = new Dictionary<int, ClientData>();

        public Server(string ip, int port)
        {
            _server = new TcpListener(IPAddress.Parse(ip), port);
            _server.Start();
        }
        public void Run()
        {
            while (!_shutDown)
            {
                Console.WriteLine("[SERVER CONSOLE]");
                Console.WriteLine("Enter '-h' to get help");
                Console.WriteLine("Enter option:");

                var option = Console.ReadLine();
                switch (option)
                {
                    case "-s":
                        StartListening();
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
        private void StartListening()
        {
            var connectionHandler = new Thread(HandleConnection);
            connectionHandler.Start();
            if (_shutDown) connectionHandler.Join();
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
                var newSessionId = _nextSessionId++;
                
                var newClientData = new ClientData(newClientId, newSessionId, newClient);
                lock (_lock) _clients.Add(newClientId, newClientData);
               
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
                var byteCount = stream.Read(buffer, 0, buffer.Length);

                var data = Encoding.ASCII.GetString(buffer, 0, byteCount);

                ProcessPacket(client, HeaderBuilder.ConvertStringToPacket(data));
            }
        }

        private void ProcessPacket(ClientData source, PacketData data)
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

        private void SendId(ClientData source, PacketData data)
        {
            var stream = source.Socket.GetStream();
            var newPacket = new PacketData(Operation.GetId, Status.Ok, source.SessionId, "dupa");
            var messageToSend = HeaderBuilder.ConvertPacketToString(newPacket);
            var buffer = Encoding.ASCII.GetBytes(messageToSend);

            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
