// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Http;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using AggrEngine.NetworkIO;

namespace Microsoft.AspNetCore.Server.Kestrel.Filter
{
    public class FilteredStreamAdapter
    {
        private readonly Stream _filteredStream;
        private readonly Stream _socketInputStream;
        private readonly ILoggerBase _log;
        private readonly MemoryPool _memory;
        private MemoryPoolBlock _block;

        public FilteredStreamAdapter(
            Stream filteredStream,
            MemoryPool memory,
            ILoggerBase logger,
            IThreadPool threadPool)
        {
            SocketInput = new SocketInput(memory, threadPool);
            SocketOutput = new StreamSocketOutput(filteredStream, memory);

            _log = logger;
            _filteredStream = filteredStream;
            _socketInputStream = new SocketInputStream(SocketInput);
            _memory = memory;
        }

        public SocketInput SocketInput { get; private set; }

        public ISocketOutput SocketOutput { get; private set; }

        public void ReadInput()
        {
            _block = _memory.Lease();
            // Use pooled block for copy
            _filteredStream.CopyToAsync(_socketInputStream, _block).ContinueWith((task, state) =>
            {
                ((FilteredStreamAdapter)state).OnStreamClose(task);
            }, this);
        }

        private void OnStreamClose(Task copyAsyncTask)
        {
            _memory.Return(_block);

            if (copyAsyncTask.IsFaulted)
            {
                SocketInput.AbortAwaiting();
                _log.Error(0, "FilteredStreamAdapter.CopyToAsync", copyAsyncTask.Exception);
            }
            else if (copyAsyncTask.IsCanceled)
            {
                SocketInput.AbortAwaiting();
                _log.Error(0, "FilteredStreamAdapter.CopyToAsync canceled.",null);
            }

            try
            {
                _filteredStream.Dispose();
                _socketInputStream.Dispose();
            }
            catch (Exception ex)
            {
                _log.Error(0, "FilteredStreamAdapter.OnStreamClose", ex);
            }
        }
    }
}
