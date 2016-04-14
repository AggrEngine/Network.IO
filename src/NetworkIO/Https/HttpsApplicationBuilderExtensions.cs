// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using AggrEngine.NetworkIO;

namespace Microsoft.AspNetCore.Server.Kestrel.Filter
{
    public static class HttpsApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseKestrelHttps(this IApplicationBuilder app, X509Certificate2 cert)
        {
            return app.UseKestrelHttps(new HttpsConnectionFilterOptions { ServerCertificate = cert});
        }

        public static IApplicationBuilder UseKestrelHttps(this IApplicationBuilder app, HttpsConnectionFilterOptions options)
        {
            var serverInfo = app.ServerFeatures.Get<NetworkConfigure>();

            if (serverInfo == null)
            {
                return app;
            }

            var prevFilter = serverInfo.ConnectionFilter ?? new NoOpConnectionFilter();

            serverInfo.ConnectionFilter = new HttpsConnectionFilter(options, prevFilter);

            return app;
        }
    }
}
