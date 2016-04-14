// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using Microsoft.AspNetCore.Server.Kestrel.Http;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using System.Text;

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
        private Vector<byte> _vectorColons;
        private bool _corruptedRequest;

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

        public void Reset()
        {
            ResetComponents();

            /* _onStarting = null;
             _onCompleted = null;

             _responseStarted = false;
             _keepAlive = false;
             _autoChunk = false;
             _applicationException = null;

             ResetFeatureCollection();

             Scheme = null;
             Method = null;
             RequestUri = null;
             PathBase = null;
             Path = null;
             QueryString = null;
             _httpVersion = HttpVersionType.Unknown;
             StatusCode = 200;
             ReasonPhrase = null;

             var httpConnectionFeature = this as IHttpConnectionFeature;
             httpConnectionFeature.RemoteIpAddress = RemoteEndPoint?.Address;
             httpConnectionFeature.RemotePort = RemoteEndPoint?.Port ?? 0;

             httpConnectionFeature.LocalIpAddress = LocalEndPoint?.Address;
             httpConnectionFeature.LocalPort = LocalEndPoint?.Port ?? 0;

             httpConnectionFeature.ConnectionId = ConnectionId;

             PrepareRequest?.Invoke(this);

             _manuallySetRequestAbortToken = null;*/
            _abortedCts = null;
        }

        public void ReportCorruptedHttpRequest(BadHttpRequestException ex)
        {
            _corruptedRequest = true;
            NetworkLogger.ConnectionBadRequest(Log, ConnectionId, ex);
        }

        public bool TakeMessageHeaders(SocketInput input, Action<ArraySegment<byte>, string> requestHeadersCallback)
        {
            var scan = input.ConsumingStart();
            var consumed = scan;
            try
            {
                int chFirst;
                int chSecond;
                while (!scan.IsEnd)
                {
                    var beginName = scan;
                    if (scan.Seek(ref _vectorColons, ref _vectorCRs) == -1)
                    {
                        return false;
                    }
                    var endName = scan;

                    chFirst = scan.Take();
                    var beginValue = scan;
                    chSecond = scan.Take();

                    if (chFirst == -1 || chSecond == -1)
                    {
                        return false;
                    }
                    if (chFirst == '\r')
                    {
                        if (chSecond == '\n')
                        {
                            consumed = scan;
                            return true;
                        }

                        ReportCorruptedHttpRequest(new BadHttpRequestException("Headers corrupted, invalid header sequence."));
                        // Headers corrupted, parsing headers is complete
                        return true;
                    }

                    while (
                        chSecond == ' ' ||
                        chSecond == '\t' ||
                        chSecond == '\r' ||
                        chSecond == '\n')
                    {
                        if (chSecond == '\r')
                        {
                            var scanAhead = scan;
                            var chAhead = scanAhead.Take();
                            if (chAhead == -1)
                            {
                                return false;
                            }
                            else if (chAhead == '\n')
                            {
                                chAhead = scanAhead.Take();
                                if (chAhead == -1)
                                {
                                    return false;
                                }
                                else if (chAhead != ' ' && chAhead != '\t')
                                {
                                    // If the "\r\n" isn't part of "linear whitespace",
                                    // then this header has no value.
                                    break;
                                }
                            }
                        }

                        beginValue = scan;
                        chSecond = scan.Take();

                        if (chSecond == -1)
                        {
                            return false;
                        }
                    }
                    scan = beginValue;

                    var wrapping = false;
                    while (!scan.IsEnd)
                    {
                        if (scan.Seek(ref _vectorCRs) == -1)
                        {
                            // no "\r" in sight, burn used bytes and go back to await more data
                            return false;
                        }

                        var endValue = scan;
                        chFirst = scan.Take(); // expecting: \r
                        chSecond = scan.Take(); // expecting: \n

                        if (chSecond == -1)
                        {
                            return false;
                        }
                        else if (chSecond != '\n')
                        {
                            // "\r" was all by itself, move just after it and try again
                            scan = endValue;
                            scan.Take();
                            continue;
                        }

                        var chThird = scan.Peek();
                        if (chThird == -1)
                        {
                            return false;
                        }
                        else if (chThird == ' ' || chThird == '\t')
                        {
                            // special case, "\r\n " or "\r\n\t".
                            // this is considered wrapping"linear whitespace" and is actually part of the header value
                            // continue past this for the next
                            wrapping = true;
                            continue;
                        }

                        var name = beginName.GetArraySegment(endName);
                        var value = beginValue.GetAsciiString(endValue);
                        if (wrapping)
                        {
                            value = value.Replace("\r\n", " ");
                        }

                        consumed = scan;
                        requestHeadersCallback(name, value);
                        break;
                    }
                }
                return false;
            }
            finally
            {
                input.ConsumingComplete(consumed, scan);
            }
        }

        public virtual async Task RequestProcessingAsync()
        {
            try
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

                    InitializeHeaders();
                    while (!_requestProcessingStopping && !TakeMessageHeaders(SocketInput, FrameRequestHeadersCallback))
                    {
                        if (SocketInput.RemoteIntakeFin)
                        {
                            // We need to attempt to consume start lines and headers even after
                            // SocketInput.RemoteIntakeFin is set to true to ensure we don't close a
                            // connection without giving the application a chance to respond to a request
                            // sent immediately before the a FIN from the client.
                            if (TakeMessageHeaders(SocketInput, FrameRequestHeadersCallback))
                            {
                                break;
                            }

                            return;
                        }

                        await SocketInput;
                    }


                    if (!_requestProcessingStopping)
                    {
                        await ProcessRequest();

                    }

                    Reset();
                }
            }
            finally
            {
                try
                {
                    ResetComponents();
                    _abortedCts = null;

                    // If _requestAborted is set, the connection has already been closed.
                    if (Volatile.Read(ref _requestAborted) == 0)
                    {
                        ConnectionControl.End(ProduceEndType.SocketShutdown);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(0, "Connection shutdown abnormally", ex);
                }
            }
        }

        private void FrameRequestHeadersCallback(ArraySegment<byte> entry, string value)
        {
        }

        private async Task ProcessRequest()
        {
            var data = Encoding.UTF8.GetBytes("success");
            var buffer = new ArraySegment<byte>(data, 0, data.Length);
            //TODO:notimp
            await SocketOutput.WriteAsync(buffer);
            /*
                    var messageBody = MessageBody.For(HttpVersion, FrameRequestHeaders, this);
                    _keepAlive = messageBody.RequestKeepAlive;

                    InitializeStreams(messageBody);

                    _abortedCts = null;
                    _manuallySetRequestAbortToken = null;

                    if (!_corruptedRequest)
                    {
                        var context = _application.CreateContext(this);
                        try
                        {
                            await _application.ProcessRequestAsync(context).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            ReportApplicationError(ex);
                        }
                        finally
                        {
                            // Trigger OnStarting if it hasn't been called yet and the app hasn't
                            // already failed. If an OnStarting callback throws we can go through
                            // our normal error handling in ProduceEnd.
                            // https://github.com/aspnet/KestrelHttpServer/issues/43
                            if (!_responseStarted && _applicationException == null && _onStarting != null)
                            {
                                await FireOnStarting();
                            }

                            PauseStreams();

                            if (_onCompleted != null)
                            {
                                await FireOnCompleted();
                            }

                            _application.DisposeContext(context, _applicationException);
                        }

                        // If _requestAbort is set, the connection has already been closed.
                        if (Volatile.Read(ref _requestAborted) == 0)
                        {
                            ResumeStreams();

                            if (_keepAlive && !_corruptedRequest)
                            {
                                try
                                {
                                    // Finish reading the request body in case the app did not.
                                    await messageBody.Consume();
                                }
                                catch (BadHttpRequestException ex)
                                {
                                    ReportCorruptedHttpRequest(ex);
                                }
                            }

                            await ProduceEnd();
                        }

                        StopStreams();
                    }

                    if (!_keepAlive || _corruptedRequest)
                    {
                        // End the connection for non keep alive and Bad Requests
                        // as data incoming may have been thrown off
                        return;
                    }*/
        }

        private void InitializeHeaders()
        {
        }

        protected void ResetComponents()
        {
            //var frameHeaders = Interlocked.Exchange(ref _frameHeaders, null);
            //if (frameHeaders != null)
            //{
            //    RequestHeaders = null;
            //    ResponseHeaders = null;
            //    HttpComponentFactory.DisposeHeaders(frameHeaders);
            //}

            //var frameStreams = Interlocked.Exchange(ref _frameStreams, null);
            //if (frameStreams != null)
            //{
            //    RequestBody = null;
            //    ResponseBody = null;
            //    DuplexStream = null;
            //    HttpComponentFactory.DisposeStreams(frameStreams);
            //}
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
