// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.Couchbase.TestUtilities
{
    public static class TestEnvironment
    {
        private static readonly ClientConfiguration _clientConfiguration;
        private static readonly IAuthenticator _authenticator;
        public static IConfiguration Config { get; }

        static TestEnvironment()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true)
                .AddJsonFile("config.test.json", optional: true)
                .AddEnvironmentVariables();

            Config = configBuilder.Build()
                .GetSection("Test:Couchbase:Sql");

            _clientConfiguration = new ClientConfiguration { Servers = new List<Uri> { new Uri("http://localhost:8091")}};
            _authenticator = new PasswordAuthenticator("Administrator", "password");
        }

        public static ClientConfiguration ClientConfiguration => _clientConfiguration; // Config["Couchbase"];

        public static IAuthenticator Authenticator => _authenticator; // Config["AuthToken"];
    }
}
