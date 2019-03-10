// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Couchbase.Infrastructure;
using Microsoft.EntityFrameworkCore.Couchbase.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public static class CouchbaseContextOptionsExtensions
    {
        public static DbContextOptionsBuilder<TContext> UseCouchbase<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] ClientConfiguration clientConfiguration,
            [NotNull] IAuthenticator authenticator,
            [CanBeNull] Action<CouchbaseContextOptionsBuilder> CouchbaseOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseCouchbase(
                (DbContextOptionsBuilder)optionsBuilder,
                clientConfiguration,
                authenticator,
                CouchbaseOptionsAction);

        public static DbContextOptionsBuilder UseCouchbase(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] ClientConfiguration clientConfiguration,
            [NotNull] IAuthenticator authenticator,
//            [NotNull] string serviceEndPoint,
//            [NotNull] string authKeyOrResourceToken,
//            [NotNull] string databaseName,
            [CanBeNull] Action<CouchbaseContextOptionsBuilder> CouchbaseOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(clientConfiguration, nameof(clientConfiguration));
            Check.NotNull(authenticator, nameof(authenticator));

//            Check.NotNull(serviceEndPoint, nameof(serviceEndPoint));
//            Check.NotEmpty(authKeyOrResourceToken, nameof(authKeyOrResourceToken));
//            Check.NotEmpty(databaseName, nameof(databaseName));

            var extension = optionsBuilder.Options.FindExtension<CouchbaseOptionsExtension>()
                            ?? new CouchbaseOptionsExtension();

            extension = extension
                .WithClientConfiguration(clientConfiguration)
                .WithAuthenticator(authenticator);
//                .WithServiceEndPoint(serviceEndPoint)
//                .WithAuthKeyOrResourceToken(authKeyOrResourceToken)
//                .WithDatabaseName(databaseName);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            CouchbaseOptionsAction?.Invoke(new CouchbaseContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }
    }
}
