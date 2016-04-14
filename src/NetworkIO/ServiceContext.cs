// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using Microsoft.AspNetCore.Server.Kestrel.Http;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Extensions.Primitives;

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
            DateHeaderValueManager = context.DateHeaderValueManager;
            ServerInformation = context.ServerInformation;
            HttpComponentFactory = context.HttpComponentFactory;
        }

        public IApplicationLifetime AppLifetime { get; set; }

        public ILoggerBase Log { get; set; }

        public IThreadPool ThreadPool { get; set; }

        public Func<ConnectionContext, Frame> FrameFactory { get; set; }

        public DateHeaderValueManager DateHeaderValueManager { get; set; }

        public NetworkConfigure ServerInformation { get; set; }

        internal IHttpComponentFactory HttpComponentFactory { get; set; }
    }

    public interface IHeaderDictionary : IDictionary<string, StringValues>
    {
        StringValues this[string key]
        {
            get;
            set;
        }
    }
    
}
