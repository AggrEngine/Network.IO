// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Networking;
using System.Collections.Generic;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.AspNetCore.Server.Kestrel.Http;

namespace AggrEngine.NetworkIO
{
    /// <summary>
    /// 
    /// </summary>
    public class AsyncNetworkHost : IDisposable
    {
        private readonly Libuv _uv;
        private readonly ServiceContext _context;
        internal readonly IApplicationLifetime _appLifetime;

        public AsyncNetworkHost(ServiceContext context)
        {
            _uv = new Libuv();
            _appLifetime = context.AppLifetime;
            Threads = new List<KestrelThread>();
            _context = context;
        }

        internal List<KestrelThread> Threads { get; private set; }
        internal Libuv Libuv { get { return _uv; } }

        public ILoggerBase Log { get; internal set; }
        public IThreadPool ThreadPool { get; internal set; }

        public void Start(int count)
        {
            for (var index = 0; index < count; index++)
            {
                Threads.Add(new KestrelThread(this));
            }

            foreach (var thread in Threads)
            {
                thread.StartAsync().Wait();
            }
        }

        public IDisposable CreateServer(NetworkAddress address)
        {
            _context.HttpComponentFactory = new HttpComponentFactory(address);
            var listeners = new List<IAsyncDisposable>();
            var usingPipes = address.IsUnixPipe;

            try
            {
                var pipeName = (Libuv.IsWindows ? @"\\.\pipe\kestrel_" : "/tmp/kestrel_") + Guid.NewGuid().ToString("n");

                var single = Threads.Count == 1;
                var first = true;

                foreach (var thread in Threads)
                {
                    if (single)
                    {
                        var listener = usingPipes ?
                            (Listener)new PipeListener(_context) :
                            new TcpListener(_context);
                        listeners.Add(listener);
                        listener.StartAsync(address, thread).Wait();
                    }
                    else if (first)
                    {
                        var listener = usingPipes
                            ? (ListenerPrimary)new PipeListenerPrimary(_context)
                            : new TcpListenerPrimary(_context);

                        listeners.Add(listener);
                        listener.StartAsync(pipeName, address, thread).Wait();
                    }
                    else
                    {
                        var listener = usingPipes
                            ? (ListenerSecondary)new PipeListenerSecondary(_context)
                            : new TcpListenerSecondary(_context);
                        listeners.Add(listener);
                        listener.StartAsync(pipeName, address, thread).Wait();
                    }

                    first = false;
                }

                return new Disposable(() =>
                {
                    DisposeListeners(listeners);
                });
            }
            catch
            {
                DisposeListeners(listeners);

                throw;
            }
        }


        private void DisposeListeners(List<IAsyncDisposable> listeners)
        {
            var disposeTasks = new List<Task>();

            foreach (var listener in listeners)
            {
                disposeTasks.Add(listener.DisposeAsync());
            }

            if (!Task.WhenAll(disposeTasks).Wait(_context.ServerInformation.ShutdownTimeout))
            {
               NetworkLogger.NotAllConnectionsClosedGracefully(Log);
            }
        }
        public void Dispose()
        {
            foreach (var thread in Threads)
            {
                thread.Stop(TimeSpan.FromSeconds(2.5));
            }
            Threads.Clear();
        }
    }
}
