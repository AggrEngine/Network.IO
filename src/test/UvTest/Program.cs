using AggrEngine.NetworkIO;
using Microsoft.AspNetCore.Server.Kestrel.Filter;
using Microsoft.AspNetCore.Server.Kestrel.Http;
using Microsoft.AspNetCore.Server.Kestrel.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UvTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestLog();
            TestTcpListener();
            Console.ReadLine();
        }


        private static void TestLog()
        {
            int eventId = 1;
            ILoggerBase log = new BaseLogger();
            log.Trace(eventId, "Trace...");
            log.Debug(eventId, "Debug...");
            log.Info(eventId, "Info...");
            log.Warning(eventId, "Warning...");
            log.Error(eventId, "Error message.", new Exception("error test."));
            log.Critical(eventId, "Critical message", new OutOfMemoryException("out range test."));

            log = new BaseLogger() { LogLevel = 2 };
            eventId = 2;
            log.Trace(eventId, "Trace...");
            log.Debug(eventId, "Debug...");
            log.Info(eventId, "Info...");
            log.Warning(eventId, "Warning...");
            log.Error(eventId, "Error message.", new Exception("error test."));
            log.Critical(eventId, "Critical message", new OutOfMemoryException("out range test."));

            log = new BaseLogger() { LogLevel = 3 };
            eventId = 3;
            log.Trace(eventId, "Trace...");
            log.Debug(eventId, "Debug...");
            log.Info(eventId, "Info...");
            log.Warning(eventId, "Warning...");
            log.Error(eventId, "Error message.", new Exception("error test."));
            log.Critical(eventId, "Critical message", new OutOfMemoryException("out range test."));

            log = new BaseLogger() { LogLevel = 4 };
            eventId = 4;
            log.Trace(eventId, "Trace...");
            log.Debug(eventId, "Debug...");
            log.Info(eventId, "Info...");
            log.Warning(eventId, "Warning...");
            log.Error(eventId, "Error message.", new Exception("error test."));
            log.Critical(eventId, "Critical message", new OutOfMemoryException("out range test."));

            log = new BaseLogger() { LogLevel = 5 };
            eventId = 5;
            log.Trace(eventId, "Trace...");
            log.Debug(eventId, "Debug...");
            log.Info(eventId, "Info...");
            log.Warning(eventId, "Warning...");
            log.Error(eventId, "Error message.", new Exception("error test."));
            log.Critical(eventId, "Critical message", new OutOfMemoryException("out range test."));

            log = new BaseLogger() { LogLevel = 6 };
            eventId = 6;
            log.Trace(eventId, "Trace...");
            log.Debug(eventId, "Debug...");
            log.Info(eventId, "Info...");
            log.Warning(eventId, "Warning...");
            log.Error(eventId, "Error message.", new Exception("error test."));
            log.Critical(eventId, "Critical message", new OutOfMemoryException("out range test."));
        }
        private static void TestTcpListener()
        {
            var log = new BaseLogger();
            log.Info(0, "test tcp start.............");

            var listener = new AsyncNetworkHost(new ServiceContext
            {
                FrameFactory = context =>
                {
                    return new Frame(context);
                },
                AppLifetime = new AppLifetime(log),
                Log = log,
                ThreadPool = new LoggingThreadPool(log),
                ServerInformation = new NetworkConfigure()
                {
                    NoDelay = true,
                    ShutdownTimeout = new TimeSpan(0, 0, 1),
                    ConnectionFilter = new ConnectFilter(log, new LoggingConnectionFilter(log, new NoOpConnectionFilter()))
                }
            });
            listener.Start(1);
            //var started = listener.CreateServer(new NetworkAddress() {
            //    Host = "127.0.0.1",
            //    Port = 5300
            //});
            var started = listener.CreateServer(NetworkAddress.FromUrl("http://127.0.0.1:5300/"));
            Console.Write("Iput enter is exit.");
            Console.ReadLine();
            started.Dispose();
            listener.Dispose();
        }
    }

    public class ConnectFilter : IConnectionFilter
    {
        private ILoggerBase _log;
        private IConnectionFilter _prev;

        public ConnectFilter(ILoggerBase log, IConnectionFilter prev)
        {
            _log = log;
            _prev = prev;
        }
        public async Task OnConnectionAsync(ConnectionFilterContext context)
        {
            await _prev.OnConnectionAsync(context);

            //string url = context.Address.ToString();
            //_log.Debug(0, "connect:" + url);

        }
    }
    public class AppLifetime : IApplicationLifetime
    {
        private ILoggerBase _log;
        public AppLifetime(ILoggerBase log)
        {
            _log = log;
        }
        public void StopApplication()
        {
            _log.Info(0, "App stoped.");
        }
    }
}
