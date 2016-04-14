// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using System;
using System.IO;

namespace AggrEngine.NetworkIO
{
    public class BaseLogger : ILoggerBase
    {
        private string NowString
        {
            get
            {
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
        }
        protected int traceLevel = 1;
        protected int debugLevel = 2;
        protected int infoLevel = 3;
        protected int warningLevel = 4;
        protected int errorLevel = 5;
        protected int criticalLevel = 6;
        private int _logLevel;

        public BaseLogger()
        {
            _logLevel = traceLevel;
        }

        public int LogLevel { set { _logLevel = value; } }

        public virtual bool IsEnabled(int logLevel)
        {
            return _logLevel <= logLevel;
        }

        public void Trace(int eventId, object state)
        {
            Trace(eventId, state, arg => arg.ToString());
        }

        public virtual void Trace(int eventId, object state, Func<object, string> formatter)
        {
            if (!IsEnabled(traceLevel)) return;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("{0} [TRACE]-{1} {2}", NowString, eventId, formatter(state));
            Console.ResetColor();
        }

        public void Debug(int eventId, object state)
        {
            Debug(eventId, state, arg => arg.ToString());
        }

        public virtual void Debug(int eventId, object state, Func<object, string> formatter)
        {
            if (!IsEnabled(debugLevel)) return;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("{0} [DEBUG]-{1} {2}", NowString, eventId, formatter(state));
            Console.ResetColor();
        }

        public void Info(int eventId, object state)
        {
            Info(eventId, state, arg => arg.ToString());
        }

        public virtual void Info(int eventId, object state, Func<object, string> formatter)
        {
            if (!IsEnabled(infoLevel)) return;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("{0} [INFO]-{1} {2}", NowString, eventId, formatter(state));
            Console.ResetColor();
        }
        public void Warning(int eventId, object state)
        {
            Warning(eventId, state, null);
        }

        public void Warning(int eventId, object state, Exception exception)
        {
            Warning(eventId, state, exception, (arg, ex) => arg.ToString() + Environment.NewLine + ex);
        }

        public virtual void Warning(int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (!IsEnabled(warningLevel)) return;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0} [WARNING]-{1} {2}", NowString, eventId, formatter(state, exception));
            Console.ResetColor();
        }
        public void Error(int eventId, object state, Exception exception)
        {
            Error(eventId, state, exception, (arg, ex) => arg.ToString() + Environment.NewLine + ex);
        }

        public virtual void Error(int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (!IsEnabled(errorLevel)) return;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0} [ERROR]-{1} {2}", NowString, eventId, formatter(state, exception));
            Console.ResetColor();
        }
        public void Critical(int eventId, object state, Exception exception)
        {
            Critical(eventId, state, exception, (arg, ex) => arg.ToString() + Environment.NewLine + ex);
        }

