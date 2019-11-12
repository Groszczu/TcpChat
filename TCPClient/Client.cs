using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Core;
using TCPClient.Models.Commands;
using TCPClient.Services.TagValidators;

namespace TCPClient
{
    public class Client
    {
        private readonly IPacketFormatter _packetFormatter;
        private readonly ICommandHandler _commandHandler;
        private ISender _byteSender;
        private TcpClient _client = new TcpClient();
        private Guid _sessionId = Guid.Empty;
        private bool _disconnecting;
        private bool _quiting;

        private NetworkStream _stream;

        private Thread _receivingThread;

        public Client(IPacketFormatter packetFormatter, ICommandHandler commandHandler)
        {
            _packetFormatter = packetFormatter;
            _commandHandler = commandHandler;
        }

        public void Run()
        {
            Console.WriteLine("[CLIENT CONSOLE]");
            while (!_quiting)
            {
                var tag = Console.ReadLine();
                ProcessTag(tag);
            }
        }

        private void ProcessTag(string tag)
        {
            ClientCommand command = null;
            ITagFollowedByValueValidator validator;

            if (new HelpTagValidator().Validate(tag))
            {
                PrintHelp();
                return;
            }

            if (new ConnectionTagValidator().Validate(tag))
            {
                var connectionTagValidator = new ConnectionTagValidator();
                var ipAddress = connectionTagValidator.GetIpAddress(tag);
                var portNumber = connectionTagValidator.GetPortNumber(tag);
                TryToConnect(ipAddress, portNumber);
                return;
            }

            if (new QuitTagValidator().Validate(tag))
            {
                if (_client.Connected)
                {
                    command = new ClientDisconnect(_sessionId, _byteSender, _packetFormatter);
                    _quiting = true;
                }
                else
                {
                    QuitProgram();
                }
            }

            if (!_client.Connected)
            {
                PrintInvalidInputOrConnectionRequiredMessage();
                return;
            }

            int destinationId;
            switch (tag)
            {
                case var input when new IdTagValidator().Validate(input):
                    command = new ClientGetId(_sessionId, _byteSender, _packetFormatter);
                    break;
                
                case var input when (validator = new InviteTagValidator()).Validate(input):
                    destinationId = int.Parse(validator.GetMatchedValue(tag));
                    command = new ClientInvite(destinationId, _sessionId, _byteSender,
                        _packetFormatter);
                    break;
                
                case var input when (validator = new AcceptTagValidator()).Validate(input):
                    destinationId = int.Parse(validator.GetMatchedValue(tag));
                    command = new ClientAcceptInvite(destinationId, _sessionId, _byteSender,
                        _packetFormatter);
                    break;
                
                case var input when (validator = new DeclineTagValidator()).Validate(input):
                    destinationId = int.Parse(validator.GetMatchedValue(tag));
                    command = new ClientDeclineInvite(destinationId, _sessionId, _byteSender,
                        _packetFormatter);
                    break;
                
                case var input when (validator = new MessageTagValidator()).Validate(input):
                    var messageToSend = validator.GetMatchedValue(tag);
                    command = new ClientSendMessage(_sessionId, _byteSender, _packetFormatter, messageToSend);
                    break;
                
                case var input when new CloseTagValidator().Validate(input):
                    command = new ClientCloseAndOpenNewSessionCommand(_sessionId, _byteSender, _packetFormatter);
                    break;
                
                case var input when new DisconnectTagValidator().Validate(input):
                    command = new ClientDisconnect(_sessionId, _byteSender, _packetFormatter);
                    _disconnecting = true;
                    break;
            }

            if (command != null)
            {
                _commandHandler.Handle(command);
            }
            else
            {
                PrintInvalidInputMessage();
            }
        }

        private void TryToConnect(IPAddress ipAddress, int portNumber, int maxAttempts = 3)
        {
            if (_client.Connected)
            {
                Console.WriteLine("Disconnect from the server in order to try to set a new connection");
                return;
            }

            _client = new TcpClient();
            var attempt = 0;
            while (!_client.Connected && ++attempt <= maxAttempts)
            {
                try
                {
                    Console.WriteLine("Connecting to the server...");
                    _client.Connect(ipAddress, portNumber);
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

                _receivingThread = new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await ReceiveAndPrint();
                });

                _receivingThread.Start();
            }
            else
            {
                Console.WriteLine("Filed to connect");
            }
        }

        private async Task ReceiveAndPrint()
        {
            while (true)
            {
                if (_disconnecting)
                {
                    _stream.Close();
                    _client.Close();
                    _disconnecting = false;
                    break;
                }

                Packet receivedPacket;
                try
                {
                    receivedPacket = await _packetFormatter.DeserializeAsync(_stream);
                }
                catch (Exception)
                {
                    Console.WriteLine("Server was forcibly closed");
                    _stream.Close();
                    _client.Close();
                    break;
                }

                ProcessPacket(receivedPacket);
            }
        }

        private void ProcessPacket(Packet data)
        {
            Console.WriteLine(data.Message.Value);
            if (data.Status.Value == Status.Unauthorized)
                return;
            
            if (_sessionId == Guid.Empty)
                _sessionId = data.Id.Value;
            
            if (_quiting && data.Operation.Value == Operation.Disconnect)
                QuitProgram();
        }

        private static void QuitProgram()
        {
            Environment.Exit(0);
        }

        private static void PrintInvalidInputOrConnectionRequiredMessage()
        {
            PrintInvalidInputMessage();
            Console.WriteLine("[Or connection required to process this tag]");
        }

        private static void PrintInvalidInputMessage()
        {
            Console.WriteLine("Invalid input");
            Console.WriteLine("Enter '-h' to get help");
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Options:");
            Console.WriteLine($"{"-h",-20}open help menu");
            Console.WriteLine(
                $"{"-cn [ipAddress:portNumber]",-20}try to connect to server with given IPv4 address and listening on given port");
            Console.WriteLine($"{"-id",-20}get your session ID and other client's IDs");
            Console.WriteLine($"{"-i [id]",-20}invite client with given ID to your session");
            Console.WriteLine($"{"-a [id]",-20}accept invite to other session from client with given ID");
            Console.WriteLine($"{"-d [id]",-20}decline invite to other session from client with given ID");
            Console.WriteLine($"{"-dn",-20}disconnect from server");
            Console.WriteLine(
                $"{"-c",-20}close current session and go to new one (possible only if other client is in your session)");
            Console.WriteLine($"{"-q",-20}quit");
        }
    }
}