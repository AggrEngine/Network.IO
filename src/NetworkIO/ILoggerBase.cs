// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using System;

namespace AggrEngine.NetworkIO
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILoggerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        bool IsEnabled(int logLevel);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        void Trace(int eventId, object state);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="formatter"></param>
        void Trace(int eventId, object state, Func<object, string> formatter);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        void Debug(int eventId, object state);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="formatter"></param>
        void Debug(int eventId, object state, Func<object, string> formatter);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        void Info(int eventId, object state);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="formatter"></param>
        void Info(int eventId, object state, Func<object, string> formatter);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        void Warning(int eventId, object state);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="formatter"></param>
        void Warning(int eventId, object state, Func<object, string> formatter);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        void Error(int eventId, object state, Exception exception);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        void Error(int eventId, object state, Exception exception, Func<object, Exception, string> formatter);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        void Critical(int eventId, object state, Exception exception);
        /// <summary>
        /// Write an system crash, or a catastrophic failure message
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        void Critical(int eventId, object state, Exception exception, Func<object, Exception, string> formatter);
    }
}