        public virtual void Critical(int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (!IsEnabled(criticalLevel)) return;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("{0} [CRITICAL]-{1} {2}", NowString, eventId, formatter(state, exception));
            Console.ResetColor();
        }

    }
    public sealed class BadHttpRequestException : IOException
    {
        internal BadHttpRequestException(string message)
            : base(message)
        {

        }
    }
    static class NetworkLogger
    {

        private static readonly Action<ILoggerBase, string, string, string, Exception> _InfoAction1;
        private static readonly Action<ILoggerBase, string, string, Exception> _ErrorAction1;
        private static readonly Action<ILoggerBase, string, string, Exception> _DebugAction1;
        private static readonly Action<ILoggerBase, string, string, int, Exception> _DebugAction2;
        private static readonly Action<ILoggerBase, string, string, string, Exception> _DebugAction3;

        static NetworkLogger()
        {
            _InfoAction1 += InfoAction1;
            _ErrorAction1 += ErrorAction1;
            _DebugAction1 += DebugAction1;
            _DebugAction2 += DebugAction2;
            _DebugAction3 += DebugAction3;
        }

        private static void InfoAction1(ILoggerBase log, string formatter, string connectionId, string message, Exception ex)
        {
            log.Debug(0, new object[] { connectionId, message }, state => string.Format(formatter, state as object[]) + Environment.NewLine + ex);
        }

        private static void ErrorAction1(ILoggerBase log, string formatter, string connectionId, Exception ex)
        {
            log.Debug(0, connectionId, state => string.Format(formatter, state) + Environment.NewLine + ex);
        }

        private static void DebugAction3(ILoggerBase log, string formatter, string connectionId, string arg0, Exception ex)
        {
            log.Debug(0, new object[] { connectionId, arg0 }, state => string.Format(formatter, state as object[]));
        }

        private static void DebugAction2(ILoggerBase log, string formatter, string connectionId, int arg0, Exception ex)
        {
            log.Debug(0, new object[] { connectionId, arg0 }, state => string.Format(formatter, state as object[]));
        }

        private static void DebugAction1(ILoggerBase log, string formatter, string connectionId, Exception ex)
        {
            log.Debug(0, connectionId, state => string.Format(formatter, state));
        }


        public static void ConnectionStart(ILoggerBase log, string connectionId)
        {
            _DebugAction1(log, @"Connection id ""{0}"" started.", connectionId, null);
        }

        public static void ConnectionStop(ILoggerBase log, string connectionId)
        {
            _DebugAction1(log, @"Connection id ""{0}"" stopped.", connectionId, null);
        }

        public static void ConnectionRead(ILoggerBase log, string connectionId, int count)
        {
            _DebugAction2(log, @"Connection id ""{0}"" read {1}.", connectionId, count, null);
        }

        public static void ConnectionPause(ILoggerBase log, string connectionId)
        {
            _DebugAction1(log, @"Connection id ""{0}"" paused.", connectionId, null);
        }

        public static void ConnectionResume(ILoggerBase log, string connectionId)
        {
            _DebugAction1(log, @"Connection id ""{0}"" resumed.", connectionId, null);
        }

        public static void ConnectionReadFin(ILoggerBase log, string connectionId)
        {
            _DebugAction1(log, @"Connection id ""{0}"" received FIN.", connectionId, null);
        }

        public static void ConnectionWriteFin(ILoggerBase log, string connectionId)
        {
            _DebugAction1(log, @"Connection id ""{0}"" sending FIN.", connectionId, null);
        }

        public static void ConnectionWroteFin(ILoggerBase log, string connectionId, int status)
        {
            _DebugAction2(log, @"Connection id ""{0}"" sent FIN with status ""{1}"".", connectionId, status, null);
        }

        public static void ConnectionKeepAlive(ILoggerBase log, string connectionId)
        {
            _DebugAction1(log, @"Connection id ""{0}"" completed keep alive response.", connectionId, null);
        }

        public static void ConnectionDisconnect(ILoggerBase log, string connectionId)
        {
            _DebugAction1(log, @"Connection id ""{0}"" disconnecting.", connectionId, null);
        }

        public static void ConnectionWrite(ILoggerBase log, string connectionId, int count)
        {
            _DebugAction2(log, @"Connection id ""{0}"" sent count ""{1}"".", connectionId, count, null);
        }

        public static void ConnectionWriteCallback(ILoggerBase log, string connectionId, int status)
        {
            _DebugAction2(log, @"Connection id ""{0}"" sent callback FIN with status ""{1}"".", connectionId, status, null);
        }

        public static void ConnectionError(ILoggerBase log, string connectionId, Exception ex)
        {
            _InfoAction1(log, @"Connection id ""{0}"": An unhandled exception was thrown by the application.", connectionId, ex.Message, ex);
        }

        public static void ConnectionDisconnectedWrite(ILoggerBase log, string connectionId, int count, Exception ex)
        {
            _DebugAction2(log, @"Connection id ""{0}"" write of ""{1}"" bytes to disconnected client.", connectionId, count, ex);
        }

        public static void ConnectionBadRequest(ILoggerBase log, string connectionId, BadHttpRequestException ex)
        {
            _InfoAction1(log, @"Connection id ""{0}"" bad request data: ""{1}""", connectionId, ex.Message, ex);
        }

        public static void NotAllConnectionsClosedGracefully(ILoggerBase log)
        {
            _DebugAction1(log, "Some connections failed to close gracefully during server shutdown.", "", null);
        }

        public static void ApplicationError(ILoggerBase log, string connectionId, Exception ex)
        {
            _ErrorAction1(log, @"Connection id ""{0}"": An unhandled exception was thrown by the application.", connectionId, ex);
        }
    }
}
