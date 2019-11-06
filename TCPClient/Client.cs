﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Microsoft.VisualBasic;
using TCPClient.Models.Commands;
using TCPClient.Services;
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

        private IPAddress _ip;
        private int _port;

        private NetworkStream _stream;

        private Thread _receivingThread;

        public Client(IPacketFormatter packetFormatter, ICommandHandler commandHandler)
        {
            _packetFormatter = packetFormatter;
            _commandHandler = commandHandler;
        }

        public void Run(string ip, int port)
        {
            Console.WriteLine("[CLIENT CONSOLE]");
            _ip = IPAddress.Parse(ip);
            _port = port;
            while (!_quiting)
            {
                var tag = Console.ReadLine();
                ProcessTag(tag);
            }
        }

        private void ProcessTag(string tag)
        {
            ICommand command = null;
            
            ITagFollowedByValueValidator validator;
            if (new HelpTagValidator().Validate(tag))
            {
                PrintHelp();
                return;
            }

            if (new ConnectionTagValidator().Validate(tag))
            {
                TryToConnect();
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
                    _quiting = true;
                    QuitProgram();
                }

            }

            if (!_client.Connected)
            {
                PrintConnectionRequest();
                return;
            }

            if (new IdTagValidator().Validate(tag))
                command = new ClientGetId(_sessionId, _byteSender, _packetFormatter);

            if ((validator = new InviteTagValidator()).Validate(tag))
            {
                var destinationId = int.Parse(validator.GetMatchedValue(tag));
                command = new ClientInvite(destinationId, _sessionId, _byteSender,
                    _packetFormatter);
            }

            if ((validator = new AcceptTagValidator()).Validate(tag))
            {
                var destinationId = int.Parse(validator.GetMatchedValue(tag));
                command = new ClientAcceptInvite(destinationId, _sessionId, _byteSender,
                    _packetFormatter);
            }

            if ((validator = new DeclineTagValidator()).Validate(tag))
            {
                var destinationId = int.Parse(validator.GetMatchedValue(tag));
                command = new ClientDeclineInvite(destinationId, _sessionId, _byteSender,
                    _packetFormatter);
            }

            if ((validator = new MessageTagValidator()).Validate(tag))
            {
                var messageToSend = validator.GetMatchedValue(tag);
                command = new ClientSendMessage(_sessionId, _byteSender, _packetFormatter, messageToSend);
            }

            if (new DisconnectTagValidator().Validate(tag))
            {
                command = new ClientDisconnect(_sessionId, _byteSender, _packetFormatter);
                _disconnecting = true;
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

        private void ProcessPacket(Packet data)
        {
            Console.WriteLine(data.Message.Value);
            if (data.Status.Value == Status.Unauthorized)
                return;
            if (data.Operation.Value == Operation.GetId)
            {
                _sessionId = data.Id.Value;
            }
            else if (_quiting && data.Operation.Value == Operation.Disconnect)
                QuitProgram();
        }

        private void QuitProgram()
        {
            Environment.Exit(0);
        }

        private void PrintConnectionRequest()
        {
            Console.WriteLine("Please connect to the server first");
        }

        private void PrintInvalidInputMessage()
        {
            Console.WriteLine("Invalid input");
            Console.WriteLine("Enter '-h' to get help");
        }

        private void PrintHelp()
        {
            Console.WriteLine("Options:");
            Console.WriteLine($"{"-cn",-20}try to connect to server");
            Console.WriteLine($"{"-id",-20}get your session ID and other clients IDs");
            Console.WriteLine($"{"-i [id]",-20}invite client with given ID to your session");
            Console.WriteLine($"{"-a [id]",-20}accept invite to other session from client with given ID");
            Console.WriteLine($"{"-d [id]",-20}decline invite to other session from client with given ID");
            Console.WriteLine($"{"-dn",-20}close current session");
            Console.WriteLine($"{"-q",-20}quit program");
        }

        private void TryToConnect(int maxAttempts = 3)
        {
            if (_client.Connected)
            {
                Console.WriteLine("You are connected to server");
                return;
            }
            
            _client = new TcpClient();
            var attempt = 0;
            while (!_client.Connected && ++attempt <= maxAttempts)
            {
                try
                {
                    Console.WriteLine("Connecting to the server...");
                    _client.Connect(_ip, _port);
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

                _receivingThread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    ReceiveAndPrint().GetAwaiter().GetResult();
                });

                _receivingThread.Start();
            }
            else
            {
                Console.WriteLine($"Filed to connect in {maxAttempts} attempts");
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
                var receivedPacket = await _packetFormatter.DeserializeAsync(_stream);
                ProcessPacket(receivedPacket);
            }
        }
    }
}