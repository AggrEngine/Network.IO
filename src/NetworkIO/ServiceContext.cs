// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using Microsoft.AspNetCore.Server.Kestrel.Http;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;

namespace AggrEngine.NetworkIO
{
    public class ServiceContext
    {
        public ServiceContext()
        {
        }

        public ServiceContext(ServiceContext context)
        {
            AppLifetime = context.AppLifetime;
            Log = context.Log;
            ThreadPool = context.ThreadPool;
            FrameFactory = context.FrameFactory;
            //DateHeaderValueManager = context.DateHeaderValueManager;
            ServerInformation = context.ServerInformation;
            //HttpComponentFactory = context.HttpComponentFactory;
        }

        public IApplicationLifetime AppLifetime { get; set; }

        public ILoggerBase Log { get; set; }

        public IThreadPool ThreadPool { get; set; }

        public Func<ConnectionContext, Frame> FrameFactory { get; set; }

        //public DateHeaderValueManager DateHeaderValueManager { get; set; }

        public NetworkConfigure ServerInformation { get; set; }

        //todo:nofix
        //internal IHttpComponentFactory HttpComponentFactory { get; set; }
    }

    public class Frame : FrameContext, IFrameControl
    {
        private bool _requestProcessingStarted;
        private Task _requestProcessingTask;
        private bool _requestProcessingStopping;
        private Vector<byte> _vectorSpaces;
        private Vector<byte> _vectorQuestionMarks;
        private Vector<byte> _vectorPercentages;
        private Vector<byte> _vectorCRs;
        private int _requestAborted;
        protected CancellationTokenSource _abortedCts;

        public Frame(ConnectionContext context)
           : base(context)
        {
        }

        public void Flush()
        {
            //TODO: notimp
        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void ProduceContinue()
        {
            //TODO: notimp
        }

        public void Write(ArraySegment<byte> data)
        {
            //TODO: notimp
        }

        public Task WriteAsync(ArraySegment<byte> data, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        internal void Start()
        {
            //TODO: notimp
            Log.Debug(0, "this async process request...");

            if (!_requestProcessingStarted)
            {
                _requestProcessingStarted = true;
                _requestProcessingTask =
                    Task.Factory.StartNew(
                        (o) => ((Frame)o).RequestProcessingAsync(),
                        this,
                        default(CancellationToken),
                        TaskCreationOptions.DenyChildAttach,
                        TaskScheduler.Default);
            }
        }

        public Task Stop()
        {
            //TODO: notimp
            if (!_requestProcessingStopping)
            {
                _requestProcessingStopping = true;
            }
            return _requestProcessingTask ?? TaskUtilities.CompletedTask;
        }

        public void Abort()
        {
            //TODO: notimp
            if (Interlocked.CompareExchange(ref _requestAborted, 1, 0) == 0)
            {
                _requestProcessingStopping = true;

                //_frameStreams?.RequestBody.Abort();
                //_frameStreams?.ResponseBody.Abort();

                try
                {
                    ConnectionControl.End(ProduceEndType.SocketDisconnect);
                }
                catch (Exception ex)
                {
                    Log.Error(0, "Abort", ex);
                }

                try
                {
                    RequestAbortedSource.Cancel();
                }
                catch (Exception ex)
                {
                    Log.Error(0, "Abort", ex);
                }
                _abortedCts = null;
            }
        }

        public virtual async Task RequestProcessingAsync()
        {
            while (!_requestProcessingStopping)
            {
                while (!_requestProcessingStopping && !TakeStartLine(SocketInput))
                {
                    if (SocketInput.RemoteIntakeFin)
                    {
                        // We need to attempt to consume start lines and headers even after
                        // SocketInput.RemoteIntakeFin is set to true to ensure we don't close a
                        // connection without giving the application a chance to respond to a request
                        // sent immediately before the a FIN from the client.
                        if (TakeStartLine(SocketInput))
                        {
                            break;
                        }

                        return;
                    }

                    await SocketInput;
                }
            }
        }

        private CancellationTokenSource RequestAbortedSource
        {
            get
            {
                // Get the abort token, lazily-initializing it if necessary.
                // Make sure it's canceled if an abort request already came in.
                var cts = LazyInitializer.EnsureInitialized(ref _abortedCts, () => new CancellationTokenSource());
                if (Volatile.Read(ref _requestAborted) == 1)
                {
                    cts.Cancel();
                }
                return cts;
            }
        }
        protected bool TakeStartLine(SocketInput input)
        {
            var scan = input.ConsumingStart();
            var consumed = scan;
            try
            {
                string method;
                var begin = scan;
                if (!begin.GetKnownMethod(ref scan, out method))
                {
                    if (scan.Seek(ref _vectorSpaces) == -1)
                    {
                        return false;
                    }
                    method = begin.GetAsciiString(scan);
                    scan.Take();
                }

                begin = scan;

                var needDecode = false;
                var chFound = scan.Seek(ref _vectorSpaces, ref _vectorQuestionMarks, ref _vectorPercentages);
                if (chFound == -1)
                {
                    return false;
                }
                else if (chFound == '%')
                {
                    needDecode = true;
                    chFound = scan.Seek(ref _vectorSpaces, ref _vectorQuestionMarks);
                    if (chFound == -1)
                    {
                        return false;
                    }
                }

                var pathBegin = begin;
                var pathEnd = scan;

                var queryString = "";
                if (chFound == '?')
                {
                    begin = scan;
                    if (scan.Seek(ref _vectorSpaces) != ' ')
                    {
                        return false;
                    }
                    queryString = begin.GetAsciiString(scan);
                }

                scan.Take();
                begin = scan;

                string httpVersion;
                if (!begin.GetKnownVersion(ref scan, out httpVersion))
                {
                    scan = begin;
                    if (scan.Seek(ref _vectorCRs) == -1)
                    {
                        return false;
                    }
                    httpVersion = begin.GetAsciiString(scan);

                    scan.Take();
                }
                if (scan.Take() != '\n')
                {
                    return false;
                }

                // URIs are always encoded/escaped to ASCII https://tools.ietf.org/html/rfc3986#page-11 
                // Multibyte Internationalized Resource Identifiers (IRIs) are first converted to utf8; 
                // then encoded/escaped to ASCII  https://www.ietf.org/rfc/rfc3987.txt "Mapping of IRIs to URIs"
                string requestUrlPath;
                if (needDecode)
                {
                    // URI was encoded, unescape and then parse as utf8
                    pathEnd = UrlPathDecoder.Unescape(pathBegin, pathEnd);
                    requestUrlPath = pathBegin.GetUtf8String(pathEnd);
                    requestUrlPath = PathNormalizer.NormalizeToNFC(requestUrlPath);
                }
                else
                {
                    // URI wasn't encoded, parse as ASCII
                    requestUrlPath = pathBegin.GetAsciiString(pathEnd);
                }
                //todo:notimp
                Log.Debug(0, requestUrlPath);
                return true;
            }
            finally
            {
                input.ConsumingComplete(consumed, scan);
            }
        }

    }

}
