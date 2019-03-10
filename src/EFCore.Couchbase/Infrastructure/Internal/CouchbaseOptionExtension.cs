// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Couchbase.Infrastructure.Internal
{
    public class CouchbaseOptionsExtension : IDbContextOptionsExtension
    {
        private ClientConfiguration _clientConfiguration;
        private IAuthenticator _authenticator;
        private string _bucketName;

        private Func<ExecutionStrategyDependencies, IExecutionStrategy> _executionStrategyFactory;
        private string _logFragment;

        public CouchbaseOptionsExtension()
        {
        }

        protected CouchbaseOptionsExtension(CouchbaseOptionsExtension copyFrom)
        {
            _clientConfiguration = copyFrom._clientConfiguration;
            _authenticator = copyFrom._authenticator;
            _executionStrategyFactory = copyFrom._executionStrategyFactory;
        }

        public virtual CouchbaseOptionsExtension WithClientConfiguration(ClientConfiguration clientConfiguration)
        {
            var clone = Clone();

            clone._clientConfiguration = clientConfiguration;

            return clone;
        }

        public virtual ClientConfiguration ClientConfiguration => _clientConfiguration;

        public virtual CouchbaseOptionsExtension WithAuthenticator(IAuthenticator authenticator)
        {
            var clone = Clone();

            clone._authenticator = authenticator;

            return clone;
        }
        public virtual string BucketName => _bucketName;

        public virtual CouchbaseOptionsExtension WithBucketName(string bucketName)
        {
            var clone = Clone();

            clone._bucketName = bucketName;

            return clone;
        }

        public virtual IAuthenticator Authenticator => _authenticator;

        /// <summary>
        ///     A factory for creating the default <see cref="IExecutionStrategy" />, or <c>null</c> if none has been
        ///     configured.
        /// </summary>
        public virtual Func<ExecutionStrategyDependencies, IExecutionStrategy> ExecutionStrategyFactory => _executionStrategyFactory;

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="executionStrategyFactory"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CouchbaseOptionsExtension WithExecutionStrategyFactory(
            [CanBeNull] Func<ExecutionStrategyDependencies, IExecutionStrategy> executionStrategyFactory)
        {
            var clone = Clone();

            clone._executionStrategyFactory = executionStrategyFactory;

            return clone;
        }

        protected virtual CouchbaseOptionsExtension Clone() => new CouchbaseOptionsExtension(this);

        public bool ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkCouchbase();

            return true;
        }

        public long GetServiceProviderHashCode()
        {
            return 0;
        }

        public void Validate(IDbContextOptions options)
        {
        }

        public virtual void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Couchbase"] = "1";
        }

        public string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var builder = new StringBuilder();

                    // TODO: log client configuration stuff?
//                    builder.Append("ServiceEndPoint=").Append(_serviceEndPoint).Append(' ');
//
//                    builder.Append("Database=").Append(_databaseName).Append(' ');

                    _logFragment = builder.ToString();
                }

                return _logFragment;
            }
        }
    }
}
