// Copyright (c) AggrEngine. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License in the project root for license information.

using System;

namespace AggrEngine.NetworkIO
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="tag">user's object</param>
    public delegate void LoggerHandle(string message, object tag);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="tag">user's object</param>
    public delegate void LoggerErrorHandle(string message, Exception exception, object tag);
}
