// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using Microsoft.AspNetCore.Server.Kestrel.Filter;
using System;

namespace AggrEngine.NetworkIO
{
    public class NetworkAddress
    {
        public string Host { get; set; }
        public bool IsUnixPipe { get; set; }
        public int Port { get; set; }
        public string UnixPipePath { get; set; }
    }


    public class NetworkConfigure
    {
        public IConnectionFilter ConnectionFilter { get; set; }
        public bool NoDelay { get; set; }
        public TimeSpan ShutdownTimeout { get; set; }
    }
    
    public interface IFeatureCollection
    {

    }
}
