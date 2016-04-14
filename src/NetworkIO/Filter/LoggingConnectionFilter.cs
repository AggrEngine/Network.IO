// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using AggrEngine.NetworkIO;

namespace Microsoft.AspNetCore.Server.Kestrel.Filter
{
    public class LoggingConnectionFilter : IConnectionFilter
    {
        private readonly ILoggerBase _logger;
        private readonly IConnectionFilter _previous;

        public LoggingConnectionFilter(ILoggerBase logger, IConnectionFilter previous)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (previous == null)
            {
                throw new ArgumentNullException(nameof(previous));
            }

            _logger = logger;
            _previous = previous;
        }

        public async Task OnConnectionAsync(ConnectionFilterContext context)
        {
            await _previous.OnConnectionAsync(context);

            context.Connection = new LoggingStream(context.Connection, _logger);
        }
    }
}
