// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Couchbase
{
    public class AwaitableNotImplementedException<TResult> : NotImplementedException
    {
        public AwaitableNotImplementedException() { }

        public AwaitableNotImplementedException(string message) : base(message) { }

        // This method makes the constructor awaitable.
        public TaskAwaiter<AwaitableNotImplementedException<TResult>> GetAwaiter()
        {
            throw this;
        }
    }
}
