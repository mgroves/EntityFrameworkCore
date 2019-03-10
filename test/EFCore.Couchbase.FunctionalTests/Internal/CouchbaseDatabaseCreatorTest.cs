// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Microsoft.EntityFrameworkCore.Couchbase.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Couchbase.Storage.Internal
{
    public class CouchbaseDatabaseCreatorTest
    {
        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EnsureCreated_returns_true_when_database_does_not_exist(bool async)
        {
            using (var testDatabase = CouchbaseTestStore.Create("NonExisting"))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.True(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
                }
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EnsureCreated_returns_true_when_database_exists_but_collections_does_not(bool async)
        {
            using (var testDatabase = CouchbaseTestStore.CreateInitialized("EnsureCreatedTest"))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.True(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
                }
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EnsureCreated_returns_false_when_database_and_collections_exists(bool async)
        {
            using (var testDatabase = CouchbaseTestStore.Create("EnsureCreatedReady"))
            {
                testDatabase.Initialize(null, () => new BloggingContext(testDatabase), null);

                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.False(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
                }
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EnsureDeleted_returns_true_when_database_exists(bool async)
        {
            using (var testDatabase = CouchbaseTestStore.CreateInitialized("EnsureDeleteBlogging"))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.True(async ? await creator.EnsureDeletedAsync() : creator.EnsureDeleted());
                }
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EnsureDeleted_returns_false_when_database_does_not_exist(bool async)
        {
            using (var testDatabase = CouchbaseTestStore.Create("EnsureDeleteBlogging"))
            {
                using (var context = new BloggingContext(testDatabase))
                {
                    var creator = context.GetService<IDatabaseCreator>();

                    Assert.False(async ? await creator.EnsureDeletedAsync() : creator.EnsureDeleted());
                }
            }
        }

        public class BloggingContext : DbContext
        {
            private readonly ClientConfiguration _clientConfiguration;
            private readonly IAuthenticator _authenticator;
            private readonly string _bucketName;

            public BloggingContext(CouchbaseTestStore testStore)
            {
                _clientConfiguration = testStore.ClientConfiguration;
                _authenticator = testStore.Authenticator;
                _bucketName = testStore.BucketName;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseCouchbase(
                        _clientConfiguration,
                        _authenticator,
                        _bucketName);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
            }

            public DbSet<Blog> Blogs { get; set; }
        }

        public class Blog
        {
            public int Id { get; set; }
        }
    }
}
