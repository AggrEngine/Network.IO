// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using Microsoft.AspNetCore.Server.Kestrel.Filter;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Server.Kestrel.Http;

namespace AggrEngine.NetworkIO
{

    public class NetworkContext
    {
        private IFeatureCollection contextFeatures;

        public NetworkContext(IFeatureCollection contextFeatures)
        {
            this.contextFeatures = contextFeatures;
        }
        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }
        public void Write(byte[] data, int offset, int count)
        {
            var context = contextFeatures as Frame<NetworkContext>;
            if(context!= null)
            {
                context.ResponseBody.WriteAsync(data, offset, count);
            }
        }
        
    }

    public class PoolingParameter
    {
        public int MaxPooledHeaders { get; internal set; }
        public int MaxPooledStreams { get; internal set; }
    }
    public class NetworkAddress
    {
        public NetworkAddress()
        {

        }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string PathBase { get; set; }
        public bool IsUnixPipe { get { return Host.StartsWith(Constants.UnixPipeHostPrefix); } }
        public int Port { get; set; }
        public string UnixPipePath
        {
            get
            {
                Debug.Assert(IsUnixPipe);

                return Host.Substring(Constants.UnixPipeHostPrefix.Length - 1);
            }
        }

        public PoolingParameter PoolingParameters { get; private set; } = new PoolingParameter();

        public override string ToString()
        {
            return Scheme.ToLowerInvariant() + "://" + Host.ToLowerInvariant() + ":" + Port.ToString(CultureInfo.InvariantCulture) + PathBase.ToLowerInvariant();
        }

        public static NetworkAddress FromUrl(string url)
        {
            url = url ?? string.Empty;

            int schemeDelimiterStart = url.IndexOf("://", StringComparison.Ordinal);
            if (schemeDelimiterStart < 0)
            {
                int port;
                if (int.TryParse(url, NumberStyles.None, CultureInfo.InvariantCulture, out port))
                {
                    return new NetworkAddress()
                    {
                        Scheme = "http",
                        Host = "+",
                        Port = port,
                        PathBase = "/"
                    };
                }
                return null;
            }
            int schemeDelimiterEnd = schemeDelimiterStart + "://".Length;

            var isUnixPipe = url.IndexOf(Constants.UnixPipeHostPrefix, schemeDelimiterEnd, StringComparison.Ordinal) == schemeDelimiterEnd;

            int pathDelimiterStart;
            int pathDelimiterEnd;
            if (!isUnixPipe)
            {
                pathDelimiterStart = url.IndexOf("/", schemeDelimiterEnd, StringComparison.Ordinal);
                pathDelimiterEnd = pathDelimiterStart;
            }
            else
            {
                pathDelimiterStart = url.IndexOf(":", schemeDelimiterEnd + Constants.UnixPipeHostPrefix.Length, StringComparison.Ordinal);
                pathDelimiterEnd = pathDelimiterStart + ":".Length;
            }

            if (pathDelimiterStart < 0)
            {
                pathDelimiterStart = pathDelimiterEnd = url.Length;
            }

            var serverAddress = new NetworkAddress();
            serverAddress.Scheme = url.Substring(0, schemeDelimiterStart);

            var hasSpecifiedPort = false;
            if (!isUnixPipe)
            {
                int portDelimiterStart = url.LastIndexOf(":", pathDelimiterStart - 1, pathDelimiterStart - schemeDelimiterEnd, StringComparison.Ordinal);
                if (portDelimiterStart >= 0)
                {
                    int portDelimiterEnd = portDelimiterStart + ":".Length;

                    string portString = url.Substring(portDelimiterEnd, pathDelimiterStart - portDelimiterEnd);
                    int portNumber;
                    if (int.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out portNumber))
                    {
                        hasSpecifiedPort = true;
                        serverAddress.Host = url.Substring(schemeDelimiterEnd, portDelimiterStart - schemeDelimiterEnd);
                        serverAddress.Port = portNumber;
                    }
                }

                if (!hasSpecifiedPort)
                {
                    if (string.Equals(serverAddress.Scheme, "http", StringComparison.OrdinalIgnoreCase))
                    {
                        serverAddress.Port = 80;
                    }
                    else if (string.Equals(serverAddress.Scheme, "https", StringComparison.OrdinalIgnoreCase))
                    {
                        serverAddress.Port = 443;
                    }
                }
            }

            if (!hasSpecifiedPort)
            {
                serverAddress.Host = url.Substring(schemeDelimiterEnd, pathDelimiterStart - schemeDelimiterEnd);
            }

            // Path should not end with a / since it will be used as PathBase later
            if (url[url.Length - 1] == '/')
            {
                serverAddress.PathBase = url.Substring(pathDelimiterEnd, url.Length - pathDelimiterEnd - 1);
            }
            else
            {
                serverAddress.PathBase = url.Substring(pathDelimiterEnd);
            }
            
            return serverAddress;
        }
    }


    public class NetworkConfigure
    {
        public IConnectionFilter ConnectionFilter { get; set; }
        public bool NoDelay { get; set; }
        public TimeSpan ShutdownTimeout { get; set; }
    }

}
