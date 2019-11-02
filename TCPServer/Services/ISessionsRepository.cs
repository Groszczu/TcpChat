﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPServer.Models;

namespace TCPServer.Services
{
    public interface ISessionsRepository
    {
        void AddSession(ClientData clientData, Guid sessionId);
        Guid GetSessionId(ClientData clientData);
    }
}