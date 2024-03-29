﻿using System;
using System.Net;
using System.Net.Sockets;
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

        private bool _shutDown;

        public Server(IPacketFormatter packetFormatter, ISessionsRepository sessionsRepository,
            ICommandHandler commandHandler, IClientIdsRepository clientIdsRepository)
        {
            _packetFormatter = packetFormatter;
            _sessionsRepository = sessionsRepository;
            _commandHandler = commandHandler;
            _clientIdsRepository = clientIdsRepository;
        }

        // Uruchomienie aplikacji serwera
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

        // Metoda zwracająca lokalny adres IP
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
                localIpAddress = IPAddress.Loopback;
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

        // Metoda zwracająca numer wolnego portu
        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint) listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }

        // Metoda wywołująca inne metody prywatne w zależności od wprowadzonej wartości przez użytkownika
        private void ProcessInput(string tag)
        {
            switch (tag)
            {
                case "-s":
                    StartListening();
                    PrintServerParameters();
                    break;
                case "-q":
                    TryToQuit();
                    break;
                case "-h":
                    PrintHelp();
                    break;
                default:
                    PrintInvalidTagMessage();
                    break;
            }
        }


        private void StartListening()
        {
            _server.Start();
            HandleConnection();
        }

        // Metoda rejetrująca nowych klientów
        private async void HandleConnection()
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

                var newClientData = CreateAndRegisterNewClientData(newClient);
                Console.WriteLine($"Connected with client {newClientData.Id}");

                SendInitialPacket(newClientData);
                HandleReceivingFromClient(newClientData);
            }
        }

        // Zarejstrowanie nowego klienta do repozytorium
        private ClientData CreateAndRegisterNewClientData(TcpClient clientSocket)
        {
            var newClientId = _clientIdsRepository.NewClientId();
            var newClientData = new ClientData(newClientId, clientSocket);
            var newClientSessionId = Guid.NewGuid();
            lock (_lock)
                _sessionsRepository.AddSessionRecord(newClientData, newClientSessionId);

            return newClientData;
        }

        // Wysłanie pakietu inicjalnego
        private void SendInitialPacket(ClientData clientData)
        {
            Guid sessionId;
            lock (_lock)
                sessionId = _sessionsRepository.GetSessionId(clientData);

            var initialPacket = new Packet(Operation.GetId, Status.Initial, sessionId, clientData.Id);
            clientData.SendTo(_packetFormatter.Serialize(initialPacket));
        }

        // Metoda odbierająca pakiety od danego klienta
        private async void HandleReceivingFromClient(ClientData client)
        {
            var stream = client.Socket.GetStream();
            while (true)
            {
                if (client.ToClose)
                {
                    EndConnection(client);
                    Console.WriteLine($"Client {client.Id} disconnected successfully");
                    break;
                }

                Packet receivedPacket;
                try
                {
                    receivedPacket = await _packetFormatter.DeserializeAsync(stream);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Connection with client {client.Id} was forcibly closed");
                    EndConnection(client);
                    break;
                }

                // Klient rozłączył się bez wcześniejszego wysłania pakietu kończącego
                if (receivedPacket == null)
                {
                    Console.WriteLine($"Connection with client {client.Id} was forcibly closed");
                    EndConnection(client);
                    break;
                }

                ProcessPacket(client, receivedPacket);
            }
        }

        // Metoda zamykająca strumień przesyłania danych
        private void EndConnection(ClientData client)
        {
            lock (_lock)
                _sessionsRepository.RemoveClient(client);

            client.Socket.Client.Shutdown(SocketShutdown.Both);
            client.Socket.Close();
        }

        // Metoda okraślająca rodzaj pakietu, czyli odczytująca pola operacji, statusu
        // W zależności od otrzymanego pakietu wykonana zosaje odpowiednia operacja
        private void ProcessPacket(ClientData source, Packet data)
        {
            try
            {
                // określenie jaka operacja ma zostać wykonana
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
                // Jeżeli został rzucowny wyjątek (InvalidOperationException) to na konsoli zostaje wyświetlona wiadomość
                // o rodzaju błędu, po czym do klienta wysłany zostaje pakiet informujący o błędie
                Console.WriteLine($"Error caused by client {source.Id}: \"{exception.Message}\"");
                Guid sourceSessionId;
                lock (_lock)
                    sourceSessionId = _sessionsRepository.GetSessionId(source);
                var errorPacket = new Packet(data.Operation.Value, Status.Unauthorized, sourceSessionId, source.Id);
                source.SendTo(_packetFormatter.Serialize(errorPacket));
            }
        }

        private void PrintServerParameters()
        {
            Console.WriteLine($"Server is listening on {_ipAddressString}:{_portNumber}");
        }

        private void TryToQuit()
        {
            if (CanQuit())
                _shutDown = true;
            else
                Console.WriteLine("Cannot quit because clients are connected");
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

        private static void PrintInvalidTagMessage()
        {
            Console.WriteLine("Invalid operation tag");
            Console.WriteLine("Enter '-h' to get help");
        }
    }
}