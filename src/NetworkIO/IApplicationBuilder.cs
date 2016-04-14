// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Http;
using System.Collections;
using System.Collections.Generic;

namespace AggrEngine.NetworkIO
{
    public interface IApplicationBuilder
    {
        IRequiredService ApplicationServices { get; set; }
        IServerFeatures ServerFeatures { get; set; }
    }

    public interface IHttpApplication<T>
    {
        T CreateContext(IFeatureCollection contextFeatures);
        Task ProcessRequestAsync(T context);
        void DisposeContext(T context, Exception _applicationException);
    }
    public interface IServerFeatures
    {
        T Get<T>() where T: NetworkConfigure;
    }

    public interface IRequiredService
    {
        T GetRequiredService<T>() where T : ILoggerFactory;
    }

    public interface ILoggerFactory
    {
        ILoggerBase CreateLogger(string loggerName);
    }

    public interface IHttpRequestFeature
    {
        Stream Body { get; set; }
        IHeaderDictionary Headers { get; set; }
        string Method { get; set; }
        string Path { get; set; }
        string PathBase { get; set; }
        string Protocol { get; set; }
        string QueryString { get; set; }
        string Scheme { get; set; }
    }

    public interface IHttpResponseFeature
    {
        Stream Body { get; set; }
        bool HasStarted { get; }
        IHeaderDictionary Headers { get; set; }
        string ReasonPhrase { get; set; }
        int StatusCode { get; set; }

        void OnStarting(Func<object, Task> callback, object state);
        void OnCompleted(Func<object, Task> callback, object state);
    }

    public interface IHttpUpgradeFeature
    {
        bool IsUpgradableRequest { get; }

        Task<Stream> UpgradeAsync();
    }

    public interface IHttpConnectionFeature
    {
        string ConnectionId { get; set; }
        IPAddress LocalIpAddress { get; set; }
        int LocalPort { get; set; }
        IPAddress RemoteIpAddress { get; set; }
        int RemotePort { get; set; }
    }

    public interface IHttpRequestLifetimeFeature
    {
        CancellationToken RequestAborted { get; set; }

        void Abort();
    }

    public interface IFeatureCollection : IEnumerable<KeyValuePair<Type, object>>, IEnumerable
    {
        bool IsReadOnly { get; }
        int Revision { get; }
        object this[Type key] { get; set; }
        TFeature Get<TFeature>();
        void Set<TFeature>(TFeature instance);
    }

    public interface IHttpRequestIdentifierFeature
    {

    }

    public interface IServiceProvidersFeature
    {

    }
    public interface IQueryFeature
    {

    }
    public interface IFormFeature
    {

    }
    public interface IResponseCookiesFeature
    {

    }
    public interface IItemsFeature
    {

    }
    public interface ITlsConnectionFeature
    {

    }
    public interface IHttpWebSocketFeature
    {

    }
    public interface ISessionFeature
    {

    }
    public interface IHttpSendFileFeature
    {

    }
    public interface IHttpAuthenticationFeature
    {

    }
    
}
