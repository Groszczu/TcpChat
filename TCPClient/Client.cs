using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        private int _id = -1;
        private Guid _sessionId = Guid.Empty;

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
            while (true)
            {
                var tag = Console.ReadLine();
                ProcessTag(tag);
            }
        }

        private void ProcessTag(string tag)
        {
            ClientCommand command = null;

            if (HelpTagValidator.Validate(tag))
            {
                PrintHelp();
                return;
            }

            if (ConnectionTagValidator.Validate(tag))
            {
                var ipAddress = ConnectionTagValidator.GetIpAddress(tag);
                var portNumber = ConnectionTagValidator.GetPortNumber(tag);
                TryToConnect(ipAddress, portNumber);
                return;
            }

            if (QuitTagValidator.Validate(tag))
            {
                if (_client.Connected)
                {
                    command = new ClientDisconnect(_sessionId, _byteSender, _packetFormatter, _id);
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
                case var input when IdTagValidator.Validate(input):
                    command = new ClientGetId(_sessionId, _byteSender, _packetFormatter, _id);
                    break;

                case var input when InviteTagValidator.Validate(input):
                    destinationId = int.Parse(InviteTagValidator.GetMatchedValue(tag));
                    command = new ClientInvite(destinationId, _sessionId, _byteSender,
                        _packetFormatter, _id);
                    break;

                case var input when AcceptTagValidator.Validate(input):
                    destinationId = int.Parse(AcceptTagValidator.GetMatchedValue(tag));
                    command = new ClientAcceptInvite(destinationId, _sessionId, _byteSender,
                        _packetFormatter, _id);
                    break;

                case var input when DeclineTagValidator.Validate(input):
                    destinationId = int.Parse(DeclineTagValidator.GetMatchedValue(tag));
                    command = new ClientDeclineInvite(destinationId, _sessionId, _byteSender,
                        _packetFormatter, _id);
                    break;

                case var input when MessageTagValidator.Validate(input):
                    var messageToSend = MessageTagValidator.GetMatchedValue(tag);
                    command = new ClientSendMessage(_sessionId, _byteSender, _packetFormatter, messageToSend, _id);
                    break;

                case var input when CloseTagValidator.Validate(input):
                    command = new ClientCloseAndOpenNewSessionCommand(_sessionId, _byteSender, _packetFormatter, _id);
                    break;

                case var input when DisconnectTagValidator.Validate(input):
                    command = new ClientDisconnect(_sessionId, _byteSender, _packetFormatter, _id);
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
                    await HandleReceivingPackets();
                });

                _receivingThread.Start();
            }
            else
            {
                Console.WriteLine("Failed to connect");
            }
        }

        private async Task HandleReceivingPackets()
        {
            while (true)
            {
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

                // server closed client's stream
                if (receivedPacket == null)
                {
                    _client.Client.Shutdown(SocketShutdown.Both);
                    _client.Close();
                    if (_quiting)
                        QuitProgram();
                    
                    break;
                }

                ProcessPacket(receivedPacket);
            }
        }

        private void ProcessPacket(Packet data)
        {
            var messageToPrint = new StringBuilder();
            if (data.Status.Value == Status.Unauthorized)
                messageToPrint.Append("Attempted to perform unauthorized operation");
            else
                switch (data.Operation.Value)
                {
                    case Operation.GetId:
                        UpdateSessionAndClientIds(data.Id.Value, data.DestinationId.Value);
                        messageToPrint.Append($"Your ID: {_id}, your session ID: '{_sessionId}'");

                        if (data.Status.Value == Status.Initial)
                            break;
                        if (data.Message.IsSet)
                            messageToPrint.Append("\nOther client's IDs: ")
                                .Append(data.Message.Value.Replace(";", ", "));
                        else
                            messageToPrint.Append("\nNo other clients connected");
                        break;
                    case Operation.Invite:
                        switch (data.Status.Value)
                        {
                            case Status.Ok:
                                messageToPrint.Append($"You got invited by client with ID: {data.SourceId.Value}");
                                break;
                            case Status.Accept:
                                messageToPrint.Append(
                                    $"Client with ID: {data.SourceId.Value} accepted your invite, You can chat now");
                                break;
                            case Status.Decline:
                                messageToPrint.Append($"Client with ID: {data.SourceId.Value} declined your invite");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case Operation.Message:
                        messageToPrint.Append($"Client {data.SourceId.Value}: ").Append(data.Message.Value);
                        break;
                    case Operation.CloseSession:
                        _sessionId = data.Id.Value;
                        messageToPrint.Append("You were moved to the new session.\n")
                            .Append($"Your new session ID: {_sessionId}");
                        break;
                    case Operation.Disconnect:
                        messageToPrint.Append("You were successfully disconnected from server");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            if (!string.IsNullOrEmpty(messageToPrint.ToString()))
                Console.WriteLine(messageToPrint.ToString());
        }

        private void UpdateSessionAndClientIds(Guid sessionId, int clientId)
        {
            _sessionId = sessionId;
            _id = clientId;
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
                $"{"-cn [ipAdr:portNum]",-20}try to connect to server with given IPv4 address and listening on given port");
            Console.WriteLine($"{"-id",-20}get your session ID and other client's IDs");
            Console.WriteLine($"{"-i [id]",-20}invite client with given ID to your session");
            Console.WriteLine($"{"-a [id]",-20}accept invite to other session from client with given ID");
            Console.WriteLine($"{"-d [id]",-20}decline invite to other session from client with given ID");
            Console.WriteLine($"{"-dn",-20}disconnect from server");
            Console.WriteLine(
                $"{"-c",-20}close current session and go to new one (possible only if other client is in your session)");
            Console.WriteLine($"{"-q",-20}quit (disconnects from server if needed)");
        }
    }
}