// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Couchbase.Storage.Internal
{
    public class CouchbaseDatabaseCreator : IDatabaseCreator
    {
        private readonly CouchbaseClientWrapper _CouchbaseClient;
        private readonly StateManagerDependencies _stateManagerDependencies;

        public CouchbaseDatabaseCreator(
            CouchbaseClientWrapper CouchbaseClient,
            StateManagerDependencies stateManagerDependencies)
        {
            _CouchbaseClient = CouchbaseClient;
            _stateManagerDependencies = stateManagerDependencies;
        }

        public bool EnsureCreated()
        {
            var created = _CouchbaseClient.CreateDatabaseIfNotExists();
            foreach (var entityType in _stateManagerDependencies.Model.GetEntityTypes())
            {
                created |= _CouchbaseClient.CreateContainerIfNotExists(entityType.Couchbase().ContainerName, "__partitionKey");
            }

            if (created)
            {
                var stateManager = new StateManager(_stateManagerDependencies);
                foreach (var entityType in _stateManagerDependencies.Model.GetEntityTypes())
                {
                    foreach (var targetSeed in entityType.GetData())
                    {
                        var entry = stateManager.CreateEntry(targetSeed, entityType);
                        entry.SetEntityState(EntityState.Added);
                    }
                }

                stateManager.SaveChanges(acceptAllChangesOnSuccess: false);
            }

            return created;
        }

        public async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
        {
            var created = await _CouchbaseClient.CreateDatabaseIfNotExistsAsync(cancellationToken);
            foreach (var entityType in _stateManagerDependencies.Model.GetEntityTypes())
            {
                created |= await _CouchbaseClient.CreateContainerIfNotExistsAsync(entityType.Couchbase().ContainerName, "__partitionKey", cancellationToken);
            }

            if (created)
            {
                var stateManager = new StateManager(_stateManagerDependencies);
                foreach (var entityType in _stateManagerDependencies.Model.GetEntityTypes())
                {
                    foreach (var targetSeed in entityType.GetData())
                    {
                        var entry = stateManager.CreateEntry(targetSeed, entityType);
                        entry.SetEntityState(EntityState.Added);
                    }
                }

                await stateManager.SaveChangesAsync(acceptAllChangesOnSuccess: false);
            }

            return created;
        }

        public bool EnsureDeleted() => _CouchbaseClient.DeleteDatabase();

        public Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
            => _CouchbaseClient.DeleteDatabaseAsync(cancellationToken);

        public virtual bool CanConnect()
            => throw new NotImplementedException();

        public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
