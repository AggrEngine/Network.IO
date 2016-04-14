// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using AggrEngine.NetworkIO;

namespace Microsoft.AspNetCore.Server.Kestrel.Filter
{
    public static class LoggingFilterApplicationBuilderExtensions
    {
        /// <summary>
        /// Emits verbose logs for bytes read from and written to the connection.
        /// </summary>
        /// <returns></returns>
        public static IApplicationBuilder UseKestrelConnectionLogging(this IApplicationBuilder app)
        {
            return app.UseKestrelConnectionLogging(nameof(LoggingConnectionFilter));
        }

        /// <summary>
        /// Emits verbose logs for bytes read from and written to the connection.
        /// </summary>
        /// <returns></returns>
        public static IApplicationBuilder UseKestrelConnectionLogging(this IApplicationBuilder app, string loggerName)
        {
            var serverInfo = app.ServerFeatures.Get<NetworkConfigure>();
            if (serverInfo != null)
            {
                var prevFilter = serverInfo.ConnectionFilter ?? new NoOpConnectionFilter();
                var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(loggerName ?? nameof(LoggingConnectionFilter));
                serverInfo.ConnectionFilter = new LoggingConnectionFilter(logger, prevFilter);
            }
            return app;
        }
    }
}
