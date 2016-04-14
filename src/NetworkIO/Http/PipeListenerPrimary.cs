// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Networking;
using AggrEngine.NetworkIO;

namespace Microsoft.AspNetCore.Server.Kestrel.Http
{
    /// <summary>
    /// An implementation of <see cref="ListenerPrimary"/> using UNIX sockets.
    /// </summary>
    public class PipeListenerPrimary : ListenerPrimary
    {
        public PipeListenerPrimary(ServiceContext serviceContext) : base(serviceContext)
        {
        }

        /// <summary>
        /// Creates the socket used to listen for incoming connections
        /// </summary>
        protected override UvStreamHandle CreateListenSocket()
        {
            var socket = new UvPipeHandle { Log = Log };
            socket.Init(Thread.Loop, Thread.QueueCloseHandle, false);
            socket.Bind(ServerAddress.UnixPipePath);
            socket.Listen(Constants.ListenBacklog, (stream, status, error, state) => ConnectionCallback(stream, status, error, state), this);
            return socket;
        }

        /// <summary>
        /// Handles an incoming connection
        /// </summary>
        /// <param name="listenSocket">Socket being used to listen on</param>
        /// <param name="status">Connection status</param>
        protected override void OnConnection(UvStreamHandle listenSocket, int status)
        {
            var acceptSocket = new UvPipeHandle { Log = Log };

            try
            {
                acceptSocket.Init(Thread.Loop, Thread.QueueCloseHandle, false);
                listenSocket.Accept(acceptSocket);
                DispatchConnection(acceptSocket);
            }
            catch (UvException ex)
            {
                Log.Error(0, "ListenerPrimary.OnConnection", ex);
                acceptSocket.Dispose();
                return;
            }
        }
    }
}
